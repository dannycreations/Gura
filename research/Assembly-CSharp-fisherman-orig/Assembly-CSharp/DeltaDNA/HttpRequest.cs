using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	internal class HttpRequest
	{
		internal HttpRequest(string url)
		{
			this.URL = url;
			this.TimeoutSeconds = Singleton<DDNA>.Instance.Settings.HttpRequestCollectTimeoutSeconds;
		}

		internal string URL { get; private set; }

		internal HttpRequest.HTTPMethodType HTTPMethod { get; set; }

		internal string HTTPBody { get; set; }

		internal int TimeoutSeconds { get; set; }

		internal Dictionary<string, string> getHeaders()
		{
			return this.headers;
		}

		internal void setHeader(string field, string value)
		{
			this.headers[field] = value;
		}

		public override string ToString()
		{
			return string.Concat(new object[] { "HttpRequest: ", this.URL, "\n", this.HTTPMethod, "\n", this.HTTPBody, "\n" });
		}

		private Dictionary<string, string> headers = new Dictionary<string, string>();

		internal enum HTTPMethodType
		{
			GET,
			POST
		}
	}
}
