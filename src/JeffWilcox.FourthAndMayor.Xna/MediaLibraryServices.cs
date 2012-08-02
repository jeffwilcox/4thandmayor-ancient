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
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;

namespace JeffWilcox.FourthAndMayor.Xna
{
    public static class MediaLibraryServices
    {
        public static void SaveToMediaLibrary(WriteableBitmap bitmap, string name, int quality = 100)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    bitmap.SaveJpeg(
                        stream, 
                        bitmap.PixelWidth, 
                        bitmap.PixelHeight, 
                        0, 
                        quality);
                    stream.Seek(0, SeekOrigin.Begin);
                    new MediaLibrary().SavePicture(name, stream);
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
