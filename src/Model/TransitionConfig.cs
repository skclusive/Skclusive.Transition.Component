using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using System;

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
        public int Timeout { set; get; }

        [Parameter]
        public int? AppearTimeout { set; get; }

        [Parameter]
        public int? EnterTimeout { set; get; }

        [Parameter]
        public int? ExitTimeout { set; get; }

        [Parameter]
        public EventCallback<(IReference, bool)> OnEnter { set; get; }

        [Parameter]
        public EventCallback<(IReference, bool)> OnEntering { set; get; }

        [Parameter]
        public EventCallback<(IReference, bool)> OnEntered { set; get; }

        [Parameter]
        public EventCallback<IReference> OnExit { set; get; }

        [Parameter]
        public EventCallback<IReference> OnExiting { set; get; }

        [Parameter]
        public EventCallback<IReference> OnExited { set; get; }

        [Parameter]
        public RenderFragment<ITransitionContext> ChildContent { get; set; }
    }
}
