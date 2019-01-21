using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using Splat;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameCtor.RxNavigation
{
    public class ViewShell : TransitioningContentControl, IViewShell, IActivatable, IEnableLogger
    {
        private readonly Frame _frame;
        private readonly IScheduler _backgroundScheduler;
        private readonly IScheduler _mainScheduler;
        private readonly ViewTypeLocator _viewTypeLocator;
        private readonly Subject<IPageViewModel> _pagePopped;

        public ViewShell(Frame frame, IScheduler backgroundScheduler, IScheduler mainScheduler, ViewTypeLocator viewTypeLocator)
        {
            _frame = frame;
            _backgroundScheduler = backgroundScheduler ?? RxApp.TaskpoolScheduler;
            _mainScheduler = mainScheduler ?? RxApp.MainThreadScheduler;
            _viewTypeLocator = viewTypeLocator ?? new ViewTypeLocator();

            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;

            Observable.FromEventPattern<NavigatedEventHandler, NavigationEventArgs>(
                h => frame.Navigated += h,
                h => frame.Navigated -= h)
                    .Do(
                        e =>
                        {
                            var viewFor = e.EventArgs.Content as IViewFor;
                            viewFor.ViewModel = e.EventArgs.Parameter;
                        });
        }

        public IObservable<IPageViewModel> PagePopped => _pagePopped.AsObservable();

        public IObservable<Unit> ModalPopped => throw new NotImplementedException();

        public void InsertPage(int index, IPageViewModel page, string contract)
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> PopModal()
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> PopPage(bool animate)
        {
            return Observable
                .Start(() => _frame.GoBack())
                .Do(_ => _pagePopped.OnNext(null));
        }

        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            return Observable
                .Start(() => _frame.Navigate(LocatePageFor(pageViewModel, contract), pageViewModel))
                .Select(_ => Unit.Default);
        }

        public void RemovePage(int index)
        {
            throw new NotImplementedException();
        }

        private Type LocatePageFor(object viewModel, string contract)
        {
            var viewType = _viewTypeLocator.ResolveView(viewModel, contract);

            if (viewType == null)
            {
                throw new InvalidOperationException($"No view could be located for type '{viewModel.GetType().FullName}', contract '{contract}'. Be sure Splat has an appropriate registration.");
            }

            return viewType;
        }
    }
}
