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
        private string markersLocation = "./markers";
        private string defaultMarkerFileName = "default_markers.json";
        private Minimap minimap;
        private MarkerManager markerManager;

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

        private void Main_Load(object sender, EventArgs e)
        {
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(mainPanel);

            InitializeMinimap();
            InitializeMenu();

            TopMost = true;
            AddClipboardFormatListener(this.Handle);
        }

        private void UpdateZoomLabel() => zoomLabel.Text = $"🔍 {minimap.Zoom}";
        private void UpdateCurrentMapLabel(string name) => currentMapLabel.Text = $"🗺️ {name}";
        private void UpdateMarkerFileLabel() => markerFileLabel.Text = $"📁 {markerManager.CurrentFileName}";

        public void InitializeMinimap()
        {
            markerManager = new MarkerManager(markersLocation, defaultMarkerFileName);
            minimap = new Minimap();
            minimap.LoadMarkersFromFile(markerManager.CurrentFilePath);
            Control minimapControl = minimap.GetInstance();
            minimapControl.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(minimapControl);
            minimap.SetPosition(minimap.x, minimap.y);
        }
        private void InitializeMenu()
        {
            menuStrip = new MenuStrip();
            statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom
            };

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
                    minimap.mapId = kvp.Key;
                    UpdateCurrentMapLabel(kvp.Value);
                    minimap.SetPosition(minimap.x, minimap.y);
                };

                mapMenu.DropDownItems.Add(mapItem);
            }

            var zoomOutButton = new ToolStripButton("🔍➖");
            var zoomInButton = new ToolStripButton("🔍➕");

            zoomLabel = new ToolStripLabel($"🔍 {minimap.Zoom}") { Padding = new Padding(10, 0, 0, 0) };
            currentMapLabel = new ToolStripLabel($"🗺️ {maps[minimap.mapId]}") { Alignment = ToolStripItemAlignment.Right, Padding = new Padding(10, 0, 0, 0) };
            markerFileLabel = new ToolStripLabel($"📁 {markerManager.CurrentFileName}") { Alignment = ToolStripItemAlignment.Right, Padding = new Padding(10, 0, 0, 0) };

            var markerMenu = new ToolStripMenuItem("Markers");
            var changeMarkerFile = new ToolStripMenuItem("📂 Load Marker File...");
            var newMarkerFile = new ToolStripMenuItem("📄 New Marker File...");
            var renameMarkerFile = new ToolStripMenuItem("✏️ Rename Marker File...");
            var addMarkerButton = new ToolStripMenuItem("📍 Add Marker");
            var removeMarkerButton = new ToolStripMenuItem("🗑️ Remove Marker");

            zoomOutButton.Click += (s, e) =>
            {
                minimap.ZoomOut();
                UpdateZoomLabel();
                minimap.SetPosition(minimap.x, minimap.y);
            };
            zoomInButton.Click += (s, e) =>
            {
                minimap.ZoomIn();
                UpdateZoomLabel();
                minimap.SetPosition(minimap.x, minimap.y);
            };
            addMarkerButton.Click += (s, e) =>
            {
                string? name = InputBox.Show(
                    "Marker Name:",
                    "Save",
                    "New Marker");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    if(minimap.AddMarker(name, minimap.x, minimap.y, minimap.mapId))
                    {
                        TopMostMessageBox.Show("Added marker.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        minimap.SaveMarkersToFile(markerManager.CurrentFilePath);
                    }
                }
            };
            removeMarkerButton.Click += (s, e) =>
            {
                var markerLabels = minimap.GetMarkerLabels();

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
                        if (minimap.RemoveMarker(selector.SelectedFile))
                        {
                            TopMostMessageBox.Show("Removed marker.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            minimap.SaveMarkersToFile(markerManager.CurrentFilePath);
                        }
                    }
                }
            };
            changeMarkerFile.Click += (s, e) =>
            {
                var files = markerManager.GetAvailableMarkerFiles();
                if (files.Count == 0)
                {
                    TopMostMessageBox.Show("No marker files found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var selection = new FormSelector("Select Marker File", files);
                if (selection.ShowDialog() == DialogResult.OK && selection.SelectedFile != null)
                {
                    markerManager.ChangeCurrentFile(selection.SelectedFile);
                    minimap.LoadMarkersFromFile(markerManager.CurrentFilePath);
                    UpdateMarkerFileLabel();
                }
            };
            renameMarkerFile.Click += (s, e) =>
            {
                string? newName = InputBox.Show(
                    "New file name (with .json):", 
                    "Rename", 
                    markerManager.CurrentFileName);

                if (!string.IsNullOrWhiteSpace(newName))
                {
                    if (markerManager.RenameCurrentFile(newName))
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
            newMarkerFile.Click += (s, e) =>
            {
                string? newFileName = InputBox.Show(
                    "Enter new file name (with .json):",
                    "Create Marker File",
                    "markers.json");

                if (!string.IsNullOrWhiteSpace(newFileName))
                {
                    if (markerManager.CreateNewMarkerFile(newFileName))
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

        private void HandleJumpLoc(string text)
        {
            var matches = Regex.Matches(text, @"([\d]+(?:\.\d+)?)");

            if (matches.Count >= 3)
            {
                int x = (int)float.Parse(matches[0].Value, CultureInfo.InvariantCulture);
                int y = (int)float.Parse(matches[2].Value, CultureInfo.InvariantCulture);
                minimap.SetPosition(x, y);
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
