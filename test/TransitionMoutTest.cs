using System;
using Xunit;
using Bunit;
using Bunit.Mocking.JSInterop;
using static Bunit.ComponentParameterFactory;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using System.Collections.Generic;

namespace Skclusive.Transition.Component.Test
{
    public class TransitionMoutTests : TestContext
    {
        [Fact]
        public void ShouldTransitionOnMountWithAppear()
        {

           bool onEnterCalled = false;

           var cut = RenderComponent<Component.Transition>(parameters =>
               parameters
               .Add(c => c.Appear, true)
               .Add(c => c.In, true)
               .Add(c => c.Timeout, 0)
               .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
               {
                   __builder.OpenElement(1, "div");
                   __builder.AddElementReferenceCapture(2, (__value) =>
                   {
                       context.RefBack.Current = __value;
                   });
                   __builder.AddContent(3, "hello");
                   __builder.CloseElement();
               }))
               .Add(c => c.OnEnter, (args) => {
                   onEnterCalled = true;
               })
           );

           TransitionState state = cut.Instance.Current;

           Assert.Equal(TransitionState.Entered, state);

          // cut.WaitForState(() => cut.Instance.Current == TransitionState.Entered, TimeSpan.FromMilliseconds(10));

           IElement div = cut.Find("div");

           Assert.NotNull(div);

           Assert.True(onEnterCalled);
        }

        [Fact]
        public void ShouldEnteredOnInUpdate()
        {

           bool onEnteredCalled = false;

           var cut = RenderComponent<Component.Transition>(parameters =>
               parameters
               .Add(c => c.Timeout, 0)
               .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
               {
                   __builder.OpenElement(1, "div");
                   __builder.AddElementReferenceCapture(2, (__value) =>
                   {
                       context.RefBack.Current = __value;
                   });
                   __builder.AddContent(3, "hello");
                   __builder.CloseElement();
               }))
           );

           IElement div = cut.Find("div");

           TransitionState state = cut.Instance.Current;

           Assert.Equal(TransitionState.Exited, state);

           Assert.NotNull(div);

           Assert.False(onEnteredCalled);

           cut.SetParametersAndRender(parameters => parameters
              .Add(c => c.In, true)
              .Add(c => c.Timeout, 0)
              .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
              {
                  __builder.OpenElement(1, "div");
                  __builder.AddElementReferenceCapture(2, (__value) =>
                  {
                      context.RefBack.Current = __value;
                  });
                  __builder.AddContent(3, "hello");
                  __builder.CloseElement();
              }))
              .Add(c => c.OnEntered, (args) => {
                  onEnteredCalled = true;
              })
           );

           //TransitionState newstate = cut.Instance.Current;

           cut.WaitForState(() => onEnteredCalled, TimeSpan.FromMilliseconds(10));

           Assert.True(onEnteredCalled);

           //Assert.Equal(TransitionState.Entered, newstate);
        }

        [Fact]
        public void ShouldMountUnmountIfNoTimeouts()
        {

           var cut = RenderComponent<Component.Transition>(parameters =>
               parameters
               .Add(c => c.In, true)
               .Add(c => c.EnterTimeout, 0)
               .Add(c => c.ExitTimeout, 0)
               .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
               {
                   __builder.OpenElement(1, "div");
                   __builder.AddElementReferenceCapture(2, (__value) =>
                   {
                       context.RefBack.Current = __value;
                   });
                   __builder.AddContent(3, "hello");
                   __builder.CloseElement();
               }))
           );

           IElement div = cut.Find("div");

           TransitionState state = cut.Instance.Current;

           Assert.Equal(TransitionState.Entered, state);

           Assert.NotNull(div);

           bool onExitedCalled = false;

           cut.SetParametersAndRender(parameters => parameters
              .Add(c => c.In, false)
              .Add(c => c.Timeout, 0)
              .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
              {
                  __builder.OpenElement(1, "div");
                  __builder.AddElementReferenceCapture(2, (__value) =>
                  {
                      context.RefBack.Current = __value;
                  });
                  __builder.AddContent(3, "hello");
                  __builder.CloseElement();
              }))
              .Add(c => c.OnExited, (f) => {
                  onExitedCalled = true;
              })
           );

           //TransitionState newstate = cut.Instance.Current;

           cut.WaitForState(() => onExitedCalled, TimeSpan.FromMilliseconds(10));

           Assert.True(onExitedCalled);

           //Assert.Equal(TransitionState.Entered, newstate);
        }

        [Fact]
        public void ShouldEnterTimeoutUsedForAppearingTimeout()
        {
            var calledBeforeEntered = false;

            //DisposableComponentBase.SetTimeout(() => {
            //    calledBeforeEntered = true;
            //}, 100);

            DisposableComponentBase.SetTimeout(() => {
                calledBeforeEntered = true;
            }, 10);

            var cut = RenderComponent<Component.Transition>(parameters =>
                parameters
                .Add(c => c.In, true)
                .Add(c => c.Appear, true)
                .Add(c => c.EnterTimeout, 310)
                .Add(c => c.ExitTimeout, 100)
                .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
                {
                    __builder.OpenElement(1, "div");
                    __builder.CloseElement();
                }))
            );

            IElement div = cut.Find("div");

            TransitionState state = cut.Instance.Current;

            Assert.Equal(TransitionState.Entering, state);

            Assert.NotNull(div);

            bool onEnteredCalled = false;

            cut.SetParametersAndRender(parameters => parameters
               .Add(c => c.OnEntered, (args) => {
                   onEnteredCalled = calledBeforeEntered;
               })
            );

            cut.WaitForState(() => onEnteredCalled, TimeSpan.FromMilliseconds(351));

            Assert.True(onEnteredCalled);

            TransitionState newstate = cut.Instance.Current;

            Assert.Equal(TransitionState.Entered, newstate);
        }

        [Fact]
        public void ShouldUseAppearingTimeout()
        {

            var cut = RenderComponent<Component.Transition>(parameters =>
                parameters
                .Add(c => c.In, true)
                .Add(c => c.Appear, true)
                .Add(c => c.AppearTimeout, 310)
                .Add(c => c.EnterTimeout, 410)
                .Add(c => c.ExitTimeout, 100)
                .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
                {
                    __builder.OpenElement(1, "div");
                    __builder.CloseElement();
                }))
            );

            IElement div = cut.Find("div");

            TransitionState state = cut.Instance.Current;

            Assert.Equal(TransitionState.Entering, state);

            Assert.NotNull(div);

            bool onEnteredCalled = false;

            var calledBeforeEntered = false;

            DisposableComponentBase.SetTimeout(() => {
                calledBeforeEntered = true;
            }, 350);

            cut.SetParametersAndRender(parameters => parameters
               .Add(c => c.OnEntered, (args) => {
                   onEnteredCalled = !calledBeforeEntered;
               })
            );

            cut.WaitForState(() => onEnteredCalled, TimeSpan.FromMilliseconds(1550));

            Assert.True(onEnteredCalled);

            TransitionState newstate = cut.Instance.Current;

            Assert.Equal(TransitionState.Entered, newstate);
        }

        [Fact]
        public void ShouldEnterTimeout()
        {

            var cut = RenderComponent<Component.Transition>(parameters =>
                parameters
                .Add(c => c.Timeout, 100)
                .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
                {
                    __builder.OpenElement(1, "div");
                    __builder.CloseElement();
                }))
            );

            IElement div = cut.Find("div");

            TransitionState state = cut.Instance.Current;

            Assert.Equal(TransitionState.Exited, state);

            Assert.NotNull(div);

            bool onEnterCalled = false;

            bool onEnteringCalled = false;

            bool onEnteredCalled = false;

            List<string> ordereds = new List<string>();

            cut.SetParametersAndRender(parameters => parameters
               .Add(c => c.In, true)
                .Add(c => c.OnEnter, (args) => {
                   onEnterCalled = true;
                   ordereds.Add("OnEnter");
               })
                .Add(c => c.OnEntering, (args) => {
                   onEnteringCalled = true;
                   ordereds.Add("OnEntering");
               })
               .Add(c => c.OnEntered, (args) => {
                   onEnteredCalled = true;
                   ordereds.Add("OnEntered");
               })
            );

            cut.WaitForState(() => onEnteredCalled, TimeSpan.FromMilliseconds(311));

            Assert.True(onEnterCalled);
            Assert.True(onEnteringCalled);
            Assert.True(onEnteredCalled);

            Assert.Equal(3, ordereds.Count);
            Assert.Equal("OnEnter", ordereds[0]);
            Assert.Equal("OnEntering", ordereds[1]);
            Assert.Equal("OnEntered", ordereds[2]);

            TransitionState newstate = cut.Instance.Current;

            Assert.Equal(TransitionState.Entered, newstate);
        }

        [Fact]
        public void ShouldEnterStateOrder()
        {

            var cut = RenderComponent<Component.Transition>(parameters =>
                parameters
                .Add(c => c.Timeout, 100)
                .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
                {
                    __builder.OpenElement(1, "div");
                    __builder.CloseElement();
                }))
            );

            IElement div = cut.Find("div");

            TransitionState state = cut.Instance.Current;

            Assert.Equal(TransitionState.Exited, state);

            Assert.NotNull(div);

            int callCount = 0;

            List<TransitionState> ordereds = new List<TransitionState>();

            cut.SetParametersAndRender(parameters => parameters
               .Add(c => c.In, true)
                .Add(c => c.OnEnter, (args) => {
                   callCount++;
                   ordereds.Add(cut.Instance.Current);
               })
                .Add(c => c.OnEntering, (args) => {
                    callCount++;
                   ordereds.Add(cut.Instance.Current);
               })
               .Add(c => c.OnEntered, (args) => {
                    callCount++;
                   ordereds.Add(cut.Instance.Current);
               })
            );

            cut.WaitForState(() => callCount == 3, TimeSpan.FromMilliseconds(311));

            Assert.Equal(3, callCount);

            Assert.Equal(3, ordereds.Count);
            Assert.Equal(TransitionState.Enter, ordereds[0]);
            Assert.Equal(TransitionState.Entering, ordereds[1]);
            Assert.Equal(TransitionState.Entered, ordereds[2]);
        }

        [Fact]
        public void ShouldExitTimeout()
        {

            var cut = RenderComponent<Component.Transition>(parameters =>
                parameters
                .Add(c => c.In, true)
                .Add(c => c.Timeout, 100)
                .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
                {
                    __builder.OpenElement(1, "div");
                    __builder.CloseElement();
                }))
            );

            IElement div = cut.Find("div");

            TransitionState state = cut.Instance.Current;

            Assert.Equal(TransitionState.Entered, state);

            Assert.NotNull(div);

            bool onExitCalled = false;

            bool onExitingCalled = false;

            bool onExitedCalled = false;

            List<string> ordereds = new List<string>();

            cut.SetParametersAndRender(parameters => parameters
               .Add(c => c.In, false)
                .Add(c => c.OnExit, (f) => {
                   onExitCalled = true;
                   ordereds.Add("OnExit");
               })
                .Add(c => c.OnExiting, (f) => {
                   onExitingCalled = true;
                   ordereds.Add("OnExiting");
               })
               .Add(c => c.OnExited, (f) => {
                   onExitedCalled = true;
                   ordereds.Add("OnExited");
               })
            );

            cut.WaitForState(() => onExitedCalled, TimeSpan.FromMilliseconds(311));

            Assert.True(onExitCalled);
            Assert.True(onExitingCalled);
            Assert.True(onExitedCalled);

            Assert.Equal(3, ordereds.Count);
            Assert.Equal("OnExit", ordereds[0]);
            Assert.Equal("OnExiting", ordereds[1]);
            Assert.Equal("OnExited", ordereds[2]);

            TransitionState newstate = cut.Instance.Current;

            Assert.Equal(TransitionState.Exited, newstate);
        }

        [Fact]
        public void ShouldStateOrder()
        {

            var cut = RenderComponent<Component.Transition>(parameters =>
                parameters
                .Add(c => c.In, true)
                .Add(c => c.Timeout, 100)
                .Add(c => c.ChildContent, (RenderFragment<ITransitionContext>)((context) => (__builder) =>
                {
                    __builder.OpenElement(1, "div");
                    __builder.CloseElement();
                }))
            );

            IElement div = cut.Find("div");

            TransitionState state = cut.Instance.Current;

            Assert.Equal(TransitionState.Entered, state);

            Assert.NotNull(div);

            int callCount = 0;

            List<TransitionState> ordereds = new List<TransitionState>();

            cut.SetParametersAndRender(parameters => parameters
               .Add(c => c.In, false)
                .Add(c => c.OnExit, (f) => {
                   callCount++;
                   ordereds.Add(cut.Instance.Current);
               })
                .Add(c => c.OnExiting, (f) => {
                    callCount++;
                   ordereds.Add(cut.Instance.Current);
               })
               .Add(c => c.OnExited, (f) => {
                    callCount++;
                   ordereds.Add(cut.Instance.Current);
               })
            );

            cut.WaitForState(() => callCount == 3, TimeSpan.FromMilliseconds(311));

            Assert.Equal(3, callCount);

            Assert.Equal(3, ordereds.Count);
            Assert.Equal(TransitionState.Exit, ordereds[0]);
            Assert.Equal(TransitionState.Exiting, ordereds[1]);
            Assert.Equal(TransitionState.Exited, ordereds[2]);
        }
    }
}
