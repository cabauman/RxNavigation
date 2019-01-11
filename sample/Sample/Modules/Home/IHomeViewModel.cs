using ReactiveUI;
using System.Reactive;

namespace Sample.Modules
{
    public interface IHomeViewModel
    {
        int? PopCount { get; set; }

        int? PageIndex { get; set; }

        int PageCount { get; }

        ReactiveCommand<Unit, Unit> PushPage { get; }

        ReactiveCommand<Unit, Unit> PushModalWithNav { get; }

        ReactiveCommand<Unit, Unit> PushModalWithoutNav { get; }

        ReactiveCommand<Unit, Unit> PopPages { get; }

        ReactiveCommand<Unit, Unit> PopToNewPage { get; }
    }
}
