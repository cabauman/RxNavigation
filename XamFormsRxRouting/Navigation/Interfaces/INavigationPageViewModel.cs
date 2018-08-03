using System.Collections.Immutable;
using XamFormsRxRouting.Navigation.Interfaces;

namespace XamFormsRxRouting.Navigation.Interfaces
{
    public interface INavigationPageViewModel : IPageViewModel
    {
        IImmutableList<IPageViewModel> PageStack { get; }
    }
}
