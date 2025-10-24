using System;
using System.Collections.Generic;
using System.Linq;
using SmartHome.Daten;
using SmartHome.Dienste;
using SmartHome.Helfer;
using SmartHome.Typ;

namespace SmartHome.Menue
{
    public class MakroMenue
    {
        private readonly MakroDienst _makros;
        private readonly SteuerungsDienst _steuerung;
        private readonly SpeicherDienst _speicher;

        public MakroMenue(MakroDienst makros, SteuerungsDienst steuerung, SpeicherDienst speicher)
        {
            _makros = makros;
            _steuerung = steuerung;
            _speicher = speicher;
        }

        public void Start(Einrichtung e)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Makrogerätesteuerung");
                Console.WriteLine("1) Erstellen");
                Console.WriteLine("2) Bearbeiten");
                Console.WriteLine("3) Ansicht aller Makroprofile");
                Console.WriteLine("4) Löschen");
                Console.WriteLine("0) Zurück");
                var w = (Console.ReadLine() ?? "").Trim();

                if (w == "0") return;
                else if (w == "1") Erstellen(e);
                else if (w == "2") Bearbeiten(e);
                else if (w == "3") Ansicht();
                else if (w == "4") Loeschen();
                else
                {
                    Console.WriteLine("Ungültige Auswahl.");
                    Eingabe.WeiterMitTaste();
                }
            }
        }

        private void Erstellen(Einrichtung e)
        {
            var alle = _makros.Laden();
            string nameVorschlag = $"Makro-{alle.Count(m => m.Name.StartsWith("Makro-", StringComparison.OrdinalIgnoreCase)) + 1}";
            Console.Write($"Makroname (leer = {nameVorschlag}): ");
            var name = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name)) name = nameVorschlag;

            var m = new Makro { Name = name };
            Console.WriteLine("Schritte hinzufügen (mindestens 1).");
            while (true)
            {
                var schritt = ErfasseSchritt(e);
                if (schritt == null) break;
                m.Schritte.Add(schritt);

                Console.Write("Weiteren Schritt hinzufügen? (j/n): ");
                var s = (Console.ReadLine() ?? "").Trim().ToLower();
                if (!(s == "j" || s == "ja")) break;
            }

            if (m.Schritte.Count == 0)
            {
                Console.WriteLine("Keine Schritte. Abbruch.");
                Eingabe.WeiterMitTaste();
                return;
            }

            _makros.SpeichernNeuOderAktualisieren(m);
            Console.WriteLine($"Makro '{m.Name}' gespeichert.");
            Eingabe.WeiterMitTaste();
        }

        private void Bearbeiten(Einrichtung e)
        {
            var alle = _makros.Laden();
            if (alle.Count == 0)
            {
                Console.WriteLine("Keine Makros vorhanden.");
                Eingabe.WeiterMitTaste();
                return;
            }
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Makros:");
                for (int i = 0; i < alle.Count; i++)
                    Console.WriteLine($"{i + 1}) {alle[i].Name} ({alle[i].Schritte.Count} Schritte)");
                Console.WriteLine("0) Zurück");
                int sel = Eingabe.LiesGanzzahl("Auswahl", 0, alle.Count);
                if (sel == 0) return;

                var m = alle[sel - 1];

                Console.WriteLine("1) Schritt hinzufügen");
                Console.WriteLine("2) Schritt löschen");
                Console.WriteLine("3) Makro ausführen");
                Console.WriteLine("4) Makronamen ändern");
                Console.WriteLine("5) Dieses Makro löschen");
                Console.WriteLine("0) Zurück");
                int w = Eingabe.LiesGanzzahl("Auswahl", 0, 5);
                if (w == 0) return;

                if (w == 1)
                {
                    var ns = ErfasseSchritt(e);
                    if (ns != null)
                    {
                        m.Schritte.Add(ns);
                        _makros.SpeichernNeuOderAktualisieren(m);
                        Console.WriteLine("Schritt hinzugefügt.");
                    }
                }
                else if (w == 2)
                {
                    if (m.Schritte.Count == 0)
                    {
                        Console.WriteLine("Keine Schritte.");
                    }
                    else
                    {
                        for (int i = 0; i < m.Schritte.Count; i++)
                        {
                            var s = m.Schritte[i];
                            var akt = Anzeige.AktionMitWert(s.Aktion, s.Wert);
                            Console.WriteLine($"{i + 1}) {s.RaumAbk}-{s.TypAbk}-{s.Geraetename} {akt}  (+{s.WarteSekunden}s)");
                        }
                        int ds = Eingabe.LiesGanzzahl("Schritt löschen", 1, m.Schritte.Count);
                        m.Schritte.RemoveAt(ds - 1);
                        _makros.SpeichernNeuOderAktualisieren(m);
                        Console.WriteLine("Schritt gelöscht.");
                    }
                }
                else if (w == 3)
                {
                    _steuerung.FuehreMakroAus(_speicher.Laden() ?? e, m);
                    Console.WriteLine("Makro gestartet (läuft im Hintergrund).");
                }
                else if (w == 4)
                {
                    // Makronamen ändern + altes Makro löschen
                    var alleMakros = _makros.Laden();
                    string alt = m.Name;

                    Console.Write($"Neuer Makroname (leer = Abbruch) [aktuell: {alt}]: ");
                    var neu = (Console.ReadLine() ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(neu))
                    {
                        Console.WriteLine("Abgebrochen.");
                    }
                    else if (neu.Equals(alt, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Name unverändert.");
                    }
                    else if (alleMakros.Any(x => x.Name.Equals(neu, StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.WriteLine("Ein Makro mit diesem Namen existiert bereits.");
                    }
                    else
                    {
                        // 1) Makro unter neuem Namen speichern
                        m.Name = neu;
                        _makros.SpeichernNeuOderAktualisieren(m);

                        // 2) Zeitplanreferenzen aktualisieren
                        var zeitplan = new ZeitplanDienst();
                        var eintraege = zeitplan.Laden();
                        int cnt = 0;
                        foreach (var z in eintraege)
                        {
                            if (z.ZielArt == ZeitplanZielArt.Makro &&
                                z.MakroName.Equals(alt, StringComparison.OrdinalIgnoreCase))
                            {
                                z.MakroName = neu;
                                cnt++;
                            }
                        }
                        if (cnt > 0) zeitplan.Speichern(eintraege);

                        // 3) Altes Makro löschen
                        bool removed = _makros.Entfernen(alt);

                        Console.WriteLine($"Makroname geändert auf '{neu}'. {cnt} zeitgesteuerte Einträge aktualisiert. Altes Makro {(removed ? "gelöscht" : "nicht gefunden")}.");

                        // Liste neu laden
                        alle = _makros.Laden();
                    }
                }
                else if (w == 5)
                {
                    Console.Write($"Makro '{m.Name}' wirklich löschen? (j/n): ");
                    var conf = (Console.ReadLine() ?? "").Trim().ToLower();
                    if (conf is "j" or "ja")
                    {
                        if (_makros.Entfernen(m.Name))
                            Console.WriteLine("Makro gelöscht.");
                        else
                            Console.WriteLine("Makro nicht gefunden.");
                        return; // zurück zur Liste
                    }
                }

                Eingabe.WeiterMitTaste();
            }
        }

        private void Ansicht()
        {
            var alle = _makros.Laden();
            Console.Clear();
            Console.WriteLine("Makroprofile:");
            foreach (var m in alle)
            {
                Console.WriteLine($"- {m.Name} ({m.Schritte.Count} Schritte)");
                foreach (var s in m.Schritte)
                {
                    var akt = Anzeige.AktionMitWert(s.Aktion, s.Wert);
                    Console.WriteLine($"    • {s.RaumAbk}-{s.TypAbk}-{s.Geraetename}: {akt}, Warte {s.WarteSekunden}s");
                }
            }
            Eingabe.WeiterMitTaste();
        }

        private void Loeschen()
        {
            Console.Clear();
            Console.WriteLine("Makros löschen");
            Console.WriteLine("1) Einzelnes Makro löschen");
            Console.WriteLine("2) Alle Makros löschen");
            Console.WriteLine("0) Zurück");
            int w = Eingabe.LiesGanzzahl("Auswahl", 0, 2);
            if (w == 0) return;

            var alle = _makros.Laden();

            if (w == 1)
            {
                if (alle.Count == 0)
                {
                    Console.WriteLine("Keine Makros vorhanden.");
                    Eingabe.WeiterMitTaste();
                    return;
                }
                for (int i = 0; i < alle.Count; i++)
                    Console.WriteLine($"{i + 1}) {alle[i].Name}");
                int sel = Eingabe.LiesGanzzahl("Makro wählen", 1, alle.Count);
                var name = alle[sel - 1].Name;
                Console.Write($"Makro '{name}' wirklich löschen? (j/n): ");
                var conf = (Console.ReadLine() ?? "").Trim().ToLower();
                if (conf is "j" or "ja")
                {
                    if (_makros.Entfernen(name)) Console.WriteLine("Makro gelöscht.");
                    else Console.WriteLine("Makro nicht gefunden.");
                }
            }
            else if (w == 2)
            {
                Console.Write("Wirklich ALLE Makros löschen? Zum Bestätigen 'ALLE' eingeben: ");
                var conf = (Console.ReadLine() ?? "").Trim();
                if (conf == "ALLE")
                {
                    _makros.Leeren();
                    Console.WriteLine("Alle Makros wurden gelöscht.");
                }
                else
                {
                    Console.WriteLine("Abgebrochen.");
                }
            }

            Eingabe.WeiterMitTaste();
        }

        // Gibt entweder einen MakroSchritt zurück oder null (Abbruch/kein Gerät)
        private MakroSchritt? ErfasseSchritt(Einrichtung e)
        {
            var typen = Katalog.Geraetetypen.Keys.ToList();
            for (int i = 0; i < typen.Count; i++)
                Console.WriteLine($"{i + 1}) {Katalog.Geraetetypen[typen[i]].Name} ({typen[i]})");
            int tSel = Eingabe.LiesGanzzahl("Gerätetyp wählen", 1, typen.Count);
            var typAbk = typen[tSel - 1];

            var alleGeraete = e.Raeume
                .SelectMany(r => r.Geraete.Where(g => g.TypAbk == typAbk).Select(g => (RaumAbk: r.RaumAbk, G: g)))
                .ToList();

            if (alleGeraete.Count == 0)
            {
                Console.WriteLine("Keine Geräte dieses Typs vorhanden.");
                return null;
            }

            for (int i = 0; i < alleGeraete.Count; i++)
            {
                var ge = alleGeraete[i];
                Console.WriteLine($"{i + 1}) {ge.RaumAbk}-{ge.G.TypAbk}-{ge.G.Name}");
            }
            int gSel = Eingabe.LiesGanzzahl("Gerät wählen", 1, alleGeraete.Count);
            var (raumAbk, g) = alleGeraete[gSel - 1];

            var schritt = new MakroSchritt { RaumAbk = raumAbk, TypAbk = g.TypAbk, Geraetename = g.Name };

            // Aktion erfassen
            switch (g.TypAbk)
            {
                case "LE":
                    Console.WriteLine("Aktion: 1) Ein  2) Aus  3) Helligkeit anpassen (5..100)%");
                    {
                        int a = Eingabe.LiesGanzzahl("Auswahl", 1, 3);
                        if (a == 1) { schritt.Aktion = GeraeteAktion.SetOn; schritt.Wert = null; }
                        else if (a == 2) { schritt.Aktion = GeraeteAktion.SetOff; schritt.Wert = null; }
                        else { schritt.Aktion = GeraeteAktion.SetDim; schritt.Wert = Eingabe.LiesGanzzahl("Helligkeit in %", 5, 100); }
                    }
                    break;
                case "HK":
                    schritt.Aktion = GeraeteAktion.SetStage;
                    schritt.Wert = Eingabe.LiesGanzzahl("(0 (Frostsicherung) = 5°C, 1 = 12°C, 2 = 16°C, 3 = 20°C, 4 = 24°C, 5 = 28°C)\nStufe anpassen", 0, 5);
                    break;
                case "SD":
                    Console.WriteLine("Aktion: 1) Ein  2) Aus");
                    {
                        int a = Eingabe.LiesGanzzahl("Auswahl", 1, 2);
                        schritt.Aktion = (a == 1) ? GeraeteAktion.SetOn : GeraeteAktion.SetOff;
                        schritt.Wert = null;
                    }
                    break;
                case "RO":
                    schritt.Aktion = GeraeteAktion.SetPosition;
                    schritt.Wert = Eingabe.LiesGanzzahl("Schließstellung in % (0 oben, 100 geschlossen)", 0, 100);
                    break;
            }

            schritt.WarteSekunden = Eingabe.LiesGanzzahl("Wartezeit bis zum nächsten Schritt (Sekunden)", 0, 86400);
            return schritt;
        }
    }
}