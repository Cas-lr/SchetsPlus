using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.IO;

public class SchetsWin : Form
{
    MenuStrip menuStrip;
    ISchetsTool huidigeTool;
    SchetsControl schetscontrol;
    Panel paneel;
    bool vast;

    private void veranderAfmeting(object o, EventArgs ea)
    {
        schetscontrol.Size = new Size(this.ClientSize.Width - 70
                                      , this.ClientSize.Height - 50);
        paneel.Location = new Point(64, this.ClientSize.Height - 30);
    }

    private void klikToolMenu(object obj, EventArgs ea)
    {
        this.huidigeTool = (ISchetsTool)((ToolStripMenuItem)obj).Tag;
    }

    private void klikToolButton(object obj, EventArgs ea)
    {
        this.huidigeTool = (ISchetsTool)((RadioButton)obj).Tag;
    }

    private void afsluiten(object obj, EventArgs ea)
    {
        if (this.IsGewijzigd) //Kijkt of de schets is gewijzigd, zo ja, vraagt of de gebruiker zeker weet dat die wil afsluiten zonder op te slaan.
        {
            var dlg = MessageBox.Show("Er zijn niet-opgeslagen wijzigingen. Wilt u afsluiten zonder op te slaan?", "Niet-opgeslagen wijzigingen", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dlg == DialogResult.No)
                return;
        }
        this.Close();
    }

    public SchetsWin()
    {
        this.Text = "Untitled";
        ISchetsTool[] deTools = { new PenTool()         
                                , new LijnTool()
                                , new RechthoekTool()
                                , new VolRechthoekTool()
                                , new CirkelTool()
                                , new VolCirkelTool()
                                , new TekstTool()
                                , new GumTool()
                                };
        String[] deKleuren = { "Black", "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan", "White" };
        String[] deLijndiktes = { "1", "3", "5", "7", "9", "15" };
        String[] deFiletypes = { ".bmp", ".gif", ".jpeg", ".jpg", ".png" }; //ondersteunde filetypes

        String[] deGemaakteFiles = { }; //nog geen gemaakte bestandsnamen

        this.ClientSize = new Size(770, 550);
        huidigeTool = deTools[0];

        schetscontrol = new SchetsControl();
        schetscontrol.Location = new Point(64, 10);

        schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                    {   vast=true;  
                                        huidigeTool.MuisVast(schetscontrol, mea.Location);
                                    };
        schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                    {   if (vast)
                                        huidigeTool.MuisDrag(schetscontrol, mea.Location);
                                    };
        schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                    {   if (vast)
                                        huidigeTool.MuisLos (schetscontrol, mea.Location);
                                        vast = false; 
                                    };
        schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                    {   huidigeTool.Letter  (schetscontrol, kpea.KeyChar);
                                    };
        this.Controls.Add(schetscontrol);


        menuStrip = new MenuStrip();
        menuStrip.Visible = false;
        this.Controls.Add(menuStrip);
        this.maakFileMenu(deFiletypes);
        this.maakToolMenu(deTools);
        this.maakActieMenu(deKleuren);
        this.maakToolButtons(deTools);
        this.maakActieButtons(deKleuren, deLijndiktes);
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
    }
    private void verandernaam(object obj, EventArgs ea)
    { //https://stackoverflow.com/questions/10797774/messagebox-with-input-field
        //Verandert de naam van het huidige venster na invoer door de gebruiker in een inputbox.
        Form owner = this.FindForm(); //Haal het huidige form op.
        string WindowNaam = owner?.Text ?? "Untitled";
        try
        {
            string nieuweNaam = Interaction.InputBox("Wat is de nieuwe naam?", "Rename", $"{WindowNaam}");
            if (nieuweNaam == "")
            {
                nieuweNaam = $"{WindowNaam}";
            }
            else
            {
                owner.Text = nieuweNaam;
                owner.Invalidate();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    public string windowNaam
    {
        get { return this.Text; }
        set { this.Text = value; }
    }
    public bool IsGewijzigd //Kijkt of de schets is gewijzigd.
    {
        get { return schetscontrol?.Schets?.IsGewijzigd ?? false; }
    }
    
    private void maakFileMenu(String[] filetypes)
    {
        ToolStripMenuItem menu = new ToolStripMenuItem("File");
        menu.MergeAction = MergeAction.MatchOnly;
        menu.DropDownItems.Add("Afsluiten", null, this.afsluiten);
        menu.DropDownItems.Add("Rename", null, this.verandernaam);
        ToolStripMenuItem submenu = new ToolStripMenuItem("Opslaan als");
        foreach (string f in filetypes)
            submenu.DropDownItems.Add(f, null, schetscontrol.Opslaan);
        submenu.DropDownItems.Add(".txt", null, schetscontrol.OpslaanAlsDoodleText);
        ToolStripMenuItem submenu2 = new ToolStripMenuItem("Openen");
        try
        {
            string sourceDirectory = @"../../../drawingtxt";
            var files = System.IO.Directory.EnumerateFiles(sourceDirectory, "*.txt", System.IO.SearchOption.AllDirectories);
            foreach (string tf in files)
                submenu2.DropDownItems.Add(Path.GetFileName(tf), null, schetscontrol.Openen);
            Console.WriteLine("Bestanden succesvol geladen voor openen menu.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        menu.DropDownItems.Add(submenu);
        menu.DropDownItems.Add(submenu2);
        menuStrip.Items.Add(menu);
    }

    private void maakToolMenu(ICollection<ISchetsTool> tools)
    {   
        ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
        foreach (ISchetsTool tool in tools)
        {   ToolStripItem item = new ToolStripMenuItem();
            item.Tag = tool;
            item.Text = tool.ToString();
            item.Image = new Bitmap($"../../../Icons/{tool.ToString()}.png");
            item.Click += this.klikToolMenu;
            menu.DropDownItems.Add(item);
        }
        menuStrip.Items.Add(menu);
    }

    private void maakActieMenu(String[] kleuren)
    {   
        ToolStripMenuItem menu = new ToolStripMenuItem("Actie");
        menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon );
        menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer );
        ToolStripMenuItem submenu = new ToolStripMenuItem("Kies kleur");
        foreach (string k in kleuren)
            submenu.DropDownItems.Add(k, null, schetscontrol.VeranderKleurViaMenu);
        menu.DropDownItems.Add(submenu);
        menuStrip.Items.Add(menu);
    }

    private void maakToolButtons(ICollection<ISchetsTool> tools)
    {
        int t = 0;
        foreach (ISchetsTool tool in tools)
        {
            RadioButton b = new RadioButton();
            b.Appearance = Appearance.Button;
            b.Size = new Size(45, 62);
            b.Location = new Point(10, 10 + t * 62);
            b.Tag = tool;
            b.Text = tool.ToString();
            b.Image = new Bitmap($"../../../Icons/{tool.ToString()}.png");
            b.TextAlign = ContentAlignment.TopCenter;
            b.ImageAlign = ContentAlignment.BottomCenter;
            b.Click += this.klikToolButton;
            this.Controls.Add(b);
            if (t == 0) b.Select();
            t++;
        }
    }

    private void maakActieButtons(String[] kleuren, String[] lijndiktes)
    {   
        paneel = new Panel(); this.Controls.Add(paneel);
        paneel.Size = new Size(600, 24);
            
        Button clear = new Button(); paneel.Controls.Add(clear);
        clear.Text = "Clear";  
        clear.Location = new Point(  0, 0); 
        clear.Click += schetscontrol.Schoon;        
            
        Button rotate = new Button(); paneel.Controls.Add(rotate);
        rotate.Text = "Rotate"; 
        rotate.Location = new Point( 80, 0); 
        rotate.Click += schetscontrol.Roteer; 
           
        Label penkleur = new Label(); paneel.Controls.Add(penkleur);
        penkleur.Text = "Penkleur:"; 
        penkleur.Location = new Point(180, 3); 
        penkleur.AutoSize = true;               
            
        ComboBox cbb = new ComboBox(); paneel.Controls.Add(cbb);
        cbb.Location = new Point(240, 0); 
        cbb.DropDownStyle = ComboBoxStyle.DropDownList; 
        cbb.SelectedValueChanged += schetscontrol.VeranderKleur;
        foreach (string k in kleuren)
            cbb.Items.Add(k);
        cbb.SelectedIndex = 0;

        Label lijndikte = new Label(); paneel.Controls.Add(lijndikte);
        lijndikte.Text = "Lijndikte:";
        lijndikte.Location = new Point(400, 3);
        lijndikte.AutoSize = true;

        ComboBox combolijn = new ComboBox(); paneel.Controls.Add(combolijn);
        combolijn.Location = new Point(460, 0);
        combolijn.DropDownStyle = ComboBoxStyle.DropDownList;
        combolijn.SelectedValueChanged += schetscontrol.VeranderDikte;
        foreach (string l in lijndiktes)
            combolijn.Items.Add(l);
        combolijn.SelectedIndex = 0;
    }
}