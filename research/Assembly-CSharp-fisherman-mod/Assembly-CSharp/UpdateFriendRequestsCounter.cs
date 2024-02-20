using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class UpdateFriendRequestsCounter : MonoBehaviour
{
	private void Awake()
	{
		PhotonConnectionFactory.Instance.OnFriendshipRequest += this.OnFriendshipRequest;
		PhotonConnectionFactory.Instance.OnFriendshipAdded += this.OnFriendshipAdded;
		PhotonConnectionFactory.Instance.OnFriendshipRemoved += this.OnFriendshipRemoved;
		PhotonConnectionFactory.Instance.OnReceiveBuoyShareRequest += this.OnBuoyReceived;
		PhotonConnectionFactory.Instance.OnBuoyShareAccepted += this.UpdateCounter;
		PhotonConnectionFactory.Instance.OnBuoyShareDeclined += this.UpdateCounter;
		PhotonConnectionFactory.Instance.OnFriendsDetailsRefreshed += this.UpdateCounter;
		base.InvokeRepeating("UpdateCounter", 0f, 60f);
	}

	private void OnFriendshipRequest(Player player)
	{
		this.UpdateCounter();
	}

	private void OnFriendshipAdded(Player player)
	{
		this.UpdateCounter();
	}

	private void OnFriendshipRemoved(Player player)
	{
		this.UpdateCounter();
	}

	private void OnBuoyReceived(BuoySetting buoy)
	{
		this.UpdateCounter();
	}

	public void UpdateCounter()
	{
		List<Player> friends = PhotonConnectionFactory.Instance.Profile.Friends;
		if (friends != null)
		{
			int num = PhotonConnectionFactory.Instance.Profile.Friends.Where((Player x) => x.Status == FriendStatus.FriendshipRequest).ToList<Player>().Count;
			if (PhotonConnectionFactory.Instance.Profile.BuoyShareRequests != null)
			{
				num += PhotonConnectionFactory.Instance.Profile.BuoyShareRequests.Count;
			}
			if (num > 0)
			{
				this.CounterText.text = num.ToString();
				this.Counter.SetActive(true);
			}
			else
			{
				this.Counter.SetActive(false);
			}
		}
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnFriendshipRequest -= this.OnFriendshipRequest;
		PhotonConnectionFactory.Instance.OnFriendshipAdded -= this.OnFriendshipAdded;
		PhotonConnectionFactory.Instance.OnFriendshipRemoved -= this.OnFriendshipRemoved;
		PhotonConnectionFactory.Instance.OnReceiveBuoyShareRequest -= this.OnBuoyReceived;
		PhotonConnectionFactory.Instance.OnBuoyShareAccepted -= this.UpdateCounter;
		PhotonConnectionFactory.Instance.OnBuoyShareDeclined -= this.UpdateCounter;
		PhotonConnectionFactory.Instance.OnFriendsDetailsRefreshed -= this.UpdateCounter;
	}

	public GameObject Counter;

	public Text CounterText;
}
