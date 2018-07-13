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
        private int _popCount;

        public HomeViewModel(IViewStackService viewStackService)
            : base(viewStackService)
        {
            Navigate = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return ViewStackService.PushPage(new HomeViewModel(ViewStackService));
                });

            var canPop = this.WhenAnyValue(
                vm => vm.PopCount,
                popCount => popCount > 0 && popCount < ViewStackService.PageCount);

            PopPages = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return ViewStackService
                        .PopPages(_popCount, true);
                        //.SelectMany(_ => ViewStackService.PushPage(new LoginViewModel(ViewStackService)));
                },
                canPop);
        }

        public string Id => nameof(HomeViewModel);

        public int PopCount
        {
            get => _popCount;
            set => this.RaiseAndSetIfChanged(ref _popCount, value);
        }

        public ReactiveCommand Navigate { get; }

        public ReactiveCommand<Unit, Unit> PopPages { get; }
    }
}
