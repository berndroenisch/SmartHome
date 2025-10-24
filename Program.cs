using System;
using System.Threading;
using SmartHome;
using SmartHome.Daten;
using SmartHome.Dienste;
using SmartHome.Menue;
using SmartHome.Typ;

class Program
{
    private static ZeitschaltDienst? _zeitschaltDienst;
    private static Mutex? _singleInstanceMutex;

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Landesformat.SetzeDeutsch();

        // Nur eine Instanz zulassen
        bool createdNew;
        _singleInstanceMutex = new Mutex(true, "Global\\SmartHomeApp_SingleInstance", out createdNew);
        if (!createdNew)
        {
            Console.WriteLine("SmartHome ist bereits gestartet. Beende diese Instanz.");
            return;
        }

        var speicher = new SpeicherDienst();
        var verlauf = new VerlaufDienst();
        var zeitplan = new ZeitplanDienst();
        var makroDienst = new MakroDienst();
        var steuerung = new SteuerungsDienst(speicher, verlauf);

        Einrichtung einrichtung = speicher.Laden() ?? new Einrichtung();

        if (einrichtung.Raeume.Count == 0)
        {
            var assistent = new EinrichtungsAssistent(speicher);
            einrichtung = assistent.Starte();
        }

        // Hintergrunddienst starten
        _zeitschaltDienst = new ZeitschaltDienst(einrichtung, zeitplan, makroDienst, steuerung);
        _zeitschaltDienst.Start();

        // Sauber herunterfahren bei Ctrl+C und Prozessende
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;  // wir beenden kontrolliert
            StoppeDienste();
            Environment.Exit(0);
        };
        AppDomain.CurrentDomain.ProcessExit += (s, e) => StoppeDienste();
        AppDomain.CurrentDomain.UnhandledException += (s, e) => StoppeDienste();

        var hauptmenue = new Hauptmenue(speicher, verlauf, zeitplan, makroDienst, steuerung);
        hauptmenue.Start(einrichtung);

        StoppeDienste();
    }

    private static void StoppeDienste()
    {
        try { _zeitschaltDienst?.Stop(); } catch { /* ignore */ }
        try
        {
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
        }
        catch { /* ignore */ }
    }
}