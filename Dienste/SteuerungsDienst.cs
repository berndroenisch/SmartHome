using System;
using System.Linq;
using System.Threading;
using SmartHome.Daten;
using SmartHome.Typ;

namespace SmartHome.Dienste
{
    public class SteuerungsDienst
    {
        private readonly SpeicherDienst _speicher;
        private readonly VerlaufDienst _verlauf;

        public SteuerungsDienst(SpeicherDienst speicher, VerlaufDienst verlauf)
        {
            _speicher = speicher;
            _verlauf = verlauf;
        }

        public bool FuehreGeraeteAktionAus(Einrichtung e, string raumAbk, string typAbk, string geraetename, GeraeteAktion aktion, int? wert, string ausloeser, string? bezeichnungPraefix = null)
        {
            var raum = e.Raeume.FirstOrDefault(r => string.Equals(r.RaumAbk, raumAbk, StringComparison.OrdinalIgnoreCase));
            if (raum == null) return false;
            var g = raum.Geraete.FirstOrDefault(x => string.Equals(x.TypAbk, typAbk, StringComparison.OrdinalIgnoreCase) &&
                                                     string.Equals(x.Name, geraetename, StringComparison.OrdinalIgnoreCase));
            if (g == null) return false;

            switch (typAbk)
            {
                case "LE":
                    if (aktion == GeraeteAktion.Toggle)
                    {
                        g.Ein = !(g.Ein ?? false);
                        Protokolliere(raumAbk, g, "Ein/Aus", g.Ein == true ? "Ein" : "Aus", ausloeser, bezeichnungPraefix);
                    }
                    else if (aktion == GeraeteAktion.SetOn)
                    {
                        g.Ein = true;
                        Protokolliere(raumAbk, g, "Ein/Aus", "Ein", ausloeser, bezeichnungPraefix);
                    }
                    else if (aktion == GeraeteAktion.SetOff)
                    {
                        g.Ein = false;
                        Protokolliere(raumAbk, g, "Ein/Aus", "Aus", ausloeser, bezeichnungPraefix);
                    }
                    else if (aktion == GeraeteAktion.SetDim && wert.HasValue)
                    {
                        int neu = Math.Clamp(wert.Value, 5, 100);
                        g.DimProzent = neu;
                        Protokolliere(raumAbk, g, "Helligkeit angepasst", $"{neu}%", ausloeser, bezeichnungPraefix);
                    }
                    else return false;
                    break;

                case "HK":
                    if (aktion != GeraeteAktion.SetStage || !wert.HasValue) return false;
                    int stufe = Math.Clamp(wert.Value, 0, 5);
                    g.Stufe = stufe;
                    g.Temperatur = Katalog.TemperaturFuerStufe(stufe);
                    Protokolliere(raumAbk, g, "Temp. angepasst", $"Stufe {stufe} ({g.Temperatur:0.#} °C)", ausloeser, bezeichnungPraefix);
                    break;

                case "SD":
                    if (aktion == GeraeteAktion.Toggle)
                    {
                        g.Ein = !(g.Ein ?? false);
                        Protokolliere(raumAbk, g, "Ein/Aus", g.Ein == true ? "Ein" : "Aus", ausloeser, bezeichnungPraefix);
                    }
                    else if (aktion == GeraeteAktion.SetOn)
                    {
                        g.Ein = true;
                        Protokolliere(raumAbk, g, "Ein/Aus", "Ein", ausloeser, bezeichnungPraefix);
                    }
                    else if (aktion == GeraeteAktion.SetOff)
                    {
                        g.Ein = false;
                        Protokolliere(raumAbk, g, "Ein/Aus", "Aus", ausloeser, bezeichnungPraefix);
                    }
                    else return false;
                    break;

                case "RO":
                    if (aktion != GeraeteAktion.SetPosition || !wert.HasValue) return false;
                    int pos = Math.Clamp(wert.Value, 0, 100);
                    g.PositionProzent = pos;
                    Protokolliere(raumAbk, g, "Schließstellung angepasst", $"Position {pos}%", ausloeser, bezeichnungPraefix);
                    break;

                default:
                    return false;
            }

            _speicher.Speichern(e);
            return true;
        }

        public void FuehreMakroAus(Einrichtung e, Makro makro)
        {
            new Thread(() =>
            {
                foreach (var s in makro.Schritte)
                {
                    FuehreGeraeteAktionAus(e, s.RaumAbk, s.TypAbk, s.Geraetename, s.Aktion, s.Wert, $"Makro-{makro.Name}", bezeichnungPraefix: "MG-");
                    if (s.WarteSekunden > 0) Thread.Sleep(TimeSpan.FromSeconds(s.WarteSekunden));
                }
            })
            { IsBackground = true }.Start();
        }

        private void Protokolliere(string raumAbk, Geraete g, string aktion, string wert, string ausloeser, string? praefix)
        {
            var bezeichnung = Katalog.Geraetebezeichnung(raumAbk, g.TypAbk, g.Name);
            if (!string.IsNullOrWhiteSpace(praefix))
                bezeichnung = $"{praefix}{bezeichnung}";

            var eintrag = new Verlaufseintrag
            {
                Zeitpunkt = DateTime.Now,
                Ausloeser = ausloeser,
                RaumAbk = raumAbk,
                TypAbk = g.TypAbk,
                Geraetename = g.Name,
                Aktion = aktion,
                Wert = wert,
                Bezeichnung = bezeichnung
            };
            _verlauf.Hinzufuegen(eintrag);
        }
    }
}