using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using PantheonRiseOfTheFallenMinimapAddon.components;
using PantheonRiseOfTheFallenMinimapAddon.foms;
using PantheonRiseOfTheFallenMinimapAddon.marker;
using PantheonRiseOfTheFallenMinimapAddon.minimap;

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

    public partial class Main : System.Windows.Forms.Form
    {
        private string _markersLocation = "./markers";
        private string _defaultMarkerName = "default_markers.json";
        private Minimap _minimap;
        private MarkerManager _markerManager;

        private MenuStrip menuStrip;
        private ToolStripLabel currentMapLabel;
        private ToolStripLabel zoomLabel;
        private ToolStripLabel markerFileLabel;
        private Panel mainPanel;
        private StatusStrip statusStrip;

        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private static IntPtr HWND_MESSAGE = new IntPtr(-3);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(mainPanel);

            statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom
            };
            Controls.Add(statusStrip);

            _markerManager = new MarkerManager(_markersLocation, _defaultMarkerName);
            _minimap = new Minimap();
            _minimap.LoadMarkersFromFile(_markerManager.CurrentFilePath);
            InitializeMinimap();
            InitializeMenu();

            Control minimapControl = _minimap.GetInstance();
            minimapControl.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(minimapControl);
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

            var addMarkerButton = new ToolStripMenuItem("📍 Add Marker");
            addMarkerButton.Click += (s, e) =>
            {
                string? name = InputBox.Show(
                    "Marker Name:",
                    "Save",
                    "New Marker");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    if(_minimap.AddMarker(name, _minimap.x, _minimap.y, _minimap.mapId))
                    {
                        TopMostMessageBox.Show("Added marker.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _minimap.SaveMarkersToFile(_markerManager.CurrentFilePath);
                    }
                }
            };

            var removeMarkerButton = new ToolStripMenuItem("🗑️ Remove Marker");
            removeMarkerButton.Click += (s, e) =>
            {
                var markerLabels = _minimap.GetMarkerLabels();

                if (markerLabels.Count == 0)
                {
                    TopMostMessageBox.Show("No markers to remove.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var selector = new FormSelector("Select Marker", markerLabels))
                {
                    selector.TopMost = true;
                    selector.ShowIcon = false;
                    selector.ShowInTaskbar = false;

                    if (selector.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(selector.SelectedFile))
                    {
                        if (_minimap.RemoveMarker(selector.SelectedFile))
                        {
                            TopMostMessageBox.Show("Removed marker.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            _minimap.SaveMarkersToFile(_markerManager.CurrentFilePath);
                        }
                    }
                }
            };

            var markerMenu = new ToolStripMenuItem("Markers");

            var changeMarkerFile = new ToolStripMenuItem("📂 Load Marker File...");
            changeMarkerFile.Click += (s, e) =>
            {
                var files = _markerManager.GetAvailableMarkerFiles();
                if (files.Count == 0)
                {
                    TopMostMessageBox.Show("No marker files found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var selection = new FormSelector("Select Marker File", files);
                if (selection.ShowDialog() == DialogResult.OK && selection.SelectedFile != null)
                {
                    _markerManager.ChangeCurrentFile(selection.SelectedFile);
                    _minimap.LoadMarkersFromFile(_markerManager.CurrentFilePath);
                    UpdateMarkerFileLabel();
                }
            };

            var renameMarkerFile = new ToolStripMenuItem("✏️ Rename Marker File...");
            renameMarkerFile.Click += (s, e) =>
            {
                string? newName = InputBox.Show(
                    "New file name (with .json):", 
                    "Rename", 
                    _markerManager.CurrentFileName);

                if (!string.IsNullOrWhiteSpace(newName))
                {
                    if (_markerManager.RenameCurrentFile(newName))
                    {
                        UpdateMarkerFileLabel();
                        TopMostMessageBox.Show("File renamed successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        TopMostMessageBox.Show("Could not rename file. File may already exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            var newMarkerFile = new ToolStripMenuItem("📄 New Marker File...");
            newMarkerFile.Click += (s, e) =>
            {
                string? newFileName = InputBox.Show(
                    "Enter new file name (with .json):",
                    "Create Marker File",
                    "markers.json");

                if (!string.IsNullOrWhiteSpace(newFileName))
                {
                    if (_markerManager.CreateNewMarkerFile(newFileName))
                    {
                        UpdateMarkerFileLabel();
                        TopMostMessageBox.Show("New marker file created successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        TopMostMessageBox.Show("Could not create file. File may already exist or name is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            markerFileLabel = new ToolStripLabel($"📁 {_markerManager.CurrentFileName}") { Alignment = ToolStripItemAlignment.Right, Padding = new Padding(10, 0, 0, 0) };


            markerMenu.DropDownItems.Add(changeMarkerFile);
            markerMenu.DropDownItems.Add(newMarkerFile);
            markerMenu.DropDownItems.Add(renameMarkerFile);
            markerMenu.DropDownItems.Add(addMarkerButton);
            markerMenu.DropDownItems.Add(removeMarkerButton);

            menuStrip.Items.Add(markerMenu);
            menuStrip.Items.Add(mapMenu);
            menuStrip.Items.Add(zoomOutButton);
            menuStrip.Items.Add(zoomInButton);
            menuStrip.Items.Add(zoomLabel);
            menuStrip.Items.Add(currentMapLabel);
            menuStrip.Items.Add(markerFileLabel);

            ToolStripStatusLabel currentTime = new ToolStripStatusLabel()
            {
                Text = $"🕛 {DateTime.Now.ToString("HH:mm:ss")}"
            };

            statusStrip.Items.Add(currentTime);

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
            {
                Interval = 1000,
                Enabled = true
            };

            timer.Tick += (s, e) =>
            {
                currentTime.Text = $"🕛 {DateTime.Now.ToString("HH:mm:ss")}";
            };

            Controls.Add(menuStrip);
            Controls.Add(statusStrip);
            MainMenuStrip = menuStrip;
        }

        private void UpdateZoomLabel() => zoomLabel.Text = $"🔍 {_minimap.Zoom}";
        private void UpdateCurrentMapLabel(string name) => currentMapLabel.Text = $"🗺️ {name}";
        private void UpdateMarkerFileLabel() => markerFileLabel.Text = $"📁 {_markerManager.CurrentFileName}";

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
    }
}
