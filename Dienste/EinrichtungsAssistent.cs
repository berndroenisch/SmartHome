using System;
using System.Collections.Generic;
using System.Linq;
using SmartHome.Helfer;
using SmartHome.Typ;
using SmartHome.Daten;

namespace SmartHome.Dienste
{
    public class EinrichtungsAssistent
    {
        private readonly SpeicherDienst _speicher;

        public EinrichtungsAssistent(SpeicherDienst speicher)
        {
            _speicher = speicher;
        }

        public Einrichtung Starte()
        {
            var einrichtung = new Einrichtung();

            Console.Clear();
            Console.WriteLine("Smart Home Einrichtung - Assistent");
            Console.WriteLine("----------------------------------");

            // Möglichkeit zum Abbrechen direkt zu Beginn
            Console.WriteLine("Möchten Sie die Einrichtung starten?");
            Console.WriteLine("  1) Einrichtung starten");
            Console.WriteLine("  2) Abbrechen und Programm beenden");
            Console.Write("Auswahl: ");
            var startWahl = (Console.ReadLine() ?? "").Trim();
            if (startWahl == "2")
            {
                Console.WriteLine("Einrichtung abgebrochen. Programm wird beendet.");
                Environment.Exit(0);
            }

            // Schritt 1: Anzahl Räume (max. 9)
            int anzahlRaeume = Eingabe.LiesGanzzahl("1) Anzahl der Räume festlegen", 1, 9);

            // Schritt 2: Räume auswählen (einzigartig)
            Console.WriteLine();
            Console.WriteLine("2) Räume auswählen (jede Abk. nur einmal verwendbar)");
            DruckeRaumOptionen();
            var verfuegbare = new HashSet<string>(Katalog.VerfuegbareRaeume.Keys);
            for (int i = 1; i <= anzahlRaeume; i++)
            {
                string abk = Eingabe.LiesAuswahlMitAbk(
                    $"  Raum {i} Abkürzung",
                    s => verfuegbare.Contains(s),
                    "Unbekannte oder bereits verwendete Abkürzung. Bitte erneut."
                );
                einrichtung.Raeume.Add(new Raum { RaumAbk = abk, RaumName = Katalog.VerfuegbareRaeume[abk] });
                verfuegbare.Remove(abk);
            }

            // Schritt 3: Gerätetypen und Anzahl je Raum
            Console.WriteLine();
            Console.WriteLine("3) Gerätetypen pro Raum zuordnen (Anzahl je Raum, 0..Max)");
            DruckeGeraetetypOptionen();
            foreach (var raum in einrichtung.Raeume)
            {
                Console.WriteLine($"  Raum {raum.RaumName} ({raum.RaumAbk})");
                foreach (var kv in Katalog.Geraetetypen)
                {
                    var abk = kv.Key;
                    var (name, max) = kv.Value;
                    int anzahl = Eingabe.LiesGanzzahl($"    Anzahl {name}", 0, max);
                    for (int n = 0; n < anzahl; n++)
                    {
                        raum.Geraete.Add(new Geraete { TypAbk = abk, TypName = name, Name = "" });
                    }
                }
            }

            // Schritt 4: Gerätenamen erfassen (je Raum pro Gerätetyp eindeutig)
            Console.WriteLine();
            Console.WriteLine("4) Gerätenamen eingeben (im Raum pro Gerätetyp eindeutig, Buchstaben/Zahlen erlaubt)");
            foreach (var raum in einrichtung.Raeume)
            {
                Console.WriteLine($"  Raum {raum.RaumName} ({raum.RaumAbk})");
                // gruppiert nach Typen in der definierten Reihenfolge
                foreach (var kv in Katalog.Geraetetypen)
                {
                    var abk = kv.Key;
                    var (name, _) = kv.Value;
                    var liste = raum.Geraete.Where(g => g.TypAbk == abk).ToList();
                    for (int i = 0; i < liste.Count; i++)
                    {
                        while (true)
                        {
                            var vorschlag = $"{name}{(i + 1)}";
                            string eingabe = Eingabe.LiesNichtLeer($"    Name für {name} #{i + 1} (z.B. {vorschlag})");
                            if (raum.GeraetenameIstFrei(abk, eingabe))
                            {
                                liste[i].Name = eingabe;
                                break;
                            }
                            Console.WriteLine("    Dieser Gerätename ist in diesem Raum für diesen Gerätetyp bereits vergeben. Bitte anderen Namen wählen.");
                        }
                    }
                }
            }

            _speicher.Speichern(einrichtung);
            Console.WriteLine();
            Console.WriteLine("Einrichtung gespeichert.");
            Eingabe.WeiterMitTaste();

            return einrichtung;
        }

        private void DruckeRaumOptionen()
        {
            Console.WriteLine("   Verfügbare Räume:");
            foreach (var kv in Katalog.VerfuegbareRaeume)
            {
                Console.WriteLine($"    {kv.Key} = {kv.Value}");
            }
        }

        private void DruckeGeraetetypOptionen()
        {
            Console.WriteLine("   Gerätetypen:");
            foreach (var kv in Katalog.Geraetetypen)
            {
                Console.WriteLine($"    {kv.Key} = {kv.Value.Name} (max. {kv.Value.MaxJeRaum} je Raum)");
            }
        }
    }
}