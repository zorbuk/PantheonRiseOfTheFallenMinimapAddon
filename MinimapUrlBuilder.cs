using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PantheonRiseOfTheFallenMinimapAddon
{
    public class MinimapUrlBuilder
    {
        public string BaseUrl { get; set; } = "https://shalazam.info/";

        public string GetUrl(int mapId, int zoom, int x, int y, IEnumerable<MinimapPin> pins)
        {
            var sb = new StringBuilder();
            sb.Append($"{BaseUrl}maps/{mapId}?zoom={zoom}&x={x}&y={y}");

            foreach (var pin in pins)
            {
                sb.Append(pin);
            }

            return sb.ToString();
        }
    }
}
