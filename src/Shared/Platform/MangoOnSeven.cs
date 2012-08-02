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
using System.Collections;
using System.Device.Location;
using System.Reflection;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.Controls
{
    public static class MangoOnSeven
    {
        [Obsolete("OLD!")]
        public static bool TryBingMapsTask(GeoCoordinate coordinate, double zoomLevel)
        {
            try
            {
                Assembly phoneAssembly = typeof(SystemTray).Assembly;

                Type bingMapsTask = phoneAssembly.GetType("Microsoft.Phone.Tasks.BingMapsTask");

                if (bingMapsTask != null)
                {
                    var center = bingMapsTask.GetProperty("Center");
                    var zoom = bingMapsTask.GetProperty("ZoomLevel");

                    var showMethod = bingMapsTask.GetMethod("Show");

                    if (center != null && zoom != null && showMethod != null)
                    {
                        var task = Activator.CreateInstance(bingMapsTask);

                        if (task != null)
                        {
                            center.SetValue(task, coordinate, null);
                            zoom.SetValue(task, zoomLevel, null);

                            showMethod.Invoke(task, null);

                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        [Obsolete("OLD!")]
        public static bool IsFirstPage()
        {
            PhoneApplicationFrame frame = Application.Current.GetFrame();

            var frameType = typeof(PhoneApplicationFrame);
            var rbe = frameType.GetProperty("BackStack");
            if (rbe != null)
            {
                var val = rbe.GetValue(frame, null);
                if (val != null)
                {
                    var ie = val as IEnumerable;
                    if (ie != null)
                    {
                        int count = 0;
                        foreach (var item in ie)
                        {
                            ++count;
                        }

                        return count == 0;
                    }
                }
            }

            return false;
        }

        [Obsolete("OLD!")]
        public static bool IsPagePinned(Uri uri)
        {
            try
            {
                Assembly phoneAssembly = typeof(SystemTray).Assembly;

                Type tileDataType = phoneAssembly.GetType("Microsoft.Phone.Shell.StandardTileData");
                Type shellTileType = phoneAssembly.GetType("Microsoft.Phone.Shell.ShellTile");

                if (tileDataType != null && shellTileType != null)
                {
                    var titleProperty = tileDataType.GetProperty("Title");
                    var createNewTileMethod = shellTileType.GetMethod("Create");
                    var backgroundImageProperty = tileDataType.GetProperty("BackgroundImage");

                    if (titleProperty != null && createNewTileMethod != null)
                    {
                        var activeTiles = shellTileType.GetProperty("ActiveTiles");
                        if (activeTiles != null)
                        {
                            var tiles = activeTiles.GetValue(null, null) as System.Collections.IEnumerable;
                            if (tiles != null)
                            {
                                foreach (var oldTile in tiles)
                                {
                                    var tileuri = shellTileType.GetProperty("NavigationUri").GetValue(oldTile, null) as Uri;
                                    if (tileuri.ToString() == uri.ToString())
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return false;
        }

        [Obsolete("OLD!")]
        public static bool TryPinSecondaryTile(string title, Uri uri, Uri optionalBackgroundImage = null)
        {
            try
            {
                Assembly phoneAssembly = typeof(SystemTray).Assembly;

                Type tileDataType = phoneAssembly.GetType("Microsoft.Phone.Shell.StandardTileData");
                Type shellTileType = phoneAssembly.GetType("Microsoft.Phone.Shell.ShellTile");

                if (tileDataType != null && shellTileType != null)
                {
                    var titleProperty = tileDataType.GetProperty("Title");
                    var createNewTileMethod = shellTileType.GetMethod("Create");
                    var backgroundImageProperty = tileDataType.GetProperty("BackgroundImage");

                    if (titleProperty != null && createNewTileMethod != null)
                    {
                        var activeTiles = shellTileType.GetProperty("ActiveTiles");
                        if (activeTiles != null)
                        {
                            var tiles = activeTiles.GetValue(null, null) as System.Collections.IEnumerable;
                            if (tiles != null)
                            {
                                foreach (var oldTile in tiles)
                                {
                                    var tileuri = shellTileType.GetProperty("NavigationUri").GetValue(oldTile, null) as Uri;
                                    if (tileuri.ToString() == uri.ToString())
                                    {
                                        // Delete the old copy.
                                        shellTileType.GetMethod("Delete").Invoke(oldTile, null);
                                        break;
                                    }
                                }

                                var tile = Activator.CreateInstance(tileDataType);

                                if (tile != null)
                                {
                                    titleProperty.SetValue(tile, title, null);

                                    if (optionalBackgroundImage != null && backgroundImageProperty != null)
                                    {
                                        backgroundImageProperty.SetValue(tile, optionalBackgroundImage, null);
                                    }

                                    createNewTileMethod.Invoke(null, new object[]
                            {
                                uri,
                                tile,
                            });

                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}
