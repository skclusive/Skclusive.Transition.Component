using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skclusive.Transition.Component
{
    public class TransitionGroupComponent : DisposableComponentBase, ITransitionGroupContext
    {
        public TransitionGroupComponent()
        {
        }

        [Parameter]
        public bool? Appear { set; get; }

        [Parameter]
        public bool? Enter { set; get; } = true;

        [Parameter]
        public bool? Exit { set; get; } = true;

        [Parameter]
        public List<ITransitionItem> Children { set; get; } = new List<ITransitionItem>();

        public List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>> PreviousItems { private set; get; } = new List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>>();

        public List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>> Items { private set; get; } = new List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>>();

        protected ITransitionGroupContext GroupContext => this;

        public bool IsMounting { get; private set; } = true;

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            PreviousItems = Items.ToList();

            await base.SetParametersAsync(parameters);

            Items = IsMounting ? GetInitialItems() : GetNextItems();
        }

        private List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>> ToItems(List<ITransitionItem> children)
        {
            return children.Select(child => Tuple.Create(new TransitionItemContextBuilder()
                .WithName(child.Name)
                .WithKey(child.Key)
                .WithIn(true)
                .WithAppear(Appear ?? child.Appear)
                .WithEnter(Enter ?? child.Enter)
                .WithExit(Exit ?? child.Exit)
                .WithOnExited(HandleExited(child))
                .Build(), child.Template)
            ).ToList();
        }

        private List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>> GetInitialItems()
        {
            return ToItems(Children);
        }

        private List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>> GetNextItems()
        {
            var nextItems = ToItems(Children);

            var mergedItems = MergeItems(PreviousItems, nextItems);

            return mergedItems;
        }

        private List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>> MergeItems(
            List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>> previousItems, List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>> nextItems)
        {
            var previousMapping = previousItems.ToDictionary(k => k.Item1.Name, k => k);
            var nextsMapping = nextItems.ToDictionary(k => k.Item1.Name, k => k);

            Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>> GetItem(string name) => nextsMapping.ContainsKey(name) ? nextsMapping[name] : previousMapping[name];

            var pendingKeys = new List<string>();

            var nextPendingKeys = new Dictionary<string, List<string>>();

            foreach(var prevous in previousItems)
            {
                var name = prevous.Item1.Name;

                if (nextsMapping.ContainsKey(name))
                {
                    if(pendingKeys.Count > 0)
                    {
                        nextPendingKeys[name] = pendingKeys.ToList();

                        pendingKeys.Clear();
                    }
                } else
                {
                    pendingKeys.Add(name);
                }
            }

            var mergedItems = new List<Tuple<ITransitionItemContext, RenderFragment<ITransitionItemContext>>>();

            foreach (var pendingKey in pendingKeys)
            {
                mergedItems.Add(GetItem(pendingKey));
            }

            foreach (var next in nextItems)
            {
                var name = next.Item1.Name;

                if (nextPendingKeys.ContainsKey(name))
                {
                    foreach(var nextPendingKey in nextPendingKeys[name])
                    {
                        mergedItems.Add(GetItem(nextPendingKey));
                    }
                }

                mergedItems.Add(GetItem(name));
            }

            return mergedItems.Select(item =>
            {
                var name = item.Item1.Name;

                var hasPrev = previousMapping.ContainsKey(name);

                var hasNext = nextsMapping.ContainsKey(name);

                var previousItem = hasPrev ? previousMapping[name] : null;

                var isLeaving = hasPrev && (!previousItem.Item1.In ?? false);

                if (hasNext && (!hasPrev || isLeaving))
                {
                    // item is new (entering)
                    return Tuple.Create(new TransitionItemContextBuilder()
                        .With(item.Item1)
                        .WithIn(true)
                        .Build(), item.Item2);
                }
                else if (!hasNext && hasPrev && !isLeaving)
                {
                    // item is old (exiting)
                    return Tuple.Create(new TransitionItemContextBuilder()
                        .With(item.Item1)
                        .WithIn(false)
                        .Build(), item.Item2);
                } else if (hasNext && hasPrev)
                {
                    // item hasn't changed transition states
                    // copy over the last transition props;
                    return Tuple.Create(new TransitionItemContextBuilder()
                    .With(item.Item1)
                    .WithIn(previousItem.Item1.In)
                    .Build(), item.Item2);
                }

                return null;

            }).Where(item => item != null).ToList();
        }

        private Action<IReference> HandleExited(ITransitionItem item)
        {
            return (IReference _ref) =>
            {
                if (Children.Any(child => child.Name == item.Name))
                {
                    return;
                }

                item.OnExited?.Invoke(_ref);

                if(Mounted)
                {
                    Items = Items.Where(it => it.Item1.Name != item.Name).ToList();

                    PreviousItems = PreviousItems.Where(it => it.Item1.Name != item.Name).ToList();

                    StateHasChanged();
                }
            };
        }

        protected override void OnAfterMount()
        {
            RunTimeout(() =>
            {
                IsMounting = false;

            }, 100);
        }

        protected override void Dispose()
        {
        }
    }
}
