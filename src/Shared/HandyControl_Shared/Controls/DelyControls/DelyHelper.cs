using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows.Controls;

namespace HandyControl.Controls
{
    public static class DelyHelper
    {
        public static Dictionary<string, Timer> Timers = new Dictionary<string, Timer>();

        public static Dictionary<string, IDelyControl> Changes = new Dictionary<string, IDelyControl>();

        public static void Dispose(string groupName)
        {
            if (Timers.ContainsKey(groupName))
            {
                Timers[groupName]?.Stop();
                Timers[groupName]?.Dispose();
                Timers.Remove(groupName);
            }

            if (Changes.ContainsKey(groupName))
            {
                Changes[groupName] = null;
                Changes.Remove(groupName);
            }
        }

        public static Timer InitTimer(this IDelyControl control)
        {
            Timer delyTimer;

            if (!Timers.ContainsKey(control.GroupName))
            {
                delyTimer = new Timer(control.DelyInterval);
                Timers[control.GroupName] = delyTimer;
            }
            else
            {
                delyTimer = Timers[control.GroupName];
                delyTimer.Elapsed -= Changes[control.GroupName].DelyTimer_Elapsed;
            }
             
            delyTimer.Stop();
            delyTimer.Elapsed += control.DelyTimer_Elapsed;
            Changes[control.GroupName] = control;

            return delyTimer;
        }

        /// <summary>
        /// 删除指定控件的指定事件
        /// </summary>
        /// <param name="control"></param>
        /// <param name="eventname"></param>
        public static void ClearEvent1(this Timer control)
        {
            var eventname = "Elapsed";
            var propertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;
            var fieldFlags = BindingFlags.Static | BindingFlags.NonPublic;
            var controlType = typeof(Timer);
            var propertyInfo = controlType.GetProperty("Events", propertyFlags);
            var eventHandlerList = (EventHandlerList) propertyInfo.GetValue(control, null);
            var fieldInfo = (typeof(Timer)).GetField("Event" + eventname, fieldFlags);
            var d = eventHandlerList[fieldInfo.GetValue(control)];

            if (d == null) return;

            var eventInfo = controlType.GetEvent(eventname);

            foreach (var dx in d.GetInvocationList())
                eventInfo.RemoveEventHandler(control, dx);

        }

        private static void ClearEvent(this Timer control)
        {
            var eventname = "Elapsed";
            FieldInfo fi = typeof(Control).GetField("Elapsed", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = fi.GetValue(control);
            PropertyInfo pi = control.GetType().GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList) pi.GetValue(control, null); 
            list.RemoveHandler(obj, list[obj]);
        }
    }
}
