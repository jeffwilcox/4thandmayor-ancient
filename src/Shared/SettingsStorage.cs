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

using System;
using System.Collections.Generic;
using System.Text;
using AgFx;

namespace JeffWilcox.FourthAndMayor
{
    using System.Linq;

    public class SettingsStorage : NotifyPropertyChangedBase
    {
        private static readonly object _lock = new object();

        private const string Prefix = "settings_";
        private const string Suffix = ".txt";
        
        private readonly string _filename;

        private readonly Dictionary<string, string> _stringSettings;

        public SettingsStorage(string storageContainerName)
        {
            _filename = Prefix + storageContainerName + Suffix;
            _stringSettings = new Dictionary<string, string>();

            /*PriorityQueue.AddStorageWorkItem(*/
            Parse();/*);*/
        }

        protected static string BoolToString(bool b)
        {
            return b ? "1" : "0";
        }

        protected static bool StringToBool(string s)
        {
            return s == "1";
        }

        protected Dictionary<string, string> Setting { get { return _stringSettings; } }

        private string SafeKey(string key)
        {
            // Works around the = sign issue.
            return key.Replace("=", "__eq__");
        }

        private string ReverseSafeKey(string key)
        {
            return key.Replace("__eq__", "=");
        }

        private void Parse()
        {
            bool readOK = false;
            _parseInProgress = true;
            lock (_lock)
            {
                byte[] d;
                if (Storage.Instance.TryGetBytes(_filename, out d))
                {
                    try
                    {
                        string text = Encoding.UTF8.GetString(d, 0, d.Length);
                        string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            int equals = line.IndexOf('=');
                            if (equals >= 0)
                            {
                                string key = ReverseSafeKey(line.Substring(0, equals));
                                string value = line.Substring(equals + 1);

                                _stringSettings[key] = value;
                            }
                        }
                        readOK = true;
                    }
                    catch
                    {
                    }
                }
            }

            // Allow the subclass to process this now.
            try
            {
                Deserialize();
            }
            catch
            {
            }

            _parseInProgress = false;

            if (!readOK)
            {
                // Save what was read so far.

                // This is trying to fix this stack that ESJ found at startup:
                /*
                IndexOutOfRangeException
System.IndexOutOfRangeException
   at System.Collections.Generic.Dictionary`2.Insert(String key, String value, Boolean add)
   at JeffWilcox.FourthAndMayor.AnalyticsSettings.Serialize()
   at JeffWilcox.FourthAndMayor.SettingsStorage.Save()
   at JeffWilcox.FourthAndMayor.SettingsStorage.Parse()
   at AgFx.PriorityQueue.WorkerThread.<>c__DisplayClass5.<WorkerThreadProc>b__3(Object s)
   at System.Threading.ThreadPool.WorkItem.doWork(Object o)*/

                Save();
//                Save();
            }
        }

        public void SaveSoon()
        {
            PriorityQueue.AddStorageWorkItem(Save);
        }

        private bool _parseInProgress;

        //private bool _pendingSave;

        protected virtual void Serialize()
        {
        }

        public void Save()
        {
            if (_parseInProgress)
            {
                return;
            }

            //_pendingSave = false;

            try
            {
                Serialize();

                var sb = new StringBuilder();
                var st = _stringSettings.Keys.ToList();
                foreach (var s in st)
                {
                    var kp = _stringSettings[s];
                    sb.Append(SafeKey(s));
                    sb.Append("=");
                    sb.AppendLine(kp);
                }
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                lock (_lock)
                {
                    Storage.Instance.Write(_filename, bytes);
                }

                //Debug.WriteLine(sb.ToString());
            }
            catch
            {
                // TODO: What really happens here? Is it threading bugs?
            }
        }

        protected virtual void Deserialize()
        {
        }
    }
}
