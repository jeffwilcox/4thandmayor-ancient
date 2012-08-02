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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AgFx;

namespace JeffWilcox.FourthAndMayor
{
    public class TombstoningStream
    {
        private readonly string _streamIdentifier;
        private string _uniqueId;
        private byte[] _data;

        public TombstoningStream(string streamIdentifier)
        {
            _streamIdentifier = streamIdentifier;
        }

        // TODO: Is this duplicated or used?

        public static void ClearTheCatacombs()
        {
            PriorityQueue.AddStorageWorkItem(ActuallyClearTheCatacombs);
        }

        private static void ActuallyClearTheCatacombs()
        {
            try
            {
                foreach (var tomb in Storage.Instance.GetFilenames().Where(f => f.EndsWith(".tomb")))
                {
                    Storage.Instance.Delete(tomb);
                }
            }
            catch
            {
            }
        }

        private string Filename
        {
            get { return _streamIdentifier + _uniqueId + ".tomb"; }
        }

        public void OnBackKeyPress()
        {
            if (Exists && _uniqueId != null)
            {
                Delete();
            }
        }

        public void OnNavigatedFrom(IDictionary<string, object> state)
        {
            Save(_uniqueId);

            if (state != null && _uniqueId != null && _data != null && _data.Length > 0)
            {
                // Only if there was enough time for the file to be written do
                // we actually then store that we have it.
                state["tombstonedStream" + _uniqueId] = Filename;
                Debug.WriteLine("tombstonedStream" + _uniqueId + ":" + Filename);
            }
        }

        public bool Exists
        {
            get
            {
                return (_data != null && _data.Length > 0);
//                return _data != null;
            }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public Stream Stream
        {
            get
            {
                return new MemoryStream(_data);
            }
        }

        public bool Load(string uniqueId)
        {
            _uniqueId = uniqueId;
            try
            {
                _data = Storage.Instance.Read(Filename);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public void Save(string uniqueId)
        {
            _uniqueId = uniqueId;
            Storage.Instance.Write(Filename, _data);
        }

        public void StoreStreamContents(Stream stream, long length)
        {
            try
            {
                using (var br = new BinaryReader(stream))
                {
                    _data = br.ReadBytes((int)length);
                }
            }
            catch
            {
            }
        }

        protected void StoreData(byte[] data)
        {
            _data = data;
        }

        public void SetToImageSource(Image image)
        {
            var bi = new BitmapImage();
            bi.SetSource(Stream);
            image.Source = bi;
        }

        public void OnNavigatedTo(string uniqueItemId, IDictionary<string, object> state)
        {
            _uniqueId = uniqueItemId;

            if (_uniqueId != null)
            {
                object fn;
                if (state.TryGetValue("tombstonedStream" + _uniqueId, out fn))
                {
                    var f = (string)fn;

                    try
                    {
                        if (!Exists)
                        {
                            _data = Storage.Instance.Read(f);
                            // does not clear the _data array. Also, if
                            // the array is already set, this is ignored.
                            // Makes it easier for integration with the
                            // photo chooser task.

                            Delete(f);
                        }
                    }
                    catch
                    {
                        _data = null;
                    }
                }
            }
        }

        private static void Delete(string filename)
        {
            try
            {
                Storage.Instance.Delete(filename);
            }
            catch
            {
            }
        }

        public void Delete()
        {
            _data = null;

            if (_uniqueId != null)
            {
                try
                {
                    Delete(Filename);
                }
                catch
                {
                }
            }
        }
    }
}
