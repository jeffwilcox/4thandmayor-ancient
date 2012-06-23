using System;
using System.IO;
using System.Windows;
using Microsoft.Phone.Controls;

namespace JeffWilcox.Controls
{
    public static class ApplicationExtensions
    {
        private const string AppManifestFilename = "WMAppManifest.xml";

        public static WilcoxTransitionFrame GetFrame(this Application app)
        {
            IExposeRootFrame f = app as IExposeRootFrame;
            if (f != null)
            {
                return f.RootFrame;
            }

            throw new InvalidOperationException("The root frame could not be located.");
            // return null;
        }

        public static void GetManifestVersion(this Application app, ref string appVersion, ref string appPlatformVersion)
        {
            Uri manifest = new Uri(AppManifestFilename, UriKind.Relative);
            var si = Application.GetResourceStream(manifest);
            if (si != null)
            {
                using (StreamReader sr = new StreamReader(si.Stream))
                {
                    bool haveApp = false;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (!haveApp)
                        {
                            int i = line.IndexOf("AppPlatformVersion=\"", StringComparison.InvariantCulture);
                            if (i >= 0)
                            {
                                haveApp = true;
                                line = line.Substring(i + 20);

                                int z = line.IndexOf("\"");
                                if (z >= 0)
                                {
                                    appPlatformVersion = line.Substring(0, z);
                                }
                            }
                        }

                        int y = line.IndexOf("Version=\"", StringComparison.InvariantCulture);
                        if (y >= 0)
                        {
                            int z = line.IndexOf("\"", y + 9, StringComparison.InvariantCulture);
                            if (z >= 0)
                            {
                                // We have the version, no need to read on.
                                appVersion = line.Substring(y + 9, z - y - 9);

                                // Let's just simplify to the Windows Phone
                                // Marketplace's Major.Minor scheme.
                                try
                                {
                                    var version = new Version(appVersion);
                                    appVersion = version.Major + "." + version.Minor;
                                }
                                catch (Exception)
                                {
                                }

                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                appVersion = "Unknown";
            }
        }
    }
}
