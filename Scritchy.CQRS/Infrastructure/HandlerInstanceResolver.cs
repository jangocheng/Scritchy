﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scritchy.CQRS.Infrastructure
{
    public class HandlerInstanceResolver : IHandlerInstanceResolver
    {
        IEventStore eventsource;
        HandlerRegistry handlerregistry;
        Func<Type, object> LoadHandler;

        public HandlerInstanceResolver(IEventStore eventsource,HandlerRegistry handlerregistry,Func<Type,object> LoadHandler)
        {
            this.eventsource = eventsource;
            this.handlerregistry = handlerregistry;
            this.LoadHandler = LoadHandler;
        }

        public void ApplyEventsToInstance(object instance, IEnumerable<object> events)
        {
            var instancetype = instance.GetType();
            foreach (var evt in eventsource.EventsForInstance(instance))
            {
                this.handlerregistry[instancetype, evt.GetType()](instance, evt);
            }
        }


        public AR LoadARSnapshot(Type t, string Id)
        {
            var ar = Activator.CreateInstance(t) as AR;
            ar.Id = Id;
            ar.Registry = handlerregistry;
            var events = eventsource.EventsForInstance(ar);
            ApplyEventsToInstance(ar, events);
            return ar;
        }

        public object ResolveHandlerFromType(Type t)
        {
            return LoadHandler(t);
        }
    }
}