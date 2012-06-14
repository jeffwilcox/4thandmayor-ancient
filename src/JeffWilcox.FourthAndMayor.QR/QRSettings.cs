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
using System.Globalization;
using JeffWilcox.Controls;

namespace JeffWilcox.FourthAndMayor
{
    public class QRSettings :
        SettingsStorage
    {
        public QRSettings()
            : base("qrCode")
        {
        }

        private bool _auto;
        private const string AutoKey = "auto";
        public bool Auto
        {
            get { return _auto; }
            set
            {
                _auto = value;
                RaisePropertyChanged("Auto");
            }
        }

        protected override void Serialize()
        {
            Setting[AutoKey] = BoolToString(_auto);
        }

        protected override void Deserialize()
        {
            string imp;
            if (Setting.TryGetValue(AutoKey, out imp))
            {
                _auto = StringToBool(imp);
            }

            base.Deserialize();
        }
    }
}
