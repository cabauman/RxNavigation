using ReactiveUI;
using System;
using XamFormsRxRouting.Navigation.Interfaces;

namespace XamFormsRxRouting.Common
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
