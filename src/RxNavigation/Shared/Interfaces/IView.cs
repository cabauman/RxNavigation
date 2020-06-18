namespace GameCtor.RxNavigation
{
    /// <summary>
    /// Interface for a view containing a corresponding view model.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Gets or sets the view model for this view.
        /// </summary>
        object ViewModel { get; set; }
    }

    /// <summary>
    /// Interface for a view containing a corresponding view model.
    /// </summary>
    /// <typeparam name="T">The view model type.</typeparam>
    public interface IView<T>
    {
        /// <summary>
        /// Gets or sets the view model for this view.
        /// </summary>
        T ViewModel { get; set; }
    }
}
