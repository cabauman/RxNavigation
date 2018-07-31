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
        private readonly IView view;
        private readonly INavigationPageViewModel defaultNavigationPage;
        private readonly BehaviorSubject<IImmutableList<INavigationPageViewModel>> modalNavigationPages;
        private readonly BehaviorSubject<IImmutableList<IPageViewModel>> modalNavigationPages2;
        private readonly BehaviorSubject<IImmutableList<IPageViewModel>> currentPageStack;

        public ViewStackService(IView view)
        {
            Ensure.ArgumentNotNull(view, nameof(view));

            this.defaultNavigationPage = new NavigationPageViewModel();
            this.currentPageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(ImmutableList<IPageViewModel>.Empty);
            this.modalNavigationPages = new BehaviorSubject<IImmutableList<INavigationPageViewModel>>(ImmutableList<INavigationPageViewModel>.Empty);
            this.view = view;

            this.modalNavigationPages
                    .Select(x => x.Count > 0 ? x[x.Count - 1].PageStack : this.defaultNavigationPage.PageStack)
                    .Subscribe(x => this.currentPageStack.OnNext(x));

            this
                .view
                .PagePopped
                .Do(
                    poppedPage =>
                    {
                        var pageStack = this.currentPageStack.Value;

                        if(pageStack.Count > 0 && poppedPage == pageStack[pageStack.Count - 1])
                        {
                            var removedPage = PopStackAndTick(this.currentPageStack);
                            this.Log().Debug("Removed page '{0}' from stack.", removedPage.Id);
                        }
                    })
                .Subscribe();
        }

        public IView View => this.view;

        public IObservable<IImmutableList<IPageViewModel>> PageStack => this.currentPageStack;

        public IObservable<IImmutableList<INavigationPageViewModel>> ModalStack => this.modalNavigationPages;

        public IObservable<Unit> PushPage(IPageViewModel page, string contract = null, bool resetStack = false, bool animate = true)
        {
            return this
                .view
                .PushPage(page, contract, resetStack, animate)
                .Do(
                    _ =>
                    {
                        AddToStackAndTick(this.currentPageStack, page, resetStack);
                        this.Log().Debug("Added page '{0}' (contract '{1}') to stack.", page.Id, contract);
                    });
        }

        public void InsertPage(int index, IPageViewModel page, string contract = null)
        {
            var stack = this.currentPageStack.Value;

            Ensure.ArgumentNotNull(page, nameof(page));
            Ensure.ArgumentCondition(index >= 0 && index < stack.Count, "Index is out of range.", nameof(index));

            stack = stack.Insert(index, page);
            this.currentPageStack.OnNext(stack);
            this.view.InsertPage(index, page, contract);
        }

        public IObservable<Unit> PopToPage(int index, bool animateLastPage = true)
        {
            var stack = this.currentPageStack.Value;

            Ensure.ArgumentCondition(index >= 0 && index < stack.Count, "Index is out of range.", nameof(index));

            int idxOfLastPage = stack.Count - 1;
            int numPagesToPop = idxOfLastPage - index;

            return PopPages(numPagesToPop, animateLastPage);
        }

        public IObservable<Unit> PopPages(int count = 1, bool animateLastPage = true)
        {
            var stack = this.currentPageStack.Value;

            Ensure.ArgumentCondition(count > 0 && count < stack.Count, "Page pop count should be greater than 0 and less than the size of the stack.", nameof(count));

            if(count > 1)
            {
                // Remove count - 1 pages (leaving the top page).
                int idxOfSecondToLastPage = stack.Count - 2;
                for(int i = idxOfSecondToLastPage; i >= stack.Count - count; --i)
                {
                    this.view.RemovePage(i);
                }
            }

            // Now remove the top page with optional animation.
            return this
                .view
                .PopPage(animateLastPage)
                .Do(
                    _ =>
                    {
                        stack = stack.RemoveRange(stack.Count - count, count - 1);
                        this.currentPageStack.OnNext(stack);
                    });
        }

        public IObservable<Unit> PushModal(IPageViewModel modal, string contract = null)
        {
            Ensure.ArgumentNotNull(modal, nameof(modal));

            return this
                .view
                .PushModal(modal, contract)
                .Do(
                    _ =>
                    {
                        var navigationPage = new NavigationPageViewModel(modal);
                        AddToStackAndTick(this.modalNavigationPages, navigationPage, false);
                        //AddToStackAndTick(this.modalNavigationPages2, modal, false);
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
                        var removedModal = PopStackAndTick(this.modalNavigationPages);
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
