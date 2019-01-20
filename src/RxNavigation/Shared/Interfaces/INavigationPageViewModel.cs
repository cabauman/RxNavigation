using System.Collections.Immutable;
using System.Reactive.Subjects;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// Interface for a page view model that contains its own page stack.
    /// </summary>
    public interface INavigationPageViewModel : IPageViewModel
    {
        /// <summary>
        /// Gets or sets the page stack.
        /// </summary>
        BehaviorSubject<IImmutableList<IPageViewModel>> PageStack { get; set; }
    }
}
