using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views.Animations;
using ReactiveUI;

namespace GameCtor.RxNavigation
{
    public class MainView : FragmentActivity, IView
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
            return Observable
                .Start(
                    () =>
                    {
                        MyFragment frag = new MyFragment();
                        SupportFragmentManager
                            .BeginTransaction()
                            .Remove(null)
                            .Commit();

                        return frag.WhenPushed;
                    })
                .Switch();
        }

        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            return Observable
                .Start(
                    () =>
                    {
                        MyFragment frag = new MyFragment();
                        SupportFragmentManager
                            .BeginTransaction()
                            .Add(Android.Resource.Id.Content, frag)
                            .AddToBackStack("name")
                            .Commit();

                        return frag.WhenPushed;
                    })
                .Switch();
        }

        public void RemovePage(int index)
        {
            throw new NotImplementedException();
        }
    }


    public class MyFragment : Fragment, Animation.IAnimationListener
    {
        private Subject<Unit> whenPushed;
        private Subject<Unit> whenPopped;
        private IObservable<Unit> WhenComplete;

        public MyFragment()
        {
        }
        
        public IObservable<Unit> WhenPushed
        {
            get { return whenPushed.AsObservable(); }
        }

        public void OnAnimationEnd(Animation animation)
        {
            whenPushed.OnNext(Unit.Default);
            whenPushed.OnCompleted();
        }

        public void OnAnimationRepeat(Animation animation)
        {
            throw new NotImplementedException();
        }

        public void OnAnimationStart(Animation animation)
        {
            throw new NotImplementedException();
        }

        public override Animation OnCreateAnimation(int transit, bool enter, int nextAnim)
        {
            Animation anim = base.OnCreateAnimation(transit, enter, nextAnim);
            //Animation anim = AnimationUtils.LoadAnimation(Activity, nextAnim);

            if(anim == null && nextAnim != 0)
            {
                anim = AnimationUtils.LoadAnimation(Activity, nextAnim);
            }

            WhenComplete = Observable.FromEventPattern<Animation.AnimationEndEventArgs>(
                h => anim.AnimationEnd += h,
                h => anim.AnimationEnd -= h)
                    .Select(_ => Unit.Default)
                    .Take(1);

            anim.SetAnimationListener(this);
            return anim;
        }
    }
}
