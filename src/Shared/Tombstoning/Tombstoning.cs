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

using System.Collections.Generic;
using Microsoft.Phone.Controls;
using System;

namespace JeffWilcox.Controls
{
    public static class Tombstoning
    {
        public static void Save(this Pivot pivot, IDictionary<string, object> state)
        {
            if (pivot != null)
            {
                try
                {
                    state["_pi"] = pivot.SelectedIndex;
                }
                catch (InvalidOperationException)
                {
                    // Occassional tombstoning bugs like this:
                    /*
You can only use State between OnNavigatedTo and OnNavigatedFrom
System.InvalidOperationException
   at Microsoft.Phone.Controls.PhoneApplicationPage.get_State()
   at JeffWilcox.FourthAndMayor.Views.Search.OnNavigatedFrom(NavigationEventArgs e)
   at Microsoft.Phone.Controls.PhoneApplicationPage.InternalOnNavigatedFrom(NavigationEventArgs e)
   at System.Windows.Navigation.NavigationService.RaiseNavigated(Object content, Uri uri, NavigationMode mode, Boolean isNavigationInitiator, PhoneApplicationPage existingContentPage, PhoneApplicationPage newContentPage)
   at System.Windows.Navigation.NavigationService.CompleteNavigation(DependencyObject content, NavigationMode mode)
   at System.Windows.Navigation.NavigationService.ContentLoader_BeginLoad_Callback(IAsyncResult result)
   at System.Windows.Navigation.PageResourceContentLoader.BeginLoad_OnUIThread(AsyncCallback userCallback, PageResourceContentLoaderAsyncResult result)
   at System.Windows.Navigation.PageResourceContentLoader.<>c__DisplayClass4.<BeginLoad>b__0(Object args)
   at System.Reflection.RuntimeMethodInfo.InternalInvoke(RuntimeMethodInfo rtmi, Object obj, BindingFlags invokeAttr, Binder binder, Object parameters, CultureInfo culture, Boolean isBinderDefault, Assembly caller, Boolean verifyAccess, StackCrawlMark& stackMark)
   at System.Reflection.RuntimeMethodInfo.InternalInvoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture, StackCrawlMark& stackMark)
                     * */
                }
            }
        }

        public static void Restore(this Pivot pivot, IDictionary<string, object> state, out bool didItChange)
        {
            didItChange = false;
            object newIndex;
            if (pivot != null)
            {
                if (state.TryGetValue("_pi", out newIndex))
                {
                    int index = (int)newIndex;
                    Pivot p = pivot;

                    if (p.SelectedIndex == index)
                    {
                        didItChange = false;
                        return;
                    }
                    //pivot.Dispatcher.BeginInvoke(() =>
                    {
                        // Since I have my fixed pivot here... this should no longer be a bug.
                        /*try
                        {*/
                            p.SelectedIndex = index;
                        /*}
                        catch (ArgumentOutOfRangeException)
                        {
                        }*/
                    }
                    //);
                }
            }
        }
    }
}