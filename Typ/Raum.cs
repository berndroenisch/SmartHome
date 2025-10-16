using System.Collections.Generic;
using System.Linq;

namespace SmartHome.Typ
{
    public class Raum
    {
        public string RaumAbk { get; set; } = "";
        public string RaumName { get; set; } = "";
        public List<Geraete> Geraete { get; set; } = new List<Geraete>();

        public int AnzahlGeraeteVomTyp(string typAbk) => Geraete.Count(g => g.TypAbk == typAbk);

        // Gerätename muss nur innerhalb desselben Gerätetyps im Raum eindeutig sein
        public bool GeraetenameIstFrei(string typAbk, string name)
        {
            return !Geraete.Any(g =>
                g.TypAbk == typAbk &&
                string.Equals(g.Name.Trim(), name.Trim(), System.StringComparison.OrdinalIgnoreCase));
        }
    }
}