using System.Windows.Forms;

sealed class Form : System.Windows.Forms.Form
{
    internal Form()
    {
        Text = "Igneous";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
    }
}