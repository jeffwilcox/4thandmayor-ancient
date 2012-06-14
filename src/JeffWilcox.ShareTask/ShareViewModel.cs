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
using System.Globalization;
using Microsoft.Phone.Tasks;

namespace JeffWilcox.ShareTask
{
    internal class ShareViewModel
    {
        public ShareViewModel()
        {
        }

        public bool Show(ShareChoice choice)
        {
            switch (choice)
            {
                case ShareChoice.Mail:
                    var ect = new EmailComposeTask
                    {
                        Subject = Title,
                        Body = string.Format(CultureInfo.CurrentCulture,
                        "{0}\n\n{1}\n{2}\n{4}{3}",
                        Message,
                        Title,
                        LinkUri,
                        Footer,
                        (string.IsNullOrEmpty(Footer) ? string.Empty : "\n"))
                    };
                    ect.Show();
                    return true;

                case ShareChoice.Messaging:
                    // No footer for SMS.
                    var sms = new SmsComposeTask
                    {
                        Body = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}\n{2}", Message, Title, LinkUri)
                    };
                    sms.Show();
                    return true;

                case ShareChoice.SocialNetwork:
                    var slt = new ShareLinkTask
                    {
                        LinkUri = LinkUri,
                        Title = Title,
                        Message = Message
                    };
                    slt.Show();
                    return true;

                default: 
                    break;
            }

            return false;
        }

        public static ShareViewModel Parse(IDictionary<string, string> queryString)
        {
            ShareViewModel svm = new ShareViewModel();

            string s;
            if (queryString.TryGetValue("uri", out s))
            {
                Uri u;
                if (Uri.TryCreate(s, UriKind.Absolute, out u))
                {
                    svm.LinkUri = u;
                }
            }

            if (queryString.TryGetValue("title", out s))
            {
                svm.Title = s;
            }

            if (queryString.TryGetValue("message", out s))
            {
                svm.Message = s;
            }

            if (queryString.TryGetValue("footer", out s))
            {
                svm.Footer = s;
            }

            return svm;
        }

        public Uri LinkUri { get; set; }
        public string Message { get; set; }
        public string Footer { get; set; }
        public string Title { get; set; }
    }
}
