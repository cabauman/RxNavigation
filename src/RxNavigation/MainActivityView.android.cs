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

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// A class that manages a stack of views.
    /// </summary>
    public class MainActivityView : IViewShell
    {
        private static int identifier = 0;

        private readonly IScheduler _backgroundScheduler;
        private readonly IScheduler _mainScheduler;
        private readonly IViewLocator _viewLocator;
        private readonly IObservable<IPageViewModel> _pagePopped;
        private readonly IObservable<Activity> _whenPageCreated;
        private readonly HashSet<Activity> _userInstigatedPops;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainActivityView"/> class.
        /// </summary>
        /// <param name="backgroundScheduler">A background scheduler.</param>
        /// <param name="mainScheduler">A main scheduler.</param>
        /// <param name="viewLocator">A view locator.</param>
        public MainActivityView(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            _backgroundScheduler = backgroundScheduler ?? RxApp.TaskpoolScheduler;
            _mainScheduler = mainScheduler ?? RxApp.MainThreadScheduler;
            _viewLocator = viewLocator ?? ViewLocator.Current;

            _userInstigatedPops = new HashSet<Activity>();

            _whenPageCreated = Observable
                .FromEventPattern<ActivityEventArgs>(
                    h => CrossCurrentActivity.Current.ActivityStateChanged += h,
                    h => CrossCurrentActivity.Current.ActivityStateChanged -= h)
                .Where(x => x.EventArgs.Event == ActivityEvent.Created)
                .Select(x => x.EventArgs.Activity);

            _pagePopped = Observable
                .FromEventPattern<ActivityEventArgs>(
                    h => CrossCurrentActivity.Current.ActivityStateChanged += h,
                    h => CrossCurrentActivity.Current.ActivityStateChanged -= h)
                .Where(x => x.EventArgs.Event == ActivityEvent.Destroyed)
                .Select(x => x.EventArgs.Activity)
                .Select(
                    x =>
                    {
                        bool removed = _userInstigatedPops.Remove(x);
                        return removed ? null : x;
                    })
                .Where(x => x != null)
                .Select(x => x as IViewFor)
                .Where(x => x != null)
                .Select(x => x.ViewModel as IPageViewModel);
        }

        /// <inheritdoc/>
        public IObservable<IPageViewModel> PagePopped => PagePopped;

        /// <inheritdoc/>
        public IObservable<Unit> ModalPopped => throw new NotImplementedException();

        /// <inheritdoc/>
        public void InsertPage(int index, IPageViewModel page, string contract)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IObservable<Unit> PopModal()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IObservable<Unit> PopPage(bool animate)
        {
            return Observable
                .Start(
                    () =>
                    {
                        _userInstigatedPops.Add(CrossCurrentActivity.Current.Activity);
                        CrossCurrentActivity.Current.Activity.Finish();
                    });
        }

        /// <inheritdoc/>
        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
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
                        return _whenPageCreated
                            .Where(x => x.Intent.GetIntExtra("Id", -1) == id)
                            .Select(x => x as IViewFor)
                            .Where(x => x != null)
                            .Do(x => x.ViewModel = pageViewModel);
                    });
        }

        /// <inheritdoc/>
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
