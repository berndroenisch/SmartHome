using System;
using System.Collections.Generic;

namespace SmartHome.Typ
{
    public class Makro
    {
        public string Name { get; set; } = "";
        public List<MakroSchritt> Schritte { get; set; } = new();
    }

    public class MakroSchritt
    {
        // Zielgerät
        public string RaumAbk { get; set; } = "";
        public string TypAbk { get; set; } = "";
        public string Geraetename { get; set; } = "";

        // Aktion
        public GeraeteAktion Aktion { get; set; }
        public int? Wert { get; set; } // Dim 5..100, Stage 0..5, Position 0..100

        // Wartezeit bis zum nächsten Schritt (Sekunden)
        public int WarteSekunden { get; set; } = 0;
    }
}