using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SmartHome.Typ;

namespace SmartHome.Daten
{
    public class MakroDienst
    {
        private const string DateiName = "makros.json";
        private static readonly JsonSerializerOptions Optionen = new JsonSerializerOptions { WriteIndented = true };

        public List<Makro> Laden()
        {
            try
            {
                if (!File.Exists(DateiName)) return new List<Makro>();
                var json = File.ReadAllText(DateiName);
                if (string.IsNullOrWhiteSpace(json)) return new List<Makro>();
                return JsonSerializer.Deserialize<List<Makro>>(json, Optionen) ?? new List<Makro>();
            }
            catch
            {
                return new List<Makro>();
            }
        }

        public void Speichern(List<Makro> makros)
        {
            var json = JsonSerializer.Serialize(makros, Optionen);
            File.WriteAllText(DateiName, json);
        }

        public void SpeichernNeuOderAktualisieren(Makro m)
        {
            var alle = Laden();
            var idx = alle.FindIndex(x => string.Equals(x.Name, m.Name, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) alle[idx] = m; else alle.Add(m);
            Speichern(alle);
        }

        public Makro? Finde(string name)
        {
            return Laden().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        // NEU: Einzelnes Makro löschen
        public bool Entfernen(string name)
        {
            var alle = Laden();
            int removed = alle.RemoveAll(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0) Speichern(alle);
            return removed > 0;
        }

        // NEU: Alle Makros löschen
        public void Leeren()
        {
            Speichern(new List<Makro>());
        }
    }
}