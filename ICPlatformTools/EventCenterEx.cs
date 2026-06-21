using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class EventCenterEx
    {
        public delegate void EventCenterHandler(EventData data);

        private static EventCenterEx m_instance;

        private readonly Dictionary<object, List<EventCenterHandler>> m_eventDict = new Dictionary<object, List<EventCenterHandler>>();

        private EventCenterEx()
        {
        }

        public static EventCenterEx Instance
        {
            get { return m_instance ?? (m_instance = new EventCenterEx()); }
        }

        public void Attatch(object id, EventCenterHandler handler)
        {
            List<EventCenterHandler> handlerCollection;
            if (!m_eventDict.TryGetValue(id, out handlerCollection))
            {
                handlerCollection = new List<EventCenterHandler>();
                m_eventDict.Add(id, handlerCollection);
            }
            handlerCollection.Add(handler);
        }

        public void Dettach(object id, EventCenterHandler handler)
        {
            List<EventCenterHandler> handlerCollection;
            if (m_eventDict.TryGetValue(id, out handlerCollection))
            {
                handlerCollection.Remove(handler);
            }
        }

        public void Notify(object id, EventData data = null)
        {
            List<EventCenterHandler> handlerCollection;
            if (m_eventDict.TryGetValue(id, out handlerCollection))
            {
                foreach (var handler in handlerCollection)
                {
                    handler(data ?? EventData.Empty);
                }
            }
        }
    }
}
