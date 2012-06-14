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
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JeffWilcox.FourthAndMayor;
using Microsoft.Phone.Controls;

namespace About
{
    public partial class FileViewer : PhoneApplicationPage
    {
        public FileViewer()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string filename = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("file", out filename))
            {
                ApplicationTitle.Text = filename.ToUpperInvariant();
            }

            var bytes = Storage.Instance.Read(filename);
            MemoryStream ms = new MemoryStream(bytes);
            using (StreamReader sr = new StreamReader(ms))
            {
                string line;
                //bool lastWasEmpty = true;
                do
                {
                    line = sr.ReadLine();

                    if (line == string.Empty)
                    {
                        Rectangle r = new Rectangle
                        {
                            Height = 20,
                        };
                        _stack.Children.Add(r);
                        //lastWasEmpty = true;
                    }
                    else
                    {
                        TextBlock tb = new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            Text = line,
                            Style = (Style)Application.Current.Resources["PhoneTextNormalStyle"],
                        };
                        /*if (!lastWasEmpty)
                        {
                            tb.Opacity = 0.7;
                        }*/
                        //lastWasEmpty = false;
                        _stack.Children.Add(tb);
                    }
                } while (line != null);
            }
        }
    }
}