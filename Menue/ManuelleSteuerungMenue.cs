using System;
using System.Linq;
using SmartHome.Daten;
using SmartHome.Helfer;
using SmartHome.Typ;

namespace SmartHome.Menue
{
    public class ManuelleSteuerungMenue
    {
        private readonly SpeicherDienst _speicher;
        private readonly VerlaufDienst _verlauf;

        public ManuelleSteuerungMenue(SpeicherDienst speicher, VerlaufDienst verlauf)
        {
            _speicher = speicher;
            _verlauf = verlauf;
        }

        public void Start(Einrichtung e)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Manuelle Steuerung - Raum wählen");
                if (e.Raeume.Count == 0)
                {
                    Console.WriteLine("Keine Räume vorhanden.");
                    Eingabe.WeiterMitTaste();
                    return;
                }

                for (int i = 0; i < e.Raeume.Count; i++)
                {
                    var r = e.Raeume[i];
                    Console.WriteLine($"{i + 1}) {r.RaumName} ({r.RaumAbk})");
                }
                Console.WriteLine("0) Zurück");
                int auswahl = Eingabe.LiesGanzzahl("Auswahl", 0, e.Raeume.Count);
                if (auswahl == 0) return;

                var raum = e.Raeume[auswahl - 1];
                GeraetetypWaehlen(e, raum);
            }
        }

        private void GeraetetypWaehlen(Einrichtung e, Raum raum)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Raum: {raum.RaumName} ({raum.RaumAbk}) - Gerätetyp wählen");
                var vorhandeneTyps = Katalog.Geraetetypen.Keys
                    .Where(k => raum.Geraete.Any(g => g.TypAbk == k))
                    .ToList();

                if (vorhandeneTyps.Count == 0)
                {
                    Console.WriteLine("Keine Gerätetypen in diesem Raum vorhanden.");
                    Eingabe.WeiterMitTaste();
                    return;
                }

                for (int i = 0; i < vorhandeneTyps.Count; i++)
                {
                    var abk = vorhandeneTyps[i];
                    var name = Katalog.Geraetetypen[abk].Name;
                    Console.WriteLine($"{i + 1}) {name} ({abk})");
                }
                Console.WriteLine("0) Zurück");
                int auswahl = Eingabe.LiesGanzzahl("Auswahl", 0, vorhandeneTyps.Count);
                if (auswahl == 0) return;

                var typAbk = vorhandeneTyps[auswahl - 1];
                GeraetWaehlen(e, raum, typAbk);
            }
        }

        private void GeraetWaehlen(Einrichtung e, Raum raum, string typAbk)
        {
            while (true)
            {
                Console.Clear();
                var typName = Katalog.Geraetetypen[typAbk].Name;
                Console.WriteLine($"Raum: {raum.RaumName} ({raum.RaumAbk}) - {typName} ({typAbk}) wählen");

                var liste = raum.Geraete.Where(g => g.TypAbk == typAbk).ToList();
                if (liste.Count == 0)
                {
                    Console.WriteLine("Keine Geräte dieses Typs vorhanden.");
                    Eingabe.WeiterMitTaste();
                    return;
                }

                for (int i = 0; i < liste.Count; i++)
                {
                    var g = liste[i];
                    Console.WriteLine($"{i + 1}) {Katalog.Geraetebezeichnung(raum.RaumAbk, g.TypAbk, g.Name)}");
                }
                Console.WriteLine("0) Zurück");
                int auswahl = Eingabe.LiesGanzzahl("Auswahl", 0, liste.Count);
                if (auswahl == 0) return;

                var geraet = liste[auswahl - 1];
                SteuereGeraet(e, raum, geraet);
            }
        }

        private void SteuereGeraet(Einrichtung e, Raum raum, Geraete g)
        {
            switch (g.TypAbk)
            {
                case "LE":
                    SteuereLeuchte(e, raum, g);
                    break;
                case "HK":
                    SteuereHeizkoerper(e, raum, g);
                    break;
                case "SD":
                    SteuereSteckdose(e, raum, g);
                    break;
                case "RO":
                    SteuereRollladen(e, raum, g);
                    break;
            }
        }

        private void SteuereLeuchte(Einrichtung e, Raum raum, Geraete g)
        {
            while (true)
            {
                Console.Clear();
                bool ein = g.Ein ?? false;
                int dim = g.DimProzent ?? 100;
                Console.WriteLine($"Leuchte: {Katalog.Geraetebezeichnung(raum.RaumAbk, g.TypAbk, g.Name)}");
                Console.WriteLine($"Status: {(ein ? "Ein" : "Aus")}, Helligkeit: {dim}% (5..100)");
                Console.WriteLine("1) Ein/Aus umschalten");
                Console.WriteLine("2) Helligkeit anpassen");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 2);
                if (w == 0) return;

                if (w == 1)
                {
                    g.Ein = !ein; // Dim bleibt unverändert
                    Protokolliere(raum, g, "Ein/Aus", g.Ein == true ? "Ein" : "Aus");
                    _speicher.Speichern(e);
                }
                else if (w == 2)
                {
                    int neu = Eingabe.LiesGanzzahl("Neue Helligkeit", 5, 100);
                    g.DimProzent = Katalog.BegrenztProzent(neu, 5, 100);
                    Protokolliere(raum, g, "Helligkeit angepasst", $"{g.DimProzent}%");
                    _speicher.Speichern(e);
                }
            }
        }

        private void SteuereHeizkoerper(Einrichtung e, Raum raum, Geraete g)
        {
            while (true)
            {
                Console.Clear();
                int stufe = g.Stufe ?? 3;
                double temp = Katalog.TemperaturFuerStufe(stufe);
                Console.WriteLine($"Heizkörper: {Katalog.Geraetebezeichnung(raum.RaumAbk, g.TypAbk, g.Name)}");
                Console.WriteLine($"Stufe: {stufe} -> {temp} °C");
                Console.WriteLine("1) Stufe anpassen (0..5)(0 (Frostsicherung) = 5°C, 1 = 12°C, 2 = 16°C, 3 = 20°C, 4 = 24°C, 5 = 28°C)");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 1);
                if (w == 0) return;

                if (w == 1)
                {
                    int neu = Eingabe.LiesGanzzahl("Neue Stufe", 0, 5);
                    g.Stufe = neu;
                    g.Temperatur = Katalog.TemperaturFuerStufe(neu);
                    Protokolliere(raum, g, "Temp. angepasst", $"Stufe {neu} ({g.Temperatur:0.#} °C)");
                    _speicher.Speichern(e);
                }
            }
        }

        private void SteuereSteckdose(Einrichtung e, Raum raum, Geraete g)
        {
            while (true)
            {
                Console.Clear();
                bool ein = g.Ein ?? false;
                Console.WriteLine($"Steckdose: {Katalog.Geraetebezeichnung(raum.RaumAbk, g.TypAbk, g.Name)}");
                Console.WriteLine($"Status: {(ein ? "Ein" : "Aus")}");
                Console.WriteLine("1) Ein/Aus umschalten");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 1);
                if (w == 0) return;

                if (w == 1)
                {
                    g.Ein = !ein;
                    Protokolliere(raum, g, "Ein/Aus", g.Ein == true ? "Ein" : "Aus");
                    _speicher.Speichern(e);
                }
            }
        }

        private void SteuereRollladen(Einrichtung e, Raum raum, Geraete g)
        {
            while (true)
            {
                Console.Clear();
                int pos = g.PositionProzent ?? 0;
                Console.WriteLine($"Rollladen: {Katalog.Geraetebezeichnung(raum.RaumAbk, g.TypAbk, g.Name)}");
                Console.WriteLine($"Schließstellung: {pos}% (0% oben, 100% geschlossen)");
                Console.WriteLine("1) Schließstellung anpassen (0..100)%");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 1);
                if (w == 0) return;

                if (w == 1)
                {
                    int neu = Eingabe.LiesGanzzahl("Neue Stellung", 0, 100);
                    g.PositionProzent = Katalog.BegrenztProzent(neu, 0, 100);
                    Protokolliere(raum, g, "Schließstellung angepasst", $"Position {g.PositionProzent}%");
                    _speicher.Speichern(e);
                }
            }
        }

        private void Protokolliere(Raum raum, Geraete g, string aktion, string wert)
        {
            var eintrag = new Verlaufseintrag
            {
                Zeitpunkt = DateTime.Now,
                Ausloeser = "manuell",
                RaumAbk = raum.RaumAbk,
                TypAbk = g.TypAbk,
                Geraetename = g.Name,
                Aktion = aktion,
                Wert = wert,
                Bezeichnung = Katalog.Geraetebezeichnung(raum.RaumAbk, g.TypAbk, g.Name)
            };
            _verlauf.Hinzufuegen(eintrag);
        }
    }
}