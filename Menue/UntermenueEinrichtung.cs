using System;
using System.Linq;
using SmartHome.Daten;
using SmartHome.Helfer;
using SmartHome.Typ;

namespace SmartHome.Menue
{
    public class UntermenueEinrichtung
    {
        private readonly SpeicherDienst _speicher;

        public UntermenueEinrichtung(SpeicherDienst speicher)
        {
            _speicher = speicher;
        }

        public void Start(Einrichtung einrichtung)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1) Räume ansehen");
                Console.WriteLine("2) Räume bearbeiten");
                Console.WriteLine("3) Gerätetypen ansehen");
                Console.WriteLine("4) Gerätetypen bearbeiten");
                Console.WriteLine("5) Gerätebezeichnungen ansehen");
                Console.WriteLine("6) Gerätebezeichnungen bearbeiten");
                Console.WriteLine("0) Zurück");
                Console.Write("Auswahl: ");
                var wahl = (Console.ReadLine() ?? "").Trim();

                switch (wahl)
                {
                    case "1":
                        RaeumeAnsehen(einrichtung);
                        break;
                    case "2":
                        RaeumeBearbeiten(einrichtung);
                        break;
                    case "3":
                        GeraetetypenAnsehen(einrichtung);
                        break;
                    case "4":
                        GeraetetypenBearbeiten(einrichtung);
                        break;
                    case "5":
                        GeraetebezeichnungenAnsehen(einrichtung);
                        break;
                    case "6":
                        GeraetebezeichnungenBearbeiten(einrichtung);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Ungültige Auswahl.");
                        Eingabe.WeiterMitTaste();
                        break;
                }
            }
        }

        private void RaeumeAnsehen(Einrichtung e)
        {
            Console.Clear();
            Console.WriteLine("Räume:");
            for (int i = 0; i < e.Raeume.Count; i++)
            {
                var r = e.Raeume[i];
                Console.WriteLine($"{i + 1}) {r.RaumName} ({r.RaumAbk})");
            }
            Eingabe.WeiterMitTaste();
        }

        private void RaeumeBearbeiten(Einrichtung e)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Räume bearbeiten");
                Console.WriteLine("----------------");
                Console.WriteLine("1) Raumdefinition wechseln");
                Console.WriteLine("2) Raum hinzufügen");
                Console.WriteLine("3) Raum löschen");
                Console.WriteLine("0) Zurück");
                Console.Write("Auswahl: ");
                var wahl = (Console.ReadLine() ?? "").Trim();

                switch (wahl)
                {
                    case "1":
                        RaeumedefinitionWechseln(e);
                        break;
                    case "2":
                        RaumHinzufuegen(e);
                        break;
                    case "3":
                        RaumLoeschen(e);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Ungültige Auswahl.");
                        Eingabe.WeiterMitTaste();
                        break;
                }
            }
        }

        private void RaeumedefinitionWechseln(Einrichtung e)
        {
            Console.Clear();
            Console.WriteLine("Räume bearbeiten (Wechsel der Raumdefinition, sofern frei)");
            for (int i = 0; i < e.Raeume.Count; i++)
            {
                var r = e.Raeume[i];
                Console.WriteLine($"{i + 1}) {r.RaumName} ({r.RaumAbk})");
            }
            if (e.Raeume.Count == 0)
            {
                Console.WriteLine("Keine Räume vorhanden.");
                Eingabe.WeiterMitTaste();
                return;
            }

            int index = Eingabe.LiesGanzzahl("Nummer des Raums wählen", 1, e.Raeume.Count) - 1;
            var raum = e.Raeume[index];

            var bereitsVerwendet = e.Raeume.Select(r => r.RaumAbk).ToHashSet(StringComparer.OrdinalIgnoreCase);
            Console.WriteLine("Verfügbare Raumdefinitionen (Abk. = Name):");
            foreach (var kv in Katalog.VerfuegbareRaeume)
            {
                bool frei = !bereitsVerwendet.Contains(kv.Key) || kv.Key == raum.RaumAbk;
                Console.WriteLine($"  {kv.Key} = {kv.Value}" + (frei ? "" : " (vergeben)"));
            }

            string neueAbk = Eingabe.LiesAuswahlMitAbk(
                "Neue Abkürzung wählen",
                s => Katalog.VerfuegbareRaeume.ContainsKey(s) && (!bereitsVerwendet.Contains(s) || s == raum.RaumAbk),
                "Abkürzung ungültig oder bereits vergeben."
            );

            raum.RaumAbk = neueAbk;
            raum.RaumName = Katalog.VerfuegbareRaeume[neueAbk];

            _speicher.Speichern(e);
            Console.WriteLine("Raum aktualisiert und gespeichert.");
            Eingabe.WeiterMitTaste();
        }

        private void RaumHinzufuegen(Einrichtung e)
        {
            Console.Clear();
            Console.WriteLine("Raum hinzufügen");
            Console.WriteLine("---------------");

            const int maxAnzahl = 9;
            if (e.Raeume.Count >= maxAnzahl)
            {
                Console.WriteLine($"Maximale Anzahl von {maxAnzahl} Räumen erreicht.");
                Eingabe.WeiterMitTaste();
                return;
            }

            var verwendet = e.Raeume.Select(r => r.RaumAbk).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var verfuegbare = Katalog.VerfuegbareRaeume.Keys.Where(k => !verwendet.Contains(k)).ToList();

            if (verfuegbare.Count == 0)
            {
                Console.WriteLine("Keine weiteren Raumdefinitionen verfügbar.");
                Eingabe.WeiterMitTaste();
                return;
            }

            Console.WriteLine("Verfügbare Raumdefinitionen (Abk. = Name):");
            foreach (var k in verfuegbare)
            {
                Console.WriteLine($"  {k} = {Katalog.VerfuegbareRaeume[k]}");
            }

            string abk = Eingabe.LiesAuswahlMitAbk(
                "Abkürzung des neuen Raums",
                s => verfuegbare.Contains(s),
                "Abkürzung ungültig oder bereits vergeben."
            );

            e.Raeume.Add(new Raum
            {
                RaumAbk = abk,
                RaumName = Katalog.VerfuegbareRaeume[abk]
            });

            _speicher.Speichern(e);
            Console.WriteLine("Neuer Raum hinzugefügt und gespeichert.");
            Eingabe.WeiterMitTaste();
        }

        private void RaumLoeschen(Einrichtung e)
        {
            Console.Clear();
            Console.WriteLine("Raum löschen");
            Console.WriteLine("------------");

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

            int index = Eingabe.LiesGanzzahl("Nummer des zu löschenden Raums wählen", 1, e.Raeume.Count) - 1;
            var raum = e.Raeume[index];

            Console.Write($"Raum '{raum.RaumName} ({raum.RaumAbk})' wirklich löschen? (j/n): ");
            var bestaetigung = (Console.ReadLine() ?? "").Trim().ToLower();
            if (bestaetigung == "j" || bestaetigung == "ja")
            {
                e.Raeume.RemoveAt(index);
                _speicher.Speichern(e);
                Console.WriteLine("Raum gelöscht und gespeichert.");
            }
            else
            {
                Console.WriteLine("Löschen abgebrochen.");
            }

            Eingabe.WeiterMitTaste();
        }

        private void GeraetetypenAnsehen(Einrichtung e)
        {
            Console.Clear();
            Console.WriteLine("Gerätetypen (Anzahlen je Raum):");
            foreach (var raum in e.Raeume)
            {
                Console.WriteLine($"{raum.RaumName} ({raum.RaumAbk}):");
                foreach (var kv in Katalog.Geraetetypen)
                {
                    var anzahl = raum.Geraete.Count(g => g.TypAbk == kv.Key);
                    Console.WriteLine($"  {kv.Value.Name} ({kv.Key}): {anzahl}");
                }
            }
            Eingabe.WeiterMitTaste();
        }

        private void GeraetetypenBearbeiten(Einrichtung e)
        {
            Console.Clear();
            Console.WriteLine("Gerätetypen bearbeiten (Anzahl je Raum anpassen)");
            for (int i = 0; i < e.Raeume.Count; i++)
            {
                var r = e.Raeume[i];
                Console.WriteLine($"{i + 1}) {r.RaumName} ({r.RaumAbk})");
            }
            int index = Eingabe.LiesGanzzahl("Nummer des Raums wählen", 1, e.Raeume.Count) - 1;
            var raum = e.Raeume[index];

            foreach (var kv in Katalog.Geraetetypen)
            {
                var abk = kv.Key;
                var (name, max) = kv.Value;
                int aktuell = raum.Geraete.Count(g => g.TypAbk == abk);
                Console.WriteLine($"  {name} ({abk}) aktuell: {aktuell}, max: {max}");
                int neu = Eingabe.LiesGanzzahl($"    Neue Anzahl für {name}", 0, max);

                if (neu > aktuell)
                {
                    int diff = neu - aktuell;
                    for (int i = 0; i < diff; i++)
                    {
                        raum.Geraete.Add(new Geraete { TypAbk = abk, TypName = name, Name = "" });
                    }
                }
                else if (neu < aktuell)
                {
                    int diff = aktuell - neu;
                    var liste = raum.Geraete.Where(g => g.TypAbk == abk).ToList();
                    for (int i = 0; i < diff && liste.Count > 0; i++)
                    {
                        var zuEntfernen = liste.Last();
                        raum.Geraete.Remove(zuEntfernen);
                        liste.RemoveAt(liste.Count - 1);
                    }
                }
            }

            _speicher.Speichern(e);
            Console.WriteLine("Anpassungen gespeichert.");

            var ohneNamen = raum.Geraete.Where(g => string.IsNullOrWhiteSpace(g.Name)).ToList();
            if (ohneNamen.Any())
            {
                Console.WriteLine("Es gibt Geräte ohne Namen. Jetzt benennen?");
                Console.WriteLine("  1) Ja");
                Console.WriteLine("  2) Nein");
                Console.Write("Auswahl: ");
                if ((Console.ReadLine() ?? "").Trim() == "1")
                {
                    foreach (var kv in Katalog.Geraetetypen)
                    {
                        var abk = kv.Key;
                        var name = kv.Value.Name;
                        var liste = raum.Geraete.Where(g => g.TypAbk == abk && string.IsNullOrWhiteSpace(g.Name)).ToList();
                        for (int i = 0; i < liste.Count; i++)
                        {
                            while (true)
                            {
                                string eingabe = Eingabe.LiesNichtLeer($"  Name für {name} (neu) #{i + 1}");
                                if (raum.GeraetenameIstFrei(abk, eingabe))
                                {
                                    liste[i].Name = eingabe;
                                    break;
                                }
                                Console.WriteLine("  Name im Raum für diesen Gerätetyp bereits vergeben, bitte anderen wählen.");
                            }
                        }
                    }
                    _speicher.Speichern(e);
                    Console.WriteLine("Namen gespeichert.");
                }
            }

            Eingabe.WeiterMitTaste();
        }

        private void GeraetebezeichnungenAnsehen(Einrichtung e)
        {
            Console.Clear();
            Console.WriteLine("Gerätebezeichnungen:");
            foreach (var raum in e.Raeume)
            {
                Console.WriteLine($"{raum.RaumName} ({raum.RaumAbk}):");
                foreach (var kv in Katalog.Geraetetypen)
                {
                    var liste = raum.Geraete.Where(g => g.TypAbk == kv.Key).ToList();
                    for (int i = 0; i < liste.Count; i++)
                    {
                        var ge = liste[i];
                        string bezeichnung = Katalog.Geraetebezeichnung(raum.RaumAbk, ge.TypAbk, ge.Name);
                        Console.WriteLine($"  - {bezeichnung}");
                    }
                }
            }
            Eingabe.WeiterMitTaste();
        }

        private void GeraetebezeichnungenBearbeiten(Einrichtung e)
        {
            Console.Clear();
            Console.WriteLine("Gerätebezeichnungen bearbeiten (Namen ändern)");
            for (int i = 0; i < e.Raeume.Count; i++)
            {
                var r = e.Raeume[i];
                Console.WriteLine($"{i + 1}) {r.RaumName} ({r.RaumAbk})");
            }
            int index = Eingabe.LiesGanzzahl("Nummer des Raums wählen", 1, e.Raeume.Count) - 1;
            var raum = e.Raeume[index];

            var alle = raum.Geraete.Select((g, idx) => new { Index = idx + 1, Geraet = g }).ToList();
            if (alle.Count == 0)
            {
                Console.WriteLine("Keine Geräte im Raum vorhanden.");
                Eingabe.WeiterMitTaste();
                return;
            }
            foreach (var eintrag in alle)
            {
                string bez = Katalog.Geraetebezeichnung(raum.RaumAbk, eintrag.Geraet.TypAbk, eintrag.Geraet.Name);
                Console.WriteLine($"{eintrag.Index}) {eintrag.Geraet.TypName} ({eintrag.Geraet.TypAbk}) - {bez}");
            }
            int auswahl = Eingabe.LiesGanzzahl("Gerätenummer wählen", 1, alle.Count);
            var ziel = alle[auswahl - 1].Geraet;

            while (true)
            {
                string neu = Eingabe.LiesNichtLeer("Neuer Gerätename");
                if (raum.GeraetenameIstFrei(ziel.TypAbk, neu) ||
                    string.Equals(neu.Trim(), ziel.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ziel.Name = neu;
                    break;
                }
                Console.WriteLine("Name im Raum für diesen Gerätetyp bereits vergeben, bitte anderen wählen.");
            }

            _speicher.Speichern(e);
            Console.WriteLine("Gerätename aktualisiert und gespeichert.");
            Eingabe.WeiterMitTaste();
        }
    }
}