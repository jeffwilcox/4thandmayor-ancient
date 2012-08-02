//
// Copyright (c) Jeff Wilcox
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;

namespace JeffWilcox.FourthAndMayor
{
    public class Storage
    {
        private static Storage _instance;

        public static Storage Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Storage();
                }
                return _instance;
            }
        }

        private bool _isDesigning;

        private Storage()
        {
            _isDesigning = DesignerProperties.IsInDesignTool;
        }

        public void DeleteAll()
        {
            var items = GetFilenames().ToArray();
            foreach (var item in items)
            {
                Delete(item);
            }
        }

        public IEnumerable<string> GetFilenames()
        {
            var temp = ItemsList;
            /*foreach (var item in temp)
            {
                Debug.WriteLine(item);
            }*/

            return temp;
        }

        public void Delete(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            try
            {
                IsoStore.DeleteFile(filename);
            }
            catch
            {
            }
#if DEBUG
            // Debug.WriteLine("- deleting " + filename);
#endif
        }

        //public bool ExistsDeprecated(string filename)
        //{
        //    // TODO: Consider refactoring into a TryGetBytes(...) method for efficiency when it exists.
        //    return (Read(filename).Length > 0);
        //}

        public bool TryGetBytes(string filename, out byte[] bytes)
        {
            bytes = Read(filename);
            return (bytes.Length > 0);
        }

        public byte[] Read(string filename)
        {
            // Debug.WriteLine("Storage: Reading " + filename);

            byte[] bytes = new byte[] { }; ;
            // Opening for read only.
            try
            {
                if (IsoStore.FileExists(filename))
                {
                    using (var fileStream = IsoStore.OpenFile(filename, FileMode.Open, FileAccess.Read))
                    {
                        bytes = new byte[fileStream.Length];
                        fileStream.Read(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (FileNotFoundException)
            {

            }
            catch (IsolatedStorageException)
            {
            }
            /*finally
            {
            }*/
            return bytes;
        }

        public void Write(string filename, byte[] data)
        {
            using (var fileStream = IsoStore.OpenFile(filename, FileMode.Create, FileAccess.Write, FileShare.Write)) // IsoStore.CreateFile(filename))
            {
                try
                {
                    if (data != null && data.Length > 0)
                    {
                        Debug.WriteLine("Storage: Writing " + filename);

                        fileStream.Write(data, 0, data.Length);
                    }
                }
                catch
                {
                }
                //finally
                //{
                //fileStream.Close();
                //}
            }
        }

        private string[] ItemsList
        {
            get
            {
                return IsoStore.GetFileNames();
            }
        }

        public List<string> AllFilesRecursive
        {
            get {
                List<string> dirs = new List<string>(ItemsList);
                foreach (string dir in IsoStore.GetDirectoryNames())
                {
                    AddFolder(dir, dirs);
                }
                return dirs;
            }
        }

        private void AddFolder(string prefix, List<string> list)
        {
            foreach (string file in IsoStore.GetFileNames(prefix + "\\*"))
            {
                list.Add(prefix + "\\" + file);
            }
            foreach (string dir in IsoStore.GetDirectoryNames(prefix + "\\*"))
            {
                AddFolder(prefix + "\\" + dir, list);
            }
        }

        private IsolatedStorageFile _isoFile;

        private IsolatedStorageFile IsoStore
        {
            get { return _isoFile ?? (_isoFile = IsolatedStorageFile.GetUserStoreForApplication()); }
        }
    }
}
