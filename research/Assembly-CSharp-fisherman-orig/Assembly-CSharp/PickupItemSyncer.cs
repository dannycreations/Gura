﻿using System;
using System.Collections.Generic;
using Photon;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PickupItemSyncer : MonoBehaviour
{
	public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if (PhotonNetwork.isMasterClient)
		{
			this.SendPickedUpItems(newPlayer);
		}
	}

	public void OnJoinedRoom()
	{
		Debug.Log(string.Concat(new object[]
		{
			"Joined Room. isMasterClient: ",
			PhotonNetwork.isMasterClient,
			" id: ",
			PhotonNetwork.player.ID
		}));
		this.IsWaitingForPickupInit = !PhotonNetwork.isMasterClient;
		if (PhotonNetwork.playerList.Length >= 2)
		{
			base.Invoke("AskForPickupItemSpawnTimes", 2f);
		}
	}

	public void AskForPickupItemSpawnTimes()
	{
		if (this.IsWaitingForPickupInit)
		{
			if (PhotonNetwork.playerList.Length < 2)
			{
				Debug.Log("Cant ask anyone else for PickupItem spawn times.");
				this.IsWaitingForPickupInit = false;
				return;
			}
			PhotonPlayer photonPlayer = PhotonNetwork.masterClient.GetNext();
			if (photonPlayer == null || photonPlayer.Equals(PhotonNetwork.player))
			{
				photonPlayer = PhotonNetwork.player.GetNext();
			}
			if (photonPlayer == null || photonPlayer.Equals(PhotonNetwork.player))
			{
				Debug.Log("No player left to ask");
				this.IsWaitingForPickupInit = false;
			}
		}
	}

	public void RequestForPickupTimes(PhotonMessageInfo msgInfo)
	{
		if (msgInfo.sender == null)
		{
			Debug.LogError("Unknown player asked for PickupItems");
			return;
		}
		this.SendPickedUpItems(msgInfo.sender);
	}

	private void SendPickedUpItems(PhotonPlayer targtePlayer)
	{
		if (targtePlayer == null)
		{
			Debug.LogWarning("Cant send PickupItem spawn times to unknown targetPlayer.");
			return;
		}
		double time = PhotonNetwork.time;
		double num = time + 0.20000000298023224;
		PickupItem[] array = new PickupItem[PickupItem.DisabledPickupItems.Count];
		PickupItem.DisabledPickupItems.CopyTo(array);
		List<float> list = new List<float>(array.Length * 2);
		foreach (PickupItem pickupItem in array)
		{
			if (pickupItem.SecondsBeforeRespawn <= 0f)
			{
				list.Add((float)pickupItem.ViewID);
				list.Add(0f);
			}
			else
			{
				double num2 = pickupItem.TimeOfRespawn - PhotonNetwork.time;
				if (pickupItem.TimeOfRespawn > num)
				{
					Debug.Log(string.Concat(new object[]
					{
						pickupItem.ViewID,
						" respawn: ",
						pickupItem.TimeOfRespawn,
						" timeUntilRespawn: ",
						num2,
						" (now: ",
						PhotonNetwork.time,
						")"
					}));
					list.Add((float)pickupItem.ViewID);
					list.Add((float)num2);
				}
			}
		}
		Debug.Log(string.Concat(new object[] { "Sent count: ", list.Count, " now: ", time }));
	}

	public void PickupItemInit(double timeBase, float[] inactivePickupsAndTimes)
	{
		this.IsWaitingForPickupInit = false;
		for (int i = 0; i < inactivePickupsAndTimes.Length / 2; i++)
		{
			int num = i * 2;
			int num2 = (int)inactivePickupsAndTimes[num];
			float num3 = inactivePickupsAndTimes[num + 1];
			PhotonView photonView = PhotonView.Find(num2);
			PickupItem component = photonView.GetComponent<PickupItem>();
			if (num3 <= 0f)
			{
				component.PickedUp(0f);
			}
			else
			{
				double num4 = (double)num3 + timeBase;
				Debug.Log(string.Concat(new object[] { photonView.viewID, " respawn: ", num4, " timeUntilRespawnBasedOnTimeBase:", num3, " SecondsBeforeRespawn: ", component.SecondsBeforeRespawn }));
				double num5 = num4 - PhotonNetwork.time;
				if (num3 <= 0f)
				{
					num5 = 0.0;
				}
				component.PickedUp((float)num5);
			}
		}
	}

	public bool IsWaitingForPickupInit;

	private const float TimeDeltaToIgnore = 0.2f;
}
