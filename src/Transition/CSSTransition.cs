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
    public class CSSTransitionComponent : TransitionConfig
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

            string baseClass = null, activeClass = null, doneClass = null;

            if (hasPrefix)
            {
                baseClass = $"{EventClass}-{@event}";

                activeClass = $"{baseClass}-Active";

                doneClass = $"{baseClass}-Done";
            } else
            {
                if (Enum.TryParse($"{@event}", out TransitionEventClass baseEvent))
                {
                    baseClass = EventClasses.ContainsKey(baseEvent) ? EventClasses[baseEvent] : null;
                }

                if (Enum.TryParse($"{@event}{TransitionEventPhase.Active}", out TransitionEventClass activeEvent))
                {
                    activeClass = EventClasses.ContainsKey(activeEvent) ? EventClasses[activeEvent] : null;
                }

                if (Enum.TryParse($"{@event}{TransitionEventPhase.Done}", out TransitionEventClass doneEvent))
                {
                    doneClass = EventClasses.ContainsKey(doneEvent) ? EventClasses[doneEvent] : null;
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

            yield return _class;

            AppliedClasses[@event][phase] = _class;
        }

        protected IEnumerable<string> RemoveClasses(TransitionEvent @event)
        {
            var classes = AppliedClasses[@event];

            AppliedClasses[@event] = new Dictionary<TransitionEventPhase, string>();

            if (classes.TryGetValue(TransitionEventPhase.Base, out var baseClass))
            {
                yield return baseClass;
            }

            if (classes.TryGetValue(TransitionEventPhase.Active, out var activeClass))
            {
                yield return activeClass;
            }

            if (classes.TryGetValue(TransitionEventPhase.Done, out var doneClass))
            {
                yield return doneClass;
            }
        }

        protected void UpdateClasses(IReference refback, IEnumerable<string> adds, IEnumerable<string> removes, bool trigger = false)
        {
            _ = DomHelpers.UpdateClassesAsync(refback.Current, removes.ToList(), adds.ToList(), trigger);
        }

        protected void HandleEnter(IReference refback, bool appear)
        {
            var @event = appear ? TransitionEvent.Appear : TransitionEvent.Enter;

            var removes = RemoveClasses(TransitionEvent.Exit);

            var adds = AddClass(@event, TransitionEventPhase.Base);

            UpdateClasses(refback, adds, removes);

            OnEnter?.Invoke(refback, appear);
        }

        protected void HandleEntering(IReference refback, bool appearing)
        {
            var @event = appearing ? TransitionEvent.Appear : TransitionEvent.Enter;

            var adds = AddClass(@event, TransitionEventPhase.Active);

            UpdateClasses(refback, adds, new string[] { }, trigger: true);

            OnEntering?.Invoke(refback, appearing);
        }

        protected void HandleEntered(IReference refback, bool appeared)
        {
            var @event = appeared ? TransitionEvent.Appear : TransitionEvent.Enter;

            var removes = RemoveClasses(@event);

            var adds = AddClass(@event, TransitionEventPhase.Done);

            UpdateClasses(refback, adds, removes);

            OnEntered?.Invoke(refback, appeared);
        }

        protected void HandleExit(IReference refback)
        {
            var removes = RemoveClasses(TransitionEvent.Appear).Concat(RemoveClasses(TransitionEvent.Enter));

            var adds = AddClass(TransitionEvent.Exit, TransitionEventPhase.Base);

            UpdateClasses(refback, adds, removes);

            OnExit?.Invoke(refback);
        }

        protected void HandleExiting(IReference refback)
        {
            var adds = AddClass(TransitionEvent.Exit, TransitionEventPhase.Active);

            UpdateClasses(refback, adds, new string[] { }, trigger: true);

            OnExiting?.Invoke(refback);
        }

        protected void HandleExited(IReference refback)
        {
            var removes = RemoveClasses(TransitionEvent.Exit);

            var adds = AddClass(TransitionEvent.Exit, TransitionEventPhase.Done);

            UpdateClasses(refback, adds, removes);

            OnExited?.Invoke(refback);
        }
    }
}
