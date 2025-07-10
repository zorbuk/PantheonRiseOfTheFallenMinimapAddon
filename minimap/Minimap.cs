using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;

namespace PantheonRiseOfTheFallenMinimapAddon.minimap
{
    public class Minimap
    {
        private readonly MinimapUrlBuilder minimapUrlBuilder;
        private readonly WebView2 instance;

        public int mapId = 1;
        private int zoom = 7;
        public int Zoom
        {
            get => zoom;
            private set => zoom = Math.Clamp(value, 1, 9);
        }
        public int x = 0;
        public int y = 0;

        private readonly List<MinimapMarker> extraMarkers = new();

        public Minimap()
        {
            minimapUrlBuilder = new();
            instance = new();

            instance.NavigationCompleted += async (s, e) =>
            {
                string script = "" +
                    "document.querySelector('header')?.remove();" +
                    "document.querySelector('.leaflet-bottom')?.remove();" +
                    "document.querySelector('.leaflet-control-attribution')?.remove();";
                await instance.ExecuteScriptAsync(script);
            };

            instance.EnsureCoreWebView2Async(null);
        }

        public WebView2 GetInstance() => instance;

        public void ZoomOut() => Zoom--;
        public void ZoomIn() => Zoom++;

        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;

            UpdateMiniMap();
        }

        public void UpdateMiniMap()
        {
            List<MinimapMarker> markers = new(extraMarkers)
            {
                new MinimapMarker(x, y, "👤Player", mapId)
            };

            string url = minimapUrlBuilder.GetUrl(mapId, zoom, x, y, markers.Where(marker => marker.MapId == mapId));
            instance.Source = new Uri(url);
        }

        public bool AddMarker(string name, int x, int y, int mapId)
        {
            if (extraMarkers.Any(p => p.Label.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            extraMarkers.Add(new MinimapMarker
            {
                Label = name,
                X = x,
                Y = y,
                MapId = mapId
            });

            UpdateMiniMap();
            return true;
        }

        public bool RemoveMarker(string name)
        {
            var marker = extraMarkers.FirstOrDefault(p => p.Label.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (marker != null)
            {
                extraMarkers.Remove(marker);

                UpdateMiniMap();
                return true;
            }
            return false;
        }

        public void SetExtraMarkers(IEnumerable<MinimapMarker> markers)
        {
            extraMarkers.Clear();
            extraMarkers.AddRange(markers);
        }

        public void SaveMarkersToFile(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            File.WriteAllText(filePath, JsonSerializer.Serialize(extraMarkers, options));
        }

        public void LoadMarkersFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                string json = File.ReadAllText(filePath);
                var loadedMarkers = JsonSerializer.Deserialize<List<MinimapMarker>>(json);
                if (loadedMarkers != null)
                {
                    extraMarkers.Clear();
                    extraMarkers.AddRange(loadedMarkers);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading markers from JSON: " + ex.Message);
            }
        }
    }
}
