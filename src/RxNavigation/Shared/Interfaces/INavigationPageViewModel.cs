using System.Collections.Immutable;

namespace RxNavigation
{
    public interface INavigationPageViewModel : IPageViewModel
    {
        IImmutableList<IPageViewModel> PageStack { get; set; }
    }
}
