using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PantheonRiseOfTheFallenMinimapAddon.minimap
{
    public class MinimapUrlBuilder
    {
        public string BaseUrl { get; set; } = "https://shalazam.info/";

        public string GetUrl(int mapId, int zoom, int x, int y, IEnumerable<MinimapMarker> markers)
        {
            var sb = new StringBuilder();
            sb.Append($"{BaseUrl}maps/{mapId}?zoom={zoom}&x={x}&y={y}");

            foreach (var marker in markers)
            {
                sb.Append(marker);
            }

            return sb.ToString();
        }
    }
}
