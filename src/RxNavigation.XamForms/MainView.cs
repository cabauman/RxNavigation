using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using Xamarin.Forms;

namespace GameCtor.RxNavigation.XamForms
{
    public sealed class MainView : Xamarin.Forms.NavigationPage, IView
    {
        private readonly IScheduler backgroundScheduler;
        private readonly IScheduler mainScheduler;
        private readonly IViewLocator viewLocator;
        private readonly IObservable<IPageViewModel> pagePopped;
        private readonly IObservable<Unit> modalPopped;
        private readonly IObservable<Page> modalPushed;

        private Stack<NavigationPage> navigationPages;

        public MainView(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
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

        public IObservable<IPageViewModel> PagePopped => this.pagePopped;

        public IObservable<Unit> ModalPopped => this.modalPopped;

        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            return Observable
                .Start(
                    () =>
                    {
                        var page = this.LocatePageFor(modalViewModel, contract);
                        page.Title = modalViewModel.Title;
                        if(withNavStack)
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

        public IObservable<Unit> PopModal() =>
            this
                .Navigation
                .PopModalAsync()
                .ToObservable()
                .Select(_ => Unit.Default)
                // XF completes the pop operation on a background thread :/
                .ObserveOn(this.mainScheduler);

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
                                this
                                        .Navigation
                                        .InsertPageBefore(page, this.Navigation.NavigationStack[0]);

                                return this
                                    .navigationPages
                                    .Peek()
                                    .Navigation
                                    .PopToRootAsync(animated: false)
                                    .ToObservable();
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

        public void InsertPage(int index, IPageViewModel pageViewModel, string contract = null)
        {
            var page = this.LocatePageFor(pageViewModel, contract);
            page.Title = pageViewModel.Title;
            var currentNavigationPage = this.navigationPages.Peek();
            currentNavigationPage.Navigation.InsertPageBefore(page, currentNavigationPage.Navigation.NavigationStack[index]);
        }

        public void RemovePage(int index)
        {
            var page = this.navigationPages.Peek().Navigation.NavigationStack[index];
            this.navigationPages.Peek().Navigation.RemovePage(page);
        }

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
