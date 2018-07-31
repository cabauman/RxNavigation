using System.Collections.Immutable;
using XamFormsRxRouting.Navigation.Interfaces;

namespace XamFormsRxRouting.Navigation
{
    public interface INavigationPageViewModel : IPageViewModel
    {
        IImmutableList<IPageViewModel> PageStack { get; }
    }

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

        public IImmutableList<IPageViewModel> PageStack { get; set; }

        public string Id => "";
    }
}
