using Genesis.Ensure;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Xamarin.Forms;
using XamFormsRxRouting.Extensions;
using XamFormsRxRouting.Navigation.Interfaces;

namespace XamFormsRxRouting.Navigation
{
    public sealed class MainView : Xamarin.Forms.NavigationPage, IView
    {
        private readonly IScheduler backgroundScheduler;
        private readonly IScheduler mainScheduler;
        private readonly IViewLocator viewLocator;
        private readonly IObservable<IPageViewModel> pagePopped;

        private Stack<Xamarin.Forms.NavigationPage> navigationPages;

        public MainView(
            IScheduler backgroundScheduler,
            IScheduler mainScheduler,
            IViewLocator viewLocator)
        {
            Ensure.ArgumentNotNull(backgroundScheduler, nameof(backgroundScheduler));
            Ensure.ArgumentNotNull(mainScheduler, nameof(mainScheduler));
            Ensure.ArgumentNotNull(viewLocator, nameof(viewLocator));

            this.backgroundScheduler = backgroundScheduler;
            this.mainScheduler = mainScheduler;
            this.viewLocator = viewLocator;

            this.navigationPages = new Stack<Xamarin.Forms.NavigationPage>();
            this.navigationPages.Push(this);

            this.pagePopped = Observable
                .FromEventPattern<NavigationEventArgs>(x => this.Popped += x, x => this.Popped -= x)
                .Select(ep => ep.EventArgs.Page.BindingContext as IPageViewModel)
                .WhereNotNull();
        }

        public IObservable<IPageViewModel> PagePopped => this.pagePopped;

        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract)
        {
            Ensure.ArgumentNotNull(modalViewModel, nameof(modalViewModel));

            return Observable
                .Start(
                    () =>
                    {
                        var page = this.LocatePageFor(modalViewModel, contract);
                        var navigationPage = new Xamarin.Forms.NavigationPage(page);
                        navigationPages.Push(navigationPage);
                        this.SetPageTitle(page, modalViewModel.Id);
                        return navigationPage;
                    },
                    this.backgroundScheduler)
                .ObserveOn(this.mainScheduler)
                .SelectMany(
                    navigationPage =>
                        this
                            .Navigation
                            .PushModalAsync(navigationPage)
                            .ToObservable());
        }

        public IObservable<Unit> PopModal() =>
            this
                .Navigation
                .PopModalAsync()
                .ToObservable()
                .ToSignal()
                // XF completes the pop operation on a background thread :/
                .ObserveOn(this.mainScheduler);

        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            Ensure.ArgumentNotNull(pageViewModel, nameof(pageViewModel));

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
                        this.SetPageTitle(page, pageViewModel.Id);
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
                .ToSignal()
                // XF completes the pop operation on a background thread :/
                .ObserveOn(this.mainScheduler);

        public void InsertPage(int index, IPageViewModel pageViewModel, string contract = null)
        {
            var page = this.LocatePageFor(pageViewModel, contract);
            this.SetPageTitle(page, pageViewModel.Id);
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
            Ensure.ArgumentNotNull(viewModel, nameof(viewModel));

            var view = viewLocator.ResolveView(viewModel, contract);
            var viewFor = view as IViewFor;
            var page = view as Page;

            if(view == null)
            {
                throw new InvalidOperationException($"No view could be located for type '{viewModel.GetType().FullName}', contract '{contract}'. Be sure Splat has an appropriate registration.");
            }

            if(viewFor == null)
            {
                throw new InvalidOperationException($"Resolved view '{view.GetType().FullName}' for type '{viewModel.GetType().FullName}', contract '{contract}' does not implement IViewFor.");
            }

            if(page == null)
            {
                throw new InvalidOperationException($"Resolved view '{view.GetType().FullName}' for type '{viewModel.GetType().FullName}', contract '{contract}' is not a Page.");
            }

            viewFor.ViewModel = viewModel;

            return page;
        }

        private void SetPageTitle(Page page, string resourceKey)
        {
            //var title = Localize.GetString(resourceKey);
            page.Title = resourceKey; // title;
        }
    }
}
