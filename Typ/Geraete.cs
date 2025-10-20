namespace SmartHome.Typ
{
    public class Geraete
    {
        public string TypAbk { get; set; } = "";   // z.B. LE, HK, SD, RO
        public string TypName { get; set; } = "";  // z.B. Leuchten, Heizkörper, ...
        public string Name { get; set; } = "";     // frei wählbar (Buchstaben/Zahlen)

        // Zustände für Regelung
        public bool? Ein { get; set; }             // LE, SD
        public int? DimProzent { get; set; }       // LE: 5..100
        public int? Stufe { get; set; }            // HK: 0..5
        public double? Temperatur { get; set; }    // HK: 5..28 (abgeleitet aus Stufe)
        public int? PositionProzent { get; set; }  // RO: 0..100
    }
}