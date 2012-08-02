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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JeffWilcox.FourthAndMayor;

namespace JeffWilcox.Controls
{
    public class AppTileSettings : SettingsStorage
    {
        private static AppTileSettings _instance;

        public static AppTileSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppTileSettings();
                }

                return _instance;
            }
        }

        public AppTileSettings.TileSettings GetTileSettings(Uri navigationUri)
        {
            if (navigationUri != null)
            {
                var tileSetting =
                    Tiles
                    .Values
                    .Where(v => v.NavigationUri == navigationUri)
                    .SingleOrDefault();
                return tileSetting;
            }

            return null;
        }

        public class TileSettings
        {
            public TileSettings(Uri navigationUri)
            {
                NavigationUri = navigationUri;
            }
            public Uri NavigationUri { get; private set; }
            public string Title { get; set; }
            public Uri FrontPhoto { get; set; }
            
            public Uri ShellFrontPhotoPath { get; set; }

            public Uri BackPhoto { get; set; }
            public string StyleType { get; set; }
        }

        private AppTileSettings()
            : base("tiles")
        {
        }

        public Dictionary<Uri, TileSettings> Tiles { get; private set; }

        protected override void Serialize()
        {
            if (Tiles != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var entry in Tiles.Values)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" tile:");
                    }

                    sb.Append(entry.NavigationUri.ToString());

                    // Serialize the individual entries.
                    string su = entry.NavigationUri.ToString();

                    Setting[su + ".title"] = entry.Title;
                    Setting[su + ".type"] = entry.StyleType;
                    Setting[su + ".frontPhoto"] = entry.FrontPhoto == null ? null : entry.FrontPhoto.ToString();
                    Setting[su + ".backPhoto"] = entry.BackPhoto == null ? null : entry.BackPhoto.ToString();
                    Setting[su + ".shellFrontPhoto"] = entry.ShellFrontPhotoPath == null ? null : entry.ShellFrontPhotoPath.ToString();
                }
                Setting["tiles"] = sb.ToString();
            }
        }

        // BUG: Deleting doesn't remove from the 'tiles' list.

        protected override void Deserialize()
        {
            Tiles = new Dictionary<Uri, TileSettings>();

            string i;
            if (Setting.TryGetValue("tiles", out i))
            {
                var t = i.Split(new string[] { " tile:"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var knownNavigationUri in t)
                {
                    Uri tileUri;
                    if (Uri.TryCreate(knownNavigationUri, UriKind.Relative, out tileUri))
                    {
                        var ts = new TileSettings(tileUri);

                        if (Setting.TryGetValue(knownNavigationUri + ".title", out i))
                        {
                            ts.Title = i;
                        }
                        if (Setting.TryGetValue(knownNavigationUri + ".type", out i))
                        {
                            ts.StyleType = i;
                        }

                        Uri uri = null;
                        if (Setting.TryGetValue(knownNavigationUri + ".frontPhoto", out i))
                        {
                            if (!string.IsNullOrEmpty(i))
                            {
                                if (Uri.TryCreate(i, UriKind.RelativeOrAbsolute, out uri))
                                {
                                    ts.FrontPhoto = uri;
                                }
                            }
                        }
                        if (Setting.TryGetValue(knownNavigationUri + ".backPhoto", out i))
                        {
                            if (!string.IsNullOrEmpty(i))
                            {
                                if (Uri.TryCreate(i, UriKind.RelativeOrAbsolute, out uri))
                                {
                                    ts.BackPhoto = uri;
                                }
                            }
                        }
                        if (Setting.TryGetValue(knownNavigationUri + ".shellFrontPhoto", out i))
                        {
                            if (!string.IsNullOrEmpty(i))
                            {
                                if (Uri.TryCreate(i, UriKind.RelativeOrAbsolute, out uri))
                                {
                                    ts.ShellFrontPhotoPath = uri;
                                }
                            }
                        }

                        Tiles[tileUri] = ts;
                    }
                }
            }

            base.Deserialize();
        }
    }
}
