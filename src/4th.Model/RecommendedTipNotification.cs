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

using Newtonsoft.Json.Linq;

namespace JeffWilcox.FourthAndMayor.Model
{
    public class RecommendedTipNotification : Notification
    {
        public Tip RecommendedTip { get; private set; }

        public string Text { get; private set; }

        public RecommendedTipNotification(JToken item)
        {
            string text = Json.TryGetJsonProperty(item, "name");
            if (string.IsNullOrEmpty(text))
            {
                text = "Popular tip";
            }
            Text = text;

            var jtip = item["tip"];
            if (jtip != null)
            {
                Tip tip = Tip.ParseJson(jtip, typeof(RecommendedTipNotification), string.Empty);
                if (tip != null)
                {
                    RecommendedTip = tip;
                }
            }
        }
    }
}
