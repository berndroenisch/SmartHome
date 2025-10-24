using System;
using System.Text;
using SmartHome.Helfer;

namespace SmartHome.Typ
{
    [Flags]
    public enum Wochentage
    {
        None = 0,
        Montag = 1 << 0,
        Dienstag = 1 << 1,
        Mittwoch = 1 << 2,
        Donnerstag = 1 << 3,
        Freitag = 1 << 4,
        Samstag = 1 << 5,
        Sonntag = 1 << 6,

        Werktage = Montag | Dienstag | Mittwoch | Donnerstag | Freitag,
        Wochenende = Samstag | Sonntag,
        Alle = Werktage | Wochenende
    }

    public enum ZeitplanZielArt
    {
        Geraet,
        Makro
    }

    public enum GeraeteAktion
    {
        Toggle = 0,
        SetDim = 1,
        SetStage = 2,
        SetPosition = 3,
        SetOn = 4,
        SetOff = 5
    }

    public class ZeitplanEintrag
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public ZeitplanZielArt ZielArt { get; set; } = ZeitplanZielArt.Geraet;

        public string RaumAbk { get; set; } = "";
        public string TypAbk { get; set; } = "";
        public string Geraetename { get; set; } = "";

        public string MakroName { get; set; } = "";

        public Wochentage Tage { get; set; } = Wochentage.Montag;
        public TimeSpan StartZeit { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan EndZeit { get; set; } = new TimeSpan(17, 0, 0);

        public GeraeteAktion? Aktion { get; set; }
        public int? Wert { get; set; }

        public bool EndAktionAktiv { get; set; } = false;
        public GeraeteAktion? EndAktion { get; set; }
        public int? EndWert { get; set; }

        public bool Aktiv { get; set; } = true;

        public DateTime? ZuletztStart { get; set; }
        public DateTime? ZuletztEnde { get; set; }

        // Legacy-Felder
        public DateTime Zeitpunkt { get; set; }
        public bool TaeglichWiederholen { get; set; } = false;
        public DateTime? ZuletztAusgefuehrt { get; set; }
        public bool Erledigt { get; set; } = false;

        public override string ToString()
        {
            if (ZielArt == ZeitplanZielArt.Makro)
            {
                return $"[Makro] {MakroName} | {TageKurz()} {StartZeit:hh\\:mm}-{EndZeit:hh\\:mm}";
            }

            // Wiederkehrendes Zeitfenster
            if (StartZeit != default || EndZeit != default)
            {
                var startStr = Anzeige.AktionMitWert(Aktion, Wert);
                var endeStr = EndAktionAktiv ? $" -> Ende: {Anzeige.AktionMitWert(EndAktion, EndWert)}" : "";
                return $"{RaumAbk}-{TypAbk}-{Geraetename} | {TageKurz()} {StartZeit:hh\\:mm}-{EndZeit:hh\\:mm} | Start: {startStr}{endeStr}";
            }

            // Legacy-Ausgabe
            string rep = TaeglichWiederholen ? " (täglich)" : "";
            var akt = Anzeige.AktionMitWert(Aktion, Wert);
            return $"{RaumAbk}-{TypAbk}-{Geraetename} {akt} @ {Zeitpunkt:g}{rep}";
        }

        private string TageKurz()
        {
            if (Tage == Wochentage.Alle) return "Mo-So";
            if (Tage == Wochentage.Werktage) return "Mo-Fr";
            if (Tage == Wochentage.Wochenende) return "Sa-So";

            StringBuilder sb = new StringBuilder();
            void add(string s) { if (sb.Length > 0) sb.Append(','); sb.Append(s); }
            if (Tage.HasFlag(Wochentage.Montag)) add("Mo");
            if (Tage.HasFlag(Wochentage.Dienstag)) add("Di");
            if (Tage.HasFlag(Wochentage.Mittwoch)) add("Mi");
            if (Tage.HasFlag(Wochentage.Donnerstag)) add("Do");
            if (Tage.HasFlag(Wochentage.Freitag)) add("Fr");
            if (Tage.HasFlag(Wochentage.Samstag)) add("Sa");
            if (Tage.HasFlag(Wochentage.Sonntag)) add("So");
            return sb.ToString();
        }
    }
}