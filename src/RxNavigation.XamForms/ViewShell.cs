using ReactiveUI;
using System;
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
        private readonly IScheduler backgroundScheduler;
        private readonly IScheduler mainScheduler;
        private readonly IViewLocator viewLocator;
        private readonly IObservable<IPageViewModel> pagePopped;
        private readonly IObservable<Unit> modalPopped;
        private readonly IObservable<Page> modalPushed;

        private Stack<NavigationPage> navigationPages;

        /// <summary>
        /// Creates an instance of ViewShell.
        /// </summary>
        public ViewShell()
            : this(RxApp.TaskpoolScheduler, RxApp.MainThreadScheduler, ViewLocator.Current)
        {
        }

        /// <summary>
        /// Creates an instance of ViewShell.
        /// </summary>
        /// <param name="backgroundScheduler">A background scheduler.</param>
        /// <param name="mainScheduler">A main scheduler.</param>
        /// <param name="viewLocator">A view locator.</param>
        public ViewShell(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            this.backgroundScheduler = backgroundScheduler ?? RxApp.TaskpoolScheduler;
            this.mainScheduler = mainScheduler ?? RxApp.MainThreadScheduler;
            this.viewLocator = viewLocator ?? ViewLocator.Current;

            this.navigationPages = new Stack<NavigationPage>();

            this.modalPushed = Observable
                .FromEventPattern<ModalPushedEventArgs>(
                    x => Application.Current.ModalPushed += x,
                    x => Application.Current.ModalPushed -= x)
                .Select(x => x.EventArgs.Modal);

            this.pagePopped = modalPushed
                .StartWith(this)
                .Select(page => page as NavigationPage)
                .Where(x => x != null)
                .Do(x => navigationPages.Push(x))
                .SelectMany(
                    navigationPage =>
                    {
                        return Observable
                            .FromEventPattern<NavigationEventArgs>(x => navigationPage.Popped += x, x => navigationPage.Popped -= x)
                            .Select(x => x.EventArgs.Page.BindingContext as IPageViewModel)
                            .Where(x => x != null);
                    });

            this.modalPopped = Observable
                .FromEventPattern<ModalPoppedEventArgs>(
                    x => Application.Current.ModalPopped += x,
                    x => Application.Current.ModalPopped -= x)
                .Do(
                    x =>
                    {
                        if(x.EventArgs.Modal is NavigationPage)
                        {
                            navigationPages.Pop();
                        }
                    })
                .Select(x => Unit.Default);
        }

        /// <summary>
        /// An observable that signals when a page is popped from the current page stack.
        /// </summary>
        public IObservable<IPageViewModel> PagePopped => this.pagePopped;

        /// <summary>
        /// An observable that signals when a page is popped from the modal stack.
        /// </summary>
        public IObservable<Unit> ModalPopped => this.modalPopped;

        /// <summary>
        /// Pushes a page onto the current page stack.
        /// </summary>
        /// <param name="pageViewModel">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <param name="resetStack">A flag signalling if the page stack should be reset.</param>
        /// <param name="animate">A flag signalling if the push should be animated.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            // If we don't have a root page yet, be sure we create one and assign one immediately because otherwise we'll get an exception.
            // Otherwise, create it off the main thread to improve responsiveness and perceived performance.
            var hasRoot = this.Navigation.NavigationStack.Count > 0;
            var mainScheduler = hasRoot ? this.mainScheduler : CurrentThreadScheduler.Instance;
            var backgroundScheduler = hasRoot ? this.backgroundScheduler : CurrentThreadScheduler.Instance;

            return Observable
                .Start(
                    () =>
                    {
                        var page = this.LocatePageFor(pageViewModel, contract);
                        page.Title = pageViewModel.Title;
                        return page;
                    },
                    backgroundScheduler)
                .ObserveOn(mainScheduler)
                .SelectMany(
                    page =>
                    {
                        if(resetStack)
                        {
                            if(this.Navigation.NavigationStack.Count == 0)
                            {
                                return this
                                    .navigationPages
                                    .Peek()
                                    .Navigation
                                    .PushAsync(page, animated: false)
                                    .ToObservable();
                            }
                            else
                            {
                                // XF does not allow us to pop to a new root page. Instead, we need to inject the new root page and then pop to it.
                                var currentNav = navigationPages.Peek().Navigation;
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
                            return this
                                .navigationPages
                                .Peek()
                                .Navigation
                                .PushAsync(page, animate)
                                .ToObservable();
                        }
                    });
        }

        /// <summary>
        /// Pops a page from the top of the current page stack.
        /// </summary>
        /// <param name="animate"></param>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PopPage(bool animate) =>
            this
                .navigationPages
                .Peek()
                .Navigation
                .PopAsync(animate)
                .ToObservable()
                .Select(_ => Unit.Default)
                // XF completes the pop operation on a background thread :/
                .ObserveOn(this.mainScheduler);

        /// <summary>
        /// Inserts a page into the current page stack at the given index.
        /// </summary>
        /// <param name="index">An insertion index.</param>
        /// <param name="pageViewModel">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        public void InsertPage(int index, IPageViewModel pageViewModel, string contract = null)
        {
            var page = this.LocatePageFor(pageViewModel, contract);
            page.Title = pageViewModel.Title;
            var currentNavigationPage = this.navigationPages.Peek();
            currentNavigationPage.Navigation.InsertPageBefore(page, currentNavigationPage.Navigation.NavigationStack[index]);
        }

        /// <summary>
        /// Removes a page from the current page stack at the given index.
        /// </summary>
        /// <param name="index">The index of the page to remove.</param>
        public void RemovePage(int index)
        {
            var page = this.navigationPages.Peek().Navigation.NavigationStack[index];
            this.navigationPages.Peek().Navigation.RemovePage(page);
        }

        /// <summary>
        /// Pushes a page onto the modal stack.
        /// </summary>
        /// <param name="modalViewModel">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <param name="withNavStack">A flag signalling if a new page stack should be created.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            return Observable
                .Start(
                    () =>
                    {
                        var page = this.LocatePageFor(modalViewModel, contract);
                        page.Title = modalViewModel.Title;
                        if (withNavStack)
                        {
                            page = new NavigationPage(page);
                        }

                        return page;
                    },
                    this.backgroundScheduler)
                .ObserveOn(this.mainScheduler)
                .SelectMany(
                    page =>
                        this
                            .Navigation
                            .PushModalAsync(page)
                            .ToObservable());
        }

        /// <summary>
        /// Pops a page from the top of the modal stack.
        /// </summary>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PopModal() =>
            this
                .Navigation
                .PopModalAsync()
                .ToObservable()
                .Select(_ => Unit.Default)
                // XF completes the pop operation on a background thread :/
                .ObserveOn(this.mainScheduler);

        private Page LocatePageFor(object viewModel, string contract)
        {
            var viewFor = viewLocator.ResolveView(viewModel, contract);
            var page = viewFor as Page;

            if(viewFor == null)
            {
                throw new InvalidOperationException($"No view could be located for type '{viewModel.GetType().FullName}', contract '{contract}'. Be sure Splat has an appropriate registration.");
            }

            if(page == null)
            {
                throw new InvalidOperationException($"Resolved view '{viewFor.GetType().FullName}' for type '{viewModel.GetType().FullName}', contract '{contract}' is not a Page.");
            }

            viewFor.ViewModel = viewModel;

            return page;
        }
    }
}
