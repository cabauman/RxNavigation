using System;
using System.Reactive;
using System.Reactive.Concurrency;
using Android.Support.V4.App;
using Android.Support.V7.App;
using ReactiveUI;

namespace RxNavigation
{
    public class MainView : AppCompatActivity, IView
    {
        private readonly IScheduler backgroundScheduler;
        private readonly IScheduler mainScheduler;
        private readonly IViewLocator viewLocator;

        public MainView(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            this.backgroundScheduler = backgroundScheduler ?? RxApp.TaskpoolScheduler;
            this.mainScheduler = mainScheduler ?? RxApp.MainThreadScheduler;
            this.viewLocator = viewLocator ?? ViewLocator.Current;

            //this.navigationPages = new Stack<UINavigationController>();
            //this.navigationPages.Push(this);
        }

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
            Fragment frag = new Fragment();
            SupportFragmentManager.BeginTransaction()
                .Add(Android.Resource.Id.Content, frag)
                .Commit();

            throw new NotImplementedException();
        }

        public void RemovePage(int index)
        {
            throw new NotImplementedException();
        }
    }
}
