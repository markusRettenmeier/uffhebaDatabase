namespace Sammlerplattform.Services.Extensions
{
    public static class UriExtensions
    {
        public static string? ChangeStringToUriToRemoveSubdomain(this string? uriString)
        {
            if (string.IsNullOrEmpty(uriString))
                return null;

            try
            {
                Uri uri = new(uriString, UriKind.Absolute);
                Uri processedUri = uri.RemoveSubdomain();
                return processedUri.ToString();
            }
            catch (UriFormatException)
            {
                // Fallback: Manuelle Reparatur des URI-Strings
                return TryRepairAndProcessUri(uriString);
            }
        }

        private static string? TryRepairAndProcessUri(string uriString)
        {
            // Versuche, das Protokoll hinzuzufügen, falls fehlend
            if (!uriString.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !uriString.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                uriString = "https://" + uriString;
            }

            // Entferne ungültige Zeichen
            uriString = uriString.Replace(" ", "%20")
                                 .Replace("ä", "%C3%A4")
                                 .Replace("ö", "%C3%B6")
                                 .Replace("ü", "%C3%BC")
                                 .Replace("ß", "%C3%9F");

            try
            {
                Uri uri = new(uriString, UriKind.Absolute);
                Uri processedUri = uri.RemoveSubdomain();
                return processedUri.ToString();
            }
            catch (UriFormatException)
            {
                // Letzter Ausweg: Nur Host-Teil manuell verarbeiten
                return ExtractAndProcessHostManually(uriString);
            }
        }

        private static string? ExtractAndProcessHostManually(string uriString)
        {
            try
            {
                // Finde das Protokoll
                int protocolEnd = uriString.IndexOf("://");
                string protocol = protocolEnd > 0 ? uriString[..(protocolEnd + 3)] : "https://";
                string remaining = protocolEnd > 0 ? uriString[(protocolEnd + 3)..] : uriString;

                // Finde den Host-Teil (bis zum ersten '/' oder Ende)
                int pathStart = remaining.IndexOf('/');
                string hostPart = pathStart > 0 ? remaining[..pathStart] : remaining;
                string pathPart = pathStart > 0 ? remaining[pathStart..] : "";

                // Entferne Subdomain (alles vor dem ersten Punkt)
                int firstDot = hostPart.IndexOf('.');
                if (firstDot > 0)
                {
                    string newHost = hostPart[(firstDot + 1)..];
                    return $"{protocol}{newHost}{pathPart}";
                }

                return $"{protocol}{hostPart}{pathPart}";
            }
            catch
            {
                // Wenn absolut nichts funktioniert, gib den Originalstring zurück
                return uriString;
            }
        }

        public static Uri RemoveSubdomain(this Uri uri)
        {
            string host = uri.Host;
            int firstDotIndex = host.IndexOf('.');

            if (firstDotIndex > 0)
            {
                string newHost = host[(firstDotIndex + 1)..];
                var builder = new UriBuilder(uri)
                {
                    Host = newHost
                };
                return builder.Uri;
            }

            return uri;
        }
    }
}
