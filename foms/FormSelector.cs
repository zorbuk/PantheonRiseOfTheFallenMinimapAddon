using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PantheonRiseOfTheFallenMinimapAddon.foms
{
    using System.Runtime.InteropServices;

    public partial class FormSelector : Form
    {
        private ListBox listBox;
        private Button okButton;

        public string? SelectedFile { get; private set; }

        public FormSelector(List<string> files)
        {
            Text = "Select Marker File";
            Size = new Size(300, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;

            listBox = new ListBox()
            {
                Dock = DockStyle.Top,
                Height = 300
            };

            listBox.Items.AddRange(files.ToArray());

            okButton = new Button()
            {
                Text = "OK",
                Dock = DockStyle.Bottom
            };

            okButton.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    SelectedFile = listBox.SelectedItem.ToString();
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };

            Controls.Add(listBox);
            Controls.Add(okButton);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ForceOnTopMost();
        }

        private void ForceOnTopMost()
        {
            SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int SWP_SHOWWINDOW = 0x0040;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);
    }
}
