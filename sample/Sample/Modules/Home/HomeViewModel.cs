using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Sample.Common;
using GameCtor.RxNavigation;

namespace Sample.Modules
{
    public class HomeViewModel : BaseViewModel, IHomeViewModel, IPageViewModel
    {
        private int? _popCount;
        private int? _pageIndex;
        private ObservableAsPropertyHelper<int> _pageCount;

        public HomeViewModel(IViewStackService viewStackService)
            : base(viewStackService)
        {
            PushPage = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return ViewStackService.PushPage(new HomeViewModel(ViewStackService));
                });

            PushModalWithNav = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return ViewStackService.PushModal(new NavigationPageViewModel(new HomeViewModel(ViewStackService)));
                });

            PushModalWithoutNav = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return ViewStackService.PushModal(new HomeViewModel(ViewStackService));
                });

            var canPop = this.WhenAnyValue(
                vm => vm.PopCount,
                vm => vm.PageCount,
                (popCount, pageCount) => popCount > 0 && popCount < pageCount);

            PopPages = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return ViewStackService
                        .PopPages(_popCount ?? 0, true);
                },
                canPop);

            var canPopToNewPage = this.WhenAnyValue(
                vm => vm.PageIndex,
                pageIndex => pageIndex >= 0 && pageIndex < PageCount);

            PopToNewPage = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    return Observable
                        .Start(() => ViewStackService.InsertPage(PageIndex ?? 0, new LoginViewModel(ViewStackService)), RxApp.MainThreadScheduler)
                        .SelectMany(_ => ViewStackService.PopToPage(PageIndex ?? 0));
                },
                canPopToNewPage);

            this.WhenActivated(
                disposables =>
                {
                    _pageCount = ViewStackService
                        .PageStack
                        .Select(
                            x =>
                            {
                                return x != null ? x.Count : 0;
                            })
                        .ToProperty(this, vm => vm.PageCount, default(int), false, RxApp.MainThreadScheduler)
                        .DisposeWith(disposables);
                });
        }

        public string Title => "Home";

        public int? PopCount
        {
            get => _popCount;
            set => this.RaiseAndSetIfChanged(ref _popCount, value);
        }

        public int? PageIndex
        {
            get => _pageIndex;
            set => this.RaiseAndSetIfChanged(ref _pageIndex, value);
        }

        public int PageCount => _pageCount != null ? _pageCount.Value : 0;

        public ReactiveCommand PushPage { get; }

        public ReactiveCommand PushModalWithNav { get; }

        public ReactiveCommand PushModalWithoutNav { get; }

        public ReactiveCommand<Unit, Unit> PopPages { get; set; }

        public ReactiveCommand<Unit, Unit> PopToNewPage { get; set; }
    }
}
