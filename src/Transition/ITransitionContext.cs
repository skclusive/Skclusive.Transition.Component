using System;
using System.Collections.Generic;
using System.Linq;
using Skclusive.Core.Component;

namespace Skclusive.Transition.Component
{
    public interface ITransitionContext
    {
        TransitionState State { get; }

        IReference RefBack { get; }

        IEnumerable<string> Classes { get; }

        IEnumerable<Tuple<string, object>> Styles { get; }
    }

    public class TransitionContextBuilder
    {
        private class TransitionContext : ITransitionContext
        {
            public TransitionState State { get; internal set; }

            public IReference RefBack { get; internal set; }

            public IEnumerable<string> Classes { get; internal set; } = new List<string>();

            public IEnumerable<Tuple<string, object>> Styles { get; internal set; } = new List<Tuple<string, object>>();
        }

        private readonly TransitionContext context = new TransitionContext();

        public ITransitionContext Build()
        {
            return context;
        }

        public TransitionContextBuilder WithState(TransitionState state)
        {
            context.State = state;

            return this;
        }

        public TransitionContextBuilder WithRefBack(IReference refBack)
        {
            context.RefBack = refBack;

            return this;
        }

        public TransitionContextBuilder WithClasses(IEnumerable<string> classes)
        {
            context.Classes = classes.ToList();

            return this;
        }

        public TransitionContextBuilder WithStyles(IEnumerable<Tuple<string, object>> styles)
        {
            context.Styles = styles.ToList();

            return this;
        }

        public TransitionContextBuilder With(ITransitionContext context)
        {
            WithState(context.State)
            .WithRefBack(context.RefBack)
            .WithClasses(context.Classes)
            .WithStyles(context.Styles);

            return this;
        }
    }
}
