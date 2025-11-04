using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

public class SchetsControl : UserControl
{   
    public bool kanAfsluiten;
    private Schets schets;
    private Color penkleur;

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
        this.kanAfsluiten = true;
        this.Paint += this.teken;
        this.Resize += this.veranderAfmeting;
        this.veranderAfmeting(null, null);
        this.schets.BitmapChanged += (object o, EventArgs ea) => { isGewijzigd(); };
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
    private void isGewijzigd()
    {
        this.kanAfsluiten = BitmapsGelijk(schets.bitmap, schets.bitmapcopy);
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
    public void VeranderKleurViaMenu(object obj, EventArgs ea)
    {   string kleurNaam = ((ToolStripMenuItem)obj).Text;
        penkleur = Color.FromName(kleurNaam);
    }
    public void Openen(object obj, EventArgs ea)
    {
        Form owner = this.FindForm();
        string fileNaam = owner?.Text ?? "Untitled";
        if (owner is SchetsWin s) fileNaam = s.windowNaam;
        string fileType = ((ToolStripMenuItem)obj).Text;
        schets.bitmap = (Bitmap)Image.FromFile($"../../../drawings/{fileNaam}{fileType}");
        Debug.WriteLine("Opened successfully.");
        this.kanAfsluiten = true;
        this.Invalidate();
    }
    public bool BitmapsGelijk(Bitmap bmp1, Bitmap bmp2)
    {
        // Compare dimensions
        if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
            return false;

        // Compare pixel data
        for (int x = 0; x < bmp1.Width; x++)
        {
            for (int y = 0; y < bmp1.Height; y++)
            {
                if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                    return false;
            }
        }
        return true;
    }
    public void Opslaan(object obj, EventArgs ea)
    {
        Form owner = this.FindForm();
        string fileNaam = owner?.Text ?? "Untitled";
        if (owner is SchetsWin s) fileNaam = s.windowNaam;
        string fileType = ((ToolStripMenuItem)obj).Text;
        schets.bitmap.Save($"../../../drawings/{fileNaam}{fileType}");
        Debug.WriteLine("Saved successfully.");
        this.schets.bitmapcopy = (Bitmap)this.schets.bitmap.Clone();
        this.kanAfsluiten = true;
    }
}