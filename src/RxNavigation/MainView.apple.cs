using CoreAnimation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using UIKit;

namespace RxNavigation
{
    public sealed class MainView : UINavigationController, IView
    {
        private readonly IScheduler backgroundScheduler;
        private readonly IScheduler mainScheduler;
        private readonly IViewLocator viewLocator;
        private readonly IObservable<IPageViewModel> pagePopped;
        private readonly IObservable<Unit> modalPopped;

        private Stack<UINavigationController> navigationPages;

        public MainView(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            this.backgroundScheduler = backgroundScheduler ?? RxApp.TaskpoolScheduler;
            this.mainScheduler = mainScheduler ?? RxApp.MainThreadScheduler;
            this.viewLocator = viewLocator ?? ViewLocator.Current;

            this.navigationPages = new Stack<UINavigationController>();
            this.navigationPages.Push(this);
        }

        public IObservable<IPageViewModel> PagePopped => this.pagePopped;

        public IObservable<Unit> ModalPopped => this.modalPopped;

        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            return Observable
                .Start(
                    () =>
                    {
                        UIViewController page = this.LocatePageFor(modalViewModel, contract);
                        this.SetPageTitle(page, modalViewModel.Id);
                        if(withNavStack)
                        {
                            page = new UINavigationController(page);
                            navigationPages.Push(page as UINavigationController);
                        }

                        return page;
                    },
                    this.backgroundScheduler)
                .ObserveOn(this.mainScheduler)
                .SelectMany(
                    page =>
                        this
                            .PresentViewControllerAsync(page, true)
                            .ToObservable());
        }

        public IObservable<Unit> PopModal() =>
            this
                .DismissViewControllerAsync(true)
                .ToObservable()
                .Select(_ => Unit.Default)
                // XF completes the pop operation on a background thread :/
                .ObserveOn(this.mainScheduler);

        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            UIViewController viewController = null;
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
                        return Observable
                            .Create<Unit>(
                                observer =>
                                {
                                    CATransaction.Begin();
                                    CATransaction.CompletionBlock = () =>
                                    {
                                        observer.OnNext(Unit.Default);
                                        observer.OnCompleted();
                                    };

                                    if(resetStack)
                                    {
                                        this.SetViewControllers(null, false);
                                    }

                                    this.PushViewController(viewController, animated: animate);

                                    CATransaction.Commit();
                                    return Disposable.Empty;
                                });
                    });
        }

        public IObservable<Unit> PopPage(bool animate) =>
            Observable
                .Create<Unit>(
                    observer =>
                    {
                        CATransaction.Begin();
                        CATransaction.CompletionBlock = () =>
                        {
                            observer.OnNext(Unit.Default);
                            observer.OnCompleted();
                        };

                        this.PopViewController(animated: animate);

                        CATransaction.Commit();
                        return Disposable.Empty;
                    });

        public void InsertPage(int index, IPageViewModel pageViewModel, string contract = null)
        {
            var page = this.LocatePageFor(pageViewModel, contract);
            this.SetPageTitle(page, pageViewModel.Id);
            var viewControllers = this.navigationPages.Peek().ViewControllers;
            viewControllers = InsertIndices(viewControllers, page, index);
            this.navigationPages.Peek().SetViewControllers(viewControllers, false);
        }

        public void RemovePage(int index)
        {
            var viewControllers = this.navigationPages.Peek().ViewControllers;
            viewControllers = RemoveIndices(viewControllers, index);
            this.navigationPages.Peek().SetViewControllers(viewControllers, false);
        }

        public override UIViewController PopViewController(bool animated)
        {
            return base.PopViewController(animated);
        }

        private UIViewController[] RemoveIndices(UIViewController[] indicesArray, int removeAt)
        {
            UIViewController[] newIndicesArray = new UIViewController[indicesArray.Length - 1];

            int i = 0;
            int j = 0;
            while(i < indicesArray.Length)
            {
                if(i != removeAt)
                {
                    newIndicesArray[j] = indicesArray[i];
                    j++;
                }

                i++;
            }

            return newIndicesArray;
        }

        private UIViewController[] InsertIndices(UIViewController[] indicesArray, UIViewController viewController, int index)
        {
            UIViewController[] newIndicesArray = new UIViewController[indicesArray.Length + 1];

            int i = 0;
            int j = 0;
            while(i < indicesArray.Length)
            {
                if(j == index)
                {
                    newIndicesArray[j] = viewController;
                }
                else
                {
                    newIndicesArray[j] = indicesArray[i];
                    i++;
                }

                j++;
            }

            return newIndicesArray;
        }

        private UIViewController LocatePageFor(object viewModel, string contract)
        {
            var viewFor = viewLocator.ResolveView(viewModel, contract);
            var page = viewFor as UIViewController;

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

        private void SetPageTitle(UIViewController page, string resourceKey)
        {
            //var title = Localize.GetString(resourceKey);
            page.Title = resourceKey; // title;
        }
    }
}
