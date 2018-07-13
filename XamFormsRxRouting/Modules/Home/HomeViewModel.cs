using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using XamFormsRxRouting.Common;
using XamFormsRxRouting.Navigation.Interfaces;

namespace XamFormsRxRouting.Modules
{
    public class HomeViewModel : BaseViewModel, IHomeViewModel, IPageViewModel
    {
        public HomeViewModel(IViewStackService viewStackService)
            : base(viewStackService)
        {
            Navigate = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return ViewStackService.PushPage(new HomeViewModel(ViewStackService));
                });

            PopPages = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return ViewStackService
                        .PopPages(3, false)
                        .SelectMany(_ => ViewStackService.PushPage(new LoginViewModel(ViewStackService)));
                });
        }

        public string Id => nameof(HomeViewModel);

        public ReactiveCommand Navigate { get; }

        public ReactiveCommand<Unit, Unit> PopPages { get; }
    }
}
