using System.Collections.Immutable;
using System.Reactive.Subjects;

namespace GameCtor.RxNavigation
{
    public interface INavigationPageViewModel : IPageViewModel
    {
        BehaviorSubject<IImmutableList<IPageViewModel>> PageStack { get; set; }
    }
}
