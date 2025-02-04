using System.Text;
using System.Text.RegularExpressions;


namespace Replic.Models
{
    public static class Utils
    {
        public static string CleanStringForWindows1252(string input)
        {
            // Reemplaza caracteres no compatibles con Windows-1252
            return Regex.Replace(input, @"[^\u0000-\u00FF]", "");
        }
    }
}
