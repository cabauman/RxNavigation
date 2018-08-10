using ReactiveUI;
using Splat;
using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxNavigation
{
    public sealed class ViewStackService : IViewStackService, IEnableLogger
    {
        private readonly IView view;
        private readonly BehaviorSubject<IImmutableList<IPageViewModel>> modalPageStack;
        private readonly BehaviorSubject<IImmutableList<IPageViewModel>> defaultNavigationStack;
        private readonly BehaviorSubject<IImmutableList<IPageViewModel>> nullPageStack;

        private BehaviorSubject<IImmutableList<IPageViewModel>> currentPageStack;

        public ViewStackService(IView view)
        {
            this.view = view ?? throw new NullReferenceException("The view can't be null.");

            this.modalPageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(ImmutableList<IPageViewModel>.Empty);
            this.defaultNavigationStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(ImmutableList<IPageViewModel>.Empty);
            this.nullPageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(null);
            this.currentPageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(ImmutableList<IPageViewModel>.Empty);

            this
                .modalPageStack
                    .Select(
                        x =>
                        {
                            if(x.Count > 0)
                            {
                                if(x[x.Count - 1] is INavigationPageViewModel navigationPage)
                                {
                                    return navigationPage.PageStack;
                                }
                                else
                                {
                                    return nullPageStack;
                                }
                            }
                            else
                            {
                                return this.defaultNavigationStack;
                            }
                        })
                    .Subscribe(x => this.currentPageStack = x);

            this
                .view
                .PagePopped
                .Do(
                    _ =>
                    {
                        var removedPage = PopStackAndTick(this.currentPageStack);
                        this.Log().Debug("Removed page '{0}' from stack.", removedPage.Title);
                    })
                .Subscribe();

            this
                .view
                .ModalPopped
                .Do(
                    _ =>
                    {
                        var removedPage = PopStackAndTick(this.modalPageStack);
                        this.Log().Debug("Removed modal page '{0}' from stack.", removedPage.Title);
                    })
                .Subscribe();
        }

        public IView View => this.view;

        public IObservable<IImmutableList<IPageViewModel>> PageStack => this.currentPageStack;

        public IObservable<IImmutableList<IPageViewModel>> ModalStack => this.modalPageStack;

        public IObservable<Unit> PushPage(IPageViewModel page, string contract = null, bool resetStack = false, bool animate = true)
        {
            if(this.currentPageStack.Value == null)
            {
                throw new InvalidOperationException("Can't push a page onto a modal with no navigation stack.");
            }

            return this
                .view
                .PushPage(page, contract, resetStack, animate)
                .Do(
                    _ =>
                    {
                        AddToStackAndTick(this.currentPageStack, page, resetStack);
                        this.Log().Debug("Added page '{0}' (contract '{1}') to stack.", page.Title, contract);
                    });
        }

        public void InsertPage(int index, IPageViewModel page, string contract = null)
        {
            if(page == null)
            {
                throw new NullReferenceException("The page you tried to insert is null.");
            }

            var stack = this.currentPageStack.Value;

            if(stack == null)
            {
                throw new InvalidOperationException("Can't insert a page into a modal with no navigation stack.");
            }

            if(index < 0 || index >= stack.Count)
            {
                throw new IndexOutOfRangeException(string.Format("Tried to insert a page at index {0}. Stack count: {1}", index, stack.Count));
            }

            stack = stack.Insert(index, page);
            this.currentPageStack.OnNext(stack);
            this.view.InsertPage(index, page, contract);
        }

        public IObservable<Unit> PopToPage(int index, bool animateLastPage = true)
        {
            var stack = this.currentPageStack.Value;

            if(stack == null)
            {
                throw new InvalidOperationException("Can't pop a page from a modal with no navigation stack.");
            }

            if(index < 0 || index >= stack.Count)
            {
                throw new IndexOutOfRangeException(string.Format("Tried to pop to page at index {0}. Stack count: {1}", index, stack.Count));
            }

            int idxOfLastPage = stack.Count - 1;
            int numPagesToPop = idxOfLastPage - index;

            return PopPages(numPagesToPop, animateLastPage);
        }

        public IObservable<Unit> PopPages(int count = 1, bool animateLastPage = true)
        {
            var stack = this.currentPageStack.Value;

            if(stack == null)
            {
                throw new InvalidOperationException("Can't pop pages from a modal with no navigation stack.");
            }

            if(count <= 0 || count >= stack.Count)
            {
                throw new IndexOutOfRangeException(
                    string.Format("Page pop count should be greater than 0 and less than the size of the stack. Pop count: {0}. Stack count: {1}", count, stack.Count));
            }

            if(count > 1)
            {
                // Remove count - 1 pages (leaving the top page).
                int idxOfSecondToLastPage = stack.Count - 2;
                for(int i = idxOfSecondToLastPage; i >= stack.Count - count; --i)
                {
                    this.view.RemovePage(i);
                }

                stack = stack.RemoveRange(stack.Count - count, count - 1);
                this.currentPageStack.OnNext(stack);
            }

            // Now remove the top page with optional animation.
            return this
                .view
                .PopPage(animateLastPage);
        }

        public IObservable<Unit> PushModal(IPageViewModel modal, string contract = null)
        {
            if(modal == null)
            {
                throw new NullReferenceException("The modal you tried to push is null.");
            }

            return this
                .view
                .PushModal(modal, contract, false)
                .Do(
                    _ =>
                    {
                        AddToStackAndTick(this.modalPageStack, modal, false);
                        this.Log().Debug("Added modal '{0}' (contract '{1}') to stack.", modal.Title, contract);
                    });
        }

        public IObservable<Unit> PushModal(INavigationPageViewModel modal, string contract = null)
        {
            if(modal == null)
            {
                throw new NullReferenceException("The modal you tried to insert is null.");
            }

            if(modal.PageStack.Value.Count <= 0)
            {
                throw new InvalidOperationException("Can't push an empty navigation page.");
            }

            return this
                .view
                .PushModal(modal.PageStack.Value[0], contract, true)
                .Do(
                    _ =>
                    {
                        AddToStackAndTick(this.modalPageStack, modal, false);
                        this.Log().Debug("Added modal '{0}' (contract '{1}') to stack.", modal.Title, contract);
                    });
        }

        public IObservable<Unit> PopModal()
        {
            return this
                .view
                .PopModal();
        }

        private static void AddToStackAndTick<T>(BehaviorSubject<IImmutableList<T>> stackSubject, T item, bool reset)
        {
            var stack = stackSubject.Value;

            if(reset)
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

            if(stack.Count == 0)
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
