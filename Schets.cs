using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

public class Schets
{   
    private Bitmap _bitmap;
    public Bitmap bitmap
    {
        get { return _bitmap; }
        set
        {
            _bitmap = value;
            OnBitmapChanged();
        }
    }
    public Bitmap bitmapcopy;

    public List<Doodle> doodles = new List<Doodle>();

    public event EventHandler BitmapChanged;

    public bool IsGewijzigd { get; private set; }

    public Schets()
    {
        _bitmap = new Bitmap(1, 1);
        bitmapcopy = (Bitmap)_bitmap.Clone();
        IsGewijzigd = false;
    }

    protected virtual void OnBitmapChanged()
    {
        BitmapChanged?.Invoke(this, EventArgs.Empty);
    }

    public void MarkeerGewijzigd()
    {
        IsGewijzigd = true;
        OnBitmapChanged();
    }

    public void MarkeerGesaved()
    {
        if (_bitmap != null)
        {
            bitmapcopy?.Dispose();
            bitmapcopy = (Bitmap)_bitmap.Clone();
        }
        IsGewijzigd = false;
        OnBitmapChanged();

    }
    public Graphics BitmapGraphics
    {
        get { return Graphics.FromImage(_bitmap); }
    }
    public void VeranderAfmeting(Size sz)
    {
        if (sz.Width > _bitmap.Size.Width || sz.Height > _bitmap.Size.Height)
        {
            Bitmap nieuw = new Bitmap( Math.Max(sz.Width, _bitmap.Size.Width)
                                     , Math.Max(sz.Height, _bitmap.Size.Height)
                                     );
            Graphics gr = Graphics.FromImage(nieuw);
            gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
            gr.DrawImage(_bitmap, 0, 0);
            _bitmap = nieuw;
            MarkeerGewijzigd();
        }
    }
    public void Teken(Graphics gr)
    {
        gr.DrawImage(_bitmap, 0, 0);
    }
    public void Schoon()
    {
        Graphics gr = Graphics.FromImage(_bitmap);
        gr.FillRectangle(Brushes.White, 0, 0, _bitmap.Width, _bitmap.Height);
        MarkeerGewijzigd();
    }
    public void Roteer()
    {
        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        MarkeerGewijzigd();
    }
}

// object dat alle gegevens van een nieuw element op de bitmap bevat
public class Doodle
{
    public string Type { get; set; }
    public Point Start { get; set; }
    public Point Eind { get; set; }
    public Color Kleur { get; set; }
    public string Tekst { get; set; }
}