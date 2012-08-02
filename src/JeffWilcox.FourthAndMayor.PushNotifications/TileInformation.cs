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

using System;
using JeffWilcox.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor.PushNotifications
{
    public class TileInformation
    {
        //private AppTileSettings.TileSettings _tileSettings;

        // BackBackgroundImage ...
        public Uri BackgroundImage { get; set; }
        public string Title { get; set; }
        public string UnderTitle { get; set; }
        public Uri NavigationUri { get; set; }

        private AppTileSettings.TileSettings _tsi;
        public AppTileSettings.TileSettings TileSettingsInstance 
        { 
            get
            {
                if (_tsi == null)
                {
                    _tsi = new AppTileSettings.TileSettings(NavigationUri);
                    _tsi.Title = Title;
                    _tsi.FrontPhoto = BackgroundImage;
                }
                return _tsi;
            }
            
            set
            {
                _tsi = value;
            }
        }

        public bool IsPinned { get; set; }

        public bool CanChangeTitle { get; set; }
        public bool CanChangeBackgroundImage { get; set; }

        public object DataInstance { get; set; }

        public static TileInformation CreateFromTileData(StandardTileData data)
        {
            var ti = new TileInformation();
            ti.Title = data.Title;
            ti.BackgroundImage = data.BackgroundImage;

            return ti;
        }

        public void OverwriteWithTileSettings(AppTileSettings.TileSettings tileSettings)
        {
            if (tileSettings != null)
            {
                TileSettingsInstance = tileSettings;

                Title = tileSettings.Title;
                BackgroundImage = tileSettings.FrontPhoto;

                // other methods are not yet supported.
            }
        }

        public static TileInformation CreateFromTileSetting(AppTileSettings.TileSettings tileSettings)
        {
            var ti = new TileInformation();
            ti.OverwriteWithTileSettings(tileSettings);

            return ti;
        }
    }
}
