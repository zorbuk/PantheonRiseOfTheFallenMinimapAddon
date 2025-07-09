using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace PantheonRiseOfTheFallenMinimapAddon
{
    /*

        Pantheon: Rise of the fallen - Minimap

        [ES]

        Creado por https://github.com/zorbuk
        X https://x.com/ChiquitinOp

        Un minimapa en este juego es algo que se agradece. Eres libre de usarlo o no.
        Se puede usar en otra pantalla o como overlay encima del juego. :)

        [EN]

        Created by https://github.com/zorbuk
        X https://x.com/ChiquitinOp

        A minimap in this game is a welcome addition. Feel free to use it or not.
        It can be used on another screen or as an overlay on top of the game. :)

        Credits:
            shalazam - thanks for the minimap.

     */

    public partial class Form : System.Windows.Forms.Form
    {
        private Minimap _minimap;

        private MenuStrip menuStrip;
        private ToolStripLabel currentMapLabel;
        private ToolStripLabel zoomLabel;

        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private static IntPtr HWND_MESSAGE = new IntPtr(-3);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        public Form()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _minimap = new Minimap();
            _minimap.LoadPinsFromFile("./pines.json");

            InitializeMinimap();
            InitializeMenu();

            Controls.Add(_minimap.GetInstance());
            _minimap.SetPosition(0, 0);

            TopMost = true;
            AddClipboardFormatListener(this.Handle);
        }

        public void InitializeMinimap()
        {
            _minimap.GetInstance().Dock = DockStyle.Fill;
        }

        private void InitializeMenu()
        {
            menuStrip = new MenuStrip();
            var mapMenu = new ToolStripMenuItem("Maps");

            var maps = new Dictionary<int, string>
            {
                { 1, "Kingsreach" },
                { 2, "Halnir Cave" },
                { 3, "Goblin Cave" }
            };

            foreach (var kvp in maps)
            {
                var mapItem = new ToolStripMenuItem(kvp.Value)
                {
                    Tag = kvp.Key
                };

                mapItem.Click += (s, e) =>
                {
                    _minimap.mapId = kvp.Key;
                    UpdateCurrentMapLabel(kvp.Value);
                    _minimap.SetPosition(_minimap.x, _minimap.y);
                };

                mapMenu.DropDownItems.Add(mapItem);
            }

            var zoomOutButton = new ToolStripButton("🔍➖");
            zoomOutButton.Click += (s, e) =>
            {
                _minimap.ZoomOut();
                UpdateZoomLabel();
                _minimap.SetPosition(_minimap.x, _minimap.y);
            };

            var zoomInButton = new ToolStripButton("🔍➕");
            zoomInButton.Click += (s, e) =>
            {
                _minimap.ZoomIn();
                UpdateZoomLabel();
                _minimap.SetPosition(_minimap.x, _minimap.y);
            };

            zoomLabel = new ToolStripLabel($"🔍 {_minimap.Zoom}") { Padding = new Padding(10, 0, 0, 0) };
            currentMapLabel = new ToolStripLabel($"🗺️ {maps[_minimap.mapId]}") { Alignment = ToolStripItemAlignment.Right, Padding = new Padding(10, 0, 0, 0) };

            var optionsMenu = new ToolStripMenuItem("Options");
            var addPinButton = new ToolStripMenuItem("📍 Add Pin");
            addPinButton.Click += (s, e) =>
            {
                string? name = Microsoft.VisualBasic.Interaction.InputBox(
                    "Pin Name:",
                    "Save",
                    "New Pin");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    if(_minimap.AddPin(name, _minimap.x, _minimap.y))
                    {
                        MessageBox.Show("Added pin.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };
            var removePinButton = new ToolStripMenuItem("🗑️ Remove Pin");
            removePinButton.Click += (s, e) =>
            {
                string? name = Microsoft.VisualBasic.Interaction.InputBox(
                    "Pin Name:",
                    "Remove",
                    "");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (_minimap.RemovePin(name))
                    {
                        MessageBox.Show("Removed pin.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            optionsMenu.DropDownItems.Add(addPinButton);
            optionsMenu.DropDownItems.Add(removePinButton);

            menuStrip.Items.Add(mapMenu);
            menuStrip.Items.Add(optionsMenu);
            menuStrip.Items.Add(zoomOutButton);
            menuStrip.Items.Add(zoomInButton);
            menuStrip.Items.Add(zoomLabel);
            menuStrip.Items.Add(currentMapLabel);

            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
        }

        private void UpdateZoomLabel() => zoomLabel.Text = $"🔍 {_minimap.Zoom}";
        private void UpdateCurrentMapLabel(string name) => currentMapLabel.Text = $"🗺️ {name}";

        private void HandleJumpLoc(string text)
        {
            var matches = Regex.Matches(text, @"([\d]+(?:\.\d+)?)");

            if (matches.Count >= 3)
            {
                int x = (int)float.Parse(matches[0].Value, CultureInfo.InvariantCulture);
                int y = (int)float.Parse(matches[2].Value, CultureInfo.InvariantCulture);
                _minimap.SetPosition(x, y);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        string text = Clipboard.GetText();
                        if (text.StartsWith("/jumploc"))
                        {
                            HandleJumpLoc(text);
                        }
                    }
                }
                catch { }
            }

            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _minimap.SavePinsToFile("./pines.json");
            base.OnFormClosing(e);
        }
    }
}
