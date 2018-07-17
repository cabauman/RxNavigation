using Genesis.Ensure;
using Splat;
using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using XamFormsRxRouting.Navigation.Interfaces;

namespace XamFormsRxRouting.Navigation
{
    public sealed class ViewStackService : IViewStackService, IEnableLogger
    {
        private readonly BehaviorSubject<IImmutableList<IModalViewModel>> modalStack;
        private readonly BehaviorSubject<IImmutableList<IPageViewModel>> pageStack;
        private readonly IView view;

        public ViewStackService(IView view)
        {
            Ensure.ArgumentNotNull(view, nameof(view));

            this.modalStack = new BehaviorSubject<IImmutableList<IModalViewModel>>(ImmutableList<IModalViewModel>.Empty);
            this.pageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(ImmutableList<IPageViewModel>.Empty);
            this.view = view;

            this
                .view
                .PagePopped
                .Do(
                    poppedPage =>
                    {
                        var currentPageStack = this.pageStack.Value;

                        if(currentPageStack.Count > 0 && poppedPage == currentPageStack[currentPageStack.Count - 1])
                        {
                            var removedPage = PopStackAndTick(this.pageStack);
                            this.Log().Debug("Removed page '{0}' from stack.", removedPage.Id);
                        }
                    })
                .Subscribe();
        }

        public IView View => this.view;

        public int PageCount => this.pageStack.Value.Count;

        public IObservable<IImmutableList<IModalViewModel>> ModalStack => this.modalStack;

        public IObservable<IImmutableList<IPageViewModel>> PageStack => this.pageStack;

        public IObservable<Unit> PushPage(IPageViewModel page, string contract = null, bool resetStack = false, bool animate = true)
        {
            Ensure.ArgumentNotNull(page, nameof(page));

            return this
                .view
                .PushPage(page, contract, resetStack, animate)
                .Do(
                    _ =>
                    {
                        AddToStackAndTick(this.pageStack, page, resetStack);
                        this.Log().Debug("Added page '{0}' (contract '{1}') to stack.", page.Id, contract);
                    });
        }

        public IObservable<Unit> PopToPage(int index, bool animateLastPage = true)
        {
            var stack = this.pageStack.Value;

            Ensure.ArgumentCondition(index >= 0 && index < stack.Count, "Index is out of range.", nameof(index));

            int idxOfLastPage = stack.Count - 1;
            int numPagesToPop = idxOfLastPage - index;

            return PopPages(numPagesToPop, animateLastPage);
        }

        public IObservable<Unit> PopPages(int count = 1, bool animateLastPage = true)
        {
            Ensure.ArgumentCondition(count > 0 && count < PageCount, "Page pop count should be greater than 0 and less than the size of the stack.", nameof(count));

            var stack = this.pageStack.Value;

            if(count > 1)
            {
                // Remove count - 1 pages (leaving the top page).
                int idxOfSecondToLastPage = stack.Count - 2;
                for(int i = idxOfSecondToLastPage; i >= stack.Count - count; --i)
                {
                    this.view.RemovePage(i);
                }

                stack = stack.RemoveRange(stack.Count - count, count - 1);
                this.pageStack.OnNext(stack);
            }

            // Now remove the top page with optional animation.
            return this
                .view
                .PopPage(animateLastPage);
        }

        public void InsertPage(int index, IPageViewModel page, string contract = null)
        {
            var stack = this.pageStack.Value;

            Ensure.ArgumentNotNull(page, nameof(page));
            Ensure.ArgumentCondition(index >= 0 && index < stack.Count, "Index is out of range.", nameof(index));

            stack = stack.Insert(index, page);
            this.pageStack.OnNext(stack);
            this.view.InsertPage(index, page, contract);
        }

        public IObservable<Unit> PushModal(IModalViewModel modal, string contract = null)
        {
            Ensure.ArgumentNotNull(modal, nameof(modal));

            return this
                .view
                .PushModal(modal, contract)
                .Do(
                    _ =>
                    {
                        AddToStackAndTick(this.modalStack, modal, false);
                        this.Log().Debug("Added modal '{0}' (contract '{1}') to stack.", modal.Id, contract);
                    });
        }

        public IObservable<Unit> PopModal() =>
            this
                .view
                .PopModal()
                .Do(
                    _ =>
                    {
                        var removedModal = PopStackAndTick(this.modalStack);
                        this.Log().Debug("Removed modal '{0}' from stack.", removedModal.Id);
                    });

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
