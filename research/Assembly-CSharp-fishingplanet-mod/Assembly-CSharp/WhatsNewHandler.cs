using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using ObjectModel;
using UnityEngine;

public class WhatsNewHandler : MonoBehaviour
{
	internal void Update()
	{
		if (!this._isSubscribed && PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.IsAuthenticated && PhotonConnectionFactory.Instance.Profile != null && CacheLibrary.AllChachesInited)
		{
			if (!CacheLibrary.ProductCache.AreProductsAvailable)
			{
				return;
			}
			this._isSubscribed = true;
			if (PhotonConnectionFactory.Instance.Profile.Level > 1)
			{
				PhotonConnectionFactory.Instance.OnGotWhatsNewList += this.PhotonServerOnGotWhatsNewList;
				PhotonConnectionFactory.Instance.OnGettingWhatsNewListFailed += this.PhotonServerOnGettingWhatsNewListFailed;
				PhotonConnectionFactory.Instance.GetWhatsNewList();
			}
			else
			{
				base.GetComponent<InfoMessageController>().CanShow = true;
			}
		}
	}

	private void PhotonServerOnGotWhatsNewList(List<WhatsNewItem> items)
	{
		Debug.Log("Got Whats New list of " + items.Count + " items");
		CacheLibrary.ProductCache.SetWhatsNewItem(items);
		PhotonConnectionFactory.Instance.OnGotWhatsNewList -= this.PhotonServerOnGotWhatsNewList;
		PhotonConnectionFactory.Instance.OnGettingWhatsNewListFailed -= this.PhotonServerOnGettingWhatsNewListFailed;
		this.OnWhatsNewShowed(items);
	}

	private void PhotonServerOnGettingWhatsNewListFailed(Failure failure)
	{
		Debug.Log("Getting Whats New list failed");
		PhotonConnectionFactory.Instance.OnGotWhatsNewList -= this.PhotonServerOnGotWhatsNewList;
		PhotonConnectionFactory.Instance.OnGettingWhatsNewListFailed -= this.PhotonServerOnGettingWhatsNewListFailed;
		base.GetComponent<InfoMessageController>().CanShow = true;
	}

	internal void OnWhatsNewShowed(List<WhatsNewItem> items)
	{
		if (this.CanShow())
		{
			if (items != null && items.Count > 0)
			{
				InfoMessage[] array = MessageFactory.InfoMessagesQueue.ToArray();
				MessageFactory.InfoMessagesQueue.Clear();
				MessageFactory.InfoMessagesQueue.Enqueue(this.GetWhatsNew(items));
				foreach (InfoMessage infoMessage in array)
				{
					MessageFactory.InfoMessagesQueue.Enqueue(infoMessage);
				}
				PhotonConnectionFactory.Instance.WhatsNewShown(items);
				ObscuredPrefs.SetLong("whatsNew", WhatsNewHandler.GetIntFromDateTime(DateTime.Now));
				Debug.Log("Whats New added to showing queue");
			}
			else
			{
				Debug.Log("Can't show Whats New - nothing to show");
			}
		}
		else
		{
			Debug.Log("Can't show Whats New - CanShow - false");
		}
		base.GetComponent<InfoMessageController>().CanShow = true;
	}

	private InfoMessage GetWhatsNew(List<WhatsNewItem> items)
	{
		GameObject gameObject = GUITools.AddChild(base.gameObject, this.WhatsNewMessagePrefab.gameObject);
		gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
		gameObject.SetActive(false);
		gameObject.GetComponent<AlphaFade>().FastHidePanel();
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		gameObject.GetComponent<WhatsNewController>().Init(items);
		return gameObject.GetComponent<InfoMessage>();
	}

	private bool CanShow()
	{
		return ConfigUtil.IsConsole || (!(StaticUserData.ServerConnectionString == "192.40.222.30:4530") && !(StaticUserData.ServerConnectionString == "192.40.222.29:4530") && !(StaticUserData.ServerConnectionString == "192.40.222.74:4530") && !(StaticUserData.ServerConnectionString == "wss://xbtest.fishingplanet.com:9090")) || true;
	}

	private static long GetIntFromDateTime(DateTime dateTime)
	{
		DateTime dateTime2 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		int num = Convert.ToInt32(dateTime.Subtract(dateTime2).TotalSeconds);
		return (long)num;
	}

	private static DateTime GetDateTimeFromInt(long value)
	{
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		return dateTime.AddSeconds((double)value);
	}

	public GameObject WhatsNewMessagePrefab;

	private bool _isSubscribed;
}
