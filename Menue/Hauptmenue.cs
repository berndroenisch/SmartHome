using System;
using SmartHome.Daten;
using SmartHome.Helfer;
using SmartHome.Typ;
using SmartHome.Dienste;

namespace SmartHome.Menue
{
    public class Hauptmenue
    {
        private readonly SpeicherDienst _speicher;
        private readonly VerlaufDienst _verlauf;
        private readonly ZeitplanDienst _zeitplan;
        private readonly MakroDienst _makros;
        private readonly SteuerungsDienst _steuerung;

        public Hauptmenue(SpeicherDienst speicher, VerlaufDienst verlauf, ZeitplanDienst zeitplan, MakroDienst makros, SteuerungsDienst steuerung)
        {
            _speicher = speicher;
            _verlauf = verlauf;
            _zeitplan = zeitplan;
            _makros = makros;
            _steuerung = steuerung;
        }

        public void Start(Einrichtung einrichtung)
        {
            while (true)
            {
                Console.Clear();
                Ueberschrift();

                Console.WriteLine("1) Einrichtung ansehen/bearbeiten");
                Console.WriteLine("2) Manuelle Steuerung");
                Console.WriteLine("3) Makrogerätesteuerung");
                Console.WriteLine("4) Zeitgesteuerte Schaltung");
                Console.WriteLine("5) Verlauf ansehen/filtern");
                Console.WriteLine("x) Einrichtung zurücksetzen (alle Daten werden gelöscht!)");
                Console.WriteLine("0) Beenden");
                Console.Write("Auswahl: ");
                var wahl = (Console.ReadLine() ?? "").Trim();

                if (wahl == "0")
                {
                    break;
                }
                else if (wahl == "1")
                {
                    new UntermenueEinrichtung(_speicher).Start(einrichtung);
                }
                else if (wahl == "2")
                {
                    new ManuelleSteuerungMenue(_speicher, _verlauf, _makros, _steuerung).Start(einrichtung);
                }
                else if (wahl == "3")
                {
                    new MakroMenue(_makros, _steuerung, _speicher).Start(einrichtung);
                }
                else if (wahl == "4")
                {
                    new ZeitsteuerungMenue(_zeitplan, _makros, _steuerung, _speicher).Start(einrichtung);
                }
                else if (wahl == "5")
                {
                    new VerlaufMenue(_verlauf).Start(einrichtung);
                }
                else if (string.Equals(wahl, "x", StringComparison.OrdinalIgnoreCase))
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
                // Einrichtung leeren
                e.Raeume.Clear();
                _speicher.Speichern(e);

                // Verlauf löschen
                _verlauf.Leeren();

                // Zeitpläne und Makros automatisch komplett löschen
                _zeitplan.Leeren();
                _makros.Leeren();

                Console.WriteLine("Einrichtung, Verlauf, Zeitpläne und Makros wurden zurückgesetzt. Der Einrichtungsassistent startet nun neu.");

                // Einrichtungsassistent erneut starten
                var assistent = new EinrichtungsAssistent(_speicher);
                var neu = assistent.Starte();

                // Neue Daten übernehmen (Referenz von e beibehalten)
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