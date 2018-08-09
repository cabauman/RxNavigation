using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using UIKit;

namespace RxNavigation
{
    public class RxNavigationController : UINavigationController
    {
        protected static readonly Subject<RxNavigationController> navigationPagePushed = new Subject<RxNavigationController>();
        protected static readonly Subject<Unit> modalPopped = new Subject<Unit>();
        public static int incrementer = 0;

        private readonly Subject<UIViewController> controllerPopped = new Subject<UIViewController>();
        private readonly Subject<UIViewController> modalPushed = new Subject<UIViewController>();
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

        public IObservable<UIViewController> ControllerPopped => this.controllerPopped.AsObservable();

        public IObservable<UIViewController> ModalPushed => this.modalPushed.AsObservable();

        public static IObservable<RxNavigationController> NavigationPagePushed => navigationPagePushed.AsObservable();

        public IObservable<Unit> ModalPopped => modalPopped.AsObservable();

        public override UIViewController PopViewController(bool animated)
        {
            var poppedController = base.PopViewController(animated);
            this.controllerPopped.OnNext(poppedController);

            return poppedController;
        }

        public override async Task PresentViewControllerAsync(UIViewController viewControllerToPresent, bool animated)
        {
            await base.PresentViewControllerAsync(viewControllerToPresent, animated);
            modalPushed.OnNext(viewControllerToPresent);
            if(viewControllerToPresent is RxNavigationController navigationPage)
            {
                navigationPagePushed.OnNext(navigationPage);
            }
        }

        public override async Task DismissViewControllerAsync(bool animated)
        {
            await base.DismissViewControllerAsync(animated);
            modalPopped.OnNext(Unit.Default);
        }
    }
}
