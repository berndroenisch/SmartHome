using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SmartHome.Typ;

namespace SmartHome.Daten
{
    public class ZeitplanDienst
    {
        private const string DateiName = "zeitplan.json";
        private static readonly JsonSerializerOptions Optionen = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public List<ZeitplanEintrag> Laden()
        {
            try
            {
                if (!File.Exists(DateiName)) return new List<ZeitplanEintrag>();
                var json = File.ReadAllText(DateiName);
                if (string.IsNullOrWhiteSpace(json)) return new List<ZeitplanEintrag>();
                return JsonSerializer.Deserialize<List<ZeitplanEintrag>>(json, Optionen) ?? new List<ZeitplanEintrag>();
            }
            catch
            {
                return new List<ZeitplanEintrag>();
            }
        }

        public void Speichern(List<ZeitplanEintrag> eintraege)
        {
            var json = JsonSerializer.Serialize(eintraege, Optionen);
            File.WriteAllText(DateiName, json);
        }

        public void Hinzufuegen(ZeitplanEintrag e)
        {
            var alle = Laden();
            alle.Add(e);
            Speichern(alle);
        }

        public void Aktualisieren(ZeitplanEintrag e)
        {
            var alle = Laden();
            var idx = alle.FindIndex(x => x.Id == e.Id);
            if (idx >= 0)
            {
                alle[idx] = e;
                Speichern(alle);
            }
        }

        public void Entfernen(Guid id)
        {
            var alle = Laden();
            alle.RemoveAll(x => x.Id == id);
            Speichern(alle);
        }

        // NEU: Alle Zeitpläne löschen
        public void Leeren()
        {
            Speichern(new List<ZeitplanEintrag>());
        }
    }
}