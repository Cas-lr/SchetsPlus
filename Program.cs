using System;
using System.Diagnostics;
using System.Windows.Forms;

static class Program
{
    [STAThreadAttribute]
    static void Main()
    {
        Debug.WriteLine($"programma wordt opgestart...");
        Application.Run(new SchetsEditor());
    }
}