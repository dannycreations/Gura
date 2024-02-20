using System;
using System.Collections.Generic;
using System.Diagnostics;
using DeltaDNA.MiniJSON;
using UnityEngine;

namespace DeltaDNA
{
	public class IosNotifications : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string> OnDidLaunchWithPushNotification;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string> OnDidReceivePushNotification;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string> OnDidRegisterForPushNotifications;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string> OnDidFailToRegisterForPushNotifications;

		private void Awake()
		{
			base.gameObject.name = base.GetType().ToString();
			Object.DontDestroyOnLoad(this);
		}

		public void RegisterForPushNotifications()
		{
			if (Application.platform == 8)
			{
			}
		}

		public void UnregisterForPushNotifications()
		{
			if (Application.platform == 8)
			{
			}
		}

		public void DidLaunchWithPushNotification(string notification)
		{
			Logger.LogDebug("Did launch with iOS push notification");
			Dictionary<string, object> dictionary = Json.Deserialize(notification) as Dictionary<string, object>;
			Singleton<DDNA>.Instance.RecordPushNotification(dictionary);
			if (this.OnDidLaunchWithPushNotification != null)
			{
				this.OnDidLaunchWithPushNotification(notification);
			}
		}

		public void DidReceivePushNotification(string notification)
		{
			Logger.LogDebug("Did receive iOS push notification");
			Dictionary<string, object> dictionary = Json.Deserialize(notification) as Dictionary<string, object>;
			Singleton<DDNA>.Instance.RecordPushNotification(dictionary);
			if (this.OnDidReceivePushNotification != null)
			{
				this.OnDidReceivePushNotification(notification);
			}
		}

		public void DidRegisterForPushNotifications(string deviceToken)
		{
			Logger.LogDebug("Did register for iOS push notifications: " + deviceToken);
			Singleton<DDNA>.Instance.PushNotificationToken = deviceToken;
			if (this.OnDidRegisterForPushNotifications != null)
			{
				this.OnDidRegisterForPushNotifications(deviceToken);
			}
		}

		public void DidFailToRegisterForPushNotifications(string error)
		{
			Logger.LogDebug("Did fail to register for iOS push notifications: " + error);
			if (this.OnDidFailToRegisterForPushNotifications != null)
			{
				this.OnDidFailToRegisterForPushNotifications(error);
			}
		}
	}
}
