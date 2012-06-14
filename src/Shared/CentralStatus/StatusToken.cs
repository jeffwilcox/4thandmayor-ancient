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

namespace JeffWilcox.Controls
{
    public class StatusToken
    {
        private bool _isLoading;
        private string _message;

        public event EventHandler IsLoadingChanged;

        public event EventHandler MessageChanged;

        public StatusToken()
        {
#if DEBUG
            _id = _allIds++;
#endif
        }

        public StatusToken(string message) : this()
        {
            Message = message;
        }

#if DEBUG
        private bool _deleted;
        private int _id;
        private static int _allIds;

        public override string ToString()
        {
            return _id.ToString();
        }
#endif

        // Will return true if it did delete and clear the reference, false if
        // it was already null.
        public static bool TryComplete(ref StatusToken token)
        {
            if (token != null)
            {
                token.Complete();
                token = null;
                return true;
            }

            return false;
        }

        public void CompleteWithAcknowledgement(string temporaryMessage = "OK")
        {
            bool isActive = false;
            if (CentralStatusManager.Instance.ActiveMessageToken == this)
            {
                isActive = true;
            }

            Complete();

            // Only acknowledge if it's the active message the user is seeing.
            if (isActive)
            {
                CentralStatusManager.Instance.BeginShowTemporaryMessage(temporaryMessage);
            }
        }

        public virtual void Complete()
        {
#if DEBUG
            if (_deleted)
            {
                throw new InvalidOperationException("This token has already been completed.");
            }

            _deleted = true;
#endif

            // Completes the message.
            CentralStatusManager.Instance.DeleteToken(this);
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }

            set
            {
                _isLoading = value;
                OnIsLoadingChanged(EventArgs.Empty);
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }

            protected set
            {
                _message = value;
                OnMessageChanged(EventArgs.Empty);
            }
        }

        private void OnIsLoadingChanged(EventArgs e)
        {
            var handler = IsLoadingChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnMessageChanged(EventArgs e)
        {
            var handler = MessageChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
