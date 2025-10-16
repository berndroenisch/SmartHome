using System.Globalization;
using SmartHome;
using SmartHome.Daten;
using SmartHome.Dienste;
using SmartHome.Menue;
using SmartHome.Typ;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Landesformat.SetzeDeutsch();

        var speicher = new SpeicherDienst();
        Einrichtung einrichtung = speicher.Laden() ?? new Einrichtung();

        if (einrichtung.Raeume.Count == 0)
        {
            var assistent = new EinrichtungsAssistent(speicher);
            einrichtung = assistent.Starte();
        }

        var hauptmenue = new Hauptmenue(speicher);
        hauptmenue.Start(einrichtung);
    }
}