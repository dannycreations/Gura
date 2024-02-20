using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DeltaDNA
{
	internal static class Network
	{
		internal static IEnumerator SendRequest(HttpRequest request, Action<int, string, string> completionHandler)
		{
			WWW www;
			if (request.HTTPMethod == HttpRequest.HTTPMethodType.POST)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				WWWForm wwwform = new WWWForm();
				foreach (KeyValuePair<string, string> keyValuePair in Utils.HashtableToDictionary<string, string>(wwwform.headers))
				{
					dictionary[keyValuePair.Key] = keyValuePair.Value;
				}
				foreach (KeyValuePair<string, string> keyValuePair2 in request.getHeaders())
				{
					dictionary[keyValuePair2.Key] = keyValuePair2.Value;
				}
				byte[] bytes = Encoding.UTF8.GetBytes(request.HTTPBody);
				www = new WWW(request.URL, bytes, dictionary);
			}
			else
			{
				www = new WWW(request.URL);
			}
			float timer = 0f;
			bool timedout = false;
			while (!www.isDone)
			{
				if (timer > (float)request.TimeoutSeconds)
				{
					timedout = true;
					break;
				}
				timer += Time.deltaTime;
				yield return null;
			}
			int statusCode = 1001;
			string data = null;
			string error = null;
			if (timedout)
			{
				www.Dispose();
				error = "connect() timed out";
			}
			else
			{
				statusCode = Network.ReadStatusCode(www);
				data = www.text;
				error = www.error;
			}
			if (completionHandler != null)
			{
				completionHandler(statusCode, data, error);
			}
			yield break;
		}

		private static int ReadStatusCode(WWW www)
		{
			int num = 200;
			if (www.responseHeaders.ContainsKey("STATUS"))
			{
				MatchCollection matchCollection = Regex.Matches(www.responseHeaders["STATUS"], "^.*\\s(\\d{3})\\s.*$");
				if (matchCollection.Count > 0 && matchCollection[0].Groups.Count > 0)
				{
					num = Convert.ToInt32(matchCollection[0].Groups[1].Value);
				}
			}
			else if (!string.IsNullOrEmpty(www.error))
			{
				MatchCollection matchCollection2 = Regex.Matches(www.error, "^(\\d{3})\\s.*$");
				if (matchCollection2.Count > 0 && matchCollection2[0].Groups.Count > 0)
				{
					num = Convert.ToInt32(matchCollection2[0].Groups[1].Value);
				}
				else
				{
					num = 1002;
				}
			}
			else if (string.IsNullOrEmpty(www.text))
			{
				num = 204;
			}
			return num;
		}

		private const string HeaderKey = "STATUS";

		private const string StatusRegex = "^.*\\s(\\d{3})\\s.*$";

		private const string ErrorRegex = "^(\\d{3})\\s.*$";
	}
}
