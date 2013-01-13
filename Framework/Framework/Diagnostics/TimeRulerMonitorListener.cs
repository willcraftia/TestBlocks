#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public sealed class TimeRulerMonitorListener : IMonitorListener
    {
        TimeRuler timeRuler;

        Dictionary<string, TimeRulerMarker> markers = new Dictionary<string, TimeRulerMarker>();

        public TimeRulerMonitorListener(TimeRuler timeRuler)
        {
            if (timeRuler == null) throw new ArgumentNullException("timeRuler");

            this.timeRuler = timeRuler;
        }

        public void CreateMarker(string name, int barIndex, Color color)
        {
            var marker = timeRuler.CreateMarker(name, barIndex, color);
            markers[marker.Name] = marker;
        }

        public void ReleaseMarker(string name)
        {
            if (!markers.ContainsKey(name)) return;

            timeRuler.ReleaseMarker(markers[name]);
            markers.Remove(name);
        }

        public void Begin(string name)
        {
            if (!markers.ContainsKey(name)) return;

            markers[name].Begin();
        }

        public void End(string name)
        {
            if (!markers.ContainsKey(name)) return;

            markers[name].End();
        }

        public void Close()
        {
            foreach (var marker in markers.Values)
                timeRuler.ReleaseMarker(marker);

            markers.Clear();
        }
    }
}
