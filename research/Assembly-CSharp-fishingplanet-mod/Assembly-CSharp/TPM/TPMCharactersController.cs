using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class TPMCharactersController
	{
		public TPMCharactersController(Camera camera)
		{
			this._playerCamera = camera;
			GameObject gameObject = GameObject.Find("bakers");
			if (gameObject != null)
			{
				this._baker = gameObject.GetComponent<MeshBakersController>();
				if (this._baker != null)
				{
					this._baker.EModelReady += this.BakerOnModelReady;
					this._baker.EModelUnbaked += this.BakerOnModelUnbaked;
				}
				else
				{
					LogHelper.Error("bakers object has no MeshBakersController attached!", new object[0]);
				}
			}
			else
			{
				LogHelper.Error("There is no bakers object in scene found!", new object[0]);
			}
			Transform rootWithPrecreatedPlayers = TPMCharacterCustomization.Instance.RootWithPrecreatedPlayers;
			if (rootWithPrecreatedPlayers != null)
			{
				while (rootWithPrecreatedPlayers.childCount > 0)
				{
					BakedPlayerData component = rootWithPrecreatedPlayers.GetChild(0).GetComponent<BakedPlayerData>();
					if (component != null)
					{
						PlayerRecord playerRecord = new PlayerRecord(this._baker, component, this._playerCamera, this._labelsVisibility);
						this._players.Add(playerRecord);
						playerRecord.EPlayerAdd += this.P_OnPlayerAdd;
						playerRecord.EPlayerCreated += this.P_OnPlayerCreated;
					}
				}
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<PlayerRecord> EPlayerAdd = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<PlayerRecord> EPlayerCreated = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<PlayerRecord> EPlayerRemove = delegate
		{
		};

		private void P_OnPlayerAdd(PlayerRecord obj)
		{
			this.EPlayerAdd(obj);
		}

		private void P_OnPlayerCreated(PlayerRecord obj)
		{
			this.EPlayerCreated(obj);
		}

		public List<PlayerRecord> GetActivePlayers()
		{
			return this._players.Where((PlayerRecord e) => e.Controller != null && e.Controller.WasControllerActivated).ToList<PlayerRecord>();
		}

		public bool IsAnyPlayerCloseEnough(Vector3 pos, float distance)
		{
			return this._players.Any((PlayerRecord p) => p.Controller != null && (p.Controller.transform.position - pos).magnitude < distance);
		}

		public TPMCharactersController.HitResult GetHitPoint(Vector3 from, Vector3 to)
		{
			Vector3 vector = to - from;
			float magnitude = vector.magnitude;
			vector.Normalize();
			Ray ray;
			ray..ctor(from, vector);
			for (int i = 0; i < this._players.Count; i++)
			{
				if (this._players[i].Controller != null)
				{
					Vector3? hitPoint = this._players[i].Controller.GetHitPoint(ray, magnitude);
					if (hitPoint != null)
					{
						return new TPMCharactersController.HitResult(hitPoint.Value, this._players[i].Controller.UserId);
					}
				}
			}
			return default(TPMCharactersController.HitResult);
		}

		public void SetLabelsVisibility(bool flag)
		{
			this._labelsVisibility = flag;
			for (int i = 0; i < this._players.Count; i++)
			{
				if (this._players[i].Controller != null)
				{
					this._players[i].SetLabelsVisibility(flag);
				}
			}
		}

		private void BakerOnModelReady(string objID, Dictionary<CustomizedParts, TPMModelLayerSettings> parts, TPMCharacterModel modelSettings, SkinnedMeshRenderer bakedRenderer)
		{
			PlayerRecord playerRecord = this._players.FirstOrDefault((PlayerRecord player) => player.PlayerName == objID);
			if (playerRecord != null)
			{
				playerRecord.BakerOnModelReady(parts, this._labelsVisibility, bakedRenderer);
			}
		}

		private void BakerOnModelUnbaked(string objID)
		{
			PlayerRecord playerRecord = this._players.FirstOrDefault((PlayerRecord player) => player.PlayerName == objID);
			if (playerRecord != null)
			{
				this.EPlayerRemove(playerRecord);
				playerRecord.BakerOnModelUnbaked();
				playerRecord.EPlayerAdd -= this.P_OnPlayerAdd;
				playerRecord.EPlayerCreated -= this.P_OnPlayerCreated;
				this._players.Remove(playerRecord);
			}
		}

		public PlayerRecord OnPlayerModelEnter(IPlayer playerData)
		{
			PlayerRecord playerRecord = this._players.FirstOrDefault((PlayerRecord player) => player.PlayerName == playerData.UserId);
			if (playerRecord != null)
			{
				return playerRecord;
			}
			playerRecord = new PlayerRecord(playerData, this._playerCamera, this._baker);
			this._players.Add(playerRecord);
			playerRecord.EPlayerAdd += this.P_OnPlayerAdd;
			playerRecord.EPlayerCreated += this.P_OnPlayerCreated;
			return playerRecord;
		}

		public void OnPlayerModelLeave(string id)
		{
			PlayerRecord playerRecord = this._players.FirstOrDefault((PlayerRecord player) => player.PlayerName == id);
			if (playerRecord != null)
			{
				playerRecord.OnPlayerLeave();
			}
		}

		public void OnUpdate(string id, Package package, bool suppressVisibility)
		{
			if (!this._isActive)
			{
				return;
			}
			PlayerRecord playerRecord = this._players.FirstOrDefault((PlayerRecord player) => player.PlayerName == id);
			if (playerRecord == null)
			{
				Player player2 = PhotonConnectionFactory.Instance.Room.FindPlayer(id);
				if (player2 == null)
				{
					if (!this._ignoredPlayers.Contains(id))
					{
						LogHelper.Error("Ignore update from unregistered player with id {0}", new object[] { id });
						this._ignoredPlayers.Add(id);
					}
					return;
				}
				playerRecord = this.OnPlayerModelEnter(player2);
			}
			playerRecord.ServerUpdate(package, suppressVisibility);
		}

		public void Update()
		{
			for (int i = 0; i < this._players.Count; i++)
			{
				if (this._players[i].Controller != null)
				{
					this._players[i].CheckCollisions(this._players);
				}
			}
		}

		public void OnIncomingChatMessage(object sender, ChatListener.EventChatMessageArgs e)
		{
			PlayerRecord playerRecord = this._players.FirstOrDefault((PlayerRecord player) => player.PlayerName == e.Message.Sender.UserId);
			if (playerRecord != null && playerRecord.Controller != null)
			{
				playerRecord.Controller.OnIncomingChatMessage(e.Message.Message);
			}
		}

		public void StartMoveToNewLocation()
		{
			this._isActive = false;
			this._baker.ClearMeshes();
			for (int i = 0; i < this._players.Count; i++)
			{
				this._players[i].Clean();
			}
			this._players.Clear();
		}

		public void FinishMoveToNewLocation()
		{
			this._isActive = true;
		}

		public void Clean()
		{
			if (this._baker != null)
			{
				this._baker.EModelReady -= this.BakerOnModelReady;
				this._baker.EModelUnbaked -= this.BakerOnModelUnbaked;
				this._baker = null;
			}
		}

		private List<PlayerRecord> _players = new List<PlayerRecord>();

		private Camera _playerCamera;

		private HashSet<string> _ignoredPlayers = new HashSet<string>();

		private MeshBakersController _baker;

		private bool _isActive = true;

		private bool _labelsVisibility = true;

		public struct HitResult
		{
			public HitResult(Vector3 pos, string userId)
			{
				this.HasValue = true;
				this.Pos = pos;
				this.UserId = userId;
			}

			public readonly bool HasValue;

			public readonly Vector3 Pos;

			public readonly string UserId;
		}
	}
}
