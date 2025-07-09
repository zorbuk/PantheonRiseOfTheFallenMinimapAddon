using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PantheonRiseOfTheFallenMinimapAddon
{
    public class MinimapPin
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Label { get; set; }

        public MinimapPin() { }

        public MinimapPin(int x, int y, string label)
        {
            X = x;
            Y = y;
            Label = label;
        }

        public override string ToString()
        {
            return $"&pin[]={X}.{Y}.{Uri.EscapeDataString(Label)}";
        }
    }
}
