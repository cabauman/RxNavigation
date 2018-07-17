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
        private int _pageIndex;

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
                },
                canPop);

            var canPopToNewPage = this.WhenAnyValue(
                vm => vm.PageIndex,
                pageIndex => pageIndex >= 0 && pageIndex < ViewStackService.PageCount);

            PopToNewPage = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return Observable
                        .Start(() => ViewStackService.InsertPage(PageIndex, new LoginViewModel(ViewStackService)), RxApp.MainThreadScheduler)
                        .Concat(ViewStackService.PopToPage(PageIndex));
                },
                canPopToNewPage);
        }

        public string Id => nameof(HomeViewModel);

        public int PopCount
        {
            get => _popCount;
            set => this.RaiseAndSetIfChanged(ref _popCount, value);
        }

        public int PageIndex
        {
            get => _pageIndex;
            set => this.RaiseAndSetIfChanged(ref _pageIndex, value);
        }

        public ReactiveCommand Navigate { get; }

        public ReactiveCommand<Unit, Unit> PopPages { get; }

        public ReactiveCommand<Unit, Unit> PopToNewPage { get; }
    }
}
