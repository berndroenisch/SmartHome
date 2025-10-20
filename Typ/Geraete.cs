namespace SmartHome.Typ
{
    public class Geraete
    {
        public string TypAbk { get; set; } = "";   // z.B. LE, HK, SD, RO
        public string TypName { get; set; } = "";  // z.B. Leuchten, Heizkörper, ...
        public string Name { get; set; } = "";     // frei wählbar (Buchstaben/Zahlen)
    }
}