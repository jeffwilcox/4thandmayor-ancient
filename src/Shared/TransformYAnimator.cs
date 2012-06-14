﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A utility for animating a horizontal translation value.
    /// </summary>
    public sealed class TransformYAnimator
    {
        /// <summary>
        /// Single static instance of a PropertyPath with string path "X".
        /// </summary>
        private static readonly PropertyPath TranslateXPropertyPath = new PropertyPath("Y");

        /// <summary>
        /// The Storyboard instance for the animation.
        /// </summary>
        private readonly Storyboard _sbRunning = new Storyboard();

        /// <summary>
        /// The DoubleAnimation instance for a running animation.
        /// </summary>
        private readonly DoubleAnimation _daRunning = new DoubleAnimation();

        /// <summary>
        /// The target translate transform instance.
        /// </summary>
        private TranslateTransform _transform;

        /// <summary>
        /// A one-time action for the current GoTo statement only. Cleared if
        /// GoTo is called before the action runs.
        /// </summary>
        private Action _oneTimeAction;

        /// <summary>
        /// Initializes a new instance of the TransformAnimator class.
        /// </summary>
        /// <param name="translateTransform">TranslateTransform instance.</param>
        public TransformYAnimator(TranslateTransform translateTransform)
        {
            Debug.Assert(translateTransform != null);
            _transform = translateTransform;

            _sbRunning.Completed += OnCompleted;
            _sbRunning.Children.Add(_daRunning);
            Storyboard.SetTarget(_daRunning, _transform);
            Storyboard.SetTargetProperty(_daRunning, TranslateXPropertyPath);
        }

        /// <summary>
        /// Gets the current offset value from the translate transform object.
        /// </summary>
        public double CurrentOffset
        {
            get { return _transform.Y; }
        }

        /// <summary>
        /// Targets a new horizontal offset over a specified duration.
        /// </summary>
        /// <param name="targetOffset">The target offset value.</param>
        /// <param name="duration">The duration for the animation.</param>
        public void GoTo(double targetOffset, Duration duration)
        {
            GoTo(targetOffset, duration, null, null);
        }

        public void GoTo(double targetOffset, Duration duration, Action completionAction)
        {
            GoTo(targetOffset, duration, null, completionAction);
        }

        public void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction)
        {
            GoTo(targetOffset, duration, easingFunction, null);
        }

        public void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction, Action completionAction)
        {
            _daRunning.To = targetOffset;
            _daRunning.Duration = duration;
            _daRunning.EasingFunction = easingFunction;
            _sbRunning.Begin();
            _sbRunning.SeekAlignedToLastTick(TimeSpan.Zero);
            _oneTimeAction = completionAction;
        }

        /// <summary>
        /// Updates the easing function of the double animation.
        /// </summary>
        /// <param name="ease">The easing funciton, if any, to use.</param>
        public void UpdateEasingFunction(IEasingFunction ease)
        {
            if (_daRunning != null && _daRunning.EasingFunction != ease)
            {
                _daRunning.EasingFunction = ease;
            }
        }

        /// <summary>
        /// Immediately updates the duration of the running double animation.
        /// </summary>
        /// <param name="duration">The new duration value to use.</param>
        public void UpdateDuration(Duration duration)
        {
            if (_daRunning != null)
            {
                _daRunning.Duration = duration;
            }
        }

        /// <summary>
        /// Handles animation completed
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void OnCompleted(object sender, EventArgs e)
        {
            Action action = _oneTimeAction;
            if (action != null)
            {
                _oneTimeAction = null;
                action();
            }
        }

        /// <summary>
        /// Ensures and creates if needed the animator for an element. Will also
        /// verify that a translate transform is present.
        /// </summary>
        /// <param name="targetElement">The target element.</param>
        /// <param name="animator">The animator reference.</param>
        public static void EnsureAnimator(FrameworkElement targetElement, ref TransformYAnimator animator)
        {
            if (animator == null)
            {
                TranslateTransform transform = TransformYAnimator.GetTranslateTransform(targetElement);
                if (transform != null)
                {
                    animator = new TransformYAnimator(transform);
                }
            }
            if (animator == null)
            {
                throw new InvalidOperationException("The animation system could not be prepared for the target element.");
            }
        }

        /// <summary>
        /// Find a translate transform for the container or create one.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>Returns the TranslateTransform reference.</returns>
        public static TranslateTransform GetTranslateTransform(UIElement container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            TranslateTransform transform = container.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                if (container.RenderTransform == null)
                {
                    transform = new TranslateTransform();
                    container.RenderTransform = transform;
                }
                else if (container.RenderTransform is TransformGroup)
                {
                    TransformGroup g = container.RenderTransform as TransformGroup;
                    transform = (from t in g.Children
                                 where t is TranslateTransform
                                 select (TranslateTransform)t).FirstOrDefault();
                    if (transform == null)
                    {
                        transform = new TranslateTransform();
                        g.Children.Add(transform);
                    }
                }
                else
                {
                    TransformGroup tg = new TransformGroup();
                    var existing = container.RenderTransform;
                    container.RenderTransform = null;
                    tg.Children.Add(existing);
                    transform = new TranslateTransform();
                    tg.Children.Add(transform);
                    container.RenderTransform = tg;
                }
            }
            return transform;
        }
    }
}