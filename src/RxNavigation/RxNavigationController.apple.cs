using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UIKit;

namespace GameCtor.RxNavigation
{
    public class RxNavigationController : UINavigationController
    {
        private readonly Subject<UIViewController> _controllerPopped = new Subject<UIViewController>();

        public static int incrementer = 0;
        public int index;

        public RxNavigationController()
        {
            index = incrementer++;
        }

        public RxNavigationController(UIViewController rootViewController)
            : base(rootViewController)
        {
            index = incrementer++;
        }

        public IObservable<UIViewController> ControllerPopped => _controllerPopped.AsObservable();

        public override UIViewController PopViewController(bool animated)
        {
            var poppedController = base.PopViewController(animated);
            _controllerPopped.OnNext(poppedController);

            return poppedController;
        }
    }
}
