using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;

namespace Skclusive.Transition.Component
{
    public interface ITransitionItem
    {
        int Key { get; }

        string Name { get; }

        bool? Appear { get; }

        bool? In { get; }

        bool? Enter { get; }

        bool? Exit { get; }

        Func<IReference, Task> OnExited { get; }

        RenderFragment<ITransitionItemContext> Template { get; }
    }

    public class TransitionItemBuilder
    {
        private class TransitionItem : ITransitionItem
        {
            public int Key { get; internal set; }

            public string Name { get; internal set; }

            public bool? Appear { get; internal set; }

            public bool? In { get; internal set; }

            public bool? Enter { get; internal set; }

            public bool? Exit { get; internal set; }

            public Func<IReference, Task> OnExited { get; internal set; }

            public RenderFragment<ITransitionItemContext> Template { get; internal set; }
        }

        private readonly TransitionItem item = new TransitionItem();

        public ITransitionItem Build()
        {
            return item;
        }

        public TransitionItemBuilder WithTemplate(RenderFragment<ITransitionItemContext> template)
        {
            item.Template = template;

            return this;
        }

        public TransitionItemBuilder WithName(string name)
        {
            item.Name = name;

            return this;
        }

        public TransitionItemBuilder WithKey(int key)
        {
            item.Key = key;

            return this;
        }

        public TransitionItemBuilder WithAppear(bool? appear)
        {
            item.Appear = appear;

            return this;
        }

        public TransitionItemBuilder WithIn(bool? xin)
        {
            item.In = xin;

            return this;
        }

        public TransitionItemBuilder WithEnter(bool? enter)
        {
            item.Enter = enter;

            return this;
        }

        public TransitionItemBuilder WithExit(bool? exit)
        {
            item.Exit = exit;

            return this;
        }

        public TransitionItemBuilder WithOnExited(Func<IReference, Task> onExited)
        {
            item.OnExited = onExited;

            return this;
        }

        public TransitionItemBuilder With(ITransitionItem item)
        {
            WithName(item.Name)
            .WithKey(item.Key)
            .WithIn(item.In)
            .WithAppear(item.Appear)
            .WithEnter(item.Enter)
            .WithExit(item.Exit)
            .WithOnExited(item.OnExited)
            .WithTemplate(item.Template);

            return this;
        }
    }
}
