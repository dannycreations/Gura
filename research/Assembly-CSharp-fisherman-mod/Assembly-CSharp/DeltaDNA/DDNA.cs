using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using DeltaDNA.MiniJSON;
using UnityEngine;

namespace DeltaDNA
{
	public class DDNA : Singleton<DDNA>
	{
		protected DDNA()
		{
			this.Settings = new Settings();
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnNewSession;

		private void Awake()
		{
			if (this.eventStore == null)
			{
				string text = null;
				if (this.Settings.UseEventStore)
				{
					text = Settings.EVENT_STORAGE_PATH.Replace("{persistent_path}", Application.persistentDataPath);
				}
				this.eventStore = new EventStore(text);
			}
			GameObject gameObject = new GameObject();
			this.IosNotifications = gameObject.AddComponent<IosNotifications>();
			gameObject.transform.parent = base.gameObject.transform;
			GameObject gameObject2 = new GameObject();
			this.AndroidNotifications = gameObject2.AddComponent<AndroidNotifications>();
			gameObject2.transform.parent = base.gameObject.transform;
		}

		public void StartSDK(string envKey, string collectURL, string engageURL)
		{
			this.StartSDK(envKey, collectURL, engageURL, null);
		}

		public void StartSDK(string envKey, string collectURL, string engageURL, string userID)
		{
			object @lock = DDNA._lock;
			lock (@lock)
			{
				bool flag = false;
				if (string.IsNullOrEmpty(this.UserID))
				{
					flag = true;
					if (string.IsNullOrEmpty(userID))
					{
						userID = this.GenerateUserID();
					}
				}
				else if (!string.IsNullOrEmpty(userID) && this.UserID != userID)
				{
					flag = true;
				}
				this.UserID = userID;
				if (flag)
				{
					Logger.LogInfo("Starting DDNA SDK with new user " + this.UserID);
				}
				else
				{
					Logger.LogInfo("Starting DDNA SDK with existing user " + this.UserID);
				}
				this.EnvironmentKey = envKey;
				this.CollectURL = collectURL;
				this.EngageURL = engageURL;
				this.Platform = ClientInfo.Platform;
				this.NewSession();
				this.started = true;
				if (this.launchNotificationEvent != null)
				{
					this.RecordEvent<GameEvent>(this.launchNotificationEvent);
					this.launchNotificationEvent = null;
				}
				this.TriggerDefaultEvents(flag);
				if (this.Settings.BackgroundEventUpload && !base.IsInvoking("Upload"))
				{
					base.InvokeRepeating("Upload", (float)this.Settings.BackgroundEventUploadStartDelaySeconds, (float)this.Settings.BackgroundEventUploadRepeatRateSeconds);
				}
			}
		}

		public void NewSession()
		{
			string text = this.GenerateSessionID();
			Logger.LogDebug("Starting new session " + text);
			this.SessionID = text;
			if (this.OnNewSession != null)
			{
				this.OnNewSession();
			}
		}

		public void StopSDK()
		{
			object @lock = DDNA._lock;
			lock (@lock)
			{
				if (this.started)
				{
					Logger.LogInfo("Stopping DDNA SDK");
					this.RecordEvent("gameEnded");
					base.CancelInvoke();
					this.Upload();
					this.started = false;
				}
				else
				{
					Logger.LogDebug("SDK not running");
				}
			}
		}

		public void RecordEvent<T>(T gameEvent) where T : GameEvent<T>
		{
			if (!this.started)
			{
				throw new Exception("You must first start the SDK via the StartSDK method");
			}
			gameEvent.AddParam("platform", this.Platform);
			gameEvent.AddParam("sdkVersion", Settings.SDK_VERSION);
			Dictionary<string, object> dictionary = gameEvent.AsDictionary();
			dictionary["userID"] = this.UserID;
			dictionary["sessionID"] = this.SessionID;
			dictionary["eventUUID"] = Guid.NewGuid().ToString();
			string currentTimestamp = DDNA.GetCurrentTimestamp();
			if (currentTimestamp != null)
			{
				dictionary["eventTimestamp"] = DDNA.GetCurrentTimestamp();
			}
			try
			{
				string text = Json.Serialize(dictionary);
				if (!this.eventStore.Push(text))
				{
					Logger.LogWarning("Event store full, dropping '" + gameEvent.Name + "' event.");
				}
			}
			catch (Exception ex)
			{
				Logger.LogWarning("Unable to generate JSON for '" + gameEvent.Name + "' event. " + ex.Message);
			}
		}

		public void RecordEvent(string eventName)
		{
			GameEvent gameEvent = new GameEvent(eventName);
			this.RecordEvent<GameEvent>(gameEvent);
		}

		public void RecordEvent(string eventName, Dictionary<string, object> eventParams)
		{
			GameEvent gameEvent = new GameEvent(eventName);
			foreach (string text in eventParams.Keys)
			{
				gameEvent.AddParam(text, eventParams[text]);
			}
			this.RecordEvent<GameEvent>(gameEvent);
		}

		[Obsolete("Prefer 'RequestEngagement' with an 'Engagement' instead.")]
		public void RequestEngagement(string decisionPoint, Dictionary<string, object> engageParams, Action<Dictionary<string, object>> callback)
		{
			Engagement engagement = new Engagement(decisionPoint);
			foreach (string text in engageParams.Keys)
			{
				engagement.AddParam(text, engageParams[text]);
			}
			this.RequestEngagement(engagement, callback);
		}

		public void RequestEngagement(Engagement engagement, Action<Dictionary<string, object>> callback)
		{
			if (!this.started)
			{
				throw new Exception("You must first start the SDK via the StartSDK method.");
			}
			if (string.IsNullOrEmpty(this.EngageURL))
			{
				throw new Exception("Engage URL not configured.");
			}
			try
			{
				Dictionary<string, object> dictionary = engagement.AsDictionary();
				EngageRequest engageRequest = new EngageRequest(dictionary["decisionPoint"] as string);
				engageRequest.Flavour = dictionary["flavour"] as string;
				engageRequest.Parameters = dictionary["parameters"] as Dictionary<string, object>;
				EngageResponse engageResponse = delegate(string response, int statusCode, string error)
				{
					Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
					if (response != null)
					{
						try
						{
							dictionary2 = Json.Deserialize(response) as Dictionary<string, object>;
						}
						catch (Exception ex2)
						{
							Logger.LogError("Engagement " + engagement.DecisionPoint + " responded with invalid JSON: " + ex2.Message);
						}
					}
					callback(dictionary2);
				};
				base.StartCoroutine(Engage.Request(this, engageRequest, engageResponse));
			}
			catch (Exception ex)
			{
				Logger.LogWarning("Engagement request failed: " + ex.Message);
			}
		}

		public void RequestEngagement(Engagement engagement, Action<Engagement> onCompleted, Action<Exception> onError)
		{
			if (!this.started)
			{
				throw new Exception("You must first start the SDK via the StartSDK method.");
			}
			if (string.IsNullOrEmpty(this.EngageURL))
			{
				throw new Exception("Engage URL not configured.");
			}
			try
			{
				Dictionary<string, object> dictionary = engagement.AsDictionary();
				EngageRequest engageRequest = new EngageRequest(dictionary["decisionPoint"] as string);
				engageRequest.Flavour = dictionary["flavour"] as string;
				engageRequest.Parameters = dictionary["parameters"] as Dictionary<string, object>;
				EngageResponse engageResponse = delegate(string response, int statusCode, string error)
				{
					engagement.Raw = response;
					engagement.StatusCode = statusCode;
					engagement.Error = error;
					onCompleted(engagement);
				};
				base.StartCoroutine(Engage.Request(this, engageRequest, engageResponse));
			}
			catch (Exception ex)
			{
				Logger.LogWarning("Engagement request failed: " + ex.Message);
			}
		}

		[Obsolete("Prefer 'RequestImageMessage' with an 'Engagement' instead.")]
		public void RequestImageMessage(string decisionPoint, Dictionary<string, object> engageParams, IPopup popup)
		{
			Engagement engagement = new Engagement(decisionPoint);
			foreach (string text in engageParams.Keys)
			{
				engagement.AddParam(text, engageParams[text]);
			}
			this.RequestImageMessage(engagement, popup, null);
		}

		[Obsolete("Prefer 'RequestImageMessage' with an 'Engagement' instead.")]
		public void RequestImageMessage(string decisionPoint, Dictionary<string, object> engageParams, IPopup popup, Action<Dictionary<string, object>> callback)
		{
			Engagement engagement = new Engagement(decisionPoint);
			foreach (string text in engageParams.Keys)
			{
				engagement.AddParam(text, engageParams[text]);
			}
			this.RequestImageMessage(engagement, popup, callback);
		}

		[Obsolete("Prefer 'RequestEngagement' and using an 'ImageMessage'.")]
		public void RequestImageMessage(Engagement engagement, IPopup popup)
		{
			this.RequestImageMessage(engagement, popup, null);
		}

		[Obsolete("Prefer 'RequestEngagement' and using an 'ImageMessage'.")]
		public void RequestImageMessage(Engagement engagement, IPopup popup, Action<Dictionary<string, object>> callback)
		{
			Action<Dictionary<string, object>> action = delegate(Dictionary<string, object> response)
			{
				if (response != null)
				{
					if (response.ContainsKey("image"))
					{
						Dictionary<string, object> dictionary = response["image"] as Dictionary<string, object>;
						popup.Prepare(dictionary);
					}
					if (callback != null)
					{
						callback(response);
					}
				}
			};
			this.RequestEngagement(engagement, action);
		}

		public void RecordPushNotification(Dictionary<string, object> payload)
		{
			Logger.LogDebug("Received push notification: " + payload);
			GameEvent gameEvent = new GameEvent("notificationOpened");
			try
			{
				if (payload.ContainsKey("_ddId"))
				{
					gameEvent.AddParam("notificationId", Convert.ToInt32(payload["_ddId"]));
				}
				if (payload.ContainsKey("_ddName"))
				{
					gameEvent.AddParam("notificationName", payload["_ddName"]);
				}
				if (payload.ContainsKey("_ddLaunch"))
				{
					gameEvent.AddParam("notificationLaunch", Convert.ToBoolean(payload["_ddLaunch"]));
				}
			}
			catch (Exception ex)
			{
				Logger.LogError("Error parsing push notification payload. " + ex.Message);
			}
			if (this.started)
			{
				this.RecordEvent<GameEvent>(gameEvent);
			}
			else
			{
				this.launchNotificationEvent = gameEvent;
			}
		}

		public void Upload()
		{
			if (!this.started)
			{
				Logger.LogDebug("You must first start the SDK via the StartSDK method.");
				return;
			}
			if (this.IsUploading)
			{
				Logger.LogWarning("Event upload already in progress, try again later.");
				return;
			}
			base.StartCoroutine(this.UploadCoroutine());
		}

		public void SetLoggingLevel(Logger.Level level)
		{
			Logger.SetLogLevel(level);
		}

		public Settings Settings { get; set; }

		public IosNotifications IosNotifications { get; private set; }

		public AndroidNotifications AndroidNotifications { get; private set; }

		public void ClearPersistentData()
		{
			PlayerPrefs.DeleteKey(DDNA.PF_KEY_USER_ID);
			if (this.eventStore != null)
			{
				this.eventStore.ClearAll();
			}
			Engage.ClearCache();
		}

		public void UseCollectTimestamp(bool useCollect)
		{
			if (!useCollect)
			{
				this.SetTimestampFunc(new Func<DateTime?>(DDNA.DefaultTimestampFunc));
			}
			else
			{
				this.SetTimestampFunc(() => null);
			}
		}

		public void SetTimestampFunc(Func<DateTime?> TimestampFunc)
		{
			DDNA.TimestampFunc = TimestampFunc;
		}

		public string EnvironmentKey { get; private set; }

		public string CollectURL
		{
			get
			{
				return this.collectURL;
			}
			private set
			{
				this.collectURL = DDNA.ValidateURL(value);
			}
		}

		public string EngageURL
		{
			get
			{
				return this.engageURL;
			}
			private set
			{
				this.engageURL = DDNA.ValidateURL(value);
			}
		}

		public string SessionID { get; private set; }

		public string Platform { get; private set; }

		public string UserID
		{
			get
			{
				string @string = PlayerPrefs.GetString(DDNA.PF_KEY_USER_ID, null);
				if (string.IsNullOrEmpty(@string))
				{
					return null;
				}
				return @string;
			}
			private set
			{
				if (!string.IsNullOrEmpty(value))
				{
					PlayerPrefs.SetString(DDNA.PF_KEY_USER_ID, value);
					PlayerPrefs.Save();
				}
			}
		}

		public bool HasStarted
		{
			get
			{
				return this.started;
			}
		}

		public bool IsUploading { get; private set; }

		public string HashSecret { get; set; }

		public string ClientVersion { get; set; }

		public string PushNotificationToken
		{
			get
			{
				return this.pushNotificationToken;
			}
			set
			{
				if (!string.IsNullOrEmpty(value) && value != this.pushNotificationToken)
				{
					GameEvent gameEvent = new GameEvent("notificationServices").AddParam("pushNotificationToken", value);
					if (this.started)
					{
						this.RecordEvent<GameEvent>(gameEvent);
					}
					this.pushNotificationToken = value;
				}
			}
		}

		public string AndroidRegistrationID
		{
			get
			{
				return this.androidRegistrationId;
			}
			set
			{
				if (!string.IsNullOrEmpty(value) && value != this.androidRegistrationId)
				{
					GameEvent gameEvent = new GameEvent("notificationServices").AddParam("androidRegistrationID", value);
					if (this.started)
					{
						this.RecordEvent<GameEvent>(gameEvent);
					}
					this.androidRegistrationId = value;
				}
			}
		}

		public override void OnDestroy()
		{
			if (this.eventStore != null)
			{
				this.eventStore.Dispose();
			}
			PlayerPrefs.Save();
			base.OnDestroy();
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				this.lastActive = TimeHelper.UtcTime();
			}
			else
			{
				double totalSeconds = (TimeHelper.UtcTime() - this.lastActive).TotalSeconds;
				if (totalSeconds > (double)this.Settings.SessionTimeoutSeconds)
				{
					this.lastActive = DateTime.MinValue;
					this.NewSession();
				}
			}
		}

		private string GenerateSessionID()
		{
			return Guid.NewGuid().ToString();
		}

		private string GenerateUserID()
		{
			return Guid.NewGuid().ToString();
		}

		private static DateTime? DefaultTimestampFunc()
		{
			return new DateTime?(TimeHelper.UtcTime());
		}

		private static string GetCurrentTimestamp()
		{
			DateTime? dateTime = DDNA.TimestampFunc();
			if (dateTime != null)
			{
				string text = dateTime.Value.ToString(Settings.EVENT_TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
				if (text.EndsWith(".1000"))
				{
					text = text.Replace(".1000", ".999");
				}
				return text;
			}
			return null;
		}

		private IEnumerator UploadCoroutine()
		{
			this.IsUploading = true;
			try
			{
				this.eventStore.Swap();
				List<string> events = this.eventStore.Read();
				if (events != null && events.Count > 0)
				{
					Logger.LogDebug("Starting event upload.");
					Action<bool, int> postCb = delegate(bool succeeded, int statusCode)
					{
						if (succeeded)
						{
							Logger.LogDebug("Event upload successful.");
							this.eventStore.ClearOut();
						}
						else if (statusCode == 400)
						{
							Logger.LogDebug("Collect rejected events, possible corruption.");
							this.eventStore.ClearOut();
						}
						else
						{
							Logger.LogWarning("Event upload failed - try again later.");
						}
					};
					yield return base.StartCoroutine(this.PostEvents(events.ToArray(), postCb));
				}
			}
			finally
			{
				this.IsUploading = false;
			}
			yield break;
		}

		private IEnumerator PostEvents(string[] events, Action<bool, int> resultCallback)
		{
			string bulkEvent = "{\"eventList\":[" + string.Join(",", events) + "]}";
			string url;
			if (this.HashSecret != null)
			{
				string text = DDNA.GenerateHash(bulkEvent, this.HashSecret);
				url = DDNA.FormatURI(Settings.COLLECT_HASH_URL_PATTERN, this.CollectURL, this.EnvironmentKey, text);
			}
			else
			{
				url = DDNA.FormatURI(Settings.COLLECT_URL_PATTERN, this.CollectURL, this.EnvironmentKey, null);
			}
			int attempts = 0;
			bool succeeded = false;
			int status = 0;
			Action<int, string, string> completionHandler = delegate(int statusCode, string data, string error)
			{
				if (statusCode < 400)
				{
					succeeded = true;
				}
				else
				{
					Logger.LogDebug("Error posting events: " + error + " " + data);
				}
				status = statusCode;
			};
			HttpRequest request = new HttpRequest(url);
			request.HTTPMethod = HttpRequest.HTTPMethodType.POST;
			request.HTTPBody = bulkEvent;
			request.setHeader("Content-Type", "application/json");
			do
			{
				yield return base.StartCoroutine(Network.SendRequest(request, completionHandler));
				if (succeeded || ++attempts < this.Settings.HttpRequestMaxRetries)
				{
					break;
				}
				yield return new WaitForSeconds(this.Settings.HttpRequestRetryDelaySeconds);
			}
			while (attempts < this.Settings.HttpRequestMaxRetries);
			resultCallback(succeeded, status);
			yield break;
		}

		internal string ResolveEngageURL(string httpBody)
		{
			string text2;
			if (httpBody != null && this.HashSecret != null)
			{
				string text = DDNA.GenerateHash(httpBody, this.HashSecret);
				text2 = DDNA.FormatURI(Settings.ENGAGE_HASH_URL_PATTERN, this.EngageURL, this.EnvironmentKey, text);
			}
			else
			{
				text2 = DDNA.FormatURI(Settings.ENGAGE_URL_PATTERN, this.EngageURL, this.EnvironmentKey, null);
			}
			return text2;
		}

		private static string FormatURI(string uriPattern, string apiHost, string envKey, string hash)
		{
			string text = uriPattern.Replace("{host}", apiHost);
			text = text.Replace("{env_key}", envKey);
			return text.Replace("{hash}", hash);
		}

		private static string ValidateURL(string url)
		{
			if (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://"))
			{
				url = "http://" + url;
			}
			return url;
		}

		private static string GenerateHash(string data, string secret)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data + secret);
			byte[] array = Utils.ComputeMD5Hash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("X2"));
			}
			return stringBuilder.ToString();
		}

		private void TriggerDefaultEvents(bool newPlayer)
		{
			if (this.Settings.OnFirstRunSendNewPlayerEvent && newPlayer)
			{
				Logger.LogDebug("Sending 'newPlayer' event");
				GameEvent gameEvent = new GameEvent("newPlayer");
				if (ClientInfo.CountryCode != null)
				{
					gameEvent.AddParam("userCountry", ClientInfo.CountryCode);
				}
				this.RecordEvent<GameEvent>(gameEvent);
			}
			if (this.Settings.OnInitSendGameStartedEvent)
			{
				Logger.LogDebug("Sending 'gameStarted' event");
				GameEvent gameEvent2 = new GameEvent("gameStarted").AddParam("clientVersion", this.ClientVersion).AddParam("userLocale", ClientInfo.Locale).AddParam("userXP", PhotonConnectionFactory.Instance.Profile.Experience)
					.AddParam("userLevel", PhotonConnectionFactory.Instance.Profile.Level)
					.AddParam("localization", ChangeLanguage.GetCurrentLanguage.InLocalizeName)
					.AddParam("userName", PhotonConnectionFactory.Instance.Profile.Name);
				if (!string.IsNullOrEmpty(this.PushNotificationToken))
				{
					gameEvent2.AddParam("pushNotificationToken", this.PushNotificationToken);
				}
				if (!string.IsNullOrEmpty(this.AndroidRegistrationID))
				{
					gameEvent2.AddParam("androidRegistrationID", this.AndroidRegistrationID);
				}
				this.RecordEvent<GameEvent>(gameEvent2);
			}
			if (this.Settings.OnInitSendClientDeviceEvent)
			{
				Logger.LogDebug("Sending 'clientDevice' event");
				GameEvent gameEvent3 = new GameEvent("clientDevice").AddParam("deviceName", ClientInfo.DeviceName).AddParam("deviceType", ClientInfo.DeviceType).AddParam("hardwareVersion", ClientInfo.DeviceModel)
					.AddParam("operatingSystem", ClientInfo.OperatingSystem)
					.AddParam("operatingSystemVersion", ClientInfo.OperatingSystemVersion)
					.AddParam("timezoneOffset", ClientInfo.TimezoneOffset)
					.AddParam("userLanguage", ClientInfo.LanguageCode);
				if (ClientInfo.Manufacturer != null)
				{
					gameEvent3.AddParam("manufacturer", ClientInfo.Manufacturer);
				}
				this.RecordEvent<GameEvent>(gameEvent3);
			}
		}

		private static readonly string PF_KEY_USER_ID = "DDSDK_USER_ID";

		private bool started;

		private string collectURL;

		private string engageURL;

		private EventStore eventStore;

		private GameEvent launchNotificationEvent;

		private string pushNotificationToken;

		private string androidRegistrationId;

		private DateTime lastActive = DateTime.MinValue;

		private static Func<DateTime?> TimestampFunc = new Func<DateTime?>(DDNA.DefaultTimestampFunc);

		private static object _lock = new object();
	}
}
