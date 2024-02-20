using System;
using System.Linq;
using Assets.Scripts.UI._2D.PlayerProfile;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class RoomFriendsPopupInit : MonoBehaviour
{
	public void Init(string[] playersIds)
	{
		RoomFriendsPopupInit.<Init>c__AnonStorey0 <Init>c__AnonStorey = new RoomFriendsPopupInit.<Init>c__AnonStorey0();
		<Init>c__AnonStorey.playersIds = playersIds;
		this.SetPopupHeight(<Init>c__AnonStorey.playersIds.Length);
		this.usersCount = <Init>c__AnonStorey.playersIds.Length;
		int i;
		for (i = 0; i < <Init>c__AnonStorey.playersIds.Length; i++)
		{
			Player player = PhotonConnectionFactory.Instance.Profile.Friends.FirstOrDefault((Player x) => x.UserId == <Init>c__AnonStorey.playersIds[i]);
			this.AddFriendToList(player);
		}
	}

	private void AddFriendToList(Player player)
	{
		GameObject gameObject = GUITools.AddChild(this.RoomFriendListContent, this.RoomFriendListItemPrefab);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		gameObject.GetComponent<Text>().text = PlayerProfileHelper.GetPlayerNameLevelRank(player);
	}

	private void SetPopupHeight(int friendsCount)
	{
		RectTransform component = this.RoomFriendListContent.GetComponent<RectTransform>();
		RectTransform component2 = base.gameObject.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.rect.width, (float)friendsCount * this.RoomFriendListItemPrefab.GetComponent<RectTransform>().rect.height);
		component2.sizeDelta = new Vector2(component2.rect.width, component.rect.height + this.ExtensionHeight);
	}

	public GameObject RoomFriendListContent;

	public GameObject RoomFriendListItemPrefab;

	public float ExtensionHeight = 20f;

	private int usersCount;
}
