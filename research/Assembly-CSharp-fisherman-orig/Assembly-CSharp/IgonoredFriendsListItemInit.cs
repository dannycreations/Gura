using System;
using UnityEngine;

public class IgonoredFriendsListItemInit : FriendListItemBase
{
	protected override void Awake()
	{
		base.Awake();
	}

	public void Unignore()
	{
		PhotonConnectionFactory.Instance.UnIgnorePlayer(this._player);
	}

	public GameObject UnignoreButton;
}
