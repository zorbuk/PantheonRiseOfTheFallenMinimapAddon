using System.Runtime.InteropServices;

namespace PantheonRiseOfTheFallenMinimapAddon.components
{
    public static class TopMostMessageBox
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public static DialogResult Show(string text, string caption,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using (var topmostForm = new Form()
            {
                TopMost = true,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new System.Drawing.Point(-32000, -32000),
                Size = new System.Drawing.Size(1, 1)
            })
            {
                topmostForm.Load += (s, e) =>
                {
                    SetWindowPos(topmostForm.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
                };

                topmostForm.Show();
                topmostForm.Focus();
                return MessageBox.Show(topmostForm, text, caption, buttons, icon);
            }
        }
    }
}
