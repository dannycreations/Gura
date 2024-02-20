using System;
using System.Collections;
using UnityEngine;

namespace DeltaDNA
{
	internal class Engage
	{
		internal static IEnumerator Request(MonoBehaviour caller, EngageRequest request, EngageResponse response)
		{
			string requestJSON = request.ToJSON();
			string url = Singleton<DDNA>.Instance.ResolveEngageURL(requestJSON);
			HttpRequest httpRequest = new HttpRequest(url);
			httpRequest.HTTPMethod = HttpRequest.HTTPMethodType.POST;
			httpRequest.HTTPBody = requestJSON;
			httpRequest.TimeoutSeconds = Singleton<DDNA>.Instance.Settings.HttpRequestEngageTimeoutSeconds;
			httpRequest.setHeader("Content-Type", "application/json");
			Action<int, string, string> httpHandler = delegate(int statusCode, string data, string error)
			{
				string text = "DDSDK_ENGAGEMENT_" + request.DecisionPoint + "_" + request.Flavour;
				if (statusCode < 400 && error == null)
				{
					try
					{
						PlayerPrefs.SetString(text, data);
					}
					catch (Exception ex)
					{
						Logger.LogWarning("Unable to cache engagement: " + ex.Message);
					}
				}
				else
				{
					Logger.LogDebug(string.Concat(new object[] { "Engagement failed with ", statusCode, " ", error }));
					if (PlayerPrefs.HasKey(text))
					{
						Logger.LogDebug("Using cached response");
						data = "{\"isCachedResponse\":true," + PlayerPrefs.GetString(text).Substring(1);
					}
				}
				response(data, statusCode, error);
			};
			yield return caller.StartCoroutine(Network.SendRequest(httpRequest, httpHandler));
			yield break;
		}

		internal static void ClearCache()
		{
		}
	}
}
