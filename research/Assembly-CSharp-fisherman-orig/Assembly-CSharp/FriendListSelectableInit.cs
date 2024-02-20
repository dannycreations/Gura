using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FriendListSelectableInit : MessageBoxBase
{
	protected override void Awake()
	{
		base.Awake();
		if (this.caption != null)
		{
			this.caption.text = string.Empty;
		}
		if (this.cancelButtonText != null)
		{
			this.cancelButtonText.text = ScriptLocalization.Get("CancelButton");
		}
		List<Player> friends = PhotonConnectionFactory.Instance.Profile.Friends;
		LogHelper.Log(string.Format("FriendListSelectableInit:Awake friends is null:{0}", friends == null));
		this.NoFriendsText.gameObject.SetActive(friends == null || friends.Count == 0);
		if (friends != null)
		{
			friends.Sort((Player a, Player b) => b.IsOnline.CompareTo(a.IsOnline));
			foreach (Player player in friends)
			{
				FriendListItemHandler component = Object.Instantiate<GameObject>(this.FriendListItemPrefab, this.ContentPanel.transform).GetComponent<FriendListItemHandler>();
				component.Init(player);
				component.IsSelectedItem += this.FriendListInit_IsSelectedItem;
				Toggle component2 = component.GetComponent<Toggle>();
				component2.group = this.ListToggleGroup;
			}
		}
		this._alphaFade.FastHidePanel();
		base.gameObject.SetActive(false);
		MessageFactory.MessageBoxQueue.Enqueue(this);
	}

	public new void Open()
	{
		this._alphaFade.ShowPanel();
	}

	public new void Close()
	{
		this._alphaFade.HidePanel();
	}

	public string Caption
	{
		set
		{
			this.caption.text = value;
		}
	}

	public string CancelButtonText
	{
		set
		{
			this.cancelButtonText.text = value;
		}
	}

	private void FriendListInit_IsSelectedItem(object sender, FriendEventArgs e)
	{
		MonoBehaviour.print("Friend list item selected: " + e.Name);
		this.ConfirmAction(e.Name);
		this.Close();
	}

	public Text caption;

	public Text cancelButtonText;

	public GameObject declineButton;

	public GameObject FriendListItemPrefab;

	public GameObject ContentPanel;

	public ToggleGroup ListToggleGroup;

	public Action<string> ConfirmAction;

	public Text NoFriendsText;
}
