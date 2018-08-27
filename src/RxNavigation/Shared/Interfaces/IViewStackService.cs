using System;
using System.Collections.Immutable;
using System.Reactive;

namespace GameCtor.RxNavigation
{
    public interface IViewStackService
    {
        IView View { get; }

        IObservable<IImmutableList<IPageViewModel>> PageStack { get; }

        IObservable<IImmutableList<IPageViewModel>> ModalStack { get; }

        IObservable<Unit> PushPage(
            IPageViewModel page,
            string contract = null,
            bool resetStack = false,
            bool animate = true);

        void InsertPage(
            int index,
            IPageViewModel page,
            string contract = null);

        IObservable<Unit> PopToPage(
            int index,
            bool animateLastPage = true);

        IObservable<Unit> PopPages(
            int count = 1,
            bool animateLastPage = true);

        IObservable<Unit> PushModal(
            IPageViewModel modal,
            string contract = null);

        IObservable<Unit> PushModal(
            INavigationPageViewModel modal,
            string contract = null);

        IObservable<Unit> PopModal();
    }
}
