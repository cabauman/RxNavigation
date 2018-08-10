using System.Collections.Immutable;
using System.Reactive.Subjects;

namespace RxNavigation
{
    public interface INavigationPageViewModel : IPageViewModel
    {
        BehaviorSubject<IImmutableList<IPageViewModel>> PageStack { get; set; }
    }
}
