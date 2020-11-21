using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using System;
using System.Threading.Tasks;

namespace Skclusive.Transition.Component
{
    public partial class Transition : TransitionConfig
    {
        public TransitionState Current { set; get; } = TransitionState.None;

        protected TransitionState Next { set; get; } = TransitionState.None;

        public IReference RefBack { get; protected set; } = new Reference();

        protected int TimeoutAppear => AppearTimeout ?? EnterTimeout ?? Timeout;

        protected int TimeoutEnter => EnterTimeout ?? Timeout;

        protected int TimeoutExit => ExitTimeout ?? Timeout;

        [CascadingParameter]
        public ITransitionGroupContext GroupContext { get; set; }

        [Parameter]
        public bool EventOnly { set; get; }

        protected bool? PrevIn { set; get; }

        private Task ExecutingTask = Task.CompletedTask;

        private bool? _In { set; get; }

        private bool _localRender;

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            var prevIn = In;

            parameters.SetParameterProperties(this);

            if (ExecutingTask != null)
            {
                await ExecutingTask;
            }

            PrevIn = prevIn;

            _In = In;

            var current = Current;

            var next = Next;

            if (!_initialized)
            {
                _initialized = true;

                var groupMounting = GroupContext?.IsMounting;

                // In the context of a TransitionGroup all enters are really appears

                var appear = groupMounting.HasValue && !groupMounting.Value ? Enter : Appear;

                var initial = TransitionState.None;

                if (_In.HasValue && _In.Value)
                {
                    if (appear.HasValue && appear.Value)
                    {
                        initial = TransitionState.Exited;
                        next = TransitionState.Entering;
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

                current = initial;
            }
            else
            {
                if (_In.HasValue && _In.Value)
                {
                    if (current == TransitionState.Unmounted)
                    {
                        current = TransitionState.Exited;
                    }
                }

                if (_In.HasValue && _In.Value)
                {
                    if (current != TransitionState.Entering && current != TransitionState.Entered)
                    {
                        next = TransitionState.Entering;
                    }
                }
                else if (current == TransitionState.Exited)
                {
                    next = TransitionState.None;
                }
                else if (current == TransitionState.Entering || current == TransitionState.Entered)
                {
                    next = TransitionState.Exiting;
                }
            }

            Current = current;

            Next = next;

            if (Mounted && (Optimized.HasValue && Optimized.Value))
            {
                await UpdateStateAsync(mounting: false);
            }
            else
            {
                _localRender = false;

                await InvokeAsync(StateHasChanged);
            }
        }

        protected override Task OnAfterMountAsync()
        {
            return UpdateStateAsync(mounting: true);
        }

        protected override Task OnAfterUpdateAsync()
        {
            return UpdateStateAsync(mounting: false);
        }

        protected Task UpdateStateAsync(bool mounting)
        {
            if (!_localRender)
            {
                ExecutingTask = UpdateStateAsync(mounting, Next);
            }

            return ExecutingTask;
        }

        private async Task UpdateStateAsync(bool mounting, TransitionState next)
        {
            if (next != TransitionState.None)
            {
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

                await LocalStateHasChanged(force: true);
            }
        }

        private async Task LocalStateHasChanged(bool force = false)
        {
            if (force || !EventOnly)
            {
                _localRender = true;

                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task PerformEnter(bool mounting)
        {
            var appearing = GroupContext?.IsMounting ?? mounting;

            if (!mounting && !(Enter.HasValue && Enter.Value))
            {
                Current = TransitionState.Entered;

                // TODO: need to check
                Next = TransitionState.None;

                await LocalStateHasChanged();

                await (OnEntered?.Invoke((RefBack, false)) ?? Task.CompletedTask);

                return;
            }

            Current = TransitionState.Enter;

            await LocalStateHasChanged(force: MountOnEnter);

            await (OnEnter?.Invoke((RefBack, appearing)) ?? Task.CompletedTask);

            Current = TransitionState.Entering;

            await LocalStateHasChanged();

            await (OnEntering?.Invoke((RefBack, appearing)) ?? Task.CompletedTask);

            await OnTransitionEnd(async () =>
            {
                Current = TransitionState.Entered;

                // TODO: need to check
                Next = TransitionState.None;

                await LocalStateHasChanged();

                await (OnEntered?.Invoke((RefBack, appearing)) ?? Task.CompletedTask);

            }, appearing ? TimeoutAppear : TimeoutEnter);
        }

        protected async Task PerformExit()
        {
            if (!(Exit.HasValue && Exit.Value))
            {
                Current = TransitionState.Exited;

                // TODO: need to check
                Next = TransitionState.None;

                await LocalStateHasChanged();

                await (OnExited?.Invoke(RefBack) ?? Task.CompletedTask);

                return;
            }

            Current = TransitionState.Exit;

            await LocalStateHasChanged();

            await (OnExit?.Invoke(RefBack) ?? Task.CompletedTask);

            Current = TransitionState.Exiting;

            await LocalStateHasChanged();

            await (OnExiting?.Invoke(RefBack) ?? Task.CompletedTask);

            await OnTransitionEnd(async () =>
            {
                Current = UnmountOnExit ? TransitionState.Unmounted : TransitionState.Exited;

                // TODO: need to check
                Next = TransitionState.None;

                await LocalStateHasChanged(force: UnmountOnExit);

                await (OnExited?.Invoke(RefBack) ?? Task.CompletedTask);

            }, TimeoutExit);
        }

        protected IDisposable TransitionDisposal { set; get; }

        protected ITransitionContext Context => new TransitionContextBuilder()
            .WithState(Current)
            .WithRefBack(RefBack)
            .Build();

        protected Task OnTransitionEnd(Func<Task> action, int delay)
        {
            var completionSource = new TaskCompletionSource<bool>();

            TransitionDisposal = SetTimeout(async () =>
            {
                await action();

                completionSource.SetResult(true);

            }, delay);

            return completionSource.Task;
        }

        protected override void Dispose()
        {
            TransitionDisposal?.Dispose();

            TransitionDisposal = null;
        }
    }
}
