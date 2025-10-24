using SmartHome.Typ;

namespace SmartHome.Helfer
{
    public static class Anzeige
    {
        public static string AktionLabel(GeraeteAktion aktion)
        {
            return aktion switch
            {
                GeraeteAktion.SetOn => "Ein",
                GeraeteAktion.SetOff => "Aus",
                GeraeteAktion.SetStage => "Stufe",
                GeraeteAktion.SetPosition => "Position",
                GeraeteAktion.SetDim => "Helligkeit",
                GeraeteAktion.Toggle => "Umschalten",
                _ => aktion.ToString()
            };
        }

        public static string AktionMitWert(GeraeteAktion? aktion, int? wert)
        {
            if (!aktion.HasValue) return "-";
            var label = AktionLabel(aktion.Value);
            return aktion.Value switch
            {
                GeraeteAktion.SetDim => wert.HasValue ? $"{label} {wert.Value}%" : label,
                GeraeteAktion.SetStage => wert.HasValue ? $"{label} {wert.Value}" : label,
                GeraeteAktion.SetPosition => wert.HasValue ? $"{label} {wert.Value}%" : label,
                GeraeteAktion.SetOn => label,
                GeraeteAktion.SetOff => label,
                GeraeteAktion.Toggle => label,
                _ => wert.HasValue ? $"{label} {wert.Value}" : label
            };
        }
    }
}