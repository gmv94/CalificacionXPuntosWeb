namespace CalificacionXPuntosWeb.Services
{
    public static class TimeHelper
    {
        /// <summary>
        /// Obtiene la hora actual en zona horaria de Colombia (UTC-5)
        /// </summary>
        public static DateTime GetColombiaTime()
        {
            try
            {
                // Intentar usar la zona horaria de Colombia
                var colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaTimeZone);
            }
            catch
            {
                // Fallback: restar 5 horas de UTC
                return DateTime.UtcNow.AddHours(-5);
            }
        }
    }
}

