using System;
using System.Collections.Generic;

namespace Weariness.Util
{
    public class EventBusSystem<TEnum> where TEnum : Enum
    {
        private static readonly Dictionary<TEnum, List<Delegate>> eventHandler;
        
        static EventBusSystem()
        {
            eventHandler = new();
        }
        
        public static void Clear()
        {
            eventHandler.Clear();
        }
        
        public static void Subscribe<T>(TEnum type, Action<T> action)
        {
            if (!eventHandler.TryGetValue(type, out var container))
            {
                container = new();
                eventHandler[type] = container;
            }

            container.Add(action);
        }
        
        public static void Unsubscribe<T>(TEnum type, Action<T> action)
        {
            if (eventHandler.TryGetValue(type, out var container))
            {
                container.Remove(action);
            }
        }
        
        public static void Publish<T>(TEnum type, T data)
        {
            if (eventHandler.TryGetValue(type, out var container))
            {
                foreach (var del in container)
                {
                    if (del is Action<T> action)
                    {
                        action.Invoke(data);
                    }
                    else if (del is Action actionNoData)
                    {
                        actionNoData.Invoke();
                    }
                }
            }
        }
    }
}