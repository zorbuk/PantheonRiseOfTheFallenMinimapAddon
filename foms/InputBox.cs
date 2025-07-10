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
    public partial class InputBox : Form
    {
        private TextBox _textBox;
        private Button _okButton;
        private Button _cancelButton;

        public string ResultText { get; private set; } = "";

        public InputBox(string prompt, string title, string defaultText = "")
        {
            Text = title;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(400, 120);
            Font = SystemFonts.MessageBoxFont;

            Label label = new Label
            {
                Text = prompt,
                Location = new Point(10, 10),
                Size = new Size(380, 20)
            };

            _textBox = new TextBox
            {
                Text = defaultText,
                Location = new Point(10, 35),
                Width = 380
            };

            _okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(230, 75),
                Width = 75
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(315, 75),
                Width = 75
            };

            AcceptButton = _okButton;
            CancelButton = _cancelButton;

            Controls.Add(label);
            Controls.Add(_textBox);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _textBox.Focus();
            _textBox.SelectAll();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
                ResultText = _textBox.Text;

            base.OnFormClosing(e);
        }

        public static string? Show(string prompt, string title, string defaultText = "")
        {
            using (var form = new InputBox(prompt, title, defaultText))
            {
                var result = form.ShowDialog();
                return result == DialogResult.OK ? form.ResultText : null;
            }
        }
    }
}
