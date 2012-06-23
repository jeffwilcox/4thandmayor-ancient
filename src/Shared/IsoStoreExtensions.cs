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
