using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using System;

namespace Skclusive.Transition.Component
{
    public class TransitionComponent : TransitionConfig
    {
        protected TransitionState Initial { set; get; } = TransitionState.None;

        protected TransitionState Current { set; get; } = TransitionState.None;

        protected TransitionState Next { set; get; } = TransitionState.None;

        public IReference RefBack { get; protected set; } = new Reference();

        protected int TimeoutAppear => AppearTimeout ?? Timeout;

        protected int TimeoutEnter => EnterTimeout ?? Timeout;

        protected int TimeoutExit => ExitTimeout ?? Timeout;

        [CascadingParameter]
        public ITransitionGroupContext GroupContext { get; set; }

        protected override void OnInitialized()
        {
            var groupMounting = GroupContext?.IsMounting;

            // In the context of a TransitionGroup all enters are really appears

            var appear = groupMounting.HasValue && !groupMounting.Value ? Enter : Appear;

            if (In.HasValue && In.Value)
            {
                if (appear.HasValue && appear.Value)
                {
                    Initial = TransitionState.Exited;
                    Next = TransitionState.Entering;
                }
                else
                {
                    Initial = TransitionState.Entered;
                }
            }
            else if (UnmountOnExit || MountOnEnter)
            {
                Initial = TransitionState.Unmounted;
            }
            else
            {
                Initial = TransitionState.Exited;
            }

            Current = Initial;
        }

        protected override void OnParametersSet()
        {
            if (In.HasValue && In.Value)
            {
                if (Initial == TransitionState.Unmounted)
                {
                    Current = TransitionState.Exited;
                }
            }
        }

        protected override void OnAfterMount()
        {
            UpdateState(mounting: true, Next);
        }

        protected override void OnAfterUpdate()
        {
            var next = TransitionState.None;

            if (In.HasValue && In.Value)
            {
                if (Current != TransitionState.Entering && Current != TransitionState.Entered)
                {
                    next = TransitionState.Entering;
                }
            }
            else if (Current == TransitionState.Entering || Current == TransitionState.Entered)
            {
                next = TransitionState.Exiting;
            }

            UpdateState(mounting: false, next);
        }

        protected void UpdateState(bool mounting, TransitionState next)
        {
            // var next = Next;
            if (next != TransitionState.None)
            {
                DisposeCallback();

                // Next will always be ENTERING or EXITING.

                if (next == TransitionState.Entering)
                {
                    PerformEnter(mounting);
                }
                else
                {
                    PerformExit();
                }
            }
            else if (UnmountOnExit && Current == TransitionState.Exited)
            {
                Current = TransitionState.Unmounted;

                StateHasChanged();
            }
        }

        protected IDisposable SafeStateChange(TransitionState state, Action action)
        {
            Current = state;

            return StateHasChanged(CreateTimeout(action, 200));
        }

        protected void PerformEnter(bool mounting)
        {
            var appearing = GroupContext?.IsMounting ?? mounting;

            if (!mounting && !(Enter.HasValue && Enter.Value))
            {
                Current = TransitionState.Entered;

                RunOnEntered(appearing: false);

                StateHasChanged();

                return;
            }

            Current = TransitionState.Enter;

            OnEnter?.Invoke(RefBack, appearing);

            SafeStateChange(TransitionState.Entering, () =>
            {
                OnEntering?.Invoke(RefBack, appearing);

                OnTransitionEnd(() =>
                {
                    Current = TransitionState.Entered;

                    RunOnEntered(appearing);

                    StateHasChanged();

                }, appearing ? TimeoutAppear : TimeoutEnter);
            });
        }

        protected void PerformExit()
        {
            if (!(Exit.HasValue && Exit.Value))
            {
                Current = TransitionState.Exited;

                RunOnExited();

                StateHasChanged();

                return;
            }

            Current = TransitionState.Exit;

            OnExit?.Invoke(RefBack);

            SafeStateChange(TransitionState.Exiting, () =>
            {
                OnExiting?.Invoke(RefBack);

                OnTransitionEnd(() =>
                {
                    Current = TransitionState.Exited;

                    RunOnExited();

                    StateHasChanged();

                }, TimeoutExit);
            });
        }

        protected void RunOnEntered(bool appearing)
        {
            OnEntered?.Invoke(RefBack, appearing);
        }

        protected void RunOnExited()
        {
            OnExited?.Invoke(RefBack);
        }

        protected IDisposable TransitionDisposal { set; get; }

        protected ITransitionContext Context  => new TransitionContextBuilder()
            .WithState(Current)
            .WithRefBack(RefBack)
            .Build();

        protected void OnTransitionEnd(Action action, int delay)
        {
            DisposeCallback();

            TransitionDisposal = SetTimeout(action, delay);
        }

        protected void DisposeCallback()
        {
            TransitionDisposal?.Dispose();

            TransitionDisposal = null;
        }

        protected override void Dispose()
        {
            DisposeCallback();

            OnEntered = null;
            OnExited = null;
        }
    }
}
