using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// A class that manages a stack of views.
    /// </summary>
    public class ViewShell : ContentControl, IViewShell
    {
        private readonly Frame _frame;
        private readonly IScheduler _backgroundScheduler;
        private readonly IScheduler _mainScheduler;
        private readonly IViewLocator _viewLocator;
        private readonly Subject<IPageViewModel> _pagePopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewShell"/> class.
        /// </summary>
        /// <param name="frame">A frame.</param>
        /// <param name="backgroundScheduler">A background scheduler.</param>
        /// <param name="mainScheduler">A main scheduler.</param>
        /// <param name="viewLocator">A view locator.</param>
        public ViewShell(Frame frame, IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            _frame = frame;
            _backgroundScheduler = backgroundScheduler;
            _mainScheduler = mainScheduler;
            _viewLocator = viewLocator;

            _pagePopped = new Subject<IPageViewModel>();

            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;

            Observable.FromEventPattern<NavigatedEventHandler, NavigationEventArgs>(
                h => frame.Navigated += h,
                h => frame.Navigated -= h)
                    .Do(
                        e =>
                        {
                            var viewFor = e.EventArgs.Content as IView;
                            viewFor.ViewModel = e.EventArgs.Parameter;
                        });
        }

        /// <inheritdoc/>
        public IObservable<IPageViewModel> PagePopped => _pagePopped.AsObservable();

        /// <inheritdoc/>
        public IObservable<Unit> ModalPopped => throw new NotImplementedException();

        /// <inheritdoc/>
        public void InsertPage(int index, IPageViewModel page, string contract)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IObservable<Unit> PopModal()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IObservable<Unit> PopPage(bool animate)
        {
            return Observable
                .Start(() => _frame.GoBack())
                .Do(_ => _pagePopped.OnNext(null));
        }

        /// <inheritdoc/>
        public IObservable<Unit> PushModal(IPageViewModel modalViewModel, string contract, bool withNavStack)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IObservable<Unit> PushPage(IPageViewModel pageViewModel, string contract, bool resetStack, bool animate)
        {
            return Observable
                .Start(() => _frame.Navigate(LocatePageFor(pageViewModel, contract), pageViewModel))
                .Select(_ => Unit.Default);
        }

        /// <inheritdoc/>
        public void RemovePage(int index)
        {
            throw new NotImplementedException();
        }

        private Type LocatePageFor(object viewModel, string contract)
        {
            var viewType = _viewLocator.ResolveViewType(viewModel, contract);

            if (viewType == null)
            {
                throw new InvalidOperationException($"No view could be located for type '{viewModel.GetType().FullName}', contract '{contract}'. Be sure Splat has an appropriate registration.");
            }

            return viewType;
        }
    }
}
