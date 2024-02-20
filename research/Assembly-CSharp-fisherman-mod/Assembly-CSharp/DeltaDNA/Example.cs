using System;
using System.Collections.Generic;
using DeltaDNA.MiniJSON;
using UnityEngine;

namespace DeltaDNA
{
	public class Example : MonoBehaviour
	{
		private void Start()
		{
			Singleton<DDNA>.Instance.SetLoggingLevel(Logger.Level.DEBUG);
			Singleton<DDNA>.Instance.HashSecret = "1VLjWqChV2YC1sJ4EPKGzSF3TbhS26hq";
			Singleton<DDNA>.Instance.ClientVersion = "1.0.0";
			Singleton<DDNA>.Instance.IosNotifications.OnDidRegisterForPushNotifications += delegate(string n)
			{
				Debug.Log("Got an iOS push token: " + n);
			};
			Singleton<DDNA>.Instance.IosNotifications.OnDidReceivePushNotification += delegate(string n)
			{
				Debug.Log("Got an iOS push notification! " + n);
			};
			Singleton<DDNA>.Instance.IosNotifications.RegisterForPushNotifications();
			Singleton<DDNA>.Instance.AndroidNotifications.OnDidRegisterForPushNotifications += delegate(string n)
			{
				Debug.Log("Got an Android registration token: " + n);
			};
			Singleton<DDNA>.Instance.AndroidNotifications.OnDidFailToRegisterForPushNotifications += delegate(string n)
			{
				Debug.Log("Failed getting an Android registration token: " + n);
			};
			Singleton<DDNA>.Instance.AndroidNotifications.RegisterForPushNotifications();
			Singleton<DDNA>.Instance.StartSDK("76410301326725846610230818914037", "http://collect2470ntysd.deltadna.net/collect/api", "http://engage2470ntysd.deltadna.net");
		}

		private void Update()
		{
		}

		private void FixedUpdate()
		{
			base.transform.Rotate(new Vector3(15f, 30f, 45f) * Time.deltaTime);
		}

		private void OnGUI()
		{
			int num = 10;
			int num2 = 10;
			int num3 = 180;
			int num4 = 70;
			int num5 = num4 + 5;
			GUI.skin.textField.wordWrap = true;
			GUI.skin.button.fontSize = 18;
			if (GUI.Button(new Rect((float)num, (float)num2, (float)num3, (float)num4), "Simple Event"))
			{
				GameEvent gameEvent = new GameEvent("options").AddParam("option", "sword").AddParam("action", "sell");
				Singleton<DDNA>.Instance.RecordEvent<GameEvent>(gameEvent);
			}
			if (GUI.Button(new Rect((float)num, (float)(num2 += num5), (float)num3, (float)num4), "Achievement Event"))
			{
				GameEvent gameEvent2 = new GameEvent("achievement").AddParam("achievementName", "Sunday Showdown Tournament Win").AddParam("achievementID", "SS-2014-03-02-01").AddParam("reward", new Params().AddParam("rewardName", "Medal").AddParam("rewardProducts", new Product().SetRealCurrency("USD", 5000).AddVirtualCurrency("VIP Points", "GRIND", 20).AddItem("Sunday Showdown Medal", "Victory Badge", 1)));
				Singleton<DDNA>.Instance.RecordEvent<GameEvent>(gameEvent2);
			}
			if (GUI.Button(new Rect((float)num, (float)(num2 += num5), (float)num3, (float)num4), "Transaction Event"))
			{
				Transaction transaction = new Transaction("Weapon type 11 manual repair", "PURCHASE", new Product().AddItem("WeaponsMaxConditionRepair:11", "WeaponMaxConditionRepair", 5), new Product().AddVirtualCurrency("Credit", "GRIND", 710)).SetTransactorId("2.212.91.84:15116").SetProductId("4019").AddParam("paymentCountry", "GB");
				Singleton<DDNA>.Instance.RecordEvent<Transaction>(transaction);
			}
			if (GUI.Button(new Rect((float)num, (float)(num2 += num5), (float)num3, (float)num4), "Engagement"))
			{
				Engagement engagement = new Engagement("gameLoaded").AddParam("userLevel", 4).AddParam("experience", 1000).AddParam("missionName", "Disco Volante");
				Singleton<DDNA>.Instance.RequestEngagement(engagement, delegate(Dictionary<string, object> response)
				{
					this.popupContent = Json.Serialize(response);
				});
				this.popupTitle = "Engage returned";
			}
			if (GUI.Button(new Rect((float)num, (float)(num2 += num5), (float)num3, (float)num4), "Image Message"))
			{
				Engagement engagement2 = new Engagement("imageMessage").AddParam("userLevel", 4).AddParam("experience", 1000).AddParam("missionName", "Disco Volante");
				Singleton<DDNA>.Instance.RequestEngagement(engagement2, delegate(Engagement response)
				{
					ImageMessage imageMessage = ImageMessage.Create(response);
					if (imageMessage != null)
					{
						Debug.Log("Engage returned a valid image message.");
						imageMessage.OnDidReceiveResources += delegate
						{
							Debug.Log("Image Message loaded resources.");
							imageMessage.Show();
						};
						imageMessage.OnDismiss += delegate(ImageMessage.EventArgs obj)
						{
							Debug.Log("Image Message dismissed by " + obj.ID);
						};
						imageMessage.OnAction += delegate(ImageMessage.EventArgs obj)
						{
							Debug.Log("Image Message actioned by " + obj.ID + " with command " + obj.ActionValue);
						};
						imageMessage.FetchResources();
					}
					else
					{
						Debug.Log("Engage didn't return an image message.");
					}
				}, delegate(Exception exception)
				{
					Debug.Log("Engage reported an error: " + exception.Message);
				});
			}
			if (GUI.Button(new Rect((float)num, (float)(num2 += num5), (float)num3, (float)num4), "Notification Opened"))
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("_ddId", 1);
				dictionary.Add("_ddName", "Example Notification");
				dictionary.Add("_ddLaunch", true);
				Singleton<DDNA>.Instance.RecordPushNotification(dictionary);
			}
			if (GUI.Button(new Rect((float)num, (float)(num2 += num5), (float)num3, (float)num4), "Upload Events"))
			{
				Singleton<DDNA>.Instance.Upload();
			}
			if (GUI.Button(new Rect((float)num, (float)(num2 += num5), (float)num3, (float)num4), "Start SDK"))
			{
				Singleton<DDNA>.Instance.StartSDK("76410301326725846610230818914037", "http://collect2470ntysd.deltadna.net/collect/api", "http://engage2470ntysd.deltadna.net");
			}
			if (GUI.Button(new Rect((float)num, (float)(num2 += num5), (float)num3, (float)num4), "Stop SDK"))
			{
				Singleton<DDNA>.Instance.StopSDK();
			}
			if (GUI.Button(new Rect((float)num, (float)(num2 + num5), (float)num3, (float)num4), "New Session"))
			{
				Singleton<DDNA>.Instance.NewSession();
			}
			if (this.popupContent != string.Empty)
			{
				GUI.ModalWindow(0, new Rect((float)(Screen.width / 2 - 150), (float)(Screen.height / 2 - 100), 300f, 200f), new GUI.WindowFunction(this.RenderPopupContent), this.popupTitle);
			}
		}

		private void RenderPopupContent(int windowID)
		{
			if (GUI.Button(new Rect(248f, 3f, 50f, 20f), "Close"))
			{
				this.popupContent = string.Empty;
			}
			GUI.TextField(new Rect(0f, 25f, 300f, 175f), this.popupContent);
		}

		public const string ENVIRONMENT_KEY = "76410301326725846610230818914037";

		public const string COLLECT_URL = "http://collect2470ntysd.deltadna.net/collect/api";

		public const string ENGAGE_URL = "http://engage2470ntysd.deltadna.net";

		public const string ENGAGE_TEST_URL = "http://www.deltadna.net/qa/engage";

		private string popupContent = string.Empty;

		private string popupTitle = "DeltaDNA Example";
	}
}
