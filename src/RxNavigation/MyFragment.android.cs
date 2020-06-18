using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views.Animations;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// A dummy fragment class for experimenting.
    /// </summary>
    public class MyFragment : Fragment, Animation.IAnimationListener
    {
        private Subject<Unit> _whenPushed;
        private Subject<Unit> _whenPopped;
        private IObservable<Unit> _whenComplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyFragment"/> class.
        /// </summary>
        public MyFragment()
        {
            _whenPushed = new Subject<Unit>();
            _whenPopped = new Subject<Unit>();
        }

        /// <summary>
        /// Gets an observable that fires when it's pushed to a stack.
        /// </summary>
        public IObservable<Unit> WhenPushed
        {
            get { return _whenPushed.AsObservable(); }
        }

        /// <inheritdoc/>
        public void OnAnimationEnd(Animation animation)
        {
            _whenPushed.OnNext(Unit.Default);
            _whenPushed.OnCompleted();
        }

        /// <inheritdoc/>
        public void OnAnimationRepeat(Animation animation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void OnAnimationStart(Animation animation)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Animation OnCreateAnimation(int transit, bool enter, int nextAnim)
        {
            Animation anim = base.OnCreateAnimation(transit, enter, nextAnim);
            // Animation anim = AnimationUtils.LoadAnimation(Activity, nextAnim);
            if (anim == null && nextAnim != 0)
            {
                anim = AnimationUtils.LoadAnimation(Activity, nextAnim);
            }

            _whenComplete = Observable.FromEventPattern<Animation.AnimationEndEventArgs>(
                h => anim.AnimationEnd += h,
                h => anim.AnimationEnd -= h)
                    .Select(_ => Unit.Default)
                    .Take(1);

            anim.SetAnimationListener(this);
            return anim;
        }
    }
}
