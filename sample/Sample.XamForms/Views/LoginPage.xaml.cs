using ReactiveUI;
using System.Reactive.Disposables;
using Xamarin.Forms;
using Sample.Common;

namespace Sample.Modules
{
	public partial class LoginPage : BaseContentPage<ILoginViewModel>
	{
		public LoginPage()
		{
			InitializeComponent();

            this.WhenActivated(
                disposables =>
                {
                    this
                        .Bind(ViewModel, vm => vm.Email, v => v.EmailEntry.Text)
                        .DisposeWith(disposables);
                    this
                        .Bind(ViewModel, vm => vm.Password, v => v.PasswordEntry.Text)
                        .DisposeWith(disposables);
                    this
                        .BindCommand(ViewModel, vm => vm.SignIn, v => v.SignInButton)
                        .DisposeWith(disposables);
                    this
                        .BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton)
                        .DisposeWith(disposables);
                });
		}
	}
}
