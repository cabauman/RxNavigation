using System;
using System.Reactive;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// Interface that manages a stack of views.
    /// </summary>
    public interface IViewShell
    {
        /// <summary>
        /// An observable that signals when a page is popped from the current page stack.
        /// </summary>
        IObservable<IPageViewModel> PagePopped { get; }

        /// <summary>
        /// An observable that signals when a page is popped from the modal stack.
        /// </summary>
        IObservable<Unit> ModalPopped { get; }

        /// <summary>
        /// Pushes a page onto the current page stack.
        /// </summary>
        /// <param name="pageViewModel">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <param name="resetStack">A flag signalling if the page stack should be reset.</param>
        /// <param name="animate">A flag signalling if the push should be animated.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PushPage(
            IPageViewModel pageViewModel,
            string contract,
            bool resetStack,
            bool animate);

        /// <summary>
        /// Pops a page from the top of the current page stack.
        /// </summary>
        /// <param name="animate"></param>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PopPage(
            bool animate);

        /// <summary>
        /// Inserts a page into the current page stack at the given index.
        /// </summary>
        /// <param name="index">An insertion index.</param>
        /// <param name="page">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        void InsertPage(
            int index,
            IPageViewModel page,
            string contract);

        /// <summary>
        /// Removes a page from the current page stack at the given index.
        /// </summary>
        /// <param name="index">The index of the page to remove.</param>
        void RemovePage(int index);

        /// <summary>
        /// Pushes a page onto the modal stack.
        /// </summary>
        /// <param name="modalViewModel">A page view model.</param>
        /// <param name="contract">A page contract.</param>
        /// <param name="withNavStack">A flag signalling if a new page stack should be created.</param>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PushModal(
            IPageViewModel modalViewModel,
            string contract,
            bool withNavStack);

        /// <summary>
        /// Pops a page from the top of the modal stack.
        /// </summary>
        /// <returns>An observable that signals the completion of this action.</returns>
        IObservable<Unit> PopModal();
    }
}
