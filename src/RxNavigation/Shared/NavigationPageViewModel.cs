using System.Collections.Immutable;

namespace RxNavigation
{
    public sealed class NavigationPageViewModel : INavigationPageViewModel
    {
        public NavigationPageViewModel(IPageViewModel page = null)
        {
            this.PageStack = page != null ? ImmutableList.Create(page) : ImmutableList<IPageViewModel>.Empty;
        }

        public string Id => PageStack[0].Id;

        public IImmutableList<IPageViewModel> PageStack { get; set; }
    }
}
