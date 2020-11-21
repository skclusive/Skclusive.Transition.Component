using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using System;
using System.Threading.Tasks;

namespace Skclusive.Transition.Component
{
    public partial class TransitionV1 : TransitionConfig
    {
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

            var initial = TransitionState.None;

            // In the context of a TransitionGroup all enters are really appears

            var appear = groupMounting.HasValue && !groupMounting.Value ? Enter : Appear;

            if (In.HasValue && In.Value)
            {
                if (appear.HasValue && appear.Value)
                {
                    initial = TransitionState.Exited;
                    Next = TransitionState.Entering;
                }
                else
                {
                    initial = TransitionState.Entered;
                }
            }
            else if (UnmountOnExit || MountOnEnter)
            {
                initial = TransitionState.Unmounted;
            }
            else
            {
                initial = TransitionState.Exited;
            }

            Current = initial;

            // Console.WriteLine($"{Name}.TransitionV1.OnInitialized: Current: {Current} Initial: {initial} Next: {Next}");
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

            // Console.WriteLine($"{Name}.TransitionV1.OnParametersSet: Current: {Current} In: {In}");
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

            // Console.WriteLine($"{Name}.TransitionV1.SafeStateChange: Current: {Current}");

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

                await (OnEntered?.Invoke((RefBack, false)) ?? Task.CompletedTask);

                await InvokeAsync(StateHasChanged);

                return;
            }

            Current = TransitionState.Enter;

            await (OnEnter?.Invoke((RefBack, appearing)) ?? Task.CompletedTask);

            await InvokeAsync(StateHasChanged);

            await SafeStateChange(TransitionState.Entering, async () =>
            {
                await (OnEntering?.Invoke((RefBack, appearing)) ?? Task.CompletedTask);

                await OnTransitionEnd(async () =>
                {
                    Current = TransitionState.Entered;

                    await (OnEntered?.Invoke((RefBack, appearing)) ?? Task.CompletedTask);

                    await InvokeAsync(StateHasChanged);

                }, appearing ? TimeoutAppear : TimeoutEnter);
            });
        }

        protected async Task PerformExit()
        {
            if (!(Exit.HasValue && Exit.Value))
            {
                Current = TransitionState.Exited;

                await (OnExited?.Invoke(RefBack) ?? Task.CompletedTask);

                await InvokeAsync(StateHasChanged);

                return;
            }

            Current = TransitionState.Exit;

            // await InvokeAsync(StateHasChanged);

            await (OnExit?.Invoke(RefBack) ?? Task.CompletedTask);

            await SafeStateChange(TransitionState.Exiting, async () =>
            {
                await (OnExiting?.Invoke(RefBack) ?? Task.CompletedTask);

                await OnTransitionEnd(async () =>
                {
                    Current = TransitionState.Exited;

                    await (OnExited?.Invoke(RefBack) ?? Task.CompletedTask);

                    await InvokeAsync(StateHasChanged);

                }, TimeoutExit);
            });
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
