using System;
using System.Collections.Immutable;
using System.Reactive;

namespace XamFormsRxRouting.Navigation.Interfaces
{
    public interface IViewStackService
    {
        IView View { get; }

        int PageCount { get; }

        IObservable<IImmutableList<IPageViewModel>> PageStack { get; }

        IObservable<IImmutableList<IModalViewModel>> ModalStack { get; }

        IObservable<Unit> PushPage(
            IPageViewModel page,
            string contract = null,
            bool resetStack = false,
            bool animate = true);

        IObservable<Unit> PopToPage(
            int index,
            bool animateLastPage = true);

        IObservable<Unit> PopPages(
            int count = 1,
            bool animateLastPage = true);

        void InsertPage(
            int index,
            IPageViewModel page,
            string contract = null);

        IObservable<Unit> PushModal(
            IModalViewModel modal,
            string contract = null);

        IObservable<Unit> PopModal();
    }
}
