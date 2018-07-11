using System;
using System.Collections.Immutable;
using System.Reactive;

namespace XamFormsRxRouting.Interfaces
{
    public interface IViewStackService
    {
        IView View { get; }

        IObservable<IImmutableList<IPageViewModel>> PageStack { get; }

        IObservable<IImmutableList<IModalViewModel>> ModalStack { get; }

        IObservable<Unit> PushPage(
            IPageViewModel page,
            string contract = null,
            bool resetStack = false,
            bool animate = true);

        IObservable<Unit> PopPage(
            bool animate = true);

        IObservable<Unit> PushModal(
            IModalViewModel modal,
            string contract = null);

        IObservable<Unit> PopModal();
    }
}
