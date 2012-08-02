
using System.Windows;
using System;

namespace JeffWilcox.Controls
{
    // refactor out sometime

    public static class ApplicationLocalizationExtensions
    {
        public static bool TryGetLocalizedString(this Application app, string key, out string @string)
        {
            return RequireAppLocalizationInterface(app).TryGetLocalizedString(key, out @string);
        }

        private static IProvideLocalizedStrings RequireAppLocalizationInterface(Application app)
        {
            var appStrings = app as IProvideLocalizedStrings;
            if (appStrings != null)
            {
                return appStrings;
            }

            throw new InvalidOperationException("The Application does not support IProvideLocalizedStrings.");
        }

        public static string GetLocalizedString(this Application app, string key)
        {
            return RequireAppLocalizationInterface(app).GetLocalizedString(key);
        }
    }

    public interface IProvideLocalizedStrings
    {
        string GetLocalizedString(string identifier);
        bool TryGetLocalizedString(string key, out string @string);
    }
}
