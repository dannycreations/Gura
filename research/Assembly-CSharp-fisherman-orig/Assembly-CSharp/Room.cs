using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class Room : RoomInfo
{
	internal Room(string roomName, RoomOptions options)
		: base(roomName, null)
	{
		if (options == null)
		{
			options = new RoomOptions();
		}
		this.visibleField = options.isVisible;
		this.openField = options.isOpen;
		this.maxPlayersField = (byte)options.maxPlayers;
		this.autoCleanUpField = false;
		base.CacheProperties(options.customRoomProperties);
		this.propertiesListedInLobby = options.customRoomPropertiesForLobby;
	}

	public new int playerCount
	{
		get
		{
			if (PhotonNetwork.playerList != null)
			{
				return PhotonNetwork.playerList.Length;
			}
			return 0;
		}
	}

	public new string name
	{
		get
		{
			return this.nameField;
		}
		internal set
		{
			this.nameField = value;
		}
	}

	public new int maxPlayers
	{
		get
		{
			return (int)this.maxPlayersField;
		}
		set
		{
			if (!this.Equals(PhotonNetwork.room))
			{
				Debug.LogWarning("Can't set maxPlayers when not in that room.");
			}
			if (value > 255)
			{
				Debug.LogWarning("Can't set Room.maxPlayers to: " + value + ". Using max value: 255.");
				value = 255;
			}
			if (value != (int)this.maxPlayersField && !PhotonNetwork.offlineMode)
			{
				LoadbalancingPeer networkingPeer = PhotonNetwork.networkingPeer;
				Hashtable hashtable = new Hashtable();
				hashtable.Add(byte.MaxValue, (byte)value);
				networkingPeer.OpSetPropertiesOfRoom(hashtable, true, 0, null);
			}
			this.maxPlayersField = (byte)value;
		}
	}

	public new bool open
	{
		get
		{
			return this.openField;
		}
		set
		{
			if (!this.Equals(PhotonNetwork.room))
			{
				Debug.LogWarning("Can't set open when not in that room.");
			}
			if (value != this.openField && !PhotonNetwork.offlineMode)
			{
				LoadbalancingPeer networkingPeer = PhotonNetwork.networkingPeer;
				Hashtable hashtable = new Hashtable();
				hashtable.Add(253, value);
				networkingPeer.OpSetPropertiesOfRoom(hashtable, true, 0, null);
			}
			this.openField = value;
		}
	}

	public new bool visible
	{
		get
		{
			return this.visibleField;
		}
		set
		{
			if (!this.Equals(PhotonNetwork.room))
			{
				Debug.LogWarning("Can't set visible when not in that room.");
			}
			if (value != this.visibleField && !PhotonNetwork.offlineMode)
			{
				LoadbalancingPeer networkingPeer = PhotonNetwork.networkingPeer;
				Hashtable hashtable = new Hashtable();
				hashtable.Add(254, value);
				networkingPeer.OpSetPropertiesOfRoom(hashtable, true, 0, null);
			}
			this.visibleField = value;
		}
	}

	public string[] propertiesListedInLobby { get; private set; }

	public bool autoCleanUp
	{
		get
		{
			return this.autoCleanUpField;
		}
	}

	public void SetCustomProperties(Hashtable propertiesToSet)
	{
		if (propertiesToSet == null)
		{
			return;
		}
		base.customProperties.MergeStringKeys(propertiesToSet);
		base.customProperties.StripKeysWithNullValues();
		Hashtable hashtable = propertiesToSet.StripToStringKeys();
		if (!PhotonNetwork.offlineMode)
		{
			PhotonNetwork.networkingPeer.OpSetCustomPropertiesOfRoom(hashtable, true, 0);
		}
		NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonCustomRoomPropertiesChanged, new object[] { propertiesToSet });
	}

	public void SetCustomProperties(Hashtable propertiesToSet, Hashtable expectedValues)
	{
		if (propertiesToSet == null)
		{
			return;
		}
		if (!PhotonNetwork.offlineMode)
		{
			Hashtable hashtable = propertiesToSet.StripToStringKeys();
			Hashtable hashtable2 = expectedValues.StripToStringKeys();
			PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(hashtable, false, 0, hashtable2);
		}
		NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonCustomRoomPropertiesChanged, new object[] { propertiesToSet });
	}

	public void SetPropertiesListedInLobby(string[] propsListedInLobby)
	{
		Hashtable hashtable = new Hashtable();
		hashtable[250] = propsListedInLobby;
		PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(hashtable, false, 0, null);
		this.propertiesListedInLobby = propsListedInLobby;
	}

	public override string ToString()
	{
		return string.Format("Room: '{0}' {1},{2} {4}/{3} players.", new object[]
		{
			this.nameField,
			(!this.visibleField) ? "hidden" : "visible",
			(!this.openField) ? "closed" : "open",
			this.maxPlayersField,
			this.playerCount
		});
	}

	public new string ToStringFull()
	{
		return string.Format("Room: '{0}' {1},{2} {4}/{3} players.\ncustomProps: {5}", new object[]
		{
			this.nameField,
			(!this.visibleField) ? "hidden" : "visible",
			(!this.openField) ? "closed" : "open",
			this.maxPlayersField,
			this.playerCount,
			base.customProperties.ToStringFull()
		});
	}
}
