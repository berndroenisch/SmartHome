using System;
using System.Linq;
using SmartHome.Daten;
using SmartHome.Helfer;
using SmartHome.Typ;
using SmartHome.Dienste;

namespace SmartHome.Menue
{
    public class Hauptmenue
    {
        private readonly SpeicherDienst _speicher;

        public Hauptmenue(SpeicherDienst speicher)
        {
            _speicher = speicher;
        }

        public void Start(Einrichtung einrichtung)
        {
            while (true)
            {
                Console.Clear();
                Ueberschrift();

                Console.WriteLine("1) Einrichtung ansehen/bearbeiten");
                Console.WriteLine("2) Einrichtung zurücksetzen (alle Daten werden gelöscht!)");
                Console.WriteLine("0) Beenden");
                Console.Write("Auswahl: ");
                var wahl = (Console.ReadLine() ?? "").Trim();

                if (wahl == "0") break;
                if (wahl == "1")
                {
                    new UntermenueEinrichtung(_speicher).Start(einrichtung);
                }
                else if (wahl == "2")
                {
                    EinrichtungZuruecksetzen(einrichtung);
                }
                else
                {
                    Console.WriteLine("Ungültige Auswahl.");
                    Eingabe.WeiterMitTaste();
                }
            }
        }

        private void Ueberschrift()
        {
            var jetzt = DateTime.Now;
            string wochentag = jetzt.ToString("dddd");  // deutscher Wochentag
            string datum = jetzt.ToString("dd.MM.yyyy");
            string zeit = jetzt.ToString("HH:mm");
            Console.WriteLine("Smart Home Steuerung");
            Console.WriteLine($"{wochentag}, {datum} {zeit}");
            Console.WriteLine(new string('-', 30));
        }

        private void EinrichtungZuruecksetzen(Einrichtung e)
        {
            Console.Write("Zum Zurücksetzen 'Reset' eingeben (alle Daten werden gelöscht!): ");
            var bestaetigung = (Console.ReadLine() ?? "").Trim();

            if (bestaetigung == "Reset")
            {
                // Bestehende Einrichtung leeren und speichern
                e.Raeume.Clear();
                _speicher.Speichern(e);
                Console.WriteLine("Einrichtung wurde zurückgesetzt. Der Einrichtungsassistent startet nun neu.");

                // Einrichtungsassistent erneut starten
                var assistent = new EinrichtungsAssistent(_speicher);
                var neu = assistent.Starte();

                // Neue Daten in das bestehende Objekt übernehmen (Referenzen erhalten)
                e.Raeume.Clear();
                e.Raeume.AddRange(neu.Raeume);

                _speicher.Speichern(e);
                Console.WriteLine("Neue Einrichtung gespeichert.");
            }
            else
            {
                Console.WriteLine("Abgebrochen. Keine Änderungen vorgenommen.");
            }

            Eingabe.WeiterMitTaste();
        }
    }
}