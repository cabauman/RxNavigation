using System.Collections.Immutable;
using XamFormsRxRouting.Navigation.Interfaces;

namespace XamFormsRxRouting.Navigation
{
    public sealed class NavigationPageViewModel : INavigationPageViewModel
    {
        public NavigationPageViewModel()
        {
            this.PageStack = ImmutableList<IPageViewModel>.Empty;
        }

        public NavigationPageViewModel(IPageViewModel page)
        {
            this.PageStack = ImmutableList.Create(page);
        }

        public string Id => PageStack[0].Id;

        public IImmutableList<IPageViewModel> PageStack { get; set; }
    }
}
