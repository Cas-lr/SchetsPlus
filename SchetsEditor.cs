using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Diagnostics;

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
        Debug.WriteLine("Nieuw schetsvenster geopend.");
    }
    public void afsluiten(object sender, EventArgs e) 
    {
        foreach (Form child in this.MdiChildren) //Kijkt of de schets is gewijzigd, zo ja, vraagt of de gebruiker zeker weet dat die wil afsluiten zonder op te slaan.
        {
            if (child is SchetsWin sw && sw.IsGewijzigd)
            {
                var waarschuwing = MessageBox.Show("Er zijn niet-opgeslagen wijzigingen. Wilt u afsluiten zonder op te slaan?", "Niet-opgeslagen wijzigingen", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (waarschuwing == DialogResult.No)
                    return; // cancel closing the editor
                break; // user chose Yes, proceed to close
            }
        }
        this.Close();
    }
}