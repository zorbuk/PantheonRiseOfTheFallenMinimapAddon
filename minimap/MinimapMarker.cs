using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PantheonRiseOfTheFallenMinimapAddon.minimap
{
    public class MinimapMarker
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int MapId { get; set; }
        public string Label { get; set; }

        public MinimapMarker() { }

        public MinimapMarker(int x, int y, string label, int mapId)
        {
            X = x;
            Y = y;
            Label = label;
            MapId = mapId;
        }

        public override string ToString()
        {
            return $"&pin[]={X}.{Y}.{Uri.EscapeDataString(Label)}";
        }
    }
}
