using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views.Animations;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// A class that manages a stack of views.
    /// </summary>
    public class ViewShell : FragmentActivity, IViewShell
    {
        private readonly IScheduler _backgroundScheduler;
        private readonly IScheduler _mainScheduler;
        private readonly IViewLocator _viewLocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewShell"/> class.
        /// </summary>
        /// <param name="backgroundScheduler">A background scheduler.</param>
        /// <param name="mainScheduler">A main scheduler.</param>
        /// <param name="viewLocator">A view locator.</param>
        public ViewShell(IScheduler backgroundScheduler, IScheduler mainScheduler, IViewLocator viewLocator)
        {
            _backgroundScheduler = backgroundScheduler;
            _mainScheduler = mainScheduler;
            _viewLocator = viewLocator;
        }

        /// <inheritdoc/>
        public IObservable<IPageViewModel> PagePopped => throw new NotImplementedException();

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
                .Start(
                    () =>
                    {
                        MyFragment frag = new MyFragment();
                        SupportFragmentManager
                            .BeginTransaction()
                            .Remove(null)
                            .Commit();

                        return frag.WhenPushed;
                    })
                .Switch();
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
                .Start(
                    () =>
                    {
                        var page = LocatePageFor(pageViewModel, contract);
                        return page;
                    },
                    _backgroundScheduler)
                .ObserveOn(_mainScheduler)
                .SelectMany(
                    page =>
                    {
                        SupportFragmentManager
                            .BeginTransaction()
                            .Add(Android.Resource.Id.Content, page)
                            .AddToBackStack("name")
                            .Commit();

                        return page.WhenPushed;
                    });
        }

        /// <inheritdoc/>
        public void RemovePage(int index)
        {
            throw new NotImplementedException();
        }

        private MyFragment LocatePageFor(object viewModel, string contract)
        {
            var viewFor = _viewLocator.ResolveView(viewModel, contract);
            var page = viewFor as MyFragment;

            if (viewFor == null)
            {
                throw new InvalidOperationException($"No view could be located for type '{viewModel.GetType().FullName}', contract '{contract}'. Be sure Splat has an appropriate registration.");
            }

            if (page == null)
            {
                throw new InvalidOperationException($"Resolved view '{viewFor.GetType().FullName}' for type '{viewModel.GetType().FullName}', contract '{contract}' is not a Page.");
            }

            viewFor.ViewModel = viewModel;

            return page;
        }
    }
}
