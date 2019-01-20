using System;
using System.Drawing;
using System.Reactive.Disposables;

using CoreFoundation;
using UIKit;
using Foundation;
using ReactiveUI;
using CoreGraphics;
using Sample.Modules;

namespace Sample.Native.iOS
{
    [Register("UniversalView")]
    public class UniversalView : UIView
    {
        public UIButton PushPageButton;
        public UIButton PopPageButton;
        public UIButton PresentPageButton;
        public UIButton DismissPageButton;

        public UniversalView()
        {
            Initialize();
        }

        public UniversalView(RectangleF bounds) : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.Red;

            PushPageButton = new UIButton(UIButtonType.System);
            //PushPageButton.Frame = new CGRect(25, 25, 300, 150);
            PushPageButton.SetTitle("Push Page!", UIControlState.Normal);
            PushPageButton.SetTitleShadowColor(UIColor.Black, UIControlState.Normal);
            PushPageButton.BackgroundColor = UIColor.Orange;
            PushPageButton.WidthAnchor.ConstraintEqualTo(Frame.Width).Active = true;
            PushPageButton.HeightAnchor.ConstraintEqualTo(20).Active = true;

            PopPageButton = new UIButton(UIButtonType.System);
            //PopPageButton.Frame = new CGRect(25, 25, 300, 150);
            PopPageButton.SetTitle("Pop Page", UIControlState.Normal);
            PopPageButton.BackgroundColor = UIColor.Orange;
            PopPageButton.WidthAnchor.ConstraintEqualTo(Frame.Width).Active = true;
            PopPageButton.HeightAnchor.ConstraintEqualTo(20).Active = true;

            PresentPageButton = new UIButton(UIButtonType.System);
            //PresentPageButton.Frame = new CGRect(25, 25, 300, 150);
            PresentPageButton.SetTitle("Present Page", UIControlState.Normal);
            PresentPageButton.BackgroundColor = UIColor.Orange;
            PresentPageButton.WidthAnchor.ConstraintEqualTo(Frame.Width).Active = true;
            PresentPageButton.HeightAnchor.ConstraintEqualTo(20).Active = true;

            DismissPageButton = new UIButton(UIButtonType.System);
            //DismissPageButton.Frame = new CGRect(25, 25, 300, 150);
            DismissPageButton.SetTitle("Dismiss Page", UIControlState.Normal);
            DismissPageButton.BackgroundColor = UIColor.Orange;
            DismissPageButton.WidthAnchor.ConstraintEqualTo(Frame.Width).Active = true;
            DismissPageButton.HeightAnchor.ConstraintEqualTo(20).Active = true;

            //UIStackView stackView = new UIStackView(new UIView[] { PushPageButton, PopPageButton, PresentPageButton, DismissPageButton });
            var stackView = new UIStackView(Frame);
            stackView.Axis = UILayoutConstraintAxis.Vertical;
            stackView.Distribution = UIStackViewDistribution.EqualSpacing;
            stackView.Alignment = UIStackViewAlignment.Center;
            stackView.Spacing = 10;

            stackView.AddArrangedSubview(PushPageButton);
            stackView.AddArrangedSubview(PopPageButton);
            stackView.AddArrangedSubview(PresentPageButton);
            stackView.AddArrangedSubview(DismissPageButton);

            stackView.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(stackView);
            stackView.CenterXAnchor.ConstraintEqualTo(CenterXAnchor).Active = true;
            stackView.CenterYAnchor.ConstraintEqualTo(CenterYAnchor).Active = true;
        }
    }

    [Register("HomeViewController")]
    public class HomeViewController : ReactiveViewController<IHomeViewModel>
    {
        private UniversalView _view;

        public UIButton PushPageButton;
        public UIButton PopPageButton;
        public UIButton PresentPageButton;
        public UIButton PresentNavigationPageButton;
        public UIButton DismissPageButton;

        public HomeViewController()
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            //View = _view = new UniversalView();
            View = new UIView();
            View.BackgroundColor = UIColor.White;

            base.ViewDidLoad();

            // Perform any additional setup after loading the view

            this.WhenActivated(
                disposables =>
                {
                    this
                        .BindCommand(ViewModel, vm => vm.PushPage, v => v.PushPageButton)
                        .DisposeWith(disposables);
                    this
                        .BindCommand(ViewModel, vm => vm.PopPages, v => v.PopPageButton)
                        .DisposeWith(disposables);
                    this
                        .BindCommand(ViewModel, vm => vm.PushModalWithoutNav, v => v.PresentPageButton)
                        .DisposeWith(disposables);
                    this
                        .BindCommand(ViewModel, vm => vm.PushModalWithNav, v => v.PresentNavigationPageButton)
                        .DisposeWith(disposables);
                    this
                        .BindCommand(ViewModel, vm => vm.PopModal, v => v.DismissPageButton)
                        .DisposeWith(disposables);
                });




            PushPageButton = new UIButton(UIButtonType.System);
            PushPageButton.Frame = new CGRect(25, 75, 300, 50);
            PushPageButton.SetTitle("Push Page!", UIControlState.Normal);
            PushPageButton.SetTitleShadowColor(UIColor.Black, UIControlState.Normal);
            PushPageButton.BackgroundColor = UIColor.Orange;
            PushPageButton.WidthAnchor.ConstraintEqualTo(View.Frame.Width).Active = true;
            PushPageButton.HeightAnchor.ConstraintEqualTo(20).Active = true;

            PopPageButton = new UIButton(UIButtonType.System);
            PopPageButton.Frame = new CGRect(25, 150, 300, 50);
            PopPageButton.SetTitle("Pop Page", UIControlState.Normal);
            PopPageButton.BackgroundColor = UIColor.Orange;
            PopPageButton.WidthAnchor.ConstraintEqualTo(View.Frame.Width).Active = true;
            PopPageButton.HeightAnchor.ConstraintEqualTo(20).Active = true;

            PresentPageButton = new UIButton(UIButtonType.System);
            PresentPageButton.Frame = new CGRect(25, 225, 300, 50);
            PresentPageButton.SetTitle("Present Page", UIControlState.Normal);
            PresentPageButton.BackgroundColor = UIColor.Orange;
            PresentPageButton.WidthAnchor.ConstraintEqualTo(View.Frame.Width).Active = true;
            PresentPageButton.HeightAnchor.ConstraintEqualTo(20).Active = true;

            PresentNavigationPageButton = new UIButton(UIButtonType.System);
            PresentNavigationPageButton.Frame = new CGRect(25, 300, 300, 50);
            PresentNavigationPageButton.SetTitle("Present Navigation Page", UIControlState.Normal);
            PresentNavigationPageButton.BackgroundColor = UIColor.Orange;
            PresentNavigationPageButton.WidthAnchor.ConstraintEqualTo(View.Frame.Width).Active = true;
            PresentNavigationPageButton.HeightAnchor.ConstraintEqualTo(20).Active = true;

            DismissPageButton = new UIButton(UIButtonType.System);
            DismissPageButton.Frame = new CGRect(25, 375, 300, 50);
            DismissPageButton.SetTitle("Dismiss Page", UIControlState.Normal);
            DismissPageButton.BackgroundColor = UIColor.Orange;
            DismissPageButton.WidthAnchor.ConstraintEqualTo(View.Frame.Width).Active = true;
            DismissPageButton.HeightAnchor.ConstraintEqualTo(20).Active = true;

            View.AddSubview(PushPageButton);
            View.AddSubview(PopPageButton);
            View.AddSubview(PresentPageButton);
            View.AddSubview(PresentNavigationPageButton);
            View.AddSubview(DismissPageButton);
        }
    }
}