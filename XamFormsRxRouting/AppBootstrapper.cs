using ReactiveUI;
using Splat;
using System;
using System.Reactive.Linq;
using Xamarin.Forms;
using XamFormsRxRouting.Navigation.Interfaces;
using XamFormsRxRouting.Modules;
using XamFormsRxRouting.Navigation;

namespace XamFormsRxRouting
{
    public sealed class AppBootstrapper
    {
        private readonly NavigationPage _navigationPage;

        public AppBootstrapper()
        {
            Locator.CurrentMutable.InitializeSplat();
            Locator.CurrentMutable.InitializeReactiveUI();

            RegisterServices();
            RegisterViews();

            IView mainView = new MainView(RxApp.TaskpoolScheduler, RxApp.MainThreadScheduler, ViewLocator.Current);
            _navigationPage = mainView as NavigationPage;
            IViewStackService viewStackService = new ViewStackService(mainView);
            Locator.CurrentMutable.RegisterConstant(viewStackService, typeof(IViewStackService));

            viewStackService
                .PushPage(new LoginViewModel(viewStackService))
                .Subscribe();
        }

        public NavigationPage GetMainView()
        {
            return _navigationPage;
        }

        private void RegisterServices()
        {

        }

        private void RegisterViews()
        {
            Locator.CurrentMutable.Register(() => new LoginPage(), typeof(IViewFor<ILoginViewModel>));
            Locator.CurrentMutable.Register(() => new HomePage(), typeof(IViewFor<IHomeViewModel>));
        }
    }
}
