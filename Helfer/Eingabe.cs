using System;
using System.Globalization;

namespace SmartHome.Helfer
{
    public static class Eingabe
    {
        public static int LiesGanzzahl(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write($"{prompt} ({min}-{max}): ");
                var s = Console.ReadLine();
                if (int.TryParse(s, out int zahl) && zahl >= min && zahl <= max)
                    return zahl;

                Console.WriteLine("Ungültige Eingabe. Bitte erneut versuchen.");
            }
        }

        public static string LiesAuswahlMitAbk(string prompt, Func<string, bool> validator, string fehlerHinweis)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpper();
                if (validator(s))
                    return s;

                Console.WriteLine(fehlerHinweis);
            }
        }

        public static string LiesNichtLeer(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                var s = (Console.ReadLine() ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(s))
                    return s;
                Console.WriteLine("Bitte einen Wert eingeben.");
            }
        }

        public static DateTime? LiesOptionalesDatum(string prompt)
        {
            Console.Write($"{prompt} (DD.MM.YYYY, leer = kein Filter): ");
            var s = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrEmpty(s)) return null;
            if (DateTime.TryParseExact(s, "dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var d))
                return d.Date;
            Console.WriteLine("Ungültiges Datum. Filter wird ignoriert.");
            return null;
        }

        public static TimeSpan? LiesOptionaleUhrzeit(string prompt)
        {
            Console.Write($"{prompt} (HH:MM, leer = kein Filter): ");
            var s = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrEmpty(s)) return null;
            if (TimeSpan.TryParseExact(s, "hh\\:mm", CultureInfo.InvariantCulture, out var t))
                return t;
            if (TimeSpan.TryParse(s, out t))
                return t;
            Console.WriteLine("Ungültige Uhrzeit. Filter wird ignoriert.");
            return null;
        }

        public static void WeiterMitTaste()
        {
            Console.WriteLine();
            Console.WriteLine("Weiter mit Eingabetaste ...");
            Console.ReadLine();
        }
    }
}