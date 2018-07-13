
using ReactiveUI;
using System.Reactive;

namespace XamFormsRxRouting.Modules
{
    public interface IHomeViewModel
    {
        ReactiveCommand Navigate { get; }

        ReactiveCommand<Unit, Unit> PopPages { get; }
    }
}
