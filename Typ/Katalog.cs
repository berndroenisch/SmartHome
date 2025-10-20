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

        // Gerätetypen: Abk. -> (Vollname, Max je Raum) (sichtbarer Text mit Umlauten bleibt)
        public static readonly Dictionary<string, (string Name, int MaxJeRaum)> Geraetetypen = new()
        {
            ["LE"] = ("Leuchten", 5),
            ["HK"] = ("Heizkörper", 2),
            ["SD"] = ("Steckdosen", 10),
            ["RO"] = ("Rollläden", 3)
        };

        public static string Geraetebezeichnung(string raumAbk, string geraeteTypAbk, string geraetename)
            => $"{raumAbk}-{geraeteTypAbk}-{geraetename}";
    }
}