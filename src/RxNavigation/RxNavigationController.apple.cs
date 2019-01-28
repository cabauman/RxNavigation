using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UIKit;

namespace GameCtor.RxNavigation
{
    /// <summary>
     /// A class that manages a stack of views.
     /// </summary>
    public class RxNavigationController : UINavigationController
    {
        private readonly Subject<UIViewController> _controllerPopped = new Subject<UIViewController>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RxNavigationController"/> class.
        /// </summary>
        public RxNavigationController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RxNavigationController"/> class.
        /// </summary>
        /// <param name="rootViewController">The first view controller on the stack.</param>
        public RxNavigationController(UIViewController rootViewController)
            : base(rootViewController)
        {
        }

        /// <summary>
        /// Gets an observable that signals when a page is popped.
        /// </summary>
        public IObservable<UIViewController> ControllerPopped => _controllerPopped.AsObservable();

        /// <inheritdoc/>
        public override UIViewController PopViewController(bool animated)
        {
            var poppedController = base.PopViewController(animated);
            _controllerPopped.OnNext(poppedController);

            return poppedController;
        }
    }
}
