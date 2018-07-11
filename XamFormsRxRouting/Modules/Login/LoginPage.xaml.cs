using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamFormsRxRouting.Modules
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
