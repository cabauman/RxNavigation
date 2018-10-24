using CoreAnimation;
using Foundation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using UIKit;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// A class that manages a stack of views.
    /// </summary>
    public sealed class ViewShell : RxNavigationController, IViewShell
    {
        private readonly IScheduler backgroundScheduler;
        private readonly IScheduler mainScheduler;
        private readonly IViewLocator viewLocator;
        private readonly IObservable<IPageViewModel> pagePopped;
        private readonly Subject<Unit> modalPopped;
        private readonly Subject<RxNavigationController> navigationPagePushed;
        private readonly Stack<UIViewController> modalStackPlusMainView;

        private UINavigationController currentNavigationController;

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

            this.navigationPagePushed = new Subject<RxNavigationController>();
            this.modalStackPlusMainView = new Stack<UIViewController>();
            this.modalPopped = new Subject<Unit>();
            modalStackPlusMainView.Push(this);
            currentNavigationController = this;

            // Each time a RxNavigationController is presented (modally), add a new "controller popped" listener.
            this.pagePopped = navigationPagePushed
                .StartWith(this)
                .SelectMany(
                    navigationController =>
                    {
                        return navigationController.ControllerPopped
                            .Select(
                                viewController =>
                                {
                                    var viewFor = viewController as IViewFor;
                                    return viewFor?.ViewModel as IPageViewModel;
                                });
                    });
        }

        /// <summary>
        /// An observable that signals when a page is popped from the current page stack.
        /// </summary>
        public IObservable<IPageViewModel> PagePopped => this.pagePopped;

        /// <summary>
        /// An observable that signals when a page is popped from the modal stack.
        /// </summary>
        public IObservable<Unit> ModalPopped => this.modalPopped.AsObservable();

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
            return Observable
                .Start(
                    () =>
                    {
                        var page = this.LocatePageFor(pageViewModel, contract);
                        return page;
                    },
                    backgroundScheduler)
                .ObserveOn(mainScheduler)
                .SelectMany(
                    page =>
                    {
                        page.Title = pageViewModel.Title;

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
                                        currentNavigationController.SetViewControllers(null, false);
                                    }

                                    currentNavigationController.PushViewController(page, animated: animate);

                                    CATransaction.Commit();
                                    return Disposable.Empty;
                                });
                    });
        }

        /// <summary>
        /// Pops a page from the top of the current page stack.
        /// </summary>
        /// <param name="animate"></param>
        /// <returns>An observable that signals the completion of this action.</returns>
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

                        currentNavigationController.PopViewController(animated: animate);

                        CATransaction.Commit();
                        return Disposable.Empty;
                    });

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
            var viewControllers = currentNavigationController.ViewControllers;
            viewControllers = InsertIndices(viewControllers, page, index);
            currentNavigationController.SetViewControllers(viewControllers, false);
        }

        /// <summary>
        /// Removes a page from the current page stack at the given index.
        /// </summary>
        /// <param name="index">The index of the page to remove.</param>
        public void RemovePage(int index)
        {
            var viewControllers = currentNavigationController.ViewControllers;
            viewControllers = RemoveIndices(viewControllers, index);
            currentNavigationController.SetViewControllers(viewControllers, false);
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
                        UIViewController page = this.LocatePageFor(modalViewModel, contract);
                        return page;
                    },
                    this.backgroundScheduler)
                .ObserveOn(this.mainScheduler)
                .SelectMany(
                    viewController =>
                    {
                        viewController.Title = modalViewModel.Title;
                        RxNavigationController navigationPage = null;
                        if (withNavStack)
                        {
                            viewController = navigationPage = new RxNavigationController(viewController);
                        }

                        return this
                            .modalStackPlusMainView.Peek()
                            .PresentViewControllerAsync(viewController, true)
                            .ToObservable()
                            .Do(
                                x =>
                                {
                                    modalStackPlusMainView.Push(viewController);
                                    if (withNavStack)
                                    {
                                        currentNavigationController = navigationPage;
                                        navigationPagePushed.OnNext(navigationPage);
                                    }
                                });
                    });
        }

        /// <summary>
        /// Pops a page from the top of the modal stack.
        /// </summary>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PopModal()
        {
            var controller = this.modalStackPlusMainView.Pop();

            return controller
                .PresentingViewController
                .DismissViewControllerAsync(true)
                .ToObservable()
                .Do(
                    x =>
                    {
                        this.modalPopped.OnNext(Unit.Default);
                        if (this.modalStackPlusMainView.Peek() is RxNavigationController navigationController)
                        {
                            currentNavigationController = navigationController;
                        }
                        else
                        {
                            currentNavigationController = null;
                        }
                    });
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
    }
}
