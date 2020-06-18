﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Xamarin.Forms;

namespace GameCtor.RxNavigation.XamForms
{
    /// <summary>
    /// A class that manages a stack of views.
    /// </summary>
    public sealed class ViewShell : Xamarin.Forms.NavigationPage, IViewShell
    {
        private readonly IScheduler _backgroundScheduler;
        private readonly IScheduler _mainScheduler;
        private readonly IViewLocator _viewLocator;
        private readonly IObservable<IPageViewModel> _pagePopped;
        private readonly IObservable<Unit> _modalPopped;
        private readonly IObservable<Page> _modalPushed;

        private Stack<NavigationPage> _navigationPages;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewShell"/> class.
        /// </summary>
        /// <param name="backgroundScheduler">A background scheduler.</param>
        /// <param name="mainScheduler">A main scheduler.</param>
        /// <param name="viewLocator">A view locator.</param>
        public ViewShell(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            _backgroundScheduler = backgroundScheduler;
            _mainScheduler = mainScheduler;
            _viewLocator = viewLocator;

            _navigationPages = new Stack<NavigationPage>();

            _modalPushed = Observable
                .FromEventPattern<ModalPushedEventArgs>(
                    x => Application.Current.ModalPushed += x,
                    x => Application.Current.ModalPushed -= x)
                .Select(x => x.EventArgs.Modal);

            _pagePopped = _modalPushed
                .StartWith(this)
                .Select(page => page as NavigationPage)
                .Where(x => x != null)
                .Do(x => _navigationPages.Push(x))
                .SelectMany(
                    navigationPage =>
                    {
                        return Observable
                            .FromEventPattern<NavigationEventArgs>(x => navigationPage.Popped += x, x => navigationPage.Popped -= x)
                            .Select(x => x.EventArgs.Page.BindingContext as IPageViewModel)
                            .Where(x => x != null);
                    });

            _modalPopped = Observable
                .FromEventPattern<ModalPoppedEventArgs>(
                    x => Application.Current.ModalPopped += x,
                    x => Application.Current.ModalPopped -= x)
                .Do(
                    x =>
                    {
                        if (x.EventArgs.Modal is NavigationPage)
                        {
                            _navigationPages.Pop();
                        }
                    })
                .Select(x => Unit.Default);
        }

        /// <inheritdoc/>
        public IObservable<IPageViewModel> PagePopped => _pagePopped;

        /// <inheritdoc/>
        public IObservable<Unit> ModalPopped => _modalPopped;

        /// <inheritdoc/>
        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            // If we don't have a root page yet, be sure we create one and assign one immediately because otherwise we'll get an exception.
            // Otherwise, create it off the main thread to improve responsiveness and perceived performance.
            var hasRoot = Navigation.NavigationStack.Count > 0;
            var mainScheduler = hasRoot ? _mainScheduler : CurrentThreadScheduler.Instance;
            var backgroundScheduler = hasRoot ? _backgroundScheduler : CurrentThreadScheduler.Instance;

            return Observable
                .Start(
                    () =>
                    {
                        var page = LocatePageFor(pageViewModel, contract);
                        page.Title = pageViewModel.Title;
                        return page;
                    },
                    backgroundScheduler)
                .ObserveOn(mainScheduler)
                .SelectMany(
                    page =>
                    {
                        if (resetStack)
                        {
                            if (Navigation.NavigationStack.Count == 0)
                            {
                                return _navigationPages
                                    .Peek()
                                    .Navigation
                                    .PushAsync(page, animated: false)
                                    .ToObservable();
                            }
                            else
                            {
                                // XF does not allow us to pop to a new root page. Instead, we need to inject the new root page and then pop to it.
                                var currentNav = _navigationPages.Peek().Navigation;
                                return currentNav
                                    .PushAsync(page, animate)
                                    .ToObservable()
                                    .Do(
                                        _ =>
                                        {
                                            for (int i = currentNav.NavigationStack.Count - 2; i >= 0; --i)
                                            {
                                                var p = currentNav.NavigationStack[i];
                                                currentNav.RemovePage(p);
                                            }
                                        });
                            }
                        }
                        else
                        {
                            return _navigationPages
                                .Peek()
                                .Navigation
                                .PushAsync(page, animate)
                                .ToObservable();
                        }
                    });
        }

        /// <inheritdoc/>
        public IObservable<Unit> PopPage(bool animate) =>
            _navigationPages
                .Peek()
                .Navigation
                .PopAsync(animate)
                .ToObservable()
                .Select(_ => Unit.Default)
                // XF completes the pop operation on a background thread :/
                .ObserveOn(_mainScheduler);

        /// <inheritdoc/>
        public void InsertPage(int index, IPageViewModel pageViewModel, string contract = null)
        {
            var page = LocatePageFor(pageViewModel, contract);
            page.Title = pageViewModel.Title;
            var currentNavigationPage = _navigationPages.Peek();
            currentNavigationPage.Navigation.InsertPageBefore(page, currentNavigationPage.Navigation.NavigationStack[index]);
        }

        /// <inheritdoc/>
        public void RemovePage(int index)
        {
            var page = _navigationPages.Peek().Navigation.NavigationStack[index];
            _navigationPages.Peek().Navigation.RemovePage(page);
        }

        /// <inheritdoc/>
        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            return Observable
                .Start(
                    () =>
                    {
                        var page = LocatePageFor(modalViewModel, contract);
                        page.Title = modalViewModel.Title;
                        if (withNavStack)
                        {
                            page = new NavigationPage(page);
                        }

                        return page;
                    },
                    _backgroundScheduler)
                .ObserveOn(_mainScheduler)
                .SelectMany(
                    page =>
                        Navigation
                            .PushModalAsync(page)
                            .ToObservable());
        }

        /// <inheritdoc/>
        public IObservable<Unit> PopModal() =>
            Navigation
                .PopModalAsync()
                .ToObservable()
                .Select(_ => Unit.Default)
                // XF completes the pop operation on a background thread :/
                .ObserveOn(_mainScheduler);

        private Page LocatePageFor(object viewModel, string contract)
        {
            var viewFor = _viewLocator.ResolveView(viewModel, contract);
            var page = viewFor as Page;

            if (viewFor == null)
            {
                throw new InvalidOperationException($"No view could be located for type '{viewModel.GetType().FullName}', contract '{contract}'. Be sure Splat has an appropriate registration.");
            }

            if (page == null)
            {
                throw new InvalidOperationException($"Resolved view '{viewFor.GetType().FullName}' for type '{viewModel.GetType().FullName}', contract '{contract}' is not a Page.");
            }

            viewFor.ViewModel = viewModel;

            return page;
        }
    }
}
