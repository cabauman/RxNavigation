using System;
using System.Collections.Immutable;
using System.Reactive;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// Interface that manages a stack of views.
    /// </summary>
    public interface IViewStackService
    {
        /// <summary>
        /// Gets the current view shell (platform-specific).
        /// </summary>
        IViewShell ViewShell { get; }

        /// <summary>
        /// Gets the current page stack.
        /// </summary>
        IObservable<IImmutableList<IPageViewModel>> PageStack { get; }

        /// <summary>
        /// Gets the modal stack.
        /// </summary>
        IObservable<IImmutableList<IPageViewModel>> ModalStack { get; }

        /// <summary>
        /// Pushes a page onto the current page stack.
        /// </summary>
        /// <param name="page">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <param name="resetStack">A flag signalling if the page stack should be reset.</param>
        /// <param name="animate">A flag signalling if the push should be animated.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PushPage(
            IPageViewModel page,
            string contract = null,
            bool resetStack = false,
            bool animate = true);

        /// <summary>
        /// Inserts a page into the current page stack at the given index.
        /// </summary>
        /// <param name="index">An insertion index.</param>
        /// <param name="page">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        void InsertPage(
            int index,
            IPageViewModel page,
            string contract = null);

        /// <summary>
        /// Pops to the page at the given index.
        /// </summary>
        /// <param name="index">The index of the page to pop to.</param>
        /// <param name="animateLastPage">A flag signalling if the final pop should be animated.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PopToPage(
            int index,
            bool animateLastPage = true);

        /// <summary>
        /// Pops the given number of pages from the top of the current page stack.
        /// </summary>
        /// <param name="count">The number of pages to pop.</param>
        /// <param name="animateLastPage">A flag signalling if the final pop should be animated.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PopPages(
            int count = 1,
            bool animateLastPage = true);

        /// <summary>
        /// Pushes a page onto the modal stack.
        /// </summary>
        /// <param name="modal">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PushModal(
            IPageViewModel modal,
            string contract = null);

        /// <summary>
        /// Pushes a navigation page onto the modal stack.
        /// </summary>
        /// <param name="modal">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PushModal(
            INavigationPageViewModel modal,
            string contract = null);

        /// <summary>
        /// Pops a page from the top of the modal stack.
        /// </summary>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PopModal();
    }
}
