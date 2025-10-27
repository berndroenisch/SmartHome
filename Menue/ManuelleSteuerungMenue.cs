using System;
using System.Linq;
using System.Collections.Generic;
using SmartHome.Daten;
using SmartHome.Helfer;
using SmartHome.Typ;
using SmartHome.Dienste;

namespace SmartHome.Menue
{
    public class ManuelleSteuerungMenue
    {
        private readonly SpeicherDienst _speicher;
        private readonly VerlaufDienst _verlauf;
        private readonly MakroDienst _makros;
        private readonly SteuerungsDienst _steuerung;

        public ManuelleSteuerungMenue(SpeicherDienst speicher, VerlaufDienst verlauf, MakroDienst makros, SteuerungsDienst steuerung)
        {
            _speicher = speicher;
            _verlauf = verlauf;
            _makros = makros;
            _steuerung = steuerung;
        }

        public void Start(Einrichtung e)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Manuelle Steuerung - Raum wählen");

                var raeume = new List<(string Name, string Abk)>(e.Raeume.Select(r => (r.RaumName, r.RaumAbk)));
                var makroListe = _makros.Laden();
                bool hatMakros = makroListe.Count > 0;
                if (hatMakros)
                {
                    raeume.Add(("Makrogeräte", "MG"));
                }

                if (raeume.Count == 0)
                {
                    Console.WriteLine("Keine Räume vorhanden.");
                    Eingabe.WeiterMitTaste();
                    return;
                }

                for (int i = 0; i < raeume.Count; i++)
                {
                    Console.WriteLine($"{i + 1}) {raeume[i].Name} ({raeume[i].Abk})");
                }
                Console.WriteLine("0) Zurück");
                int auswahl = Eingabe.LiesGanzzahl("Auswahl", 0, raeume.Count);
                if (auswahl == 0) return;

                var raumEintrag = raeume[auswahl - 1];
                if (raumEintrag.Abk == "MG")
                {
                    SteuereMakro(makroListe);
                }
                else
                {
                    var raum = e.Raeume.First(r => r.RaumAbk == raumEintrag.Abk);
                    GeraetetypWaehlen(e, raum);
                }
            }
        }

        private void SteuereMakro(List<Makro> alleMakros)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Makrogeräte (MG) - Makro wählen");
                if (alleMakros.Count == 0)
                {
                    Console.WriteLine("Keine Makros vorhanden.");
                    Eingabe.WeiterMitTaste();
                    return;
                }
                for (int i = 0; i < alleMakros.Count; i++)
                    Console.WriteLine($"{i + 1}) {alleMakros[i].Name}");
                Console.WriteLine("0) Zurück");
                int sel = Eingabe.LiesGanzzahl("Auswahl", 0, alleMakros.Count);
                if (sel == 0) return;

                var makro = alleMakros[sel - 1];
                Console.WriteLine($"Makro '{makro.Name}' starten?");
                Console.WriteLine("1) Starten");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 1);
                if (w == 1)
                {
                    var e = _speicher.Laden() ?? new Einrichtung();
                    _steuerung.FuehreMakroAus(e, makro);
                    ZeigeBestaetigung("Makro gestartet (läuft im Hintergrund).");
                }
                else if (w == 0) return;
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
                SteuereGeraet(_speicher.Laden() ?? e, raum, geraet);
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

        // Lädt vor jeder Anzeige den aktuellen Zustand neu aus dem Speicher, ermittelt Raum und Gerät frisch
        private (Raum? raum, Geraete? g) LadeAktuellenZustand(string raumAbk, string typAbk, string geraetename, ref Einrichtung e)
        {
            var geladen = _speicher.Laden();
            if (geladen != null) e = geladen; // innerhalb der Methode mit neuem Objekt weiterarbeiten
            var r = e.Raeume.FirstOrDefault(x => x.RaumAbk.Equals(raumAbk, StringComparison.OrdinalIgnoreCase));
            var gg = r?.Geraete.FirstOrDefault(x =>
                x.TypAbk.Equals(typAbk, StringComparison.OrdinalIgnoreCase) &&
                x.Name.Equals(geraetename, StringComparison.OrdinalIgnoreCase));
            return (r, gg);
        }

        private void SteuereLeuchte(Einrichtung e, Raum raum, Geraete g)
        {
            // Schlüssel einmal festhalten
            string raumAbk = raum.RaumAbk;
            string typAbk = g.TypAbk;
            string name = g.Name;

            while (true)
            {
                // Zustand frisch laden
                var (rNow, gNow) = LadeAktuellenZustand(raumAbk, typAbk, name, ref e);
                if (rNow == null || gNow == null)
                {
                    Console.WriteLine("Das Gerät ist nicht mehr verfügbar.");
                    Eingabe.WeiterMitTaste();
                    return;
                }

                Console.Clear();
                bool ein = gNow.Ein ?? false;
                int dim = gNow.DimProzent ?? 100;
                Console.WriteLine($"Leuchte: {Katalog.Geraetebezeichnung(rNow.RaumAbk, gNow.TypAbk, gNow.Name)}");
                Console.WriteLine($"Status: {(ein ? "Ein" : "Aus")}, Helligkeit: {dim}% (5-100)");
                Console.WriteLine("1) Einschalten");
                Console.WriteLine("2) Ausschalten");
                Console.WriteLine("3) Helligkeit anpassen");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 3);
                if (w == 0) return;

                if (w == 1)
                {
                    bool ok = _steuerung.FuehreGeraeteAktionAus(e, raumAbk, typAbk, name, GeraeteAktion.SetOn, null, "manuell");
                    ZeigeBestaetigung(ok ? "Eingeschaltet." : "Aktion fehlgeschlagen.");
                }
                else if (w == 2)
                {
                    bool ok = _steuerung.FuehreGeraeteAktionAus(e, raumAbk, typAbk, name, GeraeteAktion.SetOff, null, "manuell");
                    ZeigeBestaetigung(ok ? "Ausgeschaltet." : "Aktion fehlgeschlagen.");
                }
                else if (w == 3)
                {
                    int neu = Eingabe.LiesGanzzahl("Neue Helligkeit in %", 5, 100);
                    bool ok = _steuerung.FuehreGeraeteAktionAus(e, raumAbk, typAbk, name, GeraeteAktion.SetDim, neu, "manuell");
                    ZeigeBestaetigung(ok ? $"Helligkeit auf {neu}% gesetzt." : "Aktion fehlgeschlagen.");
                }
            }
        }

        private void SteuereHeizkoerper(Einrichtung e, Raum raum, Geraete g)
        {
            string raumAbk = raum.RaumAbk;
            string typAbk = g.TypAbk;
            string name = g.Name;

            while (true)
            {
                var (rNow, gNow) = LadeAktuellenZustand(raumAbk, typAbk, name, ref e);
                if (rNow == null || gNow == null)
                {
                    Console.WriteLine("Das Gerät ist nicht mehr verfügbar.");
                    Eingabe.WeiterMitTaste();
                    return;
                }

                Console.Clear();
                int stufe = gNow.Stufe ?? 3;
                double temp = Katalog.TemperaturFuerStufe(stufe);
                Console.WriteLine($"Heizkörper: {Katalog.Geraetebezeichnung(rNow.RaumAbk, gNow.TypAbk, gNow.Name)}");
                Console.WriteLine($"Stufe: {stufe} -> {temp} °C");
                Console.WriteLine("1) Stufe anpassen (0-5)(0 (Frostsicherung) = 5°C, 1 = 12°C, 2 = 16°C, 3 = 20°C, 4 = 24°C, 5 = 28°C)");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 1);
                if (w == 0) return;

                if (w == 1)
                {
                    int neu = Eingabe.LiesGanzzahl("Neue Stufe", 0, 5);
                    bool ok = _steuerung.FuehreGeraeteAktionAus(e, raumAbk, typAbk, name, GeraeteAktion.SetStage, neu, "manuell");
                    ZeigeBestaetigung(ok ? $"Stufe {neu} ({Katalog.TemperaturFuerStufe(neu):0.#} °C) gesetzt." : "Aktion fehlgeschlagen.");
                }
            }
        }

        private void SteuereSteckdose(Einrichtung e, Raum raum, Geraete g)
        {
            string raumAbk = raum.RaumAbk;
            string typAbk = g.TypAbk;
            string name = g.Name;

            while (true)
            {
                var (rNow, gNow) = LadeAktuellenZustand(raumAbk, typAbk, name, ref e);
                if (rNow == null || gNow == null)
                {
                    Console.WriteLine("Das Gerät ist nicht mehr verfügbar.");
                    Eingabe.WeiterMitTaste();
                    return;
                }

                Console.Clear();
                bool ein = gNow.Ein ?? false;
                Console.WriteLine($"Steckdose: {Katalog.Geraetebezeichnung(rNow.RaumAbk, gNow.TypAbk, gNow.Name)}");
                Console.WriteLine($"Status: {(ein ? "Ein" : "Aus")}");
                Console.WriteLine("1) Einschalten");
                Console.WriteLine("2) Ausschalten");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 2);
                if (w == 0) return;

                if (w == 1)
                {
                    bool ok = _steuerung.FuehreGeraeteAktionAus(e, raumAbk, typAbk, name, GeraeteAktion.SetOn, null, "manuell");
                    ZeigeBestaetigung(ok ? "Eingeschaltet." : "Aktion fehlgeschlagen.");
                }
                else if (w == 2)
                {
                    bool ok = _steuerung.FuehreGeraeteAktionAus(e, raumAbk, typAbk, name, GeraeteAktion.SetOff, null, "manuell");
                    ZeigeBestaetigung(ok ? "Ausgeschaltet." : "Aktion fehlgeschlagen.");
                }
            }
        }

        private void SteuereRollladen(Einrichtung e, Raum raum, Geraete g)
        {
            string raumAbk = raum.RaumAbk;
            string typAbk = g.TypAbk;
            string name = g.Name;

            while (true)
            {
                var (rNow, gNow) = LadeAktuellenZustand(raumAbk, typAbk, name, ref e);
                if (rNow == null || gNow == null)
                {
                    Console.WriteLine("Das Gerät ist nicht mehr verfügbar.");
                    Eingabe.WeiterMitTaste();
                    return;
                }

                Console.Clear();
                int pos = gNow.PositionProzent ?? 0;
                Console.WriteLine($"Rollladen: {Katalog.Geraetebezeichnung(rNow.RaumAbk, gNow.TypAbk, gNow.Name)}");
                Console.WriteLine($"Schließstellung: {pos}% (0% oben, 100% geschlossen)");
                Console.WriteLine("1) Schließstellung anpassen (0-100)%");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 1);
                if (w == 0) return;

                if (w == 1)
                {
                    int neu = Eingabe.LiesGanzzahl("Neue Stellung in %", 0, 100);
                    bool ok = _steuerung.FuehreGeraeteAktionAus(e, raumAbk, typAbk, name, GeraeteAktion.SetPosition, neu, "manuell");
                    ZeigeBestaetigung(ok ? $"Schließstellung auf {neu}% gesetzt." : "Aktion fehlgeschlagen.");
                }
            }
        }

        private void ZeigeBestaetigung(string text)
        {
            Console.WriteLine();
            Console.WriteLine($"OK: {text}");
            Eingabe.WeiterMitTaste();
        }
    }
}