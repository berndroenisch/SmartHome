using System;
using System.Collections.Generic;
using System.Linq;
using SmartHome.Daten;
using SmartHome.Helfer;
using SmartHome.Typ;
using SmartHome.Dienste;

namespace SmartHome.Menue
{
    public class ZeitsteuerungMenue
    {
        private readonly ZeitplanDienst _zeitplan;
        private readonly MakroDienst _makros;
        private readonly SteuerungsDienst _steuerung;
        private readonly SpeicherDienst _speicher;

        public ZeitsteuerungMenue(ZeitplanDienst zeitplan, MakroDienst makros, SteuerungsDienst steuerung, SpeicherDienst speicher)
        {
            _zeitplan = zeitplan;
            _makros = makros;
            _steuerung = steuerung;
            _speicher = speicher;
        }

        public void Start(Einrichtung e)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Zeitgesteuerte Schaltung");
                Console.WriteLine("1) Erstellen");
                Console.WriteLine("2) Bearbeiten");
                Console.WriteLine("3) Ansicht aller zeitgesteuerten Einträge");
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
            Console.Clear();
            Console.WriteLine("Neuen Zeitplaneintrag erstellen");
            Console.WriteLine("Ziel wählen:");
            Console.WriteLine("1) Gerät");
            Console.WriteLine("2) Makro (MG)");
            Console.WriteLine("0) Abbrechen");
            int ziel = Eingabe.LiesGanzzahl("Auswahl", 0, 2);
            if (ziel == 0) return;

            ZeitplanEintrag z = new ZeitplanEintrag();

            if (ziel == 2)
            {
                z.ZielArt = ZeitplanZielArt.Makro;
                var alleMakros = _makros.Laden();
                if (alleMakros.Count == 0)
                {
                    Console.WriteLine("Keine Makros vorhanden.");
                    Eingabe.WeiterMitTaste();
                    return;
                }
                for (int i = 0; i < alleMakros.Count; i++)
                {
                    Console.WriteLine($"{i + 1}) {alleMakros[i].Name}");
                }
                int iSel = Eingabe.LiesGanzzahl("Makro wählen", 1, alleMakros.Count);
                z.MakroName = alleMakros[iSel - 1].Name;
            }
            else
            {
                z.ZielArt = ZeitplanZielArt.Geraet;

                // Auswahl: Raum -> Typ -> Gerät
                if (e.Raeume.Count == 0)
                {
                    Console.WriteLine("Keine Räume vorhanden.");
                    Eingabe.WeiterMitTaste();
                    return;
                }

                for (int i = 0; i < e.Raeume.Count; i++)
                    Console.WriteLine($"{i + 1}) {e.Raeume[i].RaumName} ({e.Raeume[i].RaumAbk})");
                int rSel = Eingabe.LiesGanzzahl("Raum wählen", 1, e.Raeume.Count);
                var raum = e.Raeume[rSel - 1];
                z.RaumAbk = raum.RaumAbk;

                var typKeys = Katalog.Geraetetypen.Keys.Where(k => raum.Geraete.Any(g => g.TypAbk == k)).ToList();
                for (int i = 0; i < typKeys.Count; i++)
                    Console.WriteLine($"{i + 1}) {Katalog.Geraetetypen[typKeys[i]].Name} ({typKeys[i]})");
                int tSel = Eingabe.LiesGanzzahl("Gerätetyp wählen", 1, typKeys.Count);
                z.TypAbk = typKeys[tSel - 1];

                var liste = raum.Geraete.Where(g => g.TypAbk == z.TypAbk).ToList();
                for (int i = 0; i < liste.Count; i++)
                    Console.WriteLine($"{i + 1}) {Katalog.Geraetebezeichnung(raum.RaumAbk, liste[i].TypAbk, liste[i].Name)}");
                int gSel = Eingabe.LiesGanzzahl("Gerät wählen", 1, liste.Count);
                z.Geraetename = liste[gSel - 1].Name;

                // Aktion je Typ erfassen (Start)
                ErfasseAktion(z);

                // Ende-Aktion optional
                ErfasseEndAktionOptional(z);
            }

            // Wochentage
            z.Tage = FrageWochentage();

            // Zeiten
            z.StartZeit = LiesZeit("Startzeit (HH:MM)");
            z.EndZeit = LiesZeit("Endzeit (HH:MM)");
            if (z.EndZeit <= z.StartZeit)
            {
                Console.WriteLine("Endzeit muss nach Startzeit liegen (Über-Nacht-Intervalle derzeit nicht unterstützt).");
                Eingabe.WeiterMitTaste();
                return;
            }

            // Überschneidungen prüfen (nur für Geräte)
            string? konflikt = PruefeUeberschneidung(z);
            if (!string.IsNullOrEmpty(konflikt))
            {
                Console.WriteLine("Achtung: Zeitliche Überschneidung gefunden:");
                Console.WriteLine(konflikt);
                Console.Write("Trotzdem speichern? (j/n): ");
                var c = (Console.ReadLine() ?? "").Trim().ToLower();
                if (!(c == "j" || c == "ja"))
                {
                    Console.WriteLine("Abgebrochen.");
                    Eingabe.WeiterMitTaste();
                    return;
                }
            }

            _zeitplan.Hinzufuegen(z);
            Console.WriteLine("Zeitplaneintrag gespeichert.");
            Console.WriteLine("Hinweis: Manuelle Steuerungen und Makros können jederzeit ausgeführt werden. Beim nächsten Startzeitpunkt setzt der Zeitplan die Werte erneut durch.");
            Eingabe.WeiterMitTaste();
        }

        private void ErfasseAktion(ZeitplanEintrag z)
        {
            if (z.ZielArt == ZeitplanZielArt.Makro) return;

            switch (z.TypAbk)
            {
                case "LE":
                    Console.WriteLine("Start-Aktion: 1) Ein  2) Helligkeit (5..100)%");
                    {
                        int a = Eingabe.LiesGanzzahl("Auswahl", 1, 2);
                        if (a == 1) { z.Aktion = GeraeteAktion.SetOn; z.Wert = null; }
                        else { z.Aktion = GeraeteAktion.SetDim; z.Wert = Eingabe.LiesGanzzahl("Helligkeit in %", 5, 100); }
                    }
                    break;
                case "HK":
                    z.Aktion = GeraeteAktion.SetStage;
                    z.Wert = Eingabe.LiesGanzzahl("(0 (Frostsicherung) = 5°C, 1 = 12°C, 2 = 16°C, 3 = 20°C, 4 = 24°C, 5 = 28°C)\nStart-Stufe", 0, 5);
                    break;
                case "SD":
                    z.Aktion = GeraeteAktion.SetOn; z.Wert = null;
                    break;
                case "RO":
                    z.Aktion = GeraeteAktion.SetPosition;
                    z.Wert = Eingabe.LiesGanzzahl("Schließstellung in % (0 oben, 100 geschlossen)\nStart-Stellung", 0, 100);
                    break;
                default:
                    Console.WriteLine("Unbekannter Typ. Abbruch.");
                    break;
            }
        }

        private void ErfasseEndAktionOptional(ZeitplanEintrag z)
        {
            if (z.ZielArt == ZeitplanZielArt.Makro) return;

            Console.Write("Am Ende eine Aktion ausführen? (j/n): ");
            var yn = (Console.ReadLine() ?? "").Trim().ToLower();
            if (!(yn == "j" || yn == "ja"))
            {
                z.EndAktionAktiv = false;
                z.EndAktion = null;
                z.EndWert = null;
                return;
            }

            z.EndAktionAktiv = true;
            switch (z.TypAbk)
            {
                case "LE":
                    Console.WriteLine("End-Aktion: 1) Aus  2) Helligkeit (5..100)%");
                    {
                        int a = Eingabe.LiesGanzzahl("Auswahl", 1, 2);
                        if (a == 1) { z.EndAktion = GeraeteAktion.SetOff; z.EndWert = null; }
                        else { z.EndAktion = GeraeteAktion.SetDim; z.EndWert = Eingabe.LiesGanzzahl("Helligkeit in %", 5, 100); }
                    }
                    break;
                case "HK":
                    z.EndAktion = GeraeteAktion.SetStage;
                    z.EndWert = Eingabe.LiesGanzzahl("(0 (Frostsicherung) = 5°C, 1 = 12°C, 2 = 16°C, 3 = 20°C, 4 = 24°C, 5 = 28°C)\nEnd-Stufe", 0, 5);
                    break;
                case "SD":
                    z.EndAktion = GeraeteAktion.SetOff; z.EndWert = null;
                    break;
                case "RO":
                    z.EndAktion = GeraeteAktion.SetPosition;
                    z.EndWert = Eingabe.LiesGanzzahl("Schließstellung in % (0 oben, 100 geschlossen)\nEnd-Stellung", 0, 100);
                    break;
            }
        }

        private Wochentage FrageWochentage()
        {
            Console.WriteLine("Geltungsbereich:");
            Console.WriteLine("1) Einzelner Wochentag");
            Console.WriteLine("2) Montag bis Freitag");
            Console.WriteLine("3) Samstag + Sonntag");
            Console.WriteLine("4) Ganze Woche");
            int sel = Eingabe.LiesGanzzahl("Auswahl", 1, 4);
            if (sel == 2) return Wochentage.Werktage;
            if (sel == 3) return Wochentage.Wochenende;
            if (sel == 4) return Wochentage.Alle;

            // Einzelner Wochentag
            Console.WriteLine("Wochentag wählen: 1) Mo 2) Di 3) Mi 4) Do 5) Fr 6) Sa 7) So");
            int d = Eingabe.LiesGanzzahl("Tag", 1, 7);
            return d switch
            {
                1 => Wochentage.Montag,
                2 => Wochentage.Dienstag,
                3 => Wochentage.Mittwoch,
                4 => Wochentage.Donnerstag,
                5 => Wochentage.Freitag,
                6 => Wochentage.Samstag,
                7 => Wochentage.Sonntag,
                _ => Wochentage.Montag
            };
        }

        private TimeSpan LiesZeit(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                var s = (Console.ReadLine() ?? "").Trim();
                if (TimeSpan.TryParse(s, out var ts) && ts >= TimeSpan.Zero && ts < TimeSpan.FromDays(1))
                {
                    // normalisieren auf Minutenauflösung
                    return new TimeSpan(ts.Hours, ts.Minutes, 0);
                }
                Console.WriteLine("Bitte im Format HH:MM eingeben.");
            }
        }

        private string? PruefeUeberschneidung(ZeitplanEintrag neu)
        {
            if (neu.ZielArt != ZeitplanZielArt.Geraet) return null;

            var alle = _zeitplan.Laden()
                .Where(x => x.ZielArt == ZeitplanZielArt.Geraet &&
                            x.RaumAbk.Equals(neu.RaumAbk, StringComparison.OrdinalIgnoreCase) &&
                            x.TypAbk.Equals(neu.TypAbk, StringComparison.OrdinalIgnoreCase) &&
                            x.Geraetename.Equals(neu.Geraetename, StringComparison.OrdinalIgnoreCase))
                .ToList();

            bool TageUeberschneiden(Wochentage a, Wochentage b) => (a & b) != 0;
            bool IntervallOverlap(TimeSpan s1, TimeSpan e1, TimeSpan s2, TimeSpan e2) =>
                s1 < e2 && s2 < e1; // offene Intervalle [s, e)

            foreach (var z in alle)
            {
                // Legacy-Eintrag: überspringen oder approximieren
                if (z.StartZeit == default && z.EndZeit == default && z.Zeitpunkt != default)
                {
                    // approximativ: Zeitpunkt liegt im neuen Intervall eines abgedeckten Tages?
                    var dmask = z.Zeitpunkt.DayOfWeek switch
                    {
                        DayOfWeek.Monday => Wochentage.Montag,
                        DayOfWeek.Tuesday => Wochentage.Dienstag,
                        DayOfWeek.Wednesday => Wochentage.Mittwoch,
                        DayOfWeek.Thursday => Wochentage.Donnerstag,
                        DayOfWeek.Friday => Wochentage.Freitag,
                        DayOfWeek.Saturday => Wochentage.Samstag,
                        DayOfWeek.Sunday => Wochentage.Sonntag,
                        _ => Wochentage.None
                    };
                    if (TageUeberschneiden(neu.Tage, dmask) &&
                        (z.Zeitpunkt.TimeOfDay >= neu.StartZeit && z.Zeitpunkt.TimeOfDay < neu.EndZeit))
                    {
                        return $"- {z} überschneidet sich am {z.Zeitpunkt:dddd} um {z.Zeitpunkt:HH\\:mm}";
                    }
                    continue;
                }

                if (!TageUeberschneiden(neu.Tage, z.Tage)) continue;
                if (IntervallOverlap(neu.StartZeit, neu.EndZeit, z.StartZeit, z.EndZeit))
                {
                    return $"- {z}";
                }
            }
            return null;
        }

        private void Bearbeiten(Einrichtung e)
        {
            var alle = _zeitplan.Laden();
            if (alle.Count == 0)
            {
                Console.WriteLine("Keine Zeitplaneinträge vorhanden.");
                Eingabe.WeiterMitTaste();
                return;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Zeitplaneinträge:");
                for (int i = 0; i < alle.Count; i++)
                {
                    Console.WriteLine($"{i + 1}) {(alle[i].Aktiv ? "[Aktiv] " : "[Inaktiv] ")}{alle[i]}");
                }
                Console.WriteLine("0) Zurück");
                int sel = Eingabe.LiesGanzzahl("Eintrag wählen", 0, alle.Count);
                if (sel == 0) return;

                var z = alle[sel - 1];

                Console.WriteLine("1) Aktiv/Inaktiv umschalten");
                Console.WriteLine("2) Tage ändern");
                Console.WriteLine("3) Zeiten ändern");
                if (z.ZielArt == ZeitplanZielArt.Geraet) Console.WriteLine("4) Start-Aktion ändern");
                if (z.ZielArt == ZeitplanZielArt.Geraet) Console.WriteLine("5) End-Aktion ändern/entfernen");
                Console.WriteLine("9) Löschen");
                Console.WriteLine("0) Zurück");
                int aw = Eingabe.LiesGanzzahl("Auswahl", 0, 9);

                if (aw == 0) return;
                if (aw == 1) z.Aktiv = !z.Aktiv;
                else if (aw == 2) z.Tage = FrageWochentage();
                else if (aw == 3)
                {
                    z.StartZeit = LiesZeit("Neue Startzeit (HH:MM)");
                    z.EndZeit = LiesZeit("Neue Endzeit (HH:MM)");
                    if (z.EndZeit <= z.StartZeit)
                    {
                        Console.WriteLine("Endzeit muss nach Startzeit liegen.");
                        Eingabe.WeiterMitTaste();
                        continue;
                    }
                }
                else if (aw == 4 && z.ZielArt == ZeitplanZielArt.Geraet) ErfasseAktion(z);
                else if (aw == 5 && z.ZielArt == ZeitplanZielArt.Geraet) ErfasseEndAktionOptional(z);
                else if (aw == 9)
                {
                    _zeitplan.Entfernen(z.Id);
                    alle.RemoveAt(sel - 1);
                    Console.WriteLine("Eintrag gelöscht.");
                    Eingabe.WeiterMitTaste();
                    continue;
                }

                _zeitplan.Aktualisieren(z);
                Console.WriteLine("Eintrag gespeichert.");
                Eingabe.WeiterMitTaste();
            }
        }

        private void Ansicht()
        {
            var alle = _zeitplan.Laden()
                .OrderBy(x => x.Tage == Wochentage.None ? 1 : 0)
                .ThenBy(x => x.StartZeit)
                .ToList();
            Console.Clear();
            Console.WriteLine("Alle Zeitplaneinträge:");
            foreach (var z in alle)
            {
                Console.WriteLine($"- {(z.Aktiv ? "[Aktiv]" : "[Inaktiv]")} {z}");
            }
            Console.WriteLine();
            Console.WriteLine("Hinweis: Manuelle Steuerungen und Makros können jederzeit ausgeführt werden. Beim nächsten Startzeitpunkt setzt der Zeitplan die Werte erneut durch.");
            Eingabe.WeiterMitTaste();
        }

        private void Loeschen()
        {
            Console.Clear();
            Console.WriteLine("Zeitgesteuerte Schaltungen löschen");
            Console.WriteLine("1) Einzelnen Eintrag löschen");
            Console.WriteLine("2) Alle Einträge löschen");
            Console.WriteLine("0) Zurück");
            int w = Eingabe.LiesGanzzahl("Auswahl", 0, 2);
            if (w == 0) return;

            var alle = _zeitplan.Laden();
            if (w == 1)
            {
                if (alle.Count == 0)
                {
                    Console.WriteLine("Keine Einträge vorhanden.");
                    Eingabe.WeiterMitTaste();
                    return;
                }
                Console.WriteLine("Einträge:");
                for (int i = 0; i < alle.Count; i++)
                {
                    Console.WriteLine($"{i + 1}) {alle[i]}");
                }
                int sel = Eingabe.LiesGanzzahl("Eintrag wählen", 1, alle.Count);
                var id = alle[sel - 1].Id;
                Console.Write("Wirklich löschen? (j/n): ");
                var conf = (Console.ReadLine() ?? "").Trim().ToLower();
                if (conf is "j" or "ja")
                {
                    _zeitplan.Entfernen(id);
                    Console.WriteLine("Eintrag gelöscht.");
                }
            }
            else if (w == 2)
            {
                Console.Write("Wirklich ALLE zeitgesteuerten Einträge löschen? Zum Bestätigen 'ALLE' eingeben: ");
                var conf = (Console.ReadLine() ?? "").Trim();
                if (conf == "ALLE")
                {
                    _zeitplan.Leeren();
                    Console.WriteLine("Alle zeitgesteuerten Einträge wurden gelöscht.");
                }
                else
                {
                    Console.WriteLine("Abgebrochen.");
                }
            }

            Eingabe.WeiterMitTaste();
        }
    }
}