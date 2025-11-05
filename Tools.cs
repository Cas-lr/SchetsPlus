using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

public interface ISchetsTool
{
    void MuisVast(SchetsControl s, Point p);
    void MuisDrag(SchetsControl s, Point p);
    void MuisLos(SchetsControl s, Point p);
    void Letter(SchetsControl s, char c);
}

public abstract class StartpuntTool : ISchetsTool
{
    protected Point startpunt;
    protected Brush kwast;
    protected int dikte;

    public virtual void MuisVast(SchetsControl s, Point p)
    {   startpunt = p;
    }
    public virtual void MuisLos(SchetsControl s, Point p)
    {   kwast = new SolidBrush(s.PenKleur); dikte = s.PenDikte;
    }
    public abstract void MuisDrag(SchetsControl s, Point p);
    public abstract void Letter(SchetsControl s, char c);

    // Maakt een Doodle object aan om in de lijst van doodles te zetten.
    // Virtual zodat elke subklasse het heeft, en desnoods kan veranderen
    protected virtual Doodle MaakDoodle(Point start, Point eind, Color kleur, int dikte)
    {
        return new Doodle
        {
            Type = this.GetType().Name,
            Start = start,
            Eind = eind,
            Kleur = kleur,
            Dikte = dikte
        };
    }
}

public class TekstTool : StartpuntTool
{
    public override string ToString() { return "tekst"; }

    public override void MuisDrag(SchetsControl s, Point p) { }

    public override void Letter(SchetsControl s, char c)
    {
        if (c >= 32)
        {
            Doodle DoodleLetter = new Doodle
            {
                Type = "TekstTool",
                Kleur = s.PenKleur,
                Start = this.startpunt,
                Tekst = c.ToString()
            };
            s.doodles.Add(DoodleLetter);
            if (s.doodles.Count > 0)
                Debug.WriteLine($"Doodle toegevoegd: Type={DoodleLetter.Type}, Start=({DoodleLetter.Start.X},{DoodleLetter.Start.Y}), Tekst={DoodleLetter.Tekst}, Kleur={DoodleLetter.Kleur}");

            Graphics gr = s.MaakBitmapGraphics();
            Font font = new Font("Tahoma", 40);
            string tekst = c.ToString();
            SizeF sz = gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
            gr.DrawString(tekst, font, kwast, this.startpunt, StringFormat.GenericTypographic);
            // gr.DrawRectangle(Pens.Black, startpunt.X, startpunt.Y, sz.Width, sz.Height);
            startpunt.X += (int)sz.Width;

            s.Invalidate();
        }
    }
}

public abstract class TweepuntTool : StartpuntTool
{
    public static Rectangle Punten2Rechthoek(Point p1, Point p2)
    {   return new Rectangle( new Point(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y))
                            , new Size (Math.Abs(p1.X-p2.X), Math.Abs(p1.Y-p2.Y))
                            );
    }
    public static Pen MaakPen(Brush b, int dikte)
    {   Pen pen = new Pen(b, dikte);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;
        return pen;
    }
    public override void MuisVast(SchetsControl s, Point p)
    {   base.MuisVast(s, p);
        kwast = Brushes.Gray;
        dikte = s.PenDikte;
    }
    public override void MuisDrag(SchetsControl s, Point p)
    {   s.Refresh();
        this.Bezig(s.CreateGraphics(), this.startpunt, p);
    }
    public override void MuisLos(SchetsControl s, Point p)
    {   base.MuisLos(s, p);
        this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);

        // maak Doodle aan en voeg toe aan doodles lijst in SchetsControl
        // wordt geerft door alle andere TweepuntTools (rechthoek, cirkel, lijn, etc..)
        Doodle HuidigeDoodle = new Doodle
        {
            Start = this.startpunt,
            Eind = p,
            Kleur = s.PenKleur,
            Dikte = dikte,
        };
        s.doodles.Add(HuidigeDoodle);
        if (s.doodles.Count > 0)
            Debug.WriteLine($"Doodle toegevoegd: Type={HuidigeDoodle.Type}, Start=({HuidigeDoodle.Start.X},{HuidigeDoodle.Start.Y}), Eind=({HuidigeDoodle.Eind.X},{HuidigeDoodle.Eind.Y}), Kleur={HuidigeDoodle.Kleur}");

        s.Invalidate();
    }
    public override void Letter(SchetsControl s, char c)
    {
    }
    public abstract void Bezig(Graphics g, Point p1, Point p2);
        
    public virtual void Compleet(Graphics g, Point p1, Point p2)
    {   this.Bezig(g, p1, p2);
    }
}

public class RechthoekTool : TweepuntTool
{
    public override string ToString() { return "kader"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {   g.DrawRectangle(MaakPen(kwast,dikte), TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}
    
public class VolRechthoekTool : RechthoekTool
{
    public override string ToString() { return "vlak"; }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {   g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

// cirketool gebaseerd op rechthoektool code
public class CirkelTool : TweepuntTool
{
    public override string ToString() { return "cirkel"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {
        g.DrawEllipse(MaakPen(kwast, dikte), TweepuntTool.Punten2Rechthoek(p1, p2));    
    }
}

// volcirketool gebaseerd op volrechthoektool code
public class VolCirkelTool : CirkelTool
{
    public override string ToString() { return "bal"; }

    public override void Compleet(Graphics g, Point p1, Point p2)
    {
        g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
    }
}

public class LijnTool : TweepuntTool
{
    public override string ToString() { return "lijn"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {   
        g.DrawLine(MaakPen(this.kwast, this.dikte), p1, p2);
    }
}

// voor de PenTool willen we dat een lijn als een doodle wordt opgeslagen
// met een lijst van punten voor de lijn segmenten
public class PenTool : LijnTool
{
    public override string ToString() { return "pen"; }

    private Doodle HuidigeDoodle;

    // MuisVast begint de Doodle en maakt de lijst van punten aan
    public override void MuisVast(SchetsControl s, Point p)
    {
        base.MuisVast(s, p);
        HuidigeDoodle = new Doodle
        {
            Type = "PenTool",
            Kleur = s.PenKleur,
            Start = p,
            Punten = new List<Point> { p },
            Dikte = s.PenDikte,
        };
    }

    // MuisDrag tekent de segmenten nu zelf, vorige manier leidde tot ongewenste resultaten
    // (gebruik van MuisLos binnen MuisDrag zorgde ervoor dat de Doodle niet als een geheel opgeslagen kon worden)
    public override void MuisDrag(SchetsControl s, Point p)
    {
        // teken een lijnsegment van het vorige punt naar het nieuwe punt en voegt het nieuwe punt toe aan de lijst
        Graphics g = s.MaakBitmapGraphics();
        Point vorige = HuidigeDoodle.Punten[HuidigeDoodle.Punten.Count - 1];
        HuidigeDoodle.Punten.Add(p);
        using (Pen pen = new Pen(HuidigeDoodle.Kleur, HuidigeDoodle.Dikte))
            g.DrawLine(pen, vorige, p);

        s.Invalidate();
    }

    // pas bij MuisLos wordt de gehele Doodle toegevoegd aan de lijst in SchetsControl
    public override void MuisLos(SchetsControl s, Point p)
    {
        HuidigeDoodle.Eind = p;
        s.doodles.Add(HuidigeDoodle);

        if (HuidigeDoodle.Punten.Count > 0)
            Debug.WriteLine($"Doodle-pen: laatste punt={HuidigeDoodle.Punten[HuidigeDoodle.Punten.Count - 1]}, aantal punten ={HuidigeDoodle.Punten.Count}");
        if (s.doodles.Count > 0)
            Debug.WriteLine($"Doodle toegevoegd: Type={HuidigeDoodle.Type}, Start=({HuidigeDoodle.Start.X},{HuidigeDoodle.Start.Y}), Eind=({HuidigeDoodle.Eind.X},{HuidigeDoodle.Eind.Y}), Kleur={HuidigeDoodle.Kleur}");

        HuidigeDoodle = null;
    }
}
    
public class GumTool : PenTool
{
    public override string ToString() { return "gum"; }

    public override void Bezig(Graphics g, Point p1, Point p2)
    {   g.DrawLine(MaakPen(Brushes.White, 7), p1, p2);
    }
}