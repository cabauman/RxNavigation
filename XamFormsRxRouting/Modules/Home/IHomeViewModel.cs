
using ReactiveUI;
using System.Reactive;

namespace XamFormsRxRouting.Modules
{
    public interface IHomeViewModel
    {
        int PopCount { get; set; }

        ReactiveCommand Navigate { get; }

        ReactiveCommand<Unit, Unit> PopPages { get; }
    }
}
