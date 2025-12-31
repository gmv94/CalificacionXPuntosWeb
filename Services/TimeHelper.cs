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

        /// <summary>
        /// Convierte una fecha UTC a hora de Colombia (UTC-5) para mostrar
        /// Si la fecha ya está en hora local, la trata como UTC y la convierte
        /// </summary>
        public static DateTime ToColombiaTime(DateTime dateTime)
        {
            try
            {
                // Si la fecha tiene Kind.Unspecified o Kind.Local, asumimos que es UTC
                DateTime utcDateTime;
                if (dateTime.Kind == DateTimeKind.Utc)
                {
                    utcDateTime = dateTime;
                }
                else
                {
                    // Si es Unspecified o Local, asumimos que está en UTC (como viene de la BD)
                    utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                }

                // Intentar usar la zona horaria de Colombia
                var colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, colombiaTimeZone);
            }
            catch
            {
                // Fallback: restar 5 horas
                return dateTime.AddHours(-5);
            }
        }

        /// <summary>
        /// Convierte una fecha nullable UTC a hora de Colombia para mostrar
        /// </summary>
        public static DateTime? ToColombiaTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return null;
            
            return ToColombiaTime(dateTime.Value);
        }
    }
}

