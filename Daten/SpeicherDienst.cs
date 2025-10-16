using System;
using System.IO;
using System.Text.Json;
using SmartHome.Typ;

namespace SmartHome.Daten
{
    public class SpeicherDienst
    {
        private const string DateiName = "einrichtung.json";
        private static readonly JsonSerializerOptions Optionen = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public Einrichtung? Laden()
        {
            try
            {
                if (!File.Exists(DateiName)) return null;
                var json = File.ReadAllText(DateiName);
                if (string.IsNullOrWhiteSpace(json)) return null;
                return JsonSerializer.Deserialize<Einrichtung>(json, Optionen);
            }
            catch
            {
                return null;
            }
        }

        public void Speichern(Einrichtung einrichtung)
        {
            var json = JsonSerializer.Serialize(einrichtung, Optionen);
            File.WriteAllText(DateiName, json);
        }
    }
}