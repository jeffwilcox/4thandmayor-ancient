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

namespace JeffWilcox.FourthAndMayor
{
    public class UserIgnoreException : Exception
    {
        public UserIgnoreException() : base()
        {}
    }

    /// <summary>
    /// An exception type designed for use when data binding will directly show
    /// the stack trace message to end users (in message boxes or other 
    /// situations).
    /// </summary>
    public class UserIntendedException : Exception
    {
        /// <summary>
        /// Backing field for the stack text to use when overriding.
        /// </summary>
        private string _stack;

        /// <summary>
        /// Initializes a new instance of the UserIntendedException exception
        /// type. It is designed to allow the throwing of an exception that will
        /// be shown to the user in a data bound scenario.
        /// </summary>
        /// <param name="message">The message or title of the exception.</param>
        /// <param name="customStackTraceText">The descriptive details.</param>
        public UserIntendedException(string message, string customStackTraceText) : base(message)
        {
            _stack = customStackTraceText;
        }

        public UserIntendedException(string message, Exception innerException)
            : base(message, innerException)
        {
            _stack = innerException == null ? "Unknown stack" : innerException.StackTrace;
        }

        /// <summary>
        /// Gets the overridden stack trace text.
        /// </summary>
        public override string StackTrace
        {
            get
            {
                return _stack;
            }
        }
    }
}