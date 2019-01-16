using System;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using GameCtor.RxNavigation;
using NSubstitute;
using Xunit;

namespace RxNavigation.Tests
{
    public class ViewStackServiceTests
    {
        private IViewShell _viewShell;
        private IPageViewModel _pageViewModel;
        private IViewStackService _sut;

        public ViewStackServiceTests()
        {
            //_viewShell = Substitute.For<IViewShell>();
            //_pageViewModel = Substitute.For<IPageViewModel>();
            //_sut = new ViewStackService(_viewShell);
        }

        [Theory]
        [InlineData(null, false, true)]
        [InlineData(null, true, true)]
        [InlineData(null, true, false)]
        [InlineData(null, false, false)]
        public void Should_AddViewModelToPageStack_When_Pushed(string contract, bool resetStack, bool animate)
        {
            // Arrange

            // Act
            _sut.PushPage(_pageViewModel, contract, resetStack, animate).Subscribe();

            // Assert
            _sut.PageStack.FirstAsync().Wait().Count.Should().Be(1);
        }

        [Fact]
        public void Should_InvokeViewShellPushPage_When_PushPageIsInvoked()
        {
            // Arrange
            var viewShell = Substitute.For<IViewShell>();
            var viewModel = Substitute.For<IPageViewModel>();
            var sut = new ViewStackService(viewShell);

            // Act
            sut.PushPage(viewModel).Subscribe();

            // Assert
            viewShell.Received(1).PushPage(viewModel, null, false, true);
        }

        [Fact]
        public void Should_AddViewModelToModalStack_When_Pushed()
        {
            // Arrange
            var viewShell = Substitute.For<IViewShell>();
            var viewModel = Substitute.For<IPageViewModel>();
            var sut = new ViewStackService(viewShell);

            // Act
            sut.PushModal(viewModel).Subscribe();

            // Assert
            sut.ModalStack.FirstAsync().Wait().Count.Should().Be(1);
        }

        [Fact]
        public void Should_InvokeViewShellPushModal_When_PushModalIsInvoked()
        {
            // Arrange
            var viewShell = Substitute.For<IViewShell>();
            var viewModel = Substitute.For<IPageViewModel>();
            var sut = new ViewStackService(viewShell);

            // Act
            sut.PushModal(viewModel).Subscribe();

            // Assert
            viewShell.Received(1).PushModal(viewModel, null, false);
        }

        [Fact]
        public void Should_EmitViewShellPoppedSignal_When_PageIsPopped()
        {
            // Arrange
            var viewShell = Substitute.For<IViewShell>();
            var viewModel = Substitute.For<IPageViewModel>();
            var popped = false;
            var sut = new ViewStackService(viewShell);

            sut.PushPage(viewModel).Subscribe();
            sut.PushPage(viewModel).Subscribe();
            viewShell.PagePopped.Returns(Observable.Return<IPageViewModel>(viewModel));
            viewShell.PagePopped.Subscribe(
                _ =>
                {
                    popped = true;
                });

            // Act
            //sut.PopPages().Subscribe();

            // Assert
            popped.Should().Be(true);
        }

        [Fact]
        public void Should_InvokeViewShellPopPage_When_PageIsPopped()
        {
            // Arrange
            var viewShell = Substitute.For<IViewShell>();
            var viewModel = Substitute.For<IPageViewModel>();
            var sut = new ViewStackService(viewShell);

            sut.PushPage(viewModel).Subscribe();
            sut.PushPage(viewModel).Subscribe();

            // Act
            sut.PopPages().Subscribe();

            // Assert
            viewShell.Received(1).PopPage(true);
        }
    }
}
