using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Plugin.CurrentActivity;
using ReactiveUI;

namespace RxNavigation
{
    public class MainActivityView : IView
    {
        private readonly IScheduler backgroundScheduler;
        private readonly IScheduler mainScheduler;
        private readonly IViewLocator viewLocator;
        private readonly IObservable<IPageViewModel> pagePopped;
        private IObservable<Activity> whenPageCreated;
        private readonly HashSet<Activity> userInstigatedPops;

        public MainActivityView(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            this.backgroundScheduler = backgroundScheduler ?? RxApp.TaskpoolScheduler;
            this.mainScheduler = mainScheduler ?? RxApp.MainThreadScheduler;
            this.viewLocator = viewLocator ?? ViewLocator.Current;

            whenPageCreated = Observable
                .FromEventPattern<ActivityEventArgs>(
                    h => CrossCurrentActivity.Current.ActivityStateChanged += h,
                    h => CrossCurrentActivity.Current.ActivityStateChanged -= h)
                .Where(x => x.EventArgs.Event == ActivityEvent.Created)
                .Select(x => x.EventArgs.Activity);

            this.pagePopped = Observable
                .FromEventPattern<ActivityEventArgs>(
                    h => CrossCurrentActivity.Current.ActivityStateChanged += h,
                    h => CrossCurrentActivity.Current.ActivityStateChanged -= h)
                .Where(x => x.EventArgs.Event == ActivityEvent.Destroyed)
                .Select(x => x.EventArgs.Activity)
                .Select(
                    x =>
                    {
                        bool removed = userInstigatedPops.Remove(x);
                        return removed ? null : x;
                    })
                .Where(x => x != null)
                .Select(x => x as IViewFor)
                .Where(x => x != null)
                .Select(x => x.ViewModel as IPageViewModel);

            //this.navigationPages = new Stack<UINavigationController>();
            //this.navigationPages.Push(this);
        }

        public IObservable<IPageViewModel> PagePopped => this.PagePopped;

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
            return Observable
                .Start(
                    () =>
                    {
                        userInstigatedPops.Add(CrossCurrentActivity.Current.Activity);
                        CrossCurrentActivity.Current.Activity.Finish();
                    });
        }

        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            throw new NotImplementedException();
        }

        private static int identifier = 0;
        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            int id = ++identifier;
            return Observable
                .Start(
                    () =>
                    {
                        var activityType = LocatePageFor(pageViewModel);
                        Intent intent = new Intent(CrossCurrentActivity.Current.Activity, activityType);
                        intent.PutExtra("Id", id);
                        CrossCurrentActivity.Current.Activity.StartActivity(intent);
                    })
                .Delay(
                    _ =>
                    {
                        return whenPageCreated
                            .Where(x => x.Intent.GetIntExtra("Id", -1) == id)
                            .Select(x => x as IViewFor)
                            .Where(x => x != null)
                            .Do(x => x.ViewModel = pageViewModel);
                    });
        }

        public void RemovePage(int index)
        {
            throw new NotImplementedException();
        }

        private Type LocatePageFor(object viewModel)
        {
            throw new NotImplementedException();
        }
    }
}
