using System.Windows.Forms;

sealed class Form : System.Windows.Forms.Form
{
    readonly TableLayoutPanel _tableLayoutPanel = new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink
    };

    internal Form()
    {
        Text = "Igneous";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
    }
}