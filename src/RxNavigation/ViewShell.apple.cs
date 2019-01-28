using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using CoreAnimation;
using Foundation;
using ReactiveUI;
using UIKit;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// A class that manages a stack of views.
    /// </summary>
    public sealed class ViewShell : RxNavigationController, IViewShell
    {
        private readonly IScheduler _backgroundScheduler;
        private readonly IScheduler _mainScheduler;
        private readonly IViewLocator _viewLocator;
        private readonly IObservable<IPageViewModel> _pagePopped;
        private readonly Subject<Unit> _modalPopped;
        private readonly Subject<RxNavigationController> _navigationPagePushed;
        private readonly Stack<UIViewController> _modalStackPlusMainView;

        private UINavigationController _currentNavigationController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewShell"/> class.
        /// </summary>
        /// <param name="backgroundScheduler">A background scheduler.</param>
        /// <param name="mainScheduler">A main scheduler.</param>
        /// <param name="viewLocator">A view locator.</param>
        public ViewShell(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            _backgroundScheduler = backgroundScheduler ?? RxApp.TaskpoolScheduler;
            _mainScheduler = mainScheduler ?? RxApp.MainThreadScheduler;
            _viewLocator = viewLocator ?? ViewLocator.Current;

            _navigationPagePushed = new Subject<RxNavigationController>();
            _modalStackPlusMainView = new Stack<UIViewController>();
            _modalPopped = new Subject<Unit>();
            _modalStackPlusMainView.Push(this);
            _currentNavigationController = this;

            // Each time a RxNavigationController is presented (modally), add a new "controller popped" listener.
            _pagePopped = _navigationPagePushed
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

        /// <inheritdoc/>
        public IObservable<IPageViewModel> PagePopped => _pagePopped;

        /// <inheritdoc/>
        public IObservable<Unit> ModalPopped => _modalPopped.AsObservable();

        /// <inheritdoc/>
        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            return Observable
                .Start(
                    () =>
                    {
                        var page = LocatePageFor(pageViewModel, contract);
                        return page;
                    },
                    _backgroundScheduler)
                .ObserveOn(_mainScheduler)
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

                                    if (resetStack)
                                    {
                                        _currentNavigationController.SetViewControllers(null, false);
                                    }

                                    _currentNavigationController.PushViewController(page, animated: animate);

                                    CATransaction.Commit();
                                    return Disposable.Empty;
                                });
                    });
        }

        /// <inheritdoc/>
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

                        _currentNavigationController.PopViewController(animated: animate);

                        CATransaction.Commit();
                        return Disposable.Empty;
                    });

        /// <inheritdoc/>
        public void InsertPage(int index, IPageViewModel pageViewModel, string contract = null)
        {
            var page = LocatePageFor(pageViewModel, contract);
            page.Title = pageViewModel.Title;
            var viewControllers = _currentNavigationController.ViewControllers;
            viewControllers = InsertIndices(viewControllers, page, index);
            _currentNavigationController.SetViewControllers(viewControllers, false);
        }

        /// <inheritdoc/>
        public void RemovePage(int index)
        {
            var viewControllers = _currentNavigationController.ViewControllers;
            viewControllers = RemoveIndices(viewControllers, index);
            _currentNavigationController.SetViewControllers(viewControllers, false);
        }

        /// <inheritdoc/>
        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            return Observable
                .Start(
                    () =>
                    {
                        UIViewController page = LocatePageFor(modalViewModel, contract);
                        return page;
                    },
                    _backgroundScheduler)
                .ObserveOn(_mainScheduler)
                .SelectMany(
                    viewController =>
                    {
                        viewController.Title = modalViewModel.Title;
                        RxNavigationController navigationPage = null;
                        if (withNavStack)
                        {
                            viewController = navigationPage = new RxNavigationController(viewController);
                        }

                        return _modalStackPlusMainView.Peek()
                            .PresentViewControllerAsync(viewController, true)
                            .ToObservable()
                            .Do(
                                x =>
                                {
                                    _modalStackPlusMainView.Push(viewController);
                                    if (withNavStack)
                                    {
                                        _currentNavigationController = navigationPage;
                                        _navigationPagePushed.OnNext(navigationPage);
                                    }
                                });
                    });
        }

        /// <inheritdoc/>
        public IObservable<Unit> PopModal()
        {
            var controller = _modalStackPlusMainView.Pop();

            return controller
                .PresentingViewController
                .DismissViewControllerAsync(true)
                .ToObservable()
                .Do(
                    x =>
                    {
                        _modalPopped.OnNext(Unit.Default);
                        if (_modalStackPlusMainView.Peek() is RxNavigationController navigationController)
                        {
                            _currentNavigationController = navigationController;
                        }
                        else
                        {
                            _currentNavigationController = null;
                        }
                    });
        }

        private UIViewController[] RemoveIndices(UIViewController[] indicesArray, int removeAt)
        {
            UIViewController[] newIndicesArray = new UIViewController[indicesArray.Length - 1];

            int i = 0;
            int j = 0;
            while (i < indicesArray.Length)
            {
                if (i != removeAt)
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
            while (i < indicesArray.Length)
            {
                if (j == index)
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
            var viewFor = _viewLocator.ResolveView(viewModel, contract);
            var page = viewFor as UIViewController;

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
