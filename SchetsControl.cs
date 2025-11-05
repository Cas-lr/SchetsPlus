using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;

public class SchetsControl : UserControl
{   
    private Schets schets;
    private Color penkleur;
    private int pendikte;

    // lijst van alle gemaakte doodles, een Doodle wordt toegevoegd bij MuisLos in de tools
    public List<Doodle> doodles = new List<Doodle>();

    public int PenDikte
    { get { return pendikte; }
    }

    public Color PenKleur
    { get { return penkleur; }
    }

    public Schets Schets
    { get { return schets;   }
    }

    public SchetsControl()
    {   this.BorderStyle = BorderStyle.Fixed3D;
        this.schets = new Schets();
        this.schets.bitmapcopy = (Bitmap)this.schets.bitmap.Clone();
        this.Paint += this.teken;
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
    private void teken(object o, PaintEventArgs pea)
    {   schets.Teken(pea.Graphics);
    }
    private void veranderAfmeting(object o, EventArgs ea)
    {   schets.VeranderAfmeting(this.ClientSize);
        this.Invalidate();
    }
    public Graphics MaakBitmapGraphics()
    {   Graphics g = schets.BitmapGraphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        return g;
    }
    public void Schoon(object o, EventArgs ea)
    {   schets.Schoon();
        this.Invalidate();
    }
    public void Roteer(object o, EventArgs ea)
    {   schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
        schets.Roteer();
        this.Invalidate();
    }
    public void VeranderKleur(object obj, EventArgs ea)
    {   string kleurNaam = ((ComboBox)obj).Text;
        penkleur = Color.FromName(kleurNaam);
    }
    public void VeranderDikte(object obj, EventArgs ea)
    {   string dikteNaam = ((ComboBox)obj).Text;
        pendikte = int.Parse(dikteNaam);
    }
    public void VeranderKleurViaMenu(object obj, EventArgs ea)
    {   string kleurNaam = ((ToolStripMenuItem)obj).Text;
        penkleur = Color.FromName(kleurNaam);
    }
    public void Openen(object obj, EventArgs ea)
    {
        //hier komt nog wat
        try
        {
            string openfileNaam = ((ToolStripMenuItem)obj).Text;
            if (File.Exists($"../../../drawingtxt/{openfileNaam}"))
            {
                string doodletext = File.ReadAllText($"../../../drawingtxt/{openfileNaam}");
                Form owner = this.FindForm();
                owner.Text = openfileNaam.Substring(0, openfileNaam.Length - 4);
                schets.MarkeerGesaved();
                this.Invalidate();
                Console.WriteLine(doodletext);
            }
            else
            {
                throw new Exception("Bestand bestaat niet.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("De file kon niet gelezen worden:");
            Console.WriteLine(e.Message);
        }
    }
    public void OpslaanAlsDoodleText(object obj, EventArgs ea) //opslaan als doodle text bestand
    {
        Form owner = this.FindForm();
        string fileNaam = owner?.Text ?? "Untitled";
        if (owner is SchetsWin s) fileNaam = s.windowNaam;
        string fileType = ((ToolStripMenuItem)obj).Text;
        List<string> doodleLines = new List<string>();
        foreach (Doodle d in doodles)
        {
            string line = $"{d.Type},{d.Start.X},{d.Start.Y},{d.Eind.X},{d.Eind.Y},{d.Kleur.ToArgb()},{d.Dikte}";
            doodleLines.Add(line);
        }
        File.WriteAllLines($"../../../drawingtxt/{fileNaam}{fileType}", doodleLines);
        schets.MarkeerGesaved();
        Debug.WriteLine("Doodle text saved successfully.");
    }
    public void Opslaan(object obj, EventArgs ea) //opslaan als met behulp van de bestandsnaam van het huidige venster en het gekozen bestandstype
    {
        Form owner = this.FindForm();
        string fileNaam = owner?.Text ?? "Untitled";
        if (owner is SchetsWin s) fileNaam = s.windowNaam;
        string fileType = ((ToolStripMenuItem)obj).Text;
        schets.bitmap.Save($"../../../drawings/{fileNaam}{fileType}");
        schets.MarkeerGesaved();
        Debug.WriteLine("Saved successfully.");
        this.Invalidate();
    }
}