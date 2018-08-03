using ReactiveUI;
using System.Reactive;

namespace Sample.Modules
{
    public interface IHomeViewModel
    {
        int? PopCount { get; set; }

        int? PageIndex { get; set; }

        ReactiveCommand PushPage { get; }

        ReactiveCommand PushModalWithNav { get; }

        ReactiveCommand PushModalWithoutNav { get; }

        ReactiveCommand<Unit, Unit> PopPages { get; }

        ReactiveCommand<Unit, Unit> PopToNewPage { get; }
    }
}
