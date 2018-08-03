using ReactiveUI;
using XamFormsRxRouting.Interfaces;

namespace Sample.Common
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
