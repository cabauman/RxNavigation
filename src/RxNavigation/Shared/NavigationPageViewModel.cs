using System.Collections.Immutable;
using System.Reactive.Subjects;

namespace RxNavigation
{
    public sealed class NavigationPageViewModel : INavigationPageViewModel
    {
        public NavigationPageViewModel(IPageViewModel page = null)
        {
            var contents = page != null ? ImmutableList.Create(page) : ImmutableList<IPageViewModel>.Empty;
            this.PageStack = new BehaviorSubject<IImmutableList<IPageViewModel>>(contents);
        }

        public string Id => PageStack.Value[0].Id;

        public BehaviorSubject<IImmutableList<IPageViewModel>> PageStack { get; set; }
    }
}
