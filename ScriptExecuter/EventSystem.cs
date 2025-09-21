using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ScriptExecuter
{
    public class EventSystem
    {
        private readonly Dictionary<string, List<EventRegistration>> _events = new Dictionary<string, List<EventRegistration>>();

        public string RegisterEvent(string eventName, Delegate callback)
        {
            if (!_events.ContainsKey(eventName))
            {
                _events[eventName] = new List<EventRegistration>();
            }

            var registration = new EventRegistration(callback);
            _events[eventName].Add(registration);

            return registration.Guid;
        }

        public void UnregisterEvent(string guid)
        {
            foreach (var registrations in _events.Values)
            {
                registrations.RemoveAll(r => r.Guid == guid);
            }
        }

        public void TriggerEvent(string eventName, params object[] args)
        {
            if (_events.TryGetValue(eventName, out var registrations))
            {
                foreach (var registration in registrations)
                {
                    registration.Callback.DynamicInvoke(args);
                }
            }
        }

        private class EventRegistration
        {
            public string Guid { get; }
            public Delegate Callback { get; }

            public EventRegistration(Delegate callback)
            {
                Guid = GenerateGuid();
                Callback = callback;
            }

            private string GenerateGuid()
            {
                return $"{System.Guid.NewGuid():N}";
            }
        }
    }
}
