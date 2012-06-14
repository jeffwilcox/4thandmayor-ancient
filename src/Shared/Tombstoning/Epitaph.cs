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
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

// dapoll
/*
Silverlight and Beyond (SLaB) License:
--------------------------------------

Microsoft Public License (Ms-PL)
(http://www.opensource.org/licenses/ms-pl.html)

This license governs use of the accompanying software. If you use the software, you
accept this license. If you do not accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
same meaning here as under U.S. copyright law.
A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
*/

namespace JeffWilcox.Controls
{
    /// <summary>
    /// Manages tombstoning of controls through easy-to-use attached properties.
    /// </summary>
    [ContentProperty("Properties")]
    public class Epitaph
    {
        private static List<WeakReference> ElementsToTombstone { get; set; }
        static Epitaph()
        {
            ElementsToTombstone = new List<WeakReference>();
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tombstoner"/> class.
        /// </summary>
        public Epitaph()
        {
            Properties = new List<TombstoneProperty>();
        }

        private static void Initialize()
        {
            if (DesignerProperties.IsInDesignTool)
                return;
            if (PhoneApplicationService.Current == null)
                Deployment.Current.Dispatcher.BeginInvoke(Initialize);
            else
            {
                PhoneApplicationService.Current.Deactivated += (o, a) =>
                {
                    foreach (var item in ElementsToTombstone)
                    {
                        if (item.IsAlive)
                        {
                            Epitaph ts = GetTombstoner((FrameworkElement)item.Target);
                            if (ts != null)
                                ts.Tombstone();
                        }
                    }
                };
            }
        }

        private void Tombstone()
        {
            if (GetShouldTombstone(Parent))
                foreach (var prop in Properties)
                    prop.Tombstone(this.Parent, this.RealTombstoneId);
        }

        private void Untombstone()
        {
            if (GetShouldTombstone(Parent))
                foreach (var prop in Properties)
                    prop.Untombstone(this.Parent, this.RealTombstoneId);
        }

        /// <summary>
        /// Gets the tombstoner for the control.
        /// </summary>
        /// <param name="obj">The control being tombstoned.</param>
        /// <returns>The tombstoner for the control.</returns>
        public static Epitaph GetTombstoner(FrameworkElement obj)
        {
            return (Epitaph)obj.GetValue(TombstonerProperty);
        }

        /// <summary>
        /// Sets the tombstoner for the control.
        /// </summary>
        /// <param name="obj">The control being tombstoned.</param>
        /// <param name="value">The tombstoner for the control.</param>
        public static void SetTombstoner(FrameworkElement obj, Epitaph value)
        {
            obj.SetValue(TombstonerProperty, value);
        }

        /// <summary>
        /// Represents the Tombstoner attached dependency property.
        /// </summary>
        public static readonly DependencyProperty TombstonerProperty =
            DependencyProperty.RegisterAttached("Tombstoner", typeof(Epitaph), typeof(Epitaph), new PropertyMetadata(default(Epitaph), OnTombstonerChanged));

        private static void OnTombstonerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            OnTombstonerChanged((FrameworkElement)obj, (Epitaph)args.OldValue, (Epitaph)args.NewValue);
        }

        private static void OnTombstonerChanged(FrameworkElement obj, Epitaph oldValue, Epitaph newValue)
        {
            if (DesignerProperties.IsInDesignTool)
                return;
            if (oldValue != null)
                oldValue.Parent = null;
            if (newValue != null)
            {
                newValue.Parent = obj;
                ElementsToTombstone.Add(new WeakReference(obj));
                CleanupElementsToTombstone();
            }
            if (newValue != null)
                obj.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        newValue.CalculateTombstoneId();
                    }
                    catch
                    {
                        obj.Loaded += (o, a) => newValue.CalculateTombstoneId();
                    }
                });
        }

        private static int CleanupCount { get; set; }
        private static void CleanupElementsToTombstone()
        {
            CleanupCount++;
            if (CleanupCount % 100 != 0)
                return;
            for (int x = 0; x < ElementsToTombstone.Count; x++)
            {
                if (!ElementsToTombstone[x].IsAlive || GetTombstoner((FrameworkElement)ElementsToTombstone[x].Target) == null)
                {
                    ElementsToTombstone.RemoveAt(x);
                    x--;
                }
            }
        }


        /// <summary>
        /// Gets the whether the tombstoner should store values for the control.  Can be bound so conditionalize this within
        /// templates upon whether a TombstoneId has been set.
        /// </summary>
        /// <param name="obj">The object to tombstone.</param>
        /// <returns>Whether the tombstoner should store values for the control.</returns>
        public static bool GetShouldTombstone(FrameworkElement obj)
        {
            return (bool)obj.GetValue(ShouldTombstoneProperty);
        }

        /// <summary>
        /// Sets the whether the tombstoner should store values for the control.  Can be bound so conditionalize this within
        /// templates upon whether a TombstoneId has been set.
        /// </summary>
        /// <param name="obj">The object to tombstone.</param>
        /// <param name="value">if set to <c>true</c>, the object will be tombstoned.</param>
        public static void SetShouldTombstone(FrameworkElement obj, bool value)
        {
            obj.SetValue(ShouldTombstoneProperty, value);
        }

        /// <summary>
        /// Represents the ShouldTombstone attached dependency property.
        /// </summary>
        public static readonly DependencyProperty ShouldTombstoneProperty =
            DependencyProperty.RegisterAttached("ShouldTombstone", typeof(bool), typeof(Epitaph), new PropertyMetadata(true, OnShouldTombstoneChanged));

        private static void OnShouldTombstoneChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            OnShouldTombstoneChanged((FrameworkElement)obj, (bool)args.OldValue, (bool)args.NewValue);
        }

        private static void OnShouldTombstoneChanged(FrameworkElement obj, bool oldValue, bool newValue)
        {
        }


        /// <summary>
        /// Gets the tombstone id.  This value must be set on any control that will be tombstoned, and should be unique within the scope
        /// of any visual parent that also has a TombstoneId, or else within a Page.
        /// </summary>
        /// <param name="obj">The object to tombstone.</param>
        /// <returns>The TombstoneId of the control.</returns>
        public static string GetTombstoneId(FrameworkElement obj)
        {
            return (string)obj.GetValue(TombstoneIdProperty);
        }

        /// <summary>
        /// Gets the tombstone id.  This value must be set on any control that will be tombstoned, and should be unique within the scope
        /// of any visual parent that also has a TombstoneId, or else within a Page.
        /// </summary>
        /// <param name="obj">The object to tombstone.</param>
        /// <param name="value">The tombstone id to assign to the control.</param>
        public static void SetTombstoneId(FrameworkElement obj, string value)
        {
            obj.SetValue(TombstoneIdProperty, value);
        }

        /// <summary>
        /// Represents the TombstoneId attached dependency property.
        /// </summary>
        public static readonly DependencyProperty TombstoneIdProperty =
            DependencyProperty.RegisterAttached("TombstoneId", typeof(string), typeof(Epitaph), new PropertyMetadata(default(string), OnTombstoneIdChanged));

        private static void OnTombstoneIdChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            OnTombstoneIdChanged((FrameworkElement)obj, (string)args.OldValue, (string)args.NewValue);
        }

        private static void OnTombstoneIdChanged(FrameworkElement obj, string oldValue, string newValue)
        {
        }

        private string RealTombstoneId { get; set; }
        private FrameworkElement Parent { get; set; }

        private void CalculateTombstoneId()
        {
            FrameworkElement obj = Parent;
            string fullId = "";
            DependencyObject cur = obj;
            do
            {
                if (cur is FrameworkElement)
                {
                    string id = GetTombstoneId((FrameworkElement)cur);
                    if (id != null)
                        fullId = id + "." + fullId;
                    if (cur is PhoneApplicationFrame)
                        fullId = ((PhoneApplicationFrame)cur).CurrentSource.OriginalString + "." + fullId;
                }
                cur = VisualTreeHelper.GetParent(cur);
            }
            while (cur != null);
            fullId = fullId.Substring(0, fullId.Length - 1);
            RealTombstoneId = fullId;
            Untombstone();
        }

        /// <summary>
        /// Gets the set of properties to tombstone.
        /// </summary>
        public List<TombstoneProperty> Properties { get; private set; }
    }

    /// <summary>
    /// Represents a failure to restore a value from tombstoning.  Primarily used for debugging purposes.
    /// </summary>
    public class TombstoneFailureEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the reason.
        /// </summary>
        public object Reason { get; internal set; }
    }

    /// <summary>
    /// Represents a property on a control that should be tombstoned.
    /// </summary>
    public class TombstoneProperty
    {
        /// <summary>
        /// Gets or sets the name of the property to tombstone.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }
        /// <summary>
        /// Gets or sets the a style whose TargetType indicates the type on which to find the property
        /// named PropertyName when tombstoning an attached property.
        /// </summary>
        /// <value>
        /// The attached property target type style.
        /// </value>
        public Style AttachedPropertyTargetTypeStyle { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether attempting to restore a tombstoned value for this property should
        /// be repeated until the operation does not throw an exception.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the tombstoner should repeat restoration of the property value until success; otherwise, <c>false</c>.
        /// </value>
        public bool RepeatUntilSuccess { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whetherattempting to restore a tombstoned value for this property should
        /// be repeated until the operation results in a the value round-tripping (i.e. getting after setting returns the just-set value).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the tombstoner should repeat restoration of the property value until it round-trips; otherwise, <c>false</c>.
        /// </value>
        public bool RepeatUntilRoundTrip { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether attempting to repeat setting the value (due to RepeatUntilRoundTrip)
        /// should be halted if something else explicitly changes the value in the interim (e.g. the user scrolls a ScrollViewer).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the tombstoner should stop repeating restoration if the property's value changes; otherwise, <c>false</c>.
        /// </value>
        public bool StopRepeatingIfChanged { get; set; }
        /// <summary>
        /// Gets or sets the retry rate for repeating restoration of values.
        /// </summary>
        /// <value>
        /// The retry rate.
        /// </value>
        public TimeSpan RetryRate { get; set; }
        private string ComputedPropertyName
        {
            get
            {
                if (AttachedPropertyTargetTypeStyle == null)
                    return PropertyName;
                else
                    return AttachedPropertyTargetTypeStyle.TargetType.FullName + "." + PropertyName;
            }
        }
        private bool _HasLastValue;
        private object _LastValue;
        internal void Tombstone(FrameworkElement obj, string tombstoneId)
        {
            Func<object> getter;
            if (AttachedPropertyTargetTypeStyle == null)
                getter = () => obj.GetType().GetProperty(PropertyName).GetValue(obj, null);
            else
                getter = () => AttachedPropertyTargetTypeStyle.TargetType.GetMethod("Get" + PropertyName).Invoke(null, new object[] { obj });
            object value = getter();
            PhoneApplicationService.Current.State[tombstoneId + "->" + ComputedPropertyName] = value;
        }
        internal void Untombstone(FrameworkElement obj, string tombstoneId)
        {
            Func<object> getter;
            Action<object> setter;
            if (AttachedPropertyTargetTypeStyle == null)
            {
                getter = () => obj.GetType().GetProperty(PropertyName).GetValue(obj, null);
                setter = val => obj.GetType().GetProperty(PropertyName).SetValue(obj, val, null);
            }
            else
            {
                getter = () => AttachedPropertyTargetTypeStyle.TargetType.GetMethod("Get" + PropertyName).Invoke(null, new object[] { obj });
                setter = val => AttachedPropertyTargetTypeStyle.TargetType.GetMethod("Set" + PropertyName).Invoke(null, new object[] { obj, val });
            }
            if (DesignerProperties.IsInDesignTool)
                return;
            bool failed = false;
            object failureReason = null;
            if (PhoneApplicationService.Current == null)
            {
                failed = true;
                failureReason = "PhoneApplicationService not yet initialized";
            }
            else
            {
                try
                {
                    string key = tombstoneId + "->" + ComputedPropertyName;
                    if (PhoneApplicationService.Current.State.ContainsKey(key))
                    {
                        object value = PhoneApplicationService.Current.State[key];
                        setter(value);
                        object newValue;
                        if (RepeatUntilRoundTrip && !object.Equals(value, newValue = getter()))
                        {
                            failed = !(StopRepeatingIfChanged && _HasLastValue && !object.Equals(_LastValue, newValue));
                            if (failed)
                                failureReason = "Round trip failed";
                            if (StopRepeatingIfChanged)
                            {
                                _HasLastValue = true;
                                _LastValue = newValue;
                            }
                        }
                        if (!failed)
                            PhoneApplicationService.Current.State.Remove(key);
                    }
                }
                catch (Exception ex)
                {
                    failureReason = ex;
                    if (RepeatUntilSuccess)
                    {
                        failed = true;
                    }
                }
            }
            if (failed)
                Retry(() => Untombstone(obj, tombstoneId));
            var trf = TombstoneRestoreFailure;
            if (failureReason != null && trf != null)
                trf(this, new TombstoneFailureEventArgs { Reason = trf });
            else
            {
                _LastValue = null;
                _HasLastValue = false;
            }
        }
        /// <summary>
        /// Occurs when a restoring a tombstoned value fails.
        /// </summary>
        public event EventHandler<TombstoneFailureEventArgs> TombstoneRestoreFailure;
        private void Retry(Action act)
        {
            if (RetryRate.Equals(TimeSpan.Zero))
                Deployment.Current.Dispatcher.BeginInvoke(act);
            else
            {
                new Thread(o =>
                {
                    Thread.Sleep(RetryRate);
                    Deployment.Current.Dispatcher.BeginInvoke(act);
                }).Start();
            }
        }
    }
}
