﻿using ReactiveUI;
using Splat;
using System;
using System.Reactive.Linq;
using Xamarin.Forms;
using Sample.Modules;
using GameCtor.RxNavigation;
using GameCtor.RxNavigation.XamForms;

namespace Sample
{
    public sealed class AppBootstrapper
    {
        private readonly Xamarin.Forms.NavigationPage _navigationPage;

        public AppBootstrapper()
        {
            RegisterServices();
            RegisterViews();

            IViewShell mainView = new ViewShell(RxApp.TaskpoolScheduler, RxApp.MainThreadScheduler, new ReactiveUIViewLocator());
            _navigationPage = mainView as Xamarin.Forms.NavigationPage;
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
