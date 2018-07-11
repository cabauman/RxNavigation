using System;
using XamFormsRxRouting.Interfaces;

namespace XamFormsRxRouting.Modules
{
    public class HomeViewModel : BaseViewModel, IHomeViewModel, IPageViewModel
    {
        public HomeViewModel(IViewStackService viewStackService)
            : base(viewStackService)
        {
        }

        public string Id => nameof(HomeViewModel);
    }
}
