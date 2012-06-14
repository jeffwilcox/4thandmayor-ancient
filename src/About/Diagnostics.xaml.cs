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
using System.Linq;
using System.Text;
using JeffWilcox.FourthAndMayor;
using Microsoft.Phone.Controls;

namespace About
{
    public partial class Diagnostics : PhoneApplicationPage
    {
        public Diagnostics()
        {
            InitializeComponent();
        }

        public class FileItem
        {
            public FileItem(string filename)
            {
                Filename = filename;
                ViewerUri = new Uri(string.Format("/About;component/FileViewer.xaml?file={0}",
                    Uri.EscapeDataString(filename)), UriKind.Relative);
            }
            public string Filename { get; set; }
            public Uri ViewerUri { get; set; }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _tempFiles.ItemsSource = Storage.Instance.AllFilesRecursive.Select(str => new FileItem(str)).ToList();
        }
    }
}