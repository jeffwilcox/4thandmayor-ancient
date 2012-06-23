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
using System.Linq;
using System.Windows;
using AgFx;
using JeffWilcox.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor
{
    public static class PinVenueToStartHelper
    {
        public static Uri GetTileResizedUri(Uri photoUri)
        {
            if (photoUri.IsAbsoluteUri)
            {
                string s = photoUri.ToString().Replace("https://", "http://");

                if (s.EndsWith("x300"))
                {
                    s += "."; // hacking around URI issues for now.
                }

                s = System.Net.HttpUtility.UrlEncode(s);
                return ((IGenerate4thAndMayorUri)(Application.Current)).Get4thAndMayorUri("/tile.php?i=" + s, /*secure*/ false);
            }
            else
            {
                // May be a local resource or asset such as the main app logo.
                return photoUri;
            }
        }

        public static bool IsPinned(Uri navigationUri)
        {
            if (navigationUri != null)
            {
                var tile = ShellTile
                    .ActiveTiles
                    .Where(st => st.NavigationUri == navigationUri)
                    .SingleOrDefault();
                return tile != null;
            }

            return false;
        }

        public static AppTileSettings.TileSettings GetTileSettings(Uri navigationUri)
        {
            if (navigationUri != null)
            {
                var tileSetting =
                    AppTileSettings.Instance.Tiles
                    .Values
                    .Where(v => v.NavigationUri == navigationUri)
                    .SingleOrDefault();
                return tileSetting;
            }

            return null;
        }

        public static void PerformVenueUpgradeCheck(Model.Venue venu, Uri navigationUri)
        {
            if (navigationUri != null)
            {
                var tileSetting = 
                    AppTileSettings.Instance.Tiles
                    .Values
                    .Where(v => v.NavigationUri == navigationUri)
                    .SingleOrDefault();
                if (tileSetting == null && IsPinned(navigationUri))
                {
                    // This needs to be upgraded.
                    if (MessageBox.Show(
                            "It looks like you've pinned this place to your start screen before, but recently upgraded the app."
                            + Environment.NewLine +
                            "The tile needs to be removed and then pinned again, is it OK to do that now?",
                            "Pinned tile upgrade",
                            MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        PriorityQueue.AddUiWorkItem(() =>
                            {
                                PinVenueToStartHelper.PinToStartVenue(venu, navigationUri);
                            });
                    }
                }
            }
        }

        public static void PinToStartVenue(Model.Venue v, Uri navigationUri)
        {
            string title = null;
            Uri uri = null;
            Uri backgroundImage = null;

            try
            {
                title = v.Name;
                uri = v.VenueUri;
            }
            catch (ArgumentNullException)
            {
                // Work around the bug that Gilles found when hitting too
                // soon.
                return;
            }

            if (v.PhotoGroups != null && v.PhotoGroups.Count > 0)
            {
                var lastGroup = v.PhotoGroups[v.PhotoGroups.Count - 1];
                if (lastGroup.Count > 0)
                {
                    Random rand = new Random();
                    int newIndex = rand.Next(lastGroup.Count);

                    var firstPhoto = lastGroup[/*0*/ newIndex];
                    if (firstPhoto != null && firstPhoto.LargerUri != null)
                    {
                        backgroundImage = GetTileResizedUri(firstPhoto.LargerUri);
                        Debug.WriteLine("The live tile background will be set to {0}", backgroundImage);
                    }
                }
            }

            if (backgroundImage == null)
            {
                backgroundImage = ((IGenerate4thAndMayorUri)(Application.Current)).Get4thAndMayorUri("/tile.php", /*secure*/ false);
            }

            if (title != null && uri != null)
            {
                AppTileManager.Instance.UpdateOrPin(
                    new AppTileSettings.TileSettings(navigationUri)
                    {
                        FrontPhoto = backgroundImage,
                        Title = title,
                    });
            }
        }
    }
}
