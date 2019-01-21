using System.Collections.Immutable;
using System.Reactive.Subjects;

namespace GameCtor.RxNavigation
{
    /// <summary>
    /// A page view model that contains its own page stack.
    /// </summary>
    public sealed class NavigationPageViewModel : INavigationPageViewModel
    {
        /// <summary>
        /// Creates an instance of NavigationPageViewModel.
        /// </summary>
        /// <param name="page">The page to push on the page stack.</param>
        public NavigationPageViewModel(IPageViewModel page = null)
        {
            var contents = page != null ? ImmutableList.Create(page) : ImmutableList<IPageViewModel>.Empty;
            PageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(contents);
        }

        /// <summary>
        /// Gets the title of this page.
        /// </summary>
        public string Title => PageStack.Value[0].Title;

        /// <summary>
        /// Gets or sets the page stack.
        /// </summary>
        public BehaviorSubject<IImmutableList<IPageViewModel>> PageStack { get; set; }
    }
}
