using System;
using System.Linq;
using System.Threading;
using SmartHome.Daten;
using SmartHome.Typ;

namespace SmartHome.Dienste
{
    public class ZeitschaltDienst
    {
        private readonly Einrichtung _einrichtung;
        private readonly ZeitplanDienst _zeitplan;
        private readonly MakroDienst _makros;
        private readonly SteuerungsDienst _steuerung;
        private Thread? _worker;
        private volatile bool _running;

        public ZeitschaltDienst(Einrichtung e, ZeitplanDienst zeitplan, MakroDienst makros, SteuerungsDienst steuerung)
        {
            _einrichtung = e;
            _zeitplan = zeitplan;
            _makros = makros;
            _steuerung = steuerung;
        }

        public void Start()
        {
            if (_running) return;
            _running = true;
            _worker = new Thread(Loop) { IsBackground = true };
            _worker.Start();
        }

        public void Stop()
        {
            _running = false;
        }

        private static bool TagPasst(Wochentage tage, DayOfWeek dow)
        {
            return dow switch
            {
                DayOfWeek.Monday => tage.HasFlag(Wochentage.Montag),
                DayOfWeek.Tuesday => tage.HasFlag(Wochentage.Dienstag),
                DayOfWeek.Wednesday => tage.HasFlag(Wochentage.Mittwoch),
                DayOfWeek.Thursday => tage.HasFlag(Wochentage.Donnerstag),
                DayOfWeek.Friday => tage.HasFlag(Wochentage.Freitag),
                DayOfWeek.Saturday => tage.HasFlag(Wochentage.Samstag),
                DayOfWeek.Sunday => tage.HasFlag(Wochentage.Sonntag),
                _ => false
            };
        }

        private static bool InInterval(TimeSpan start, TimeSpan ende, TimeSpan t)
        {
            // Annahme: Ende > Start (gleicher Tag)
            return t >= start && t < ende;
        }

        private void Loop()
        {
            while (_running)
            {
                try
                {
                    var jetzt = DateTime.Now;
                    var t = jetzt.TimeOfDay;
                    var dow = jetzt.DayOfWeek;

                    var eintraege = _zeitplan.Laden()
                        .Where(x => x.Aktiv)
                        .ToList();

                    foreach (var z in eintraege)
                    {
                        // Legacy: einmalige Ausführung
                        if ((z.StartZeit == default && z.EndZeit == default) && z.Zeitpunkt != default)
                        {
                            if (!z.Erledigt && z.Zeitpunkt <= jetzt)
                            {
                                if (z.ZielArt == ZeitplanZielArt.Makro)
                                {
                                    var m = _makros.Finde(z.MakroName);
                                    if (m != null) _steuerung.FuehreMakroAus(_einrichtung, m);
                                }
                                else if (z.Aktion.HasValue)
                                {
                                    _steuerung.FuehreGeraeteAktionAus(_einrichtung,
                                        z.RaumAbk, z.TypAbk, z.Geraetename,
                                        z.Aktion.Value, z.Wert, "zeitgesteuert");
                                }

                                if (z.TaeglichWiederholen)
                                {
                                    z.ZuletztAusgefuehrt = jetzt;
                                    z.Zeitpunkt = z.Zeitpunkt.Date.AddDays(1).Add(z.Zeitpunkt.TimeOfDay);
                                }
                                else
                                {
                                    z.Erledigt = true;
                                    z.ZuletztAusgefuehrt = jetzt;
                                }
                                _zeitplan.Aktualisieren(z);
                            }

                            continue;
                        }

                        // Wiederkehrend
                        if (!TagPasst(z.Tage, dow)) continue;

                        // Endzeit muss nach Startzeit liegen (nur gleiche-Tages-Intervalle)
                        if (z.EndZeit <= z.StartZeit) continue;

                        bool inIntervall = InInterval(z.StartZeit, z.EndZeit, t);

                        // Start-Aktion einmal pro Tag
                        if (inIntervall && (z.ZuletztStart == null || z.ZuletztStart.Value.Date != jetzt.Date))
                        {
                            if (z.ZielArt == ZeitplanZielArt.Makro)
                            {
                                var m = _makros.Finde(z.MakroName);
                                if (m != null) _steuerung.FuehreMakroAus(_einrichtung, m);
                            }
                            else if (z.Aktion.HasValue)
                            {
                                _steuerung.FuehreGeraeteAktionAus(_einrichtung,
                                    z.RaumAbk, z.TypAbk, z.Geraetename,
                                    z.Aktion.Value, z.Wert, "zeitgesteuert");
                            }
                            z.ZuletztStart = jetzt;
                            _zeitplan.Aktualisieren(z);
                        }

                        // End-Aktion einmal pro Tag nach Ende
                        if (!inIntervall && z.EndAktionAktiv && z.EndAktion.HasValue)
                        {
                            // prüfe: war heute bereits im Intervall und Endaktion heute noch nicht ausgeführt,
                            // und wir sind nach dem Endzeitpunkt dieses Tages
                            if (z.ZuletztStart != null && z.ZuletztStart.Value.Date == jetzt.Date &&
                                (z.ZuletztEnde == null || z.ZuletztEnde.Value.Date != jetzt.Date) &&
                                t >= z.EndZeit)
                            {
                                _steuerung.FuehreGeraeteAktionAus(_einrichtung,
                                    z.RaumAbk, z.TypAbk, z.Geraetename,
                                    z.EndAktion.Value, z.EndWert, "zeitgesteuert");
                                z.ZuletztEnde = jetzt;
                                _zeitplan.Aktualisieren(z);
                            }
                        }
                    }
                }
                catch
                {
                    // Fehler im Hintergrund ignorieren
                }

                Thread.Sleep(1000);
            }
        }
    }
}