using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using GameCtor.RxNavigation;
using NSubstitute;
using Xunit;

namespace RxNavigation.Tests
{
    public class ViewStackServiceTests : IDisposable
    {
        private IViewShell _viewShell;
        private IPageViewModel _page;
        private Subject<IPageViewModel> _pagePopped;
        private Subject<Unit> _modalPopped;

        public ViewStackServiceTests()
        {
            _viewShell = Substitute.For<IViewShell>();
            _page = Substitute.For<IPageViewModel>();
            _pagePopped = new Subject<IPageViewModel>();
            _modalPopped = new Subject<Unit>();
            _viewShell.PagePopped.Returns(_pagePopped);
            _viewShell.ModalPopped.Returns(_modalPopped);

            _viewShell
                .When(x => x.PopPage(Arg.Any<bool>()))
                .Do(_ => _pagePopped.OnNext(_page));

            _viewShell
                .When(x => x.PopModal())
                .Do(_ => _modalPopped.OnNext(Unit.Default));
        }

        public void Dispose()
        {
            _pagePopped.Dispose();
            _modalPopped.Dispose();
        }

        [Fact]
        public void Should_AddViewModelToPageStack_When_Pushed()
        {
            // Arrange
            var sut = new ViewStackService(_viewShell);

            // Act
            sut.PushPage(_page).Subscribe();

            // Assert
            sut.PageStack.FirstAsync().Wait().Count.Should().Be(1);
        }

        [Theory]
        [InlineData(null, false, true)]
        [InlineData(null, true, true)]
        [InlineData("SomeContract", true, false)]
        [InlineData("SomeContract", false, false)]
        public void Should_InvokeViewShellPushPage_When_PushPageIsInvoked(string contract, bool resetStack, bool animate)
        {
            // Arrange
            var sut = new ViewStackService(_viewShell);

            // Act
            sut.PushPage(_page, contract, resetStack, animate).Subscribe();

            // Assert
            _viewShell.Received(1).PushPage(_page, contract, resetStack, animate);
        }

        [Fact]
        public void Should_AddViewModelToModalStack_When_Pushed()
        {
            // Arrange
            var sut = new ViewStackService(_viewShell);

            // Act
            sut.PushModal(_page).Subscribe();

            // Assert
            sut.ModalStack.FirstAsync().Wait().Count.Should().Be(1);
        }

        [Fact]
        public void Should_InvokeViewShellPushModal_When_PushModalIsInvoked()
        {
            // Arrange
            var sut = new ViewStackService(_viewShell);

            // Act
            sut.PushModal(_page).Subscribe();

            // Assert
            _viewShell.Received(1).PushModal(_page, null, false);
        }

        [Fact]
        public void Should_InvokeViewShellInsertPageTwice_When_InitializingViewStackServiceWithTwoPages()
        {
            // Arrange

            // Act
            var sut = new ViewStackService(_viewShell, new[] { _page, _page });

            // Assert
            _viewShell.Received(2).InsertPage(Arg.Any<int>(), _page, string.Empty);
        }

        [Fact]
        public void Should_InvokeViewShellPopPage_When_PageIsPopped()
        {
            // Arrange
            var sut = new ViewStackService(_viewShell, new[] { _page, _page });

            // Act
            sut.PopPages().Subscribe();

            // Assert
            _viewShell.Received(1).PopPage(true);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Should_HavePageIndexPlusOnePagesRemaining_When_PopToPageIsInvoked(int pageIndex)
        {
            // Arrange
            var sut = new ViewStackService(_viewShell, new[] { _page, _page, _page, _page });

            // Act
            sut.PopToPage(pageIndex).Subscribe();

            // Assert
            int numPages = pageIndex + 1;
            sut.PageStack.FirstAsync().Wait().Count.Should().Be(numPages);
        }
    }
}
