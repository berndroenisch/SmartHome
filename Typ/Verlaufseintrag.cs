using System;

namespace SmartHome.Typ
{
    public class Verlaufseintrag
    {
        public DateTime Zeitpunkt { get; set; }
        public string Ausloeser { get; set; } = "manuell"; // z.B. manuell, automatisiert (zukünftig)
        public string RaumAbk { get; set; } = "";
        public string TypAbk { get; set; } = "";
        public string Geraetename { get; set; } = "";
        public string Aktion { get; set; } = "";        // z.B. toggle, set_dim, set_stage, set_position
        public string Wert { get; set; } = "";          // z.B. "Ein", "Aus", "75%", "Stufe 3 (20 °C)", "Position 50%"
        public string Bezeichnung { get; set; } = "";   // z.B. WZ-LE-Decke
    }
}