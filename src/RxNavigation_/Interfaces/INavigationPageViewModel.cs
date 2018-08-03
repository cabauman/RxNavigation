using System.Collections.Immutable;

namespace RxNavigation.Interfaces
{
    public interface INavigationPageViewModel : IPageViewModel
    {
        IImmutableList<IPageViewModel> PageStack { get; }
    }
}
