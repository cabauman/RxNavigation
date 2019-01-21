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
    public class ViewShell : FragmentActivity, IViewShell
    {
        private readonly IScheduler _backgroundScheduler;
        private readonly IScheduler _mainScheduler;
        private readonly IViewLocator _viewLocator;

        public ViewShell(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            _backgroundScheduler = backgroundScheduler ?? RxApp.TaskpoolScheduler;
            _mainScheduler = mainScheduler ?? RxApp.MainThreadScheduler;
            _viewLocator = viewLocator ?? ViewLocator.Current;

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
                        var page = LocatePageFor(pageViewModel, contract);
                        return page;
                    },
                    _backgroundScheduler)
                .ObserveOn(_mainScheduler)
                .SelectMany(
                    page =>
                    {
                        SupportFragmentManager
                            .BeginTransaction()
                            .Add(Android.Resource.Id.Content, page)
                            .AddToBackStack("name")
                            .Commit();

                        return page.WhenPushed;
                    });
        }

        public void RemovePage(int index)
        {
            throw new NotImplementedException();
        }

        private MyFragment LocatePageFor(object viewModel, string contract)
        {
            var viewFor = _viewLocator.ResolveView(viewModel, contract);
            var page = viewFor as MyFragment;

            if (viewFor == null)
            {
                throw new InvalidOperationException($"No view could be located for type '{viewModel.GetType().FullName}', contract '{contract}'. Be sure Splat has an appropriate registration.");
            }

            if (page == null)
            {
                throw new InvalidOperationException($"Resolved view '{viewFor.GetType().FullName}' for type '{viewModel.GetType().FullName}', contract '{contract}' is not a Page.");
            }

            viewFor.ViewModel = viewModel;

            return page;
        }
    }


    public class MyFragment : Fragment, Animation.IAnimationListener
    {
        private Subject<Unit> _whenPushed;
        private Subject<Unit> _whenPopped;
        private IObservable<Unit> _WhenComplete;

        public MyFragment()
        {
        }
        
        public IObservable<Unit> WhenPushed
        {
            get { return _whenPushed.AsObservable(); }
        }

        public void OnAnimationEnd(Animation animation)
        {
            _whenPushed.OnNext(Unit.Default);
            _whenPushed.OnCompleted();
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

            if (anim == null && nextAnim != 0)
            {
                anim = AnimationUtils.LoadAnimation(Activity, nextAnim);
            }

            _WhenComplete = Observable.FromEventPattern<Animation.AnimationEndEventArgs>(
                h => anim.AnimationEnd += h,
                h => anim.AnimationEnd -= h)
                    .Select(_ => Unit.Default)
                    .Take(1);

            anim.SetAnimationListener(this);
            return anim;
        }
    }
}
