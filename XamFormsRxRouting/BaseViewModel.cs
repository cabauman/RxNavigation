using ReactiveUI;
using System;
using XamFormsRxRouting.Interfaces;

namespace XamFormsRxRouting
{
    public class BaseViewModel : ReactiveObject
    {
        public BaseViewModel(IViewStackService viewStackService)
        {
            ViewStackService = viewStackService;
        }

        protected IViewStackService ViewStackService { get; }
    }
}
