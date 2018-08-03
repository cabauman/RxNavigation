using ReactiveUI;
using System;
using XamFormsRxRouting.Navigation.Interfaces;

namespace XamFormsRxRouting.Common
{
    public class BaseViewModel : ReactiveObject, ISupportsActivation
    {
        public BaseViewModel(IViewStackService viewStackService)
        {
            ViewStackService = viewStackService;
        }

        public ViewModelActivator Activator => new ViewModelActivator();

        protected IViewStackService ViewStackService { get; }
    }
}
