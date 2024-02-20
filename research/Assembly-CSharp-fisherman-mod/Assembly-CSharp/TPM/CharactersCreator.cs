using System;
using System.Collections.Generic;
using System.Diagnostics;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class CharactersCreator : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CharactersCreator.AllCharactersLoadedDelegate EAllCharactersLoaded = delegate
		{
		};

		private void Awake()
		{
			if (base.enabled && this._baker == null)
			{
				GameObject gameObject = GameObject.Find("bakers");
				if (gameObject != null)
				{
					this._baker = gameObject.GetComponent<MeshBakersController>();
				}
				else
				{
					LogHelper.Error("MeshBakersController is not found", new object[0]);
				}
			}
		}

		private void Start()
		{
			if (this._mainCamera == null)
			{
				this._mainCamera = Camera.main;
			}
			this._prevPlayerPos = this._debugPlayersRoot.position;
			if (this._autoAddDebugPlayers)
			{
				foreach (DebugCustomizationSettings debugCustomizationSettings in this._debugPlayers)
				{
					this.AddPlayer(new TPMCharacterModel(debugCustomizationSettings.head, debugCustomizationSettings.hair, debugCustomizationSettings.pants, debugCustomizationSettings.hat, debugCustomizationSettings.shirt, debugCustomizationSettings.shoes));
				}
			}
			else if (this._autoAddFirstPlayer)
			{
				this.AddPlayer(true);
			}
		}

		private void Update()
		{
			if (this._isServerListened && this._stopWaitingFirtsUpdateAt > 0f && Time.time > this._stopWaitingFirtsUpdateAt)
			{
				this._stopWaitingFirtsUpdateAt = -1f;
				this.CheckAllPlayersLoaded();
			}
		}

		private void OnDestroy()
		{
			if (this._isServerListened)
			{
				if (PhotonConnectionFactory.Instance != null)
				{
					PhotonConnectionFactory.Instance.OnCharacterUpdated -= this.ServerOnCharacterUpdated;
					PhotonConnectionFactory.Instance.OnCharacterDestroyed -= this.ServerOnCharacterDestroyed;
				}
				if (this._baker != null)
				{
					this._baker.EModelReady -= this.BakerOnModelReady;
				}
			}
			this._baker = null;
		}

		public void StartServerListening()
		{
			if (PhotonConnectionFactory.Instance != null)
			{
				this._isServerListened = true;
				this._stopWaitingFirtsUpdateAt = Time.time + this._firstUpdateMinimalWaitingTime;
				this._requestedPlayers.Clear();
				this._loadedPlayersNames.Clear();
				this._baker.EModelReady += this.BakerOnModelReady;
				PhotonConnectionFactory.Instance.OnCharacterUpdated += this.ServerOnCharacterUpdated;
				PhotonConnectionFactory.Instance.OnCharacterDestroyed += this.ServerOnCharacterDestroyed;
			}
		}

		private void ServerOnCharacterUpdated(CharacterInfo character)
		{
			if (this._baker.IsPresentPlayer(character.Id) || this._requestedPlayers.ContainsKey(character.Id))
			{
				return;
			}
			Player player = PhotonConnectionFactory.Instance.Room.FindPlayer(character.Id);
			GameObject gameObject = Object.Instantiate<GameObject>(this._bakedPlayerPrefab);
			gameObject.name = player.UserId;
			gameObject.transform.parent = this._playersRoot;
			gameObject.transform.localPosition = Vector3.zero;
			this._requestedPlayers[character.Id] = player;
			this._baker.AskToCreateModelParts(gameObject, player.UserId, player.TpmCharacterModel, false);
		}

		private void ServerOnCharacterDestroyed(Player player)
		{
			string userId = player.UserId;
			if (this._requestedPlayers.ContainsKey(userId))
			{
				this._baker.AskToDeleteModelParts(userId);
				this._requestedPlayers.Remove(userId);
				this._loadedPlayersNames.Remove(userId);
				this.CheckAllPlayersLoaded();
			}
		}

		private void BakerOnModelReady(string objID, Dictionary<CustomizedParts, TPMModelLayerSettings> parts, TPMCharacterModel modelSettings, SkinnedMeshRenderer bakedRenderer)
		{
			if (this._debugPlayersNames.Contains(objID))
			{
				Transform transform = this.CreatePlayerAndConnectParts(objID, this._debugPlayersRoot, this._debugPlayerPrefab);
				MovementTestPlayerSetup component = transform.GetComponent<MovementTestPlayerSetup>();
				component.Init(this._mainCamera, null, parts, modelSettings, false, 0f);
			}
			else
			{
				if (this._requestedPlayers.ContainsKey(objID))
				{
					this._loadedPlayersNames.Add(objID);
					Transform transform2 = this._playersRoot.Find(objID);
					BakedPlayerData component2 = transform2.GetComponent<BakedPlayerData>();
					component2.SetData(this._requestedPlayers[objID], parts, bakedRenderer);
				}
				this.CheckAllPlayersLoaded();
			}
		}

		private void CheckAllPlayersLoaded()
		{
			if (this._loadedPlayersNames.Count == this._requestedPlayers.Count)
			{
				this._isServerListened = false;
				this._stopWaitingFirtsUpdateAt = -1f;
				PhotonConnectionFactory.Instance.OnCharacterUpdated -= this.ServerOnCharacterUpdated;
				PhotonConnectionFactory.Instance.OnCharacterDestroyed -= this.ServerOnCharacterDestroyed;
				this._baker.EModelReady -= this.BakerOnModelReady;
				TPMCharacterCustomization.Instance.RootWithPrecreatedPlayers = this._playersRoot;
				this.EAllCharactersLoaded();
			}
		}

		private Transform CreatePlayerAndConnectParts(string objID, Transform root, GameObject playerPrefab)
		{
			Transform transform = root.Find(objID);
			if (transform != null)
			{
				Transform transform2 = Object.Instantiate<GameObject>(playerPrefab).transform;
				transform2.name = objID;
				transform.SetParent(transform2, false);
				transform2.SetParent(root);
				transform2.position = this.GenerateNextPlayerPos(1);
				return transform2;
			}
			LogHelper.Error("Can't find player transform {0}", new object[] { objID });
			return null;
		}

		private Vector3 GenerateNextPlayerPos(int sign = 1)
		{
			return this._prevPlayerPos += (float)sign * this._debugPlayersRoot.TransformDirection(new Vector3(this._distBetweenPlayers, 0f, 0f));
		}

		private void AddPlayer(bool isCommonSettings)
		{
			DebugCustomizationSettings debugCustomizationSettings = ((!isCommonSettings) ? TPMCharacterCustomization.DebugSettings2 : TPMCharacterCustomization.DebugSettings1);
			this.AddPlayer(new TPMCharacterModel(debugCustomizationSettings.head, debugCustomizationSettings.hair, debugCustomizationSettings.pants, debugCustomizationSettings.hat, debugCustomizationSettings.shirt, debugCustomizationSettings.shoes));
		}

		private void AddPlayer(TPMCharacterModel modelSettings)
		{
			GameObject gameObject = new GameObject();
			string text = gameObject.transform.GetInstanceID().ToString();
			gameObject.name = text;
			this._debugPlayersNames.Add(text);
			gameObject.transform.SetParent(this._debugPlayersRoot, true);
			this._baker.AskToCreateModelParts(gameObject, text, modelSettings, false);
		}

		[Tooltip("Controller - creator player from the parts")]
		[SerializeField]
		private MeshBakersController _baker;

		[SerializeField]
		private GameObject _debugPlayerPrefab;

		[SerializeField]
		private GameObject _bakedPlayerPrefab;

		[SerializeField]
		private Transform _playersRoot;

		[SerializeField]
		private Transform _debugPlayersRoot;

		[SerializeField]
		private float _distBetweenPlayers;

		[Tooltip("Leave empty if you want to setup it automatically")]
		[SerializeField]
		private Camera _mainCamera;

		[SerializeField]
		private bool _autoAddFirstPlayer;

		[SerializeField]
		private bool _autoAddDebugPlayers;

		[SerializeField]
		private float _firstUpdateMinimalWaitingTime = 10f;

		private DebugCustomizationSettings[] _debugPlayers = new DebugCustomizationSettings[]
		{
			new DebugCustomizationSettings
			{
				head = Faces.M1,
				hat = Hats.None,
				hair = Hair.M1,
				shirt = Shirts.VestAngler,
				pants = Pants.M1,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.M2,
				hat = Hats.None,
				hair = Hair.M2,
				shirt = Shirts.VestCamper,
				pants = Pants.M2,
				shoes = Shoes.SNEAKERS
			},
			new DebugCustomizationSettings
			{
				head = Faces.M3,
				hat = Hats.None,
				hair = Hair.M3,
				shirt = Shirts.VestElement,
				pants = Pants.M3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.M1,
				hat = Hats.DlcBass,
				hair = Hair.M3,
				shirt = Shirts.VestElementDeluxe,
				pants = Pants.M3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.M1,
				hat = Hats.DlcBass,
				hair = Hair.M3,
				shirt = Shirts.VestGrizzly,
				pants = Pants.M3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.M1,
				hat = Hats.DlcBass,
				hair = Hair.M3,
				shirt = Shirts.VestTravel,
				pants = Pants.M3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.M1,
				hat = Hats.DlcBass,
				hair = Hair.M3,
				shirt = Shirts.VestTravelPro,
				pants = Pants.M3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.M1,
				hat = Hats.DlcBass,
				hair = Hair.M3,
				shirt = Shirts.VestDenim,
				pants = Pants.M3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.F1,
				hat = Hats.None,
				hair = Hair.F1,
				shirt = Shirts.VestAngler,
				pants = Pants.F1,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.F2,
				hat = Hats.None,
				hair = Hair.F2,
				shirt = Shirts.VestCamper,
				pants = Pants.F2,
				shoes = Shoes.SNEAKERS
			},
			new DebugCustomizationSettings
			{
				head = Faces.F3,
				hat = Hats.None,
				hair = Hair.F3,
				shirt = Shirts.VestElement,
				pants = Pants.F3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.F1,
				hat = Hats.Cap,
				hair = Hair.F3,
				shirt = Shirts.VestElementDeluxe,
				pants = Pants.F3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.F1,
				hat = Hats.Cap,
				hair = Hair.F3,
				shirt = Shirts.VestGrizzly,
				pants = Pants.F3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.F1,
				hat = Hats.Cap,
				hair = Hair.F3,
				shirt = Shirts.VestTravel,
				pants = Pants.F3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.F1,
				hat = Hats.Cap,
				hair = Hair.F3,
				shirt = Shirts.VestTravelPro,
				pants = Pants.F3,
				shoes = Shoes.BOOTS
			},
			new DebugCustomizationSettings
			{
				head = Faces.F1,
				hat = Hats.Cap,
				hair = Hair.F3,
				shirt = Shirts.VestDenim,
				pants = Pants.F3,
				shoes = Shoes.SNEAKERS
			}
		};

		private Vector3 _prevPlayerPos;

		private HashSet<string> _debugPlayersNames = new HashSet<string>();

		private Dictionary<string, Player> _requestedPlayers = new Dictionary<string, Player>();

		private HashSet<string> _loadedPlayersNames = new HashSet<string>();

		private float _stopWaitingFirtsUpdateAt = -1f;

		private bool _isServerListened;

		public delegate void AllCharactersLoadedDelegate();
	}
}
