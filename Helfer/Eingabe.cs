using System;

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

        public static void WeiterMitTaste()
        {
            Console.WriteLine();
            Console.WriteLine("Weiter mit Eingabetaste ...");
            Console.ReadLine();
        }
    }
}