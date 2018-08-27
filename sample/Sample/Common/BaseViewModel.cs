using ReactiveUI;
using GameCtor.RxNavigation;

namespace Sample.Common
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
