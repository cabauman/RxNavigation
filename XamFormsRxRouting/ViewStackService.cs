using Genesis.Ensure;
using Splat;
using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using XamFormsRxRouting.Interfaces;

namespace XamFormsRxRouting
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

        public IObservable<Unit> PopPage(bool animate = true) =>
            this
                .view
                .PopPage(animate);

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
