using System;
using System.Drawing;
using System.Windows.Forms;

public class SchetsEditor : Form
{
    private MenuStrip menuStrip;
    

    public SchetsEditor()
    {   
        this.ClientSize = new Size(1000, 700);
        menuStrip = new MenuStrip();
        this.Controls.Add(menuStrip);
        this.maakFileMenu();
        this.maakHelpMenu();
        this.Text = $"Schets Editor CJ";
        this.IsMdiContainer = true;
        this.MainMenuStrip = menuStrip;
    }
    private void maakFileMenu()
    {   
        ToolStripDropDownItem menu = new ToolStripMenuItem("File");
        menu.DropDownItems.Add("Nieuw", null, this.nieuw);
        menu.DropDownItems.Add("Exit", null, this.afsluiten);
        menuStrip.Items.Add(menu);
    }
    private void maakHelpMenu()
    {   
        ToolStripDropDownItem menu = new ToolStripMenuItem("Help");
        menu.DropDownItems.Add("Over \"Schets\"", null, this.about);
        menuStrip.Items.Add(menu);
    }
    private void about(object o, EventArgs ea)
    {   
        MessageBox.Show ( "Schets versie 2.0\n(c) UU Informatica 2024"
                        , "Over \"Schets\""
                        , MessageBoxButtons.OK
                        , MessageBoxIcon.Information
                        );
    }

    private void nieuw(object sender, EventArgs e)
    {   
        SchetsWin s = new SchetsWin();
        s.MdiParent = this;
        s.Show();
    }
    private void afsluiten(object sender, EventArgs e)
    {
        Form child = this.FindForm();
        if (child is SchetsControl sw && sw.IsGewijzigd)
        {
            var result = MessageBox.Show("Er zijn niet-opgeslagen wijzigingen. Weet u zeker dat u wilt afsluiten?", "Bevestig afsluiten", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {
                return; // Annuleer afsluiten
            }
        }
        this.Close();
    }
}