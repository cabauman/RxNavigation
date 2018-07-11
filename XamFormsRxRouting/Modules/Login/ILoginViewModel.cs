using ReactiveUI;
using System.Reactive;

namespace XamFormsRxRouting.Modules
{
    public interface ILoginViewModel
    {
        string Email { get; set; }

        string Password { get; set; }

        ReactiveCommand<Unit, Unit> SignIn { get; }

        ReactiveCommand<Unit, Unit> Cancel { get; }
    }
}
