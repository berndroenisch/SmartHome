using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SmartHome.Typ;

namespace SmartHome.Daten
{
    public class VerlaufDienst
    {
        private const string DateiName = "verlauf.json";
        private static readonly JsonSerializerOptions Optionen = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public List<Verlaufseintrag> Laden()
        {
            try
            {
                if (!File.Exists(DateiName)) return new List<Verlaufseintrag>();
                var json = File.ReadAllText(DateiName);
                if (string.IsNullOrWhiteSpace(json)) return new List<Verlaufseintrag>();
                return JsonSerializer.Deserialize<List<Verlaufseintrag>>(json, Optionen) ?? new List<Verlaufseintrag>();
            }
            catch
            {
                return new List<Verlaufseintrag>();
            }
        }

        public void Speichern(List<Verlaufseintrag> eintraege)
        {
            var json = JsonSerializer.Serialize(eintraege, Optionen);
            File.WriteAllText(DateiName, json);
        }

        public void Hinzufuegen(Verlaufseintrag eintrag)
        {
            var alle = Laden();
            alle.Add(eintrag);
            Speichern(alle);
        }

        // Verlauf vollständig löschen
        public void Leeren()
        {
            try
            {
                if (File.Exists(DateiName))
                {
                    File.Delete(DateiName);
                }
            }
            catch
            {
                // Fallback: Datei überschreiben, falls Löschen fehlschlägt
                try
                {
                    Speichern(new List<Verlaufseintrag>());
                }
                catch
                {
                    // Ignorieren: Wenn auch das fehlschlägt, gibt es nichts weiter zu tun
                }
            }
        }
    }
}