using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Net.Http
{
    public class HttpClient
    {
    }

    public class HttpResponseMessage
    {
        public bool IsSuccessStatusCode { get; private set; }
        public HttpContent Content { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }
    }

    public class HttpContent
    {
    }

    public class StringContent : HttpContent
    {
        private string _value;

        public StringContent(string value)
        {
            _value = value;
        }
    }
}
