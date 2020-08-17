using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using System;
using System.Threading.Tasks;

namespace Skclusive.Transition.Component
{
    public class TransitionComponent : TransitionConfig
    {
        protected TransitionState Initial { set; get; } = TransitionState.None;

        public TransitionState Current { set; get; } = TransitionState.None;

        protected TransitionState Next { set; get; } = TransitionState.None;

        public IReference RefBack { get; protected set; } = new Reference();

        protected int TimeoutAppear => AppearTimeout ?? EnterTimeout ?? Timeout;

        protected int TimeoutEnter => EnterTimeout ?? Timeout;

        protected int TimeoutExit => ExitTimeout ?? Timeout;

        [CascadingParameter]
        public ITransitionGroupContext GroupContext { get; set; }

        protected bool? PrevIn { set; get; }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            PrevIn = In;

            await base.SetParametersAsync(parameters);
        }

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
                if (Current == TransitionState.Unmounted)
                {
                    Current = TransitionState.Exited;
                }
            }
        }

        protected override Task OnAfterMountAsync()
        {
            return UpdateState(mounting: true, Next);
        }

        protected override Task OnAfterUpdateAsync()
        {
            var next = TransitionState.None;

            if (PrevIn != In)
            {
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
            }

            return UpdateState(mounting: false, next);
        }

        protected async Task UpdateState(bool mounting, TransitionState next)
        {
            // var next = Next;
            if (next != TransitionState.None)
            {
                DisposeCallback();

                // Next will always be ENTERING or EXITING.

                if (next == TransitionState.Entering)
                {
                    await PerformEnter(mounting);
                }
                else
                {
                    await PerformExit();
                }
            }
            else if (UnmountOnExit && Current == TransitionState.Exited)
            {
                Current = TransitionState.Unmounted;

                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task<IDisposable> SafeStateChange(TransitionState state, Func<Task> action)
        {
            Current = state;

            var disposable = StateHasChanged(CreateTimeout(async () =>
            {
                await action();

                await InvokeAsync(StateHasChanged);

            }, 0), immediate: false);

            await InvokeAsync(StateHasChanged);

            return disposable;

            // return Task.FromResult(disposable);
        }

        protected async Task PerformEnter(bool mounting)
        {
            var appearing = GroupContext?.IsMounting ?? mounting;

            if (!mounting && !(Enter.HasValue && Enter.Value))
            {
                Current = TransitionState.Entered;

                await RunOnEntered(appearing: false);

                await InvokeAsync(StateHasChanged);

                return;
            }

            Current = TransitionState.Enter;

            await OnEnter.InvokeAsync((RefBack, appearing));

            await InvokeAsync(StateHasChanged);

            await SafeStateChange(TransitionState.Entering, async () =>
            {
                await OnEntering.InvokeAsync((RefBack, appearing));

                await OnTransitionEnd(async () =>
                {
                    Current = TransitionState.Entered;

                    await RunOnEntered(appearing);

                    await InvokeAsync(StateHasChanged);

                }, appearing ? TimeoutAppear : TimeoutEnter);
            });
        }

        protected async Task PerformExit()
        {
            if (!(Exit.HasValue && Exit.Value))
            {
                Current = TransitionState.Exited;

                await RunOnExited();

                await InvokeAsync(StateHasChanged);

                return;
            }

            Current = TransitionState.Exit;

            // await InvokeAsync(StateHasChanged);

            await OnExit.InvokeAsync(RefBack);

            await SafeStateChange(TransitionState.Exiting, async () =>
            {
                await OnExiting.InvokeAsync(RefBack);

                await OnTransitionEnd(async () =>
                {
                    Current = TransitionState.Exited;

                    await RunOnExited();

                    await InvokeAsync(StateHasChanged);

                }, TimeoutExit);
            });
        }

        protected Task RunOnEntered(bool appearing)
        {
            return OnEntered.InvokeAsync((RefBack, appearing));
        }

        protected Task RunOnExited()
        {
            return OnExited.InvokeAsync(RefBack);
        }

        protected IDisposable TransitionDisposal { set; get; }

        protected ITransitionContext Context  => new TransitionContextBuilder()
            .WithState(Current)
            .WithRefBack(RefBack)
            .Build();

        protected Task OnTransitionEnd(Func<Task> action, int delay)
        {
            DisposeCallback();

            var completionSource = new TaskCompletionSource<object>();

            TransitionDisposal = SetTimeout(async () =>
            {
                await action();

                completionSource.SetResult(null);

            }, delay);

            return completionSource.Task;
        }

        protected void DisposeCallback()
        {
            TransitionDisposal?.Dispose();

            TransitionDisposal = null;
        }

        protected override void Dispose()
        {
            DisposeCallback();

            // OnEntered = null;
            // OnExited = null;
        }
    }
}
