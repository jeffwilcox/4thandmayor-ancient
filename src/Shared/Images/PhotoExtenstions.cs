﻿//
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

using System.IO;
using System.Windows.Media.Imaging;

namespace JeffWilcox.Controls
{
    /// <summary>
    /// Provides useful photo resizing code.
    /// </summary>
    public static class PhotoExtenstions
    {
        public static MemoryStream ResizeBitmapImage(this BitmapImage bitmapImage, int targetWidth, int targetHeight, int jpegQuality = 70)
        {
            var pw = bitmapImage.PixelWidth;
            var ph = bitmapImage.PixelHeight;

            WriteableBitmap wb = new WriteableBitmap(bitmapImage);

            bool isLandscape = pw > ph;

            MemoryStream ms = new MemoryStream();
            wb.SaveJpeg(
                ms,
                isLandscape ? targetWidth : targetHeight,
                isLandscape ? targetHeight : targetWidth,
                0, // not used
                jpegQuality);

            byte[] by = ms.ToArray();

            return new MemoryStream(by);
        }
    }
}
