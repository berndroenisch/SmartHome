using System;
using System.Collections.Generic;
using System.Linq;
using SmartHome.Daten;
using SmartHome.Helfer;
using SmartHome.Typ;

namespace SmartHome.Menue
{
    public class VerlaufMenue
    {
        private readonly VerlaufDienst _verlauf;

        public VerlaufMenue(VerlaufDienst verlauf)
        {
            _verlauf = verlauf;
        }

        public void Start(Einrichtung e)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Verlauf ansehen/filtern");
                Console.WriteLine("1) Verlauf anzeigen/filtern");
                Console.WriteLine("0) Zurück");
                int aw = Eingabe.LiesGanzzahl("Auswahl", 0, 1);
                if (aw == 0) return;
                if (aw == 1) AnzeigenUndFiltern(e);
            }
        }

        private void AnzeigenUndFiltern(Einrichtung e)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Verlauf anzeigen/filtern - Filter setzen");

                // Filter erfassen (alle optional)
                // Raum
                var raumFilter = RaumFilter(e);
                // Typ
                var typFilter = TypFilter();
                // Datum
                var vonDatum = Eingabe.LiesOptionalesDatum("Startdatum");
                var bisDatum = Eingabe.LiesOptionalesDatum("Enddatum");
                // Uhrzeit
                var vonZeit = Eingabe.LiesOptionaleUhrzeit("Startzeit");
                var bisZeit = Eingabe.LiesOptionaleUhrzeit("Endzeit");
                // Auslöser
                Console.Write("Auslöser (leer=alle, z.B. 'manuell'): ");
                var ausloeser = (Console.ReadLine() ?? "").Trim().ToLower();
                if (ausloeser == "") ausloeser = "alle";

                var alle = _verlauf.Laden();

                IEnumerable<Verlaufseintrag> query = alle;

                if (raumFilter != null)
                    query = query.Where(x => x.RaumAbk.Equals(raumFilter, StringComparison.OrdinalIgnoreCase));

                if (typFilter != null)
                    query = query.Where(x => x.TypAbk.Equals(typFilter, StringComparison.OrdinalIgnoreCase));

                if (vonDatum.HasValue)
                {
                    var start = vonDatum.Value.Date;
                    query = query.Where(x => x.Zeitpunkt >= start);
                }
                if (bisDatum.HasValue)
                {
                    var ende = bisDatum.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(x => x.Zeitpunkt <= ende);
                }

                if (vonZeit.HasValue)
                    query = query.Where(x => x.Zeitpunkt.TimeOfDay >= vonZeit.Value);
                if (bisZeit.HasValue)
                    query = query.Where(x => x.Zeitpunkt.TimeOfDay <= bisZeit.Value);

                if (ausloeser != "alle")
                    query = query.Where(x => string.Equals(x.Ausloeser, ausloeser, StringComparison.OrdinalIgnoreCase));

                var ergebnis = query
                    .OrderByDescending(x => x.Zeitpunkt)
                    .ToList();

                Console.WriteLine();
                Console.WriteLine($"Gefundene Einträge: {ergebnis.Count}");

                // Saubere Spaltentrennung mit ausgerichteten Pipes
                string H(string s) => s ?? "";
                string P(string s, int w) => (s ?? "").PadRight(w);

                var header1 = "Datum/Uhrzeit";
                var header2 = "Bezeichnung";
                var header3 = "Aktion";
                var header4 = "Wert";
                var header5 = "Auslöser";

                int w1 = Math.Max(header1.Length, "dd.MM.yyyy HH:mm:ss".Length); // 19
                int w2 = Math.Max(header2.Length, ergebnis.Count == 0 ? 0 : ergebnis.Max(v => (v.Bezeichnung ?? "").Length));
                int w3 = Math.Max(header3.Length, ergebnis.Count == 0 ? 0 : ergebnis.Max(v => (v.Aktion ?? "").Length));
                int w4 = Math.Max(header4.Length, ergebnis.Count == 0 ? 0 : ergebnis.Max(v => (v.Wert ?? "").Length));
                int w5 = Math.Max(header5.Length, ergebnis.Count == 0 ? 0 : ergebnis.Max(v => (v.Ausloeser ?? "").Length));

                // Mindesbreite, damit Spalten nicht zu schmal sind
                w2 = Math.Max(w2, 12);
                w3 = Math.Max(w3, 8);
                w4 = Math.Max(w4, 6);
                w5 = Math.Max(w5, 9);

                string sep = $"{new string('-', w1)}-+-{new string('-', w2)}-+-{new string('-', w3)}-+-{new string('-', w4)}-+-{new string('-', w5)}";

                Console.WriteLine(sep);
                Console.WriteLine($"{P(header1, w1)} | {P(header2, w2)} | {P(header3, w3)} | {P(header4, w4)} | {P(header5, w5)}");
                Console.WriteLine(sep);

                foreach (var v in ergebnis)
                {
                    string c1 = v.Zeitpunkt.ToString("dd.MM.yyyy HH:mm:ss");
                    string c2 = H(v.Bezeichnung);
                    string c3 = H(v.Aktion);
                    string c4 = H(v.Wert);
                    string c5 = H(v.Ausloeser);

                    Console.WriteLine($"{P(c1, w1)} | {P(c2, w2)} | {P(c3, w3)} | {P(c4, w4)} | {P(c5, w5)}");
                }

                Console.WriteLine(sep);

                Console.WriteLine("1) Neue Filter setzen");
                Console.WriteLine("0) Zurück");
                int aw = Eingabe.LiesGanzzahl("Auswahl", 0, 1);
                if (aw == 0) return;
                // aw == 1 -> neue Filter setzen, Schleife wiederholt sich
            }
        }

        private string? RaumFilter(Einrichtung e)
        {
            var raeume = e.Raeume;
            Console.WriteLine("Räume filtern:");
            Console.WriteLine("0) Alle");
            for (int i = 0; i < raeume.Count; i++)
            {
                Console.WriteLine($"{i + 1}) {raeume[i].RaumName} ({raeume[i].RaumAbk})");
            }
            int w = Eingabe.LiesGanzzahl("Auswahl", 0, raeume.Count);
            if (w == 0) return null;
            return raeume[w - 1].RaumAbk;
        }

        private string? TypFilter()
        {
            var keys = Katalog.Geraetetypen.Keys.ToList();
            Console.WriteLine("Gerätetypen filtern:");
            Console.WriteLine("0) Alle");
            for (int i = 0; i < keys.Count; i++)
            {
                var abk = keys[i];
                Console.WriteLine($"{i + 1}) {Katalog.Geraetetypen[abk].Name} ({abk})");
            }
            int w = Eingabe.LiesGanzzahl("Auswahl", 0, keys.Count);
            if (w == 0) return null;
            return keys[w - 1];
        }
    }
}