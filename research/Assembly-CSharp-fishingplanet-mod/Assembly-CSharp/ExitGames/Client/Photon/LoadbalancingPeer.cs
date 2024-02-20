using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace ExitGames.Client.Photon
{
	public class LoadbalancingPeer : PhotonPeer
	{
		public LoadbalancingPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
			: base(listener, protocolType)
		{
		}

		public override bool Connect(string serverAddress, string applicationName)
		{
			this.SetupProtocol();
			return base.Connect(serverAddress, applicationName);
		}

		private void SetupProtocol()
		{
			ConnectionProtocol transportProtocol = base.TransportProtocol;
			Type type = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp", false);
			if (type == null)
			{
				type = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp-firstpass", false);
			}
			if (type != null)
			{
				this.SocketImplementationConfig[4] = type;
				this.SocketImplementationConfig[5] = type;
			}
			if (PhotonHandler.PingImplementation == null)
			{
				PhotonHandler.PingImplementation = typeof(PingMono);
			}
			if (base.TransportProtocol == transportProtocol)
			{
				return;
			}
			if (PhotonNetwork.logLevel >= PhotonLogLevel.ErrorsOnly)
			{
				Debug.LogWarning(string.Concat(new object[] { "Protocol switch from: ", base.TransportProtocol, " to: ", transportProtocol, "." }));
			}
			base.TransportProtocol = transportProtocol;
		}

		public virtual bool OpGetRegions(string appId)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary[224] = appId;
			return this.OpCustom(220, dictionary, true, 0, true);
		}

		public virtual bool OpJoinLobby(TypedLobby lobby)
		{
			if (this.DebugOut >= 3)
			{
				base.Listener.DebugReturn(3, "OpJoinLobby()");
			}
			Dictionary<byte, object> dictionary = null;
			if (lobby != null && !lobby.IsDefault)
			{
				dictionary = new Dictionary<byte, object>();
				dictionary[213] = lobby.Name;
				dictionary[212] = (byte)lobby.Type;
			}
			return this.OpCustom(229, dictionary, true, 0, false);
		}

		public virtual bool OpLeaveLobby()
		{
			if (this.DebugOut >= 3)
			{
				base.Listener.DebugReturn(3, "OpLeaveLobby()");
			}
			return this.OpCustom(228, null, true, 0, false);
		}

		public virtual bool OpCreateRoom(string roomName, RoomOptions roomOptions, TypedLobby lobby, Hashtable playerProperties, bool onGameServer)
		{
			if (this.DebugOut >= 3)
			{
				base.Listener.DebugReturn(3, "OpCreateRoom()");
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			if (!string.IsNullOrEmpty(roomName))
			{
				dictionary[byte.MaxValue] = roomName;
			}
			if (lobby != null)
			{
				dictionary[213] = lobby.Name;
				dictionary[212] = (byte)lobby.Type;
			}
			if (onGameServer)
			{
				if (playerProperties != null && playerProperties.Count > 0)
				{
					dictionary[249] = playerProperties;
					dictionary[250] = true;
				}
				if (roomOptions == null)
				{
					roomOptions = new RoomOptions();
				}
				Hashtable hashtable = new Hashtable();
				dictionary[248] = hashtable;
				hashtable.MergeStringKeys(roomOptions.customRoomProperties);
				hashtable[253] = roomOptions.isOpen;
				hashtable[254] = roomOptions.isVisible;
				hashtable[250] = roomOptions.customRoomPropertiesForLobby;
				if (roomOptions.maxPlayers > 0)
				{
					hashtable[byte.MaxValue] = (byte)roomOptions.maxPlayers;
				}
				if (roomOptions.cleanupCacheOnLeave)
				{
					dictionary[241] = true;
					hashtable[249] = true;
				}
			}
			return this.OpCustom(227, dictionary, true, 0, false);
		}

		public virtual bool OpJoinRoom(string roomName, RoomOptions roomOptions, TypedLobby lobby, bool createIfNotExists, Hashtable playerProperties, bool onGameServer)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			if (!string.IsNullOrEmpty(roomName))
			{
				dictionary[byte.MaxValue] = roomName;
			}
			if (createIfNotExists)
			{
				dictionary[1] = true;
				if (lobby != null)
				{
					dictionary[213] = lobby.Name;
					dictionary[212] = (byte)lobby.Type;
				}
			}
			if (onGameServer)
			{
				if (playerProperties != null && playerProperties.Count > 0)
				{
					dictionary[249] = playerProperties;
					dictionary[250] = true;
				}
				Hashtable hashtable = new Hashtable();
				dictionary[248] = hashtable;
				if (createIfNotExists)
				{
					if (roomOptions == null)
					{
						roomOptions = new RoomOptions();
					}
					hashtable.MergeStringKeys(roomOptions.customRoomProperties);
					hashtable[253] = roomOptions.isOpen;
					hashtable[254] = roomOptions.isVisible;
					hashtable[250] = roomOptions.customRoomPropertiesForLobby;
					if (roomOptions.maxPlayers > 0)
					{
						hashtable[byte.MaxValue] = (byte)roomOptions.maxPlayers;
					}
					if (roomOptions.cleanupCacheOnLeave)
					{
						dictionary[241] = true;
						hashtable[249] = true;
					}
				}
				else
				{
					hashtable.MergeStringKeys(roomOptions.customRoomProperties);
				}
			}
			else if (roomOptions != null && roomOptions.customRoomProperties != null)
			{
				Hashtable hashtable2 = new Hashtable();
				hashtable2.MergeStringKeys(roomOptions.customRoomProperties);
				dictionary[248] = hashtable2;
				this.opParameters[223] = 0;
			}
			return this.OpCustom(226, dictionary, true, 0, false);
		}

		public virtual bool OpJoinRandomRoom(Hashtable expectedCustomRoomProperties, byte expectedMaxPlayers, Hashtable playerProperties, MatchmakingMode matchingType, TypedLobby typedLobby, string sqlLobbyFilter)
		{
			if (this.DebugOut >= 3)
			{
				base.Listener.DebugReturn(3, "OpJoinRandomRoom()");
			}
			Hashtable hashtable = new Hashtable();
			hashtable.MergeStringKeys(expectedCustomRoomProperties);
			if (expectedMaxPlayers > 0)
			{
				hashtable[byte.MaxValue] = expectedMaxPlayers;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			if (hashtable.Count > 0)
			{
				dictionary[248] = hashtable;
			}
			if (playerProperties != null && playerProperties.Count > 0)
			{
				dictionary[249] = playerProperties;
			}
			if (matchingType != MatchmakingMode.FillRoom)
			{
				dictionary[223] = (byte)matchingType;
			}
			if (typedLobby != null)
			{
				dictionary[213] = typedLobby.Name;
				dictionary[212] = (byte)typedLobby.Type;
			}
			if (!string.IsNullOrEmpty(sqlLobbyFilter))
			{
				dictionary[245] = sqlLobbyFilter;
			}
			return this.OpCustom(225, dictionary, true, 0, false);
		}

		public virtual bool OpFindFriends(string[] friendsToFind)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			if (friendsToFind != null && friendsToFind.Length > 0)
			{
				dictionary[1] = friendsToFind;
			}
			return this.OpCustom(222, dictionary, true, 0, false);
		}

		public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable actorProperties, bool broadcast, byte channelId)
		{
			return this.OpSetPropertiesOfActor(actorNr, actorProperties.StripToStringKeys(), broadcast, channelId, null);
		}

		protected internal bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties, bool broadcast, byte channelId, Hashtable expectedValues)
		{
			if (this.DebugOut >= 3)
			{
				base.Listener.DebugReturn(3, "OpSetPropertiesOfActor()");
			}
			if (actorNr <= 0 || actorProperties == null)
			{
				if (this.DebugOut >= 3)
				{
					base.Listener.DebugReturn(3, "OpSetPropertiesOfActor not sent. ActorNr must be > 0 and actorProperties != null.");
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(251, actorProperties);
			dictionary.Add(254, actorNr);
			if (broadcast)
			{
				dictionary.Add(250, broadcast);
			}
			if (expectedValues != null && expectedValues.Count > 0)
			{
				dictionary.Add(231, expectedValues);
			}
			return this.OpCustom(252, dictionary, broadcast, channelId, false);
		}

		protected void OpSetPropertyOfRoom(byte propCode, object value)
		{
			Hashtable hashtable = new Hashtable();
			hashtable[propCode] = value;
			this.OpSetPropertiesOfRoom(hashtable, true, 0, null);
		}

		public bool OpSetCustomPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId)
		{
			return this.OpSetPropertiesOfRoom(gameProperties.StripToStringKeys(), broadcast, channelId, null);
		}

		protected internal bool OpSetPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId, Hashtable expectedValues)
		{
			if (this.DebugOut >= 3)
			{
				base.Listener.DebugReturn(3, "OpSetPropertiesOfRoom()");
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(251, gameProperties);
			if (broadcast)
			{
				dictionary.Add(250, true);
			}
			if (expectedValues != null && expectedValues.Count > 0)
			{
				dictionary.Add(231, expectedValues);
			}
			return this.OpCustom(252, dictionary, broadcast, channelId, false);
		}

		public static Func<string> GetPlayerSessionInfo { get; set; }

		public virtual bool OpAuthenticate(string appId, string appVersion, string userId, AuthenticationValues authValues, string regionCode)
		{
			if (this.DebugOut >= 3)
			{
				base.Listener.DebugReturn(3, "OpAuthenticate()");
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			if (authValues != null && authValues.Secret != null)
			{
				dictionary[221] = authValues.Secret;
			}
			dictionary[220] = appVersion;
			dictionary[224] = appId;
			if (!string.IsNullOrEmpty(regionCode))
			{
				dictionary[210] = regionCode;
			}
			if (!string.IsNullOrEmpty(userId))
			{
				dictionary[225] = userId;
			}
			if (LoadbalancingPeer.GetPlayerSessionInfo != null)
			{
				string text = LoadbalancingPeer.GetPlayerSessionInfo();
				if (text != null)
				{
					dictionary[218] = text;
				}
			}
			if (authValues != null && authValues.AuthType != CustomAuthenticationType.None)
			{
				if (!base.IsEncryptionAvailable)
				{
					base.Listener.DebugReturn(1, "OpAuthenticate() failed. When you want Custom Authentication encryption is mandatory.");
					return false;
				}
				dictionary[217] = (byte)authValues.AuthType;
				if (!string.IsNullOrEmpty(authValues.Secret))
				{
					dictionary[221] = authValues.Secret;
				}
				if (!string.IsNullOrEmpty(authValues.AuthParameters))
				{
					dictionary[216] = authValues.AuthParameters;
				}
				if (authValues.AuthPostData != null)
				{
					dictionary[214] = authValues.AuthPostData;
				}
			}
			bool flag = this.OpCustom(230, dictionary, true, 0, base.IsEncryptionAvailable);
			if (!flag)
			{
				base.Listener.DebugReturn(1, "Error calling OpAuthenticate! Did not work. Check log output, CustomAuthenticationValues and if you're connected.");
			}
			return flag;
		}

		public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
		{
			if (this.DebugOut >= 5)
			{
				base.Listener.DebugReturn(5, "OpChangeGroups()");
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			if (groupsToRemove != null)
			{
				dictionary[239] = groupsToRemove;
			}
			if (groupsToAdd != null)
			{
				dictionary[238] = groupsToAdd;
			}
			return this.OpCustom(248, dictionary, true, 0, false);
		}

		public virtual bool OpRaiseEvent(byte eventCode, object customEventContent, bool sendReliable, RaiseEventOptions raiseEventOptions)
		{
			this.opParameters.Clear();
			this.opParameters[244] = eventCode;
			if (customEventContent != null)
			{
				this.opParameters[245] = customEventContent;
			}
			if (raiseEventOptions == null)
			{
				raiseEventOptions = RaiseEventOptions.Default;
			}
			else
			{
				if (raiseEventOptions.CachingOption != EventCaching.DoNotCache)
				{
					this.opParameters[247] = (byte)raiseEventOptions.CachingOption;
				}
				if (raiseEventOptions.Receivers != ReceiverGroup.Others)
				{
					this.opParameters[246] = (byte)raiseEventOptions.Receivers;
				}
				if (raiseEventOptions.InterestGroup != 0)
				{
					this.opParameters[240] = raiseEventOptions.InterestGroup;
				}
				if (raiseEventOptions.TargetActors != null)
				{
					this.opParameters[252] = raiseEventOptions.TargetActors;
				}
				if (raiseEventOptions.ForwardToWebhook)
				{
					this.opParameters[234] = true;
				}
			}
			return this.OpCustom(253, this.opParameters, sendReliable, raiseEventOptions.SequenceChannel, raiseEventOptions.Encrypt);
		}

		private readonly Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
	}
}
