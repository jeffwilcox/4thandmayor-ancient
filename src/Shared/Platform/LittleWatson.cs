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
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using JeffWilcox.Controls;
using Microsoft.Phone.Tasks;

// LittleWatson originally by Andy Pennell
// http://blogs.msdn.com/b/andypennell/archive/2010/11/01/error-reporting-on-windows-phone-7.aspx

namespace JeffWilcox.FourthAndMayor
{
    public class LittleWatson
    {
        const string filename = "LittleWatson.txt";
        const string neverFilename = "IgnoreLittleWatson.txt";

        public static void ReportException(Exception ex, string extra)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    SafeDeleteFile(store);
                    using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                    {
                        IAppInfo iai = Application.Current as IAppInfo;
                        output.WriteLine(iai == null ? "Unknown version" : iai.Version);

                        output.WriteLine(extra);
                        output.WriteLine(ex.Message);
                        output.WriteLine(ex.GetType().ToString());
                        output.WriteLine(ex.StackTrace);

                        output.WriteLine("-- App details --");
                        IShareDiagnosticInformation diag = Application.Current as IShareDiagnosticInformation;
                        if (diag != null)
                        {
                            output.WriteLine(diag.DiagnosticInformation);
                        }

                        output.WriteLine("-- Other details --");
                        output.WriteLine("Time: {0}", DateTime.Now.ToUniversalTime().ToString("r"));
                        output.WriteLine("Culture: {0}", CultureInfo.CurrentCulture);
                        output.WriteLine("OS  Version: {0}", Environment.OSVersion);
                        output.WriteLine("CLR Version: {0}", Environment.Version);
                        output.WriteLine("Device Type: {0}", Microsoft.Devices.Environment.DeviceType);

                        try
                        {
                            using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                output.WriteLine("FreeSpace: {0:f3} MB", appStorage.AvailableFreeSpace/1024f/1024f);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void CheckForPreviousException()
        {
            try
            {
                //bool ignoreAll = false;
                string contents = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(filename))
                    {
                        using (TextReader reader = new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            contents = reader.ReadToEnd();
                        }
                        SafeDeleteFile(store);
                    }
                    /*if (store.FileExists(neverFilename))
                    {
                        ignoreAll = true;
                        Debug.WriteLine("This user has requested to not be bugged in this manner.");
                    }*/
                }
                if (contents != null)
                {
                    if(MessageBox.Show(
                        "Will you help improve the app by sending the error to the developer?", 
                        "A crash happened previously",
                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        EmailComposeTask email = new EmailComposeTask();
                        email.To = "4thandmayor@gmail.com";
                        email.Subject = "Bug Report for 4th and Mayor";
                        email.Body = contents;
                        email.Show();
                    };
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }

        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(filename);
            }
            catch (Exception)
            {
            }
        }
    }
}
