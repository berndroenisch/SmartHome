using System;
using System.Collections.Generic;

namespace SmartHome.Typ
{
    public static class Katalog
    {
        // Raumdefinitionen: Abk. -> Vollname (sichtbarer Text mit Umlauten bleibt)
        public static readonly Dictionary<string, string> VerfuegbareRaeume = new()
        {
            ["WZ"] = "Wohnzimmer",
            ["KZ"] = "Kinderzimmer",
            ["SZ"] = "Schlafzimmer",
            ["BD"] = "Bad",
            ["BU"] = "Büro",
            ["TO"] = "Toilette",
            ["FL"] = "Flur",
            ["KU"] = "Küche",
            ["HR"] = "Hauswirtschaftsraum",
            ["AK"] = "Abstellkammer"
        };

        // Gerätetypen: Abk. -> (Vollname, Max je Raum)
        public static readonly Dictionary<string, (string Name, int MaxJeRaum)> Geraetetypen = new()
        {
            ["LE"] = ("Leuchten", 5),
            ["HK"] = ("Heizkörper", 2),
            ["SD"] = ("Steckdosen", 10),
            ["RO"] = ("Rollläden", 3)
        };

        // HK Stufen -> Temperatur
        public static readonly Dictionary<int, double> HK_StufenTemperatur = new()
        {
            [0] = 5,
            [1] = 12,
            [2] = 16,
            [3] = 20,
            [4] = 24,
            [5] = 28
        };

        public static double TemperaturFuerStufe(int stufe)
        {
            stufe = Math.Clamp(stufe, 0, 5);
            return HK_StufenTemperatur[stufe];
        }

        public static string Geraetebezeichnung(string raumAbk, string geraeteTypAbk, string geraetename)
            => $"{raumAbk}-{geraeteTypAbk}-{geraetename}";

        public static void SetzeStandardZustand(Geraete g)
        {
            switch (g.TypAbk)
            {
                case "LE":
                    g.Ein = false;
                    g.DimProzent = 100; // 5..100, beim Einschalten bleibt letzter Wert erhalten
                    break;
                case "HK":
                    g.Stufe = 3;
                    g.Temperatur = TemperaturFuerStufe(3);
                    break;
                case "SD":
                    g.Ein = false;
                    break;
                case "RO":
                    g.PositionProzent = 0;
                    break;
            }
        }

        public static int BegrenztProzent(int wert, int min, int max)
        {
            if (wert < min) return min;
            if (wert > max) return max;
            return wert;
        }
    }
}