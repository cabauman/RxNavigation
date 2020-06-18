using System;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// Interface for mapping view models to views.
    /// </summary>
    public interface IViewLocator
    {
        /// <summary>
        /// Determines the view for an associated ViewModel.
        /// </summary>
        /// <typeparam name="T">The view model type.</typeparam>
        /// <param name="viewModel">The view model.</param>
        /// <param name="contract">The contract.</param>
        /// <returns>The view, with the ViewModel property assigned to viewModel.</returns>
        IView ResolveView<T>(T viewModel, string contract = null);

        /// <summary>
        /// Determines the view type for an associated view model.
        /// </summary>
        /// <typeparam name="T">The view model type.</typeparam>
        /// <param name="viewModel">The view model.</param>
        /// <param name="contract">The contract.</param>
        /// <returns>The view type.</returns>
        Type ResolveViewType<T>(T viewModel, string contract = null);
    }
}
