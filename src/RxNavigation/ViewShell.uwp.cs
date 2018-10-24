using System;
using System.Reactive;

namespace GameCtor.RxNavigation
{
    public class ViewShell : IViewShell
    {
        public IObservable<IPageViewModel> PagePopped => throw new NotImplementedException();

        public IObservable<Unit> ModalPopped => throw new NotImplementedException();

        public void InsertPage(int index, IPageViewModel page, string contract)
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> PopModal()
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> PopPage(bool animate)
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            throw new NotImplementedException();
        }

        public void RemovePage(int index)
        {
            throw new NotImplementedException();
        }
    }
}
