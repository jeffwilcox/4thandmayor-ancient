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
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using Microsoft.Phone.Shell;
using AgFx;

namespace JeffWilcox.Controls
{
    public class AppTileManager
    {
        private static AppTileManager _instance;

        public static AppTileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppTileManager();
                }

                return _instance;
            }
        }

        private AppTileManager()
        {
        }

        public void Unpin(AppTileSettings.TileSettings tileSettings)
        {
            Unpin(tileSettings, null);
        }

        public void Unpin(Uri navigationUri)
        {
            Unpin(null, navigationUri);
        }

        private void Unpin(AppTileSettings.TileSettings tileSettings, Uri navigationUri)
        {
            if (tileSettings == null && navigationUri != null)
            {
                tileSettings = AppTileSettings.Instance.GetTileSettings(navigationUri);
            }

            if (tileSettings != null)
            {
                navigationUri = tileSettings.NavigationUri;
                // slow impl. bored.
                if (AppTileSettings.Instance.Tiles.ContainsKey(navigationUri))
                {
                    AppTileSettings.Instance.Tiles.Remove(navigationUri);
                }
                AppTileSettings.Instance.Save();
            }

            if (navigationUri != null)
            {
                var tile = ShellTile.ActiveTiles.Where(
                        st => st.NavigationUri == navigationUri)
                        .SingleOrDefault();
                if (tile != null)
                {
                    tile.Delete();
                }
            }
        }

        public void UpdateOrPin(AppTileSettings.TileSettings tileSettings)
        {
            Uri navigationUri = tileSettings.NavigationUri;

            AppTileSettings.TileSettings knownTileSettings = null;
            AppTileSettings.Instance.Tiles.TryGetValue(navigationUri, out knownTileSettings);

            var tile = ShellTile.ActiveTiles.Where(
                st => st.NavigationUri == navigationUri)
                .SingleOrDefault();
            if (tile != null && knownTileSettings == null)
            {
                // This tile needs to be deleted and then re-pinned, it's old.
                tile.Delete();
                tile = null;
            }

            // If there isn't an available shell path... Note a potential bug 
            // here, if you want to change an existing pin and the shell path
            // is set, this code path won't run and the pin won't actually
            // update. So clear it if you want to really change the front
            // photo path. For now I am not putting that into the data model
            // for the settings type. Ick.
            if (tileSettings.FrontPhoto != null && tileSettings.ShellFrontPhotoPath == null && tileSettings.FrontPhoto.IsAbsoluteUri)
            {
                if (tileSettings.FrontPhoto.Scheme == "http" || tileSettings.FrontPhoto.Scheme == "https")
                {
                    string expectedExtension = null;
                    var asString = tileSettings.FrontPhoto.ToString();
                    if (asString.Contains(".jpg"))
                    {
                        expectedExtension = ".jpg";
                    }
                    else if (asString.Contains(".png"))
                    {
                        expectedExtension = ".png";
                    }

                    StatusToken updateToken = null;

                    // Need to do some work to store it in the local isolated
                    // storage instead.
                    WebClient wc = new WebClient();
                    wc.OpenReadCompleted += (s, e) =>
                        {
                            if (updateToken != null)
                            {
                                StatusToken.TryComplete(ref updateToken);
                            }
                            var tt = tileSettings;
                            if (e != null)
                            {
                                if (e.Error != null)
                                {
                                    PriorityQueue.AddUiWorkItem(() =>
                                    {
                                        MessageBox.Show("The network connection was not available to download the image needed to create the tile. Please try again later.");
                                    });
                                    
                                    return;
                                }
                                else if (e.Result != null)
                                {
                                    byte[] bytes = null;
                                    using (var stream = e.Result)
                                    {
                                        bytes = new byte[stream.Length];
                                        stream.Read(bytes, 0, (int)stream.Length);
                                    }

                                    // GUID for this page.
                                    var path = Guid.NewGuid().ToString() + expectedExtension;
                                    var store = IsolatedStorageFile.GetUserStoreForApplication();
                                    string file = "Shared\\ShellContent\\" + path;

                                    Stream st = null;
                                    bool ok = false;

                                    try
                                    {
                                        store.EnsurePath(file);
                                        st = store.OpenFile(file, FileMode.Create, FileAccess.Write, FileShare.Read);
                                        st.Write(bytes, 0, bytes.Length);
                                        st.Flush();
                                        ok = true;
                                    }
                                    catch (IsolatedStorageException)
                                    {
                                        Debug.WriteLine("Exception writing file: Name={0}, Length={1}", file, bytes.Length);
                                    }
                                    catch (ObjectDisposedException)
                                    {
                                    }
                                    finally
                                    {
                                        if (s != null)
                                        {
                                            st.Close();
                                        }
                                    }

                                    if (ok)
                                    {
                                        Uri isoPath = new Uri("isostore:/Shared/ShellContent/" + path, UriKind.RelativeOrAbsolute);
                                        tt.ShellFrontPhotoPath = isoPath;
                                        UpdateOrPinForRealz(tile, navigationUri, tt);
                                    }
                                }
                            }
                        };
                    updateToken = CentralStatusManager.Instance.BeginShowEllipsisMessage("Preparing tile graphics");
                    wc.OpenReadAsync(tileSettings.FrontPhoto);
                    return;
                }
            }

            UpdateOrPinForRealz(tile, navigationUri, tileSettings);
        }

        private void UpdateOrPinForRealz(ShellTile tile, Uri navigationUri, AppTileSettings.TileSettings tileSettings)
        {
            AppTileSettings.Instance.Tiles[navigationUri] = tileSettings;
            AppTileSettings.Instance.Save();

            var shellTileData = new StandardTileData
            {
                Title = tileSettings.Title,
                BackgroundImage = tileSettings.ShellFrontPhotoPath != null ? tileSettings.ShellFrontPhotoPath : tileSettings.FrontPhoto,
                BackBackgroundImage = tileSettings.BackPhoto,
            };

            if (tile == null)
            {
                // Pin.
                try
                {
                    ShellTile.Create(navigationUri, shellTileData);
                }
                catch (InvalidOperationException)
                {
                    AppTileSettings.Instance.Tiles.Remove(navigationUri);
                    AppTileSettings.Instance.Save();
                    MessageBox.Show("Unfortunately the tile could not be created at this time.");
                }
            }
            else
            {
                // Update.
                tile.Update(shellTileData);
            }
        }
    }
}
