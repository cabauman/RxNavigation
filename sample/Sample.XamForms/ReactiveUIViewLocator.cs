using ReactiveUI;
using System;
using GameCtor.RxNavigation;

namespace Sample
{
    public class ReactiveUIViewLocator : GameCtor.RxNavigation.IViewLocator
    {
        public IView ResolveView<T>(T viewModel, string contract = null)
        {
            return ViewLocator.Current.ResolveView(new object()) as IView;
        }

        public Type ResolveViewType<T>(T viewModel, string contract = null)
        {
            throw new NotImplementedException();
        }
    }
}
