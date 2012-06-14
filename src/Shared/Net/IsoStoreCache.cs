//
// Copyright (c) 2010-2011 Jeff Wilcox
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

//
//
// There will never be a cache hit in this new implementation. IT's disabled
// for now.
//
//
// TODO: Consider bringing isostoreCache back at some point.
//
//
//
//
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;

namespace JeffWilcox.Controls
{
    internal class IsoStoreCache
    {
        private const string CacheDirectoryName = "«img";

        internal const string CacheDirectoryPrefix = CacheDirectoryName + "\\";

        internal const string CacheDirectorySearchPrefix = CacheDirectoryPrefix + "*";

        private static object LockObject = new object();

        private IsolatedStorageFile _isoFile;

        private IsolatedStorageFile IsoStore
        {
            get
            {

                if (_isoFile == null)
                {
                    _isoFile = IsolatedStorageFile.GetUserStoreForApplication();
                }
                return _isoFile;
            }
        }

        //private IEnumerable<string> GetFilesRecursive(string root)
        //{
        //    string search = Path.Combine(root, "*");

        //    List<string> files = new List<string>(IsoStore.GetFileNames(search));

        //    foreach (var d in IsoStore.GetDirectoryNames(search))
        //    {
        //        files.AddRange(GetFilesRecursive(Path.Combine(root, d)));
        //    }
        //    return files;
        //}

        public enum ItemCacheStatus
        {
            Hit,
            Miss,
            Failed,
        }

        private string GetUriPath(string uriAsString)
        {
            var dir = DirectoryHash(uriAsString);

            var filename = EncodePathName(uriAsString);

            return dir + "\\" + filename;
        }

        public void CollectGarbage()
        {
            int deletedItems = 0;

            // Is it a magic time?
            var random = new Random();
            var x = random.Next(100);
            if (x >= 3)
            {
                return;
            }

            try
            {
                if (_knownExistingFiles != null)
                {
                    _knownExistingFiles.Clear();
                }

                var iso = IsoStore;
                if (iso != null)
                {
                    foreach (var dir in iso.GetDirectoryNames(CacheDirectorySearchPrefix).ToList())
                    {
                        string path = CacheDirectoryPrefix + dir;

                        foreach (var file in iso.GetFileNames(path + "\\*.*").ToList())
                        {
                            iso.DeleteFile(file);
                            ++deletedItems;
                        }

                        // iso.DeleteDirectory(path);
                    }

                    Debug.WriteLine("IsoStoreCache: Deleted {0} items from the garbage collection pass of downloaded images.", deletedItems);
                }
            }
            catch (Exception)
            {
            }
        }

        private static Dictionary<string, bool> _knownExistingFiles = new Dictionary<string, bool>();

        public void GetItem(Uri uri, Action<byte[], Exception, ItemCacheStatus> callback)
        {
            if (uri == null)
            {
                return;
            }

            string uriString = uri.ToString();

            if (uriString.Length == 0)
            {
                return;
            }

            // DISABLED CACHING HERE IF YOU WANT...
            //var sstatus = ItemCacheStatus.Miss;
            //callback(null, null, sstatus);
            //return;

            try
            {
                var path = GetUriPath(uriString);

                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                ItemCacheStatus status = ItemCacheStatus.Failed;
                Stream stream = null;
                byte[] bytes = null;
                Exception e = null;

                bool knownToExist = false;
                lock (LockObject)
                {
                    if (_knownExistingFiles.ContainsKey(path))
                    {
                        knownToExist = true;
                    }
                    else if (IsoStore.FileExists(path))
                    {
                        knownToExist = true;
                        _knownExistingFiles[path] = true;
                    }
                }

                if (knownToExist)
                {
                    try
                    {
                        stream = IsoStore.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Write);

                        bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, (int)stream.Length);

                        if (bytes != null && bytes.Length > 0)
                        {
                            status = ItemCacheStatus.Hit;
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        status = ItemCacheStatus.Miss;
                    }
                    catch (Exception ex)
                    {
                        status = ItemCacheStatus.Miss;
                        e = ex;
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Close();
                        }
                    }
                }
                else
                {
                    status = ItemCacheStatus.Miss;
                }

                if (callback != null)
                {
                    callback(bytes, e, status);
                }
            }
            catch (NullReferenceException)
            {
                // Fixing a bug report. Not sure when it happens exactly.
            }
        }

        public void Delete(string path)
        {
            AgFx.PriorityQueue.AddStorageWorkItem(() =>
            {
                lock (LockObject)
                {
                    if (_knownExistingFiles.ContainsKey(path))
                    {
                        _knownExistingFiles.Remove(path);
                    }
                    if (IsoStore.FileExists(path))
                    {
                        IsoStore.DeleteFile(path);
                    }
                }
            });
        }

        private const int WriteRetries = 3;

        public void Write(Uri uri, byte[] data)
        {
            Write(GetUriPath(uri.ToString()), data);
        }

        public void Write(string path, byte[] data)
        {
            // Debug.WriteLine("IsoStoreCache: Saving {0}", path);

            AgFx.PriorityQueue.AddStorageWorkItem(() =>
            {
                lock (LockObject)
                {
                    for (int r = 0; r < WriteRetries; r++)
                    {
                        Stream stream = null;

                        try
                        {
                            EnsurePath(IsoStore, path);
                            stream = IsoStore.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                            stream.Write(data, 0, data.Length);
                            stream.Flush();
                            break;
                        }
                        catch (IsolatedStorageException)
                        {
                            Debug.WriteLine("Exception writing file: Name={0}, Length={1}", path, data.Length);
                            // These IsolatedStorageExceptions seem to happen at random,
                            // haven't yet found a repro.  So for the retry,
                            // if we failed, sleep for a bit and then try again.
                            //
                            try
                            {
                                Thread.Sleep(50);
                            }
                            catch (ObjectDisposedException)
                            {
                                // Another crazy crash found. This will 
                                // probably still be pretty fatal.
                                /*
                                 System.ObjectDisposedException
                                   at System.Threading.WaitHandle.WaitMultiple(WaitHandle[] waitHandles, Int32 millisecondsTimeout, Boolean WaitAll)
                                   at System.Threading.WaitHandle.WaitOne(Int32 millisecondsTimeout, Boolean exitContext)
                                   at System.Threading.EventWaitHandle.WaitOne(Int32 millisecondsTimeout, Boolean exitContext)
                                   at System.Threading.WaitHandle.WaitOne(Int32 millisecondsTimeout)
                                   at System.Threading.Thread.Sleep(Int32 millisecondsTimeout) */
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Close();
                            }
                        }
                    }
                }
            });
        }

        public static void EnsurePath(IsolatedStorageFile store, string filename)
        {
            store.EnsurePath(filename);
        }

        public static string DirectoryHash(string uniqueName)
        {
            return Path.Combine(CacheDirectoryPrefix, uniqueName.GetHashCode().ToString());
        }

        private static string DecodePathName(string encodedPath)
        {
            return Uri.UnescapeDataString(encodedPath);
        }

        private static string EncodePathName(string path)
        {
            return Uri.EscapeDataString(path);
        }
    }
}
