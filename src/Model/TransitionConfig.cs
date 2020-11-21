using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using System;
using System.Threading.Tasks;

namespace Skclusive.Transition.Component
{
    public class TransitionConfig : DisposableComponentBase
    {
        [Parameter]
        public string Name { get; set; } = Guid.NewGuid().ToString();

        [Parameter]
        public bool? In { set; get; }

        [Parameter]
        public bool? Appear { set; get; }

        [Parameter]
        public bool? Enter { set; get; } = true;

        [Parameter]
        public bool? Exit { set; get; } = true;

        [Parameter]
        public bool MountOnEnter { set; get; }

        [Parameter]
        public bool UnmountOnExit { set; get; }

        [Parameter]
        public bool? Optimized { set; get; }

        [Parameter]
        public int Timeout { set; get; }

        [Parameter]
        public int? AppearTimeout { set; get; }

        [Parameter]
        public int? EnterTimeout { set; get; }

        [Parameter]
        public int? ExitTimeout { set; get; }

        [Parameter]
        public Func<(IReference, bool), Task> OnEnter { set; get; }

        [Parameter]
        public Func<(IReference, bool), Task> OnEntering { set; get; }

        [Parameter]
        public Func<(IReference, bool), Task> OnEntered { set; get; }

        [Parameter]
        public Func<IReference, Task> OnExit { set; get; }

        [Parameter]
        public Func<IReference, Task> OnExiting { set; get; }

        [Parameter]
        public Func<IReference, Task> OnExited { set; get; }

        [Parameter]
        public RenderFragment<ITransitionContext> ChildContent { get; set; }
    }
}
