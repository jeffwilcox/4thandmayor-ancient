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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.Controls
{
    public class WindowBase : HeaderedContentControl
    {
        public event EventHandler Closing;

        private void OnClosing()
        {
            var handler = Closing;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private PhoneApplicationPage _page;

        private bool _transitionedIn;

        private Panel _templateRoot;

        public event EventHandler PressedBack;

        private Grid _overlay;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;

            _templateRoot = MoreVisualTreeExtensions.FindFirstChildOfType<Panel>(this);
            if (_templateRoot == null)
            {
                throw new InvalidOperationException("Must include a Panel in the root of the template.");
            }

            if (!_transitionedIn)
            {
                var @in = GetTransitionIn();
                if (@in != null && _templateRoot != null)
                {
                    var transition = @in.GetTransition(_templateRoot);
                    transition.Completed += (x, xe) => transition.Stop();
                    transition.Begin();
                }
                _transitionedIn = true;
            }
        }

        private void BuildOverlay()
        {
            var bg = (Color)Resources["PhoneBackgroundColor"];
            _overlay = new Grid();
            _overlay.IsHitTestVisible = true;
            _overlay.Background = new SolidColorBrush(Color.FromArgb(0xa0, bg.R, bg.G, bg.B));
        }

        protected void InsertIntoVisualTree(Panel parentPanel)
        {
            // TODO: Shout form doesn't work here most of the time when coming back from tombstoning... protection added.

            Debug.Assert(parentPanel != null);

            if (parentPanel != null)
            {

                BuildOverlay();
                parentPanel.Children.Add(_overlay);
                parentPanel.Children.Add(this);
            }

            // WARNING: This version will not attach to page back button 
            // key presses and could fail ingestion.
        }

        private object _trayBackgroundColorToRestore;

        private void RestoreTrayBackground()
        {
            if (_trayBackgroundColorToRestore != null)
            {
                var st = typeof(SystemTray);
                var bc = st.GetProperty("BackgroundColor");

                if (bc != null)
                {
                    bc.SetValue(null, _trayBackgroundColorToRestore, null);
                    _trayBackgroundColorToRestore = null;
                }
            }
        }

        private void SaveAndSetTrayBackground()
        {
            var st = typeof(SystemTray);
            var bc = st.GetProperty("BackgroundColor");

            if (bc != null)
            {
                _trayBackgroundColorToRestore = bc.GetValue(null, null);
                if (_trayBackgroundColorToRestore != null)
                {
                    Color c = (Color)_trayBackgroundColorToRestore;
                    if (c.A == 0 && c.R == 0 && c.G == 0 && c.B == 0) //if (c == Colors.Black)
                    {
                        // didn't get set...
                        _trayBackgroundColorToRestore = PhoneTheme.IsDarkTheme ? Colors.Black : Colors.White;
                    }

                    object chrome = Application.Current.Resources["PhoneChromeColor"];
                    if (chrome != null)
                    {
                        bc.SetValue(null, chrome, null);
                    }
                }
            }
        }

        protected void InsertIntoFrame()
        {
            WilcoxTransitionFrame wtf = Application.Current.GetFrame();
            InsertIntoVisualTree(wtf.OverlayGrid);

            _page = wtf.Content as PhoneApplicationPage;
            if (_page != null)
            {
                _page.BackKeyPress += OnBackKeyPress;

                SaveAndSetTrayBackground();
            }
        }

        private void OnBackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            var handler = PressedBack;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            CloseWindow();
        }

        // Worth exposing here?
        public void Close()
        {
            CloseWindow();
        }

        protected virtual void OnClose()
        {
        }

        protected void CloseWindow()
        {
            // for users
            OnClosing();

            // for us
            OnClose();

            RestoreTrayBackground();

            // Remove from the parent visual tree.
            var me = this;

            if (_page != null)
            {
                _page.BackKeyPress -= OnBackKeyPress;
                _page = null;
            }

            Action removeVisualFromParent = () => 
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        Panel p = me.Parent as Panel;
                        if (p != null)
                        {
                            p.Children.Remove(me);
                        }
                    });
                };
            Action removeOverlay = () =>
                {
                    Grid overlay = _overlay;
                    _overlay = null;
                    if (overlay != null)
                    {
                        Panel p = overlay.Parent as Panel;
                        if (p != null)
                        {
                            p.Children.Remove(overlay);
                        }
                    }
                };
            Action removeVisuals = () =>
                {
                    removeOverlay();
                    removeVisualFromParent();
                };

            // Animate.
            var @out = GetTransitionOut();
            if (@out != null && _templateRoot != null)
            {
                var transition = @out.GetTransition(_templateRoot);
                transition.Completed += (x, xe) =>
                    {
                        me.Opacity = 0;

                        transition.Stop();

                        removeVisuals();
                    };
                transition.Begin();
            }
            else
            {
                removeVisuals();
            }
        }

        protected virtual TransitionElement GetTransitionIn()
        {
            return new SwivelTransition
            {
                Mode = SwivelTransitionMode.BackwardIn,
            };
        }

        protected virtual TransitionElement GetTransitionOut()
        {
            return new SwivelTransition
            {
                Mode = SwivelTransitionMode.BackwardOut,
            };
        }
    }
}
