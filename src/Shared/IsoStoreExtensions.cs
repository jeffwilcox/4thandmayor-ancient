using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;
using System.IO.IsolatedStorage;

namespace JeffWilcox.Controls
{
    public static class IsoStoreExtensions
    {
        public static void EnsurePath(this IsolatedStorageFile store, string filename)
        {
            for (string path = Path.GetDirectoryName(filename);
            path != "";
            path = Path.GetDirectoryName(path))
            {

                if (!store.DirectoryExists(path))
                {
                    store.CreateDirectory(path);
                }
            }

        }
    }
}
