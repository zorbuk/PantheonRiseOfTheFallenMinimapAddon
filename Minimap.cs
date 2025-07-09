using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;

namespace PantheonRiseOfTheFallenMinimapAddon
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

        private readonly List<MinimapPin> extraPins = new();

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
            List<MinimapPin> pins = new(extraPins)
            {
                new MinimapPin(x, y, "👤Player")
            };
            string url = minimapUrlBuilder.GetUrl(mapId, zoom, x, y, pins);
            instance.Source = new Uri(url);
        }

        public bool AddPin(string name, int x, int y)
        {
            if (extraPins.Any(p => p.Label.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return false;

            extraPins.Add(new MinimapPin
            {
                Label = name,
                X = x,
                Y = y
            });

            UpdateMiniMap();
            return true;
        }

        public bool RemovePin(string name)
        {
            var pin = extraPins.FirstOrDefault(p => p.Label.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (pin != null)
            {
                extraPins.Remove(pin);

                UpdateMiniMap();
                return true;
            }
            return false;
        }

        public void SetExtraPins(IEnumerable<MinimapPin> pins)
        {
            extraPins.Clear();
            extraPins.AddRange(pins);
        }

        public void SavePinsToFile(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            File.WriteAllText(filePath, JsonSerializer.Serialize(extraPins, options));
        }

        public void LoadPinsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                string json = File.ReadAllText(filePath);
                var loadedPins = JsonSerializer.Deserialize<List<MinimapPin>>(json);
                if (loadedPins != null)
                {
                    extraPins.Clear();
                    extraPins.AddRange(loadedPins);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading pins from JSON: " + ex.Message);
            }
        }
    }
}
