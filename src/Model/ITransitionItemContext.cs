using System;
using System.Threading.Tasks;
using Skclusive.Core.Component;

namespace Skclusive.Transition.Component
{
    public interface ITransitionItemContext
    {
        int Key { get; }

        string Name { get; }

        bool? Appear { get; }

        bool? In { get; }

        bool? Enter { get; }

        bool? Exit { get; }

        Func<IReference, Task> OnExited { get; }
    }

    public class TransitionItemContextBuilder
    {
        private class TransitionItemContext : ITransitionItemContext
        {
            public int Key { get; internal set; }

            public string Name { get; internal set; }

            public bool? Appear { get; internal set; }

            public bool? In { get; internal set; }

            public bool? Enter { get; internal set; }

            public bool? Exit { get; internal set; }

            public Func<IReference, Task> OnExited { get; internal set; }
        }

        private readonly TransitionItemContext context = new TransitionItemContext();

        public ITransitionItemContext Build()
        {
            return context;
        }

        public TransitionItemContextBuilder WithKey(int key)
        {
            context.Key = key;

            return this;
        }

        public TransitionItemContextBuilder WithName(string name)
        {
            context.Name = name;

            return this;
        }

        public TransitionItemContextBuilder WithIn(bool? xin)
        {
            context.In = xin;

            return this;
        }

        public TransitionItemContextBuilder WithAppear(bool? appear)
        {
            context.Appear = appear;

            return this;
        }

        public TransitionItemContextBuilder WithEnter(bool? enter)
        {
            context.Enter = enter;

            return this;
        }

        public TransitionItemContextBuilder WithExit(bool? exit)
        {
            context.Exit = exit;

            return this;
        }

        public TransitionItemContextBuilder WithOnExited(Func<IReference, Task> onExited)
        {
            context.OnExited = onExited;

            return this;
        }

        public TransitionItemContextBuilder With(ITransitionItemContext context)
        {
            WithName(context.Name)
            .WithKey(context.Key)
            .WithIn(context.In)
            .WithAppear(context.Appear)
            .WithEnter(context.Enter)
            .WithExit(context.Exit)
            .WithOnExited(context.OnExited);

            return this;
        }
    }
}
