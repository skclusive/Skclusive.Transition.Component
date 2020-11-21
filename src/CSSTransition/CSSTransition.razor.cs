using Microsoft.AspNetCore.Components;
using Skclusive.Core.Component;
using Skclusive.Script.DomHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skclusive.Transition.Component
{
    public partial class CSSTransition : TransitionConfig
    {
        [Inject]
        public DomHelpers DomHelpers { set; get; }

        [Parameter]
        public string EventClass { set; get; }

        [Parameter]
        public IDictionary<TransitionEventClass, string> EventClasses { set; get; } = new Dictionary<TransitionEventClass, string>();

        private IDictionary<TransitionEvent, IDictionary<TransitionEventPhase, string>> AppliedClasses { get; } = new Dictionary<TransitionEvent, IDictionary<TransitionEventPhase, string>>
        {
            { TransitionEvent.Appear, new Dictionary<TransitionEventPhase, string>() },

            { TransitionEvent.Enter, new Dictionary<TransitionEventPhase, string>() },

            { TransitionEvent.Exit, new Dictionary<TransitionEventPhase, string>() },
        };

        private static string ToString<K, V>(IDictionary<K, V> dictionary, Func<V, string> toString)
        {
            return dictionary.Aggregate(new StringBuilder(),
              (sb, kvp) => sb.AppendFormat("{0}{1} = {2}",
                           sb.Length > 0 ? ", " : "", kvp.Key, toString(kvp.Value)),
              sb => sb.ToString());
        }

        public override Task SetParametersAsync(ParameterView parameters)
        {
            return base.SetParametersAsync(parameters);
        }

        protected IDictionary<TransitionEventPhase, string> GetClassNames(TransitionEvent @event)
        {
            bool hasPrefix = !string.IsNullOrWhiteSpace(EventClass);

            string baseClass = "", activeClass = "", doneClass = "";

            if (hasPrefix)
            {
                baseClass = $"{EventClass}-{@event}";

                activeClass = $"{baseClass}-Active";

                doneClass = $"{baseClass}-Done";
            } else
            {
                if (Enum.TryParse($"{@event}", out TransitionEventClass baseEvent))
                {
                    baseClass = EventClasses.ContainsKey(baseEvent) ? EventClasses[baseEvent] : "";
                }

                if (Enum.TryParse($"{@event}{TransitionEventPhase.Active}", out TransitionEventClass activeEvent))
                {
                    activeClass = EventClasses.ContainsKey(activeEvent) ? EventClasses[activeEvent] : "";
                }

                if (Enum.TryParse($"{@event}{TransitionEventPhase.Done}", out TransitionEventClass doneEvent))
                {
                    doneClass = EventClasses.ContainsKey(doneEvent) ? EventClasses[doneEvent] : "";
                }
            }

            var classes = new Dictionary<TransitionEventPhase, string>
            {
                { TransitionEventPhase.Base, baseClass },

                { TransitionEventPhase.Active, activeClass },

                { TransitionEventPhase.Done, doneClass }
            };

            return classes;
        }

        protected IEnumerable<string> AddClass(TransitionEvent @event, TransitionEventPhase phase)
        {
            var _class = GetClassNames(@event)[phase];

            if (@event == TransitionEvent.Appear && phase == TransitionEventPhase.Done)
            {
                _class += $" {GetClassNames(TransitionEvent.Enter)[TransitionEventPhase.Done]}";
            }

            if (!string.IsNullOrWhiteSpace(_class))
            {
                foreach(var _clas in _class.Split(' '))
                    yield return _clas;
            }

            AppliedClasses[@event][phase] = _class;
        }

        protected IEnumerable<string> RemoveClasses(TransitionEvent @event)
        {
            var classes = AppliedClasses[@event];

            AppliedClasses[@event] = new Dictionary<TransitionEventPhase, string>();

            if (classes.TryGetValue(TransitionEventPhase.Base, out var baseClass) && !string.IsNullOrWhiteSpace(baseClass))
            {
                yield return baseClass;
            }

            if (classes.TryGetValue(TransitionEventPhase.Active, out var activeClass) && !string.IsNullOrWhiteSpace(activeClass))
            {
                yield return activeClass;
            }

            if (classes.TryGetValue(TransitionEventPhase.Done, out var doneClass) && !string.IsNullOrWhiteSpace(doneClass))
            {
                yield return doneClass;
            }
        }

        protected async Task UpdateClassesAsync(IReference refback, IEnumerable<string> adds, IEnumerable<string> removes, bool trigger = false)
        {
            List<string> removeClasses = removes.ToList();

            List<string> addClasses = adds.ToList();

            if (removeClasses.Count > 0 || addClasses.Count > 0)
            {
                await DomHelpers.UpdateClassesAsync(refback.Current, removeClasses, addClasses, trigger);
            }
        }

        protected async Task HandleEnterAsync((IReference, bool) args)
        {
            await (OnEnter?.Invoke(args) ?? Task.CompletedTask);

            (IReference refback, bool appear) = args;

            var @event = appear ? TransitionEvent.Appear : TransitionEvent.Enter;

            var removes = RemoveClasses(TransitionEvent.Exit);

            var adds = AddClass(@event, TransitionEventPhase.Base);

            await UpdateClassesAsync(refback, adds, removes);
        }

        protected async Task HandleEnteringAsync((IReference, bool) args)
        {
            await (OnEntering?.Invoke(args) ?? Task.CompletedTask);

            (IReference refback, bool appearing) = args;

            var @event = appearing ? TransitionEvent.Appear : TransitionEvent.Enter;

            var adds = AddClass(@event, TransitionEventPhase.Active);

            await UpdateClassesAsync(refback, adds, new string[] { }, trigger: true);
        }

        protected async Task HandleEnteredAsync((IReference, bool) args)
        {
            await (OnEntered?.Invoke(args) ?? Task.CompletedTask);

            (IReference refback, bool appeared) = args;

            var @event = appeared ? TransitionEvent.Appear : TransitionEvent.Enter;

            var removes = RemoveClasses(@event);

            var adds = AddClass(@event, TransitionEventPhase.Done);

            await UpdateClassesAsync(refback, adds, removes);
        }

        protected async Task HandleExitAsync(IReference refback)
        {
            await (OnExit?.Invoke(refback) ?? Task.CompletedTask);

            var removes = RemoveClasses(TransitionEvent.Appear).Concat(RemoveClasses(TransitionEvent.Enter));

            var adds = AddClass(TransitionEvent.Exit, TransitionEventPhase.Base);

            await UpdateClassesAsync(refback, adds, removes);
        }

        protected async Task HandleExitingAsync(IReference refback)
        {
            await (OnExiting?.Invoke(refback) ?? Task.CompletedTask);

            var adds = AddClass(TransitionEvent.Exit, TransitionEventPhase.Active);

            await UpdateClassesAsync(refback, adds, new string[] { }, trigger: true);
        }

        protected async Task HandleExitedAsync(IReference refback)
        {
            await (OnExited?.Invoke(refback) ?? Task.CompletedTask);

            var removes = RemoveClasses(TransitionEvent.Exit);

            var adds = AddClass(TransitionEvent.Exit, TransitionEventPhase.Done);

            await UpdateClassesAsync(refback, adds, removes);
        }
    }
}
