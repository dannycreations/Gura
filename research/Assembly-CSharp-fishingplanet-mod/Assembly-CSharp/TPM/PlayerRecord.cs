using System;
using System.Collections.Generic;
using System.Diagnostics;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class PlayerRecord
	{
		public PlayerRecord(IPlayer player, Camera playerCamera, MeshBakersController baker)
		{
			this._player = player;
			this._playerCamera = playerCamera;
			this._baker = baker;
			this._partsRoot = new GameObject(player.UserName);
			this._partsRoot.transform.position = this.INITIAL_POS;
			if (this._baker != null)
			{
				this._baker.AskToCreateModelParts(this._partsRoot, player.UserId, player.TpmCharacterModel, false);
			}
		}

		public PlayerRecord(MeshBakersController baker, BakedPlayerData playerData, Camera playerCamera, bool labelsVisibility)
		{
			this._isModelReady = true;
			this._baker = baker;
			this._player = playerData.Player;
			this._playerCamera = playerCamera;
			this._partsRoot = playerData.gameObject;
			this.BakerOnModelReady(playerData.Parts, labelsVisibility, playerData.BakedRenderer);
		}

		public string UserName
		{
			get
			{
				return this._player.UserName;
			}
		}

		public string PlayerName
		{
			get
			{
				return this._player.UserId;
			}
		}

		public bool IsModelReady
		{
			get
			{
				return this._isModelReady;
			}
		}

		public HandsViewController Controller
		{
			get
			{
				return this._controller;
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

		public void BakerOnModelReady(Dictionary<CustomizedParts, TPMModelLayerSettings> parts, bool labelsVisibility, SkinnedMeshRenderer bakedRenderer)
		{
			if (this._player != null && this._player.TpmCharacterModel != null)
			{
				this._isModelReady = true;
				GameObject gameObject = Object.Instantiate<GameObject>(TPMCharacterCustomization.Instance.PlayerPrefab);
				gameObject.transform.parent = this._baker.PlayersRoot;
				this._controller = gameObject.GetComponent<HandsViewController>();
				this._controller.Initialize(this._playerCamera, this._player, parts, bakedRenderer, labelsVisibility, false, null, true);
				this._controller.OnPlayerActive += this._controller_OnPlayerActive;
				this.EPlayerCreated(this);
			}
		}

		private void _controller_OnPlayerActive()
		{
			this.EPlayerAdd(this);
		}

		public void BakerOnModelUnbaked()
		{
			if (this._controller != null)
			{
				this._controller.OnPlayerActive -= this._controller_OnPlayerActive;
			}
			this.Clean();
		}

		public void ServerUpdate(Package package, bool suppressVisibility)
		{
			if (this._isModelReady)
			{
				this._controller.ProcessPackage(package, suppressVisibility);
			}
		}

		public void SetLabelsVisibility(bool flag)
		{
			this._controller.SetLabelsVisibility(flag);
		}

		public void CheckCollisions(List<PlayerRecord> players)
		{
			this._controller.CheckCollisions(players);
		}

		public void OnPlayerLeave()
		{
			this._isModelReady = false;
			this._baker.AskToDeleteModelParts(this._player.UserId);
		}

		public void Clean()
		{
			if (this._controller != null)
			{
				Object.DestroyObject(this._controller.gameObject);
			}
			this._player = null;
			this._playerCamera = null;
			this._baker = null;
			Object.DestroyObject(this._partsRoot);
		}

		private readonly Vector3 INITIAL_POS = new Vector3(0f, -3f, 0f);

		private HandsViewController _controller;

		private IPlayer _player;

		private Camera _playerCamera;

		private GameObject _partsRoot;

		private MeshBakersController _baker;

		private bool _isModelReady;
	}
}
