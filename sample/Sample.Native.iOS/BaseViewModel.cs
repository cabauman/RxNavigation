using ReactiveUI;
using RxNavigation;

namespace Sample.Native.iOS
{
    public class BaseViewModel : ReactiveObject, ISupportsActivation
    {
        private readonly ViewModelActivator _activator = new ViewModelActivator();

        public BaseViewModel(IViewStackService viewStackService)
        {
            ViewStackService = viewStackService;
        }

        public ViewModelActivator Activator => _activator;

        protected IViewStackService ViewStackService { get; }
    }
}
