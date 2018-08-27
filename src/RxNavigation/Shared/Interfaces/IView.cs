using System;
using System.Reactive;

namespace GameCtor.RxNavigation
{
    public interface IView
    {
        IObservable<IPageViewModel> PagePopped { get; }

        IObservable<Unit> ModalPopped { get; }

        IObservable<Unit> PushPage(
            IPageViewModel pageViewModel,
            string contract,
            bool resetStack,
            bool animate);

        IObservable<Unit> PopPage(
            bool animate);

        void InsertPage(
            int index,
            IPageViewModel page,
            string contract);

        void RemovePage(int index);

        IObservable<Unit> PushModal(
            IPageViewModel modalViewModel,
            string contract,
            bool withNavStack);

        IObservable<Unit> PopModal();
    }
}
