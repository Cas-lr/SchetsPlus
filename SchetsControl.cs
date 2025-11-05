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
    public void TekenDoodle(string[] d)
    {
        if (d == null) return;
        this.doodles.Clear();
        schets.Schoon();
        foreach (string line in d) //Zoekt elke regel in het bestand af en maakt een Doodle object aan op basis van de gegevens in die regel.
        {
            string[] parts = line.Split(',');
            if (parts.Length >= 7)
            {
                Doodle doodle = new Doodle
                {
                    Type = parts[0],
                    Start = new Point(int.Parse(parts[1]), int.Parse(parts[2])),
                    Eind = new Point(int.Parse(parts[3]), int.Parse(parts[4])),
                    Kleur = Color.FromArgb(int.Parse(parts[5])),
                    Dikte = int.Parse(parts[6]),
                    Tekst = parts[7],
                    Punten = ParsePunten(parts.Length > 8 ? parts[8] : null)
                };
                doodles.Add(doodle);
                // Hier zou je code kunnen toevoegen om de doodle daadwerkelijk te tekenen op de bitmap
                Debug.WriteLine($"Doodle geladen: Type={doodle.Type}, Start=({doodle.Start.X},{doodle.Start.Y}), Eind=({doodle.Eind.X},{doodle.Eind.Y}), Kleur={doodle.Kleur}, Dikte={doodle.Dikte}, Tekst={doodle.Tekst}");
                Graphics gr = this.MaakBitmapGraphics();
                if (doodle.Type == "TekstTool" && !string.IsNullOrEmpty(doodle.Tekst))
                {
                    Font font = new Font("Tahoma", 40);
                    gr.DrawString(doodle.Tekst, font, new SolidBrush(doodle.Kleur), doodle.Start, StringFormat.GenericTypographic);
                }
                if (doodle.Type == "PenTool")
                {
                    Pen pen = new Pen(doodle.Kleur, doodle.Dikte)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    };
                    if (doodle.Punten.Count == 0) //Teken een lijn als er geen punten zijn opgeslagen
                        gr.DrawLine(pen, doodle.Start, doodle.Eind);
                    if (doodle.Punten.Count > 1)
                    {
                        for (int i = 0; i < doodle.Punten.Count - 1; i++)
                        {
                            gr.DrawLine(pen, doodle.Punten[i], doodle.Punten[i + 1]);
                        }
                    }
                }
                if (doodle.Type == "LijnTool")
                {
                    Pen pen = new Pen(doodle.Kleur, doodle.Dikte)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    };
                    gr.DrawLine(pen, doodle.Start, doodle.Eind);
                }
                if (doodle.Type == "RechthoekTool")
                {
                    Pen pen = new Pen(doodle.Kleur, doodle.Dikte)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    };
                    gr.DrawRectangle(pen, TweepuntTool.Punten2Rechthoek(doodle.Start, doodle.Eind));
                }
                if (doodle.Type == "VolRechthoekTool")
                {
                    Brush brush = new SolidBrush(doodle.Kleur);
                    gr.FillRectangle(brush, TweepuntTool.Punten2Rechthoek(doodle.Start, doodle.Eind));
                }
                if (doodle.Type == "CirkelTool")
                {
                    Pen pen = new Pen(doodle.Kleur, doodle.Dikte)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    };
                    gr.DrawEllipse(pen, TweepuntTool.Punten2Rechthoek(doodle.Start, doodle.Eind));
                }
                if (doodle.Type == "VolCirkelTool")
                {
                    Brush brush = new SolidBrush(doodle.Kleur);
                    gr.FillEllipse(brush, TweepuntTool.Punten2Rechthoek(doodle.Start, doodle.Eind));
                }
            }
        }
        this.Invalidate();
    }
    //public void ReplaceBitmap()
    //{
    //    Graphics gr = Graphics.FromImage(schets.bitmap);
    //    gr.FillRectangle(Brushes.White, 0, 0, schets.bitmap.Width, schets.bitmap.Height);
    //    schets.MarkeerGewijzigd();
    //}
    public void Openen(object obj, EventArgs ea)
    {
        //Opent een file
        try
        {
            string openfileNaam = ((ToolStripMenuItem)obj).Text;
            if (File.Exists($"../../../drawingtxt/{openfileNaam}"))
            {
                string doodletext = File.ReadAllText($"../../../drawingtxt/{openfileNaam}");
                string [] lijnen = doodletext.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                TekenDoodle(lijnen);
                Form owner = this.FindForm();
                owner.Text = openfileNaam.Substring(0, openfileNaam.Length - 4);
                //ReplaceBitmap();
                schets.MarkeerGesaved();
                this.Invalidate();
                Debug.WriteLine(lijnen);
            }
            else
            {
                throw new Exception("Bestand bestaat niet.");
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("De file kon niet gelezen worden:");
            Debug.WriteLine(e.Message);
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
            List<string> puntenlijst = new List<string>();
            if (d.Type == "PenTool")
            {
                foreach (Point p in d.Punten)
                {
                    puntenlijst.Add(p.X + ":" + p.Y);
                }
            }
            string punten = string.Join(" ", puntenlijst);
            string line = $"{d.Type},{d.Start.X},{d.Start.Y},{d.Eind.X},{d.Eind.Y},{d.Kleur.ToArgb()},{d.Dikte}, {d.Tekst}, {punten}";
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

    private List<Point> ParsePunten(string puntenStr)
    {
        var punten = new List<Point>();
        if (string.IsNullOrWhiteSpace(puntenStr))
            return punten;
        var pairs = puntenStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var xy = pair.Split(':');
            if (xy.Length == 2 && int.TryParse(xy[0], out int x) && int.TryParse(xy[1], out int y))
                punten.Add(new Point(x, y));
        }
        return punten;
    }
}