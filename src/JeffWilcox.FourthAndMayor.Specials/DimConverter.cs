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
using System.Windows.Data;

namespace AgFx
{
    public class DimConverter : IValueConverter
    {
        public double TrueOpacity { get; set; }
        public double FalseOpacity { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = true;

            if (value is bool)
            {
                isTrue = (bool)value;
            }
            else if (value is int || value is short || value is long)
            {
                isTrue = 0 != (int)value;
            }
            else if (value is float || value is double)
            {
                isTrue = 0.0 != (double)value;
            }
            else if (value == null)
            {
                isTrue = false;
            }

            if ((string)parameter == "!")
            {
                isTrue = !isTrue;
            }

            return isTrue ? TrueOpacity : FalseOpacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
