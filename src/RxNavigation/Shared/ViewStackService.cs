using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// Service that manages a stack of views.
    /// </summary>
    public sealed class ViewStackService : IViewStackService
    {
        private readonly IViewShell _viewShell;
        private readonly BehaviorSubject<IImmutableList<IPageViewModel>> _modalPageStack;
        private readonly BehaviorSubject<IImmutableList<IPageViewModel>> _defaultNavigationStack;
        private readonly BehaviorSubject<IImmutableList<IPageViewModel>> _nullPageStack;

        private BehaviorSubject<IImmutableList<IPageViewModel>> _currentPageStack;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewStackService"/> class.
        /// </summary>
        /// <param name="viewShell">The view shell (platform specific).</param>
        /// <param name="pages">A list of pages to initialize the page stack with.</param>
        public ViewStackService(IViewShell viewShell, IList<IPageViewModel> pages = null)
        {
            _viewShell = viewShell ?? throw new NullReferenceException("The viewShell can't be null.");

            _currentPageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(ImmutableList<IPageViewModel>.Empty);
            _modalPageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(ImmutableList<IPageViewModel>.Empty);
            _nullPageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(null);

            var immutablePages = pages != null ? pages.ToImmutableList() : ImmutableList<IPageViewModel>.Empty;
            _defaultNavigationStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(immutablePages);
            for (int i = 0; i < immutablePages.Count; ++i)
            {
                ViewShell.InsertPage(i, pages[i], null);
            }

            _modalPageStack
                    .Select(
                        x =>
                        {
                            if (x.Count > 0)
                            {
                                if (x[x.Count - 1] is INavigationPageViewModel navigationPage)
                                {
                                    return navigationPage.PageStack;
                                }
                                else
                                {
                                    return _nullPageStack;
                                }
                            }
                            else
                            {
                                return _defaultNavigationStack;
                            }
                        })
                    .Subscribe(x => _currentPageStack = x);

            _viewShell
                .PagePopped
                .Do(
                    _ =>
                    {
                        var removedPage = PopStackAndTick(_currentPageStack);
                    })
                .Subscribe();

            _viewShell
                .ModalPopped
                .Do(
                    _ =>
                    {
                        var removedPage = PopStackAndTick(_modalPageStack);
                    })
                .Subscribe();
        }

        /// <summary>
        /// Gets the current view shell (platform-specific).
        /// </summary>
        public IViewShell ViewShell => _viewShell;

        /// <summary>
        /// Gets the current page stack.
        /// </summary>
        public IObservable<IImmutableList<IPageViewModel>> PageStack => _currentPageStack;

        /// <summary>
        /// Gets the modal stack.
        /// </summary>
        public IObservable<IImmutableList<IPageViewModel>> ModalStack => _modalPageStack;

        /// <summary>
        /// Pushes a page onto the current page stack.
        /// </summary>
        /// <param name="page">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <param name="resetStack">A flag signalling if the page stack should be reset.</param>
        /// <param name="animate">A flag signalling if the push should be animated.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PushPage(IPageViewModel page, string contract = null, bool resetStack = false, bool animate = true)
        {
            if (_currentPageStack.Value == null)
            {
                throw new InvalidOperationException("Can't push a page onto a modal with no navigation stack.");
            }

            return _viewShell
                .PushPage(page, contract, resetStack, animate)
                .Do(
                    _ =>
                    {
                        AddToStackAndTick(_currentPageStack, page, resetStack);
                        if (resetStack)
                        {
                            var stack = _currentPageStack.Value;
                            stack = stack.RemoveRange(0, stack.Count - 1);
                            _currentPageStack.OnNext(stack);
                        }
                    });
        }

        /// <summary>
        /// Inserts a page into the current page stack at the given index.
        /// </summary>
        /// <param name="index">An insertion index.</param>
        /// <param name="page">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        public void InsertPage(int index, IPageViewModel page, string contract = null)
        {
            if (page == null)
            {
                throw new NullReferenceException("The page you tried to insert is null.");
            }

            var stack = _currentPageStack.Value;

            if (stack == null)
            {
                throw new InvalidOperationException("Can't insert a page into a modal with no navigation stack.");
            }

            if (index < 0 || index >= stack.Count)
            {
                throw new IndexOutOfRangeException(string.Format("Tried to insert a page at index {0}. Stack count: {1}", index, stack.Count));
            }

            stack = stack.Insert(index, page);
            _currentPageStack.OnNext(stack);
            _viewShell.InsertPage(index, page, contract);
        }

        /// <summary>
        /// Pops to the page at the given index.
        /// </summary>
        /// <param name="index">The index of the page to pop to.</param>
        /// <param name="animateLastPage">A flag signalling if the final pop should be animated.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PopToPage(int index, bool animateLastPage = true)
        {
            var stack = _currentPageStack.Value;

            if (stack == null)
            {
                throw new InvalidOperationException("Can't pop a page from a modal with no navigation stack.");
            }

            if (index < 0 || index >= stack.Count)
            {
                throw new IndexOutOfRangeException(string.Format("Tried to pop to page at index {0}. Stack count: {1}", index, stack.Count));
            }

            int idxOfLastPage = stack.Count - 1;
            int numPagesToPop = idxOfLastPage - index;

            return PopPages(numPagesToPop, animateLastPage);
        }

        /// <summary>
        /// Pops the given number of pages from the top of the current page stack.
        /// </summary>
        /// <param name="count">The number of pages to pop.</param>
        /// <param name="animateLastPage">A flag signalling if the final pop should be animated.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PopPages(int count = 1, bool animateLastPage = true)
        {
            var stack = _currentPageStack.Value;

            if (stack == null)
            {
                throw new InvalidOperationException("Can't pop pages from a modal with no navigation stack.");
            }

            if (count <= 0 || count >= stack.Count)
            {
                throw new IndexOutOfRangeException(
                    string.Format("Page pop count should be greater than 0 and less than the size of the stack. Pop count: {0}. Stack count: {1}", count, stack.Count));
            }

            if (count > 1)
            {
                // Remove count - 1 pages (leaving the top page).
                int idxOfSecondToLastPage = stack.Count - 2;
                for (int i = idxOfSecondToLastPage; i >= stack.Count - count; --i)
                {
                    _viewShell.RemovePage(i);
                }

                stack = stack.RemoveRange(stack.Count - count, count - 1);
                _currentPageStack.OnNext(stack);
            }

            // Now remove the top page with optional animation.
            return _viewShell
                .PopPage(animateLastPage);
        }

        /// <summary>
        /// Pushes a page onto the modal stack.
        /// </summary>
        /// <param name="modal">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PushModal(IPageViewModel modal, string contract = null)
        {
            if (modal == null)
            {
                throw new NullReferenceException("The modal you tried to push is null.");
            }

            return _viewShell
                .PushModal(modal, contract, false)
                .Do(
                    _ =>
                    {
                        AddToStackAndTick(_modalPageStack, modal, false);
                    });
        }

        /// <summary>
        /// Pushes a navigation page onto the modal stack.
        /// </summary>
        /// <param name="modal">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PushModal(INavigationPageViewModel modal, string contract = null)
        {
            if (modal == null)
            {
                throw new NullReferenceException("The modal you tried to insert is null.");
            }

            if (modal.PageStack.Value.Count <= 0)
            {
                throw new InvalidOperationException("Can't push an empty navigation page.");
            }

            return _viewShell
                .PushModal(modal.PageStack.Value[0], contract, true)
                .Do(
                    _ =>
                    {
                        AddToStackAndTick(_modalPageStack, modal, false);
                    });
        }

        /// <summary>
        /// Pops a page from the top of the modal stack.
        /// </summary>
        /// <returns>An observable that signals the completion of this action.</returns>
        public IObservable<Unit> PopModal()
        {
            return _viewShell
                .PopModal();
        }

        private static void AddToStackAndTick<T>(BehaviorSubject<IImmutableList<T>> stackSubject, T item, bool reset)
        {
            var stack = stackSubject.Value;

            if (reset)
            {
                stack = new[] { item }.ToImmutableList();
            }
            else
            {
                stack = stack.Add(item);
            }

            stackSubject.OnNext(stack);
        }

        private static T PopStackAndTick<T>(BehaviorSubject<IImmutableList<T>> stackSubject)
        {
            var stack = stackSubject.Value;

            if (stack.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            var removedItem = stack[stack.Count - 1];
            stack = stack.RemoveAt(stack.Count - 1);
            stackSubject.OnNext(stack);
            return removedItem;
        }
    }
}
