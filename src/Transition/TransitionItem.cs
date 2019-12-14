using Microsoft.AspNetCore.Components;

namespace Skclusive.Transition.Component
{
    public class TransitionItem
    {
        public string Name { get; set; }

        public RenderFragment<ITransitionGroupContext> Template { get; set; }
    }
}
