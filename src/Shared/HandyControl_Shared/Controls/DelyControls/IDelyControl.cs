using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Timers;

namespace HandyControl.Controls
{
    public interface IDelyControl
    {
        bool IsProcessing { get; }

        object OldValue { get; }

        object NewValue { get; }

        bool IsDely { get; set; }

        bool IsFirstChange { get; }

        double DelyInterval { get; set; }

        string GroupName { get; set; }

        void DelyTimer_Elapsed(object sender, ElapsedEventArgs e);
    }
}
