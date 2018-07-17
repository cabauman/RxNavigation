
using ReactiveUI;
using System.Reactive;

namespace XamFormsRxRouting.Modules
{
    public interface IHomeViewModel
    {
        int PopCount { get; set; }

        int PageIndex { get; set; }

        ReactiveCommand Navigate { get; }

        ReactiveCommand<Unit, Unit> PopPages { get; }

        ReactiveCommand<Unit, Unit> PopToNewPage { get; }
    }
}
