using System;
using System.Collections.Generic;
using DeltaDNA.MiniJSON;

namespace DeltaDNA
{
	internal class EngageRequest
	{
		public EngageRequest(string decisionPoint)
		{
			this.DecisionPoint = decisionPoint;
			this.Flavour = "engagement";
			this.Parameters = new Dictionary<string, object>();
		}

		public string DecisionPoint { get; private set; }

		public string Flavour { get; set; }

		public Dictionary<string, object> Parameters { get; set; }

		public string ToJSON()
		{
			string text;
			try
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>
				{
					{
						"userID",
						Singleton<DDNA>.Instance.UserID
					},
					{ "decisionPoint", this.DecisionPoint },
					{ "flavour", this.Flavour },
					{
						"sessionID",
						Singleton<DDNA>.Instance.SessionID
					},
					{
						"version",
						Settings.ENGAGE_API_VERSION
					},
					{
						"sdkVersion",
						Settings.SDK_VERSION
					},
					{
						"platform",
						Singleton<DDNA>.Instance.Platform
					},
					{
						"timezoneOffset",
						Convert.ToInt32(ClientInfo.TimezoneOffset)
					}
				};
				if (ClientInfo.Locale != null)
				{
					dictionary.Add("locale", ClientInfo.Locale);
				}
				if (this.Parameters != null && this.Parameters.Count > 0)
				{
					dictionary.Add("parameters", this.Parameters);
				}
				text = Json.Serialize(dictionary);
			}
			catch (Exception ex)
			{
				Logger.LogError("Error serialising engage request: " + ex.Message);
				text = null;
			}
			return text;
		}

		public override string ToString()
		{
			return string.Format(string.Concat(new object[] { "[EngageRequest]", this.DecisionPoint, "(", this.Flavour, ")\n", this.Parameters }), new object[0]);
		}
	}
}
