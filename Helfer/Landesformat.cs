using System.Globalization;
using System.Threading;

namespace SmartHome
{
    public static class Landesformat
    {
        public static void SetzeDeutsch()
        {
            var kultur = new CultureInfo("de-DE");
            Thread.CurrentThread.CurrentCulture = kultur;
            Thread.CurrentThread.CurrentUICulture = kultur;
        }
    }
}