using System;
using System.Collections.Generic;
using System.Diagnostics;
using DeltaDNA.MiniJSON;
using UnityEngine;

namespace DeltaDNA
{
	public class AndroidNotifications : MonoBehaviour
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
			if (Application.platform == 11)
			{
			}
		}

		public void UnregisterForPushNotifications()
		{
			if (Application.platform == 11)
			{
			}
		}

		public void DidLaunchWithPushNotification(string notification)
		{
			Logger.LogDebug("Did launch with Android push notification");
			Dictionary<string, object> dictionary = Json.Deserialize(notification) as Dictionary<string, object>;
			Singleton<DDNA>.Instance.RecordPushNotification(dictionary);
			if (this.OnDidLaunchWithPushNotification != null)
			{
				this.OnDidLaunchWithPushNotification(notification);
			}
		}

		public void DidReceivePushNotification(string notification)
		{
			Logger.LogDebug("Did receive Android push notification");
			Dictionary<string, object> dictionary = Json.Deserialize(notification) as Dictionary<string, object>;
			Singleton<DDNA>.Instance.RecordPushNotification(dictionary);
			if (this.OnDidReceivePushNotification != null)
			{
				this.OnDidReceivePushNotification(notification);
			}
		}

		public void DidRegisterForPushNotifications(string registrationId)
		{
			Logger.LogDebug("Did register for Android push notifications: " + registrationId);
			Singleton<DDNA>.Instance.AndroidRegistrationID = registrationId;
			if (this.OnDidRegisterForPushNotifications != null)
			{
				this.OnDidRegisterForPushNotifications(registrationId);
			}
		}

		public void DidFailToRegisterForPushNotifications(string error)
		{
			Logger.LogDebug("Did fail to register for Android push notifications: " + error);
			if (this.OnDidFailToRegisterForPushNotifications != null)
			{
				this.OnDidFailToRegisterForPushNotifications(error);
			}
		}
	}
}
