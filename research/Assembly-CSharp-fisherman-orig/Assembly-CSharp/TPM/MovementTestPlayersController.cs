using System;
using System.Collections.Generic;
using ObjectModel;
using RootMotion.FinalIK;
using UnityEngine;

namespace TPM
{
	public class MovementTestPlayersController : MonoBehaviour
	{
		private void Awake()
		{
			if (this._baker == null)
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
			this._mainCamera = Camera.main;
			this._waitingQueueRoot = new GameObject
			{
				name = "waitingQueue"
			}.transform;
			this._baker.EModelReady += this.BakerOnModelReady;
			this._baker.EModelUnbaked += this.BakerOnEModelUnbaked;
			this._prevPlayerPos = this._playersRoot.position;
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

		private void BakerOnModelReady(string objID, Dictionary<CustomizedParts, TPMModelLayerSettings> parts, TPMCharacterModel modelSettings, SkinnedMeshRenderer bakedRenderer)
		{
			Transform transform = this.FindWaitingPlayer(this._waitingQueueRoot, objID);
			if (transform != null)
			{
				Transform transform2 = Object.Instantiate<GameObject>(this._playerPrefab).transform;
				transform2.name = objID;
				transform.SetParent(transform2, false);
				transform2.position = this.GenerateNextPlayerPos(1);
				transform2.SetParent(this._playersRoot, true);
				MovementTestPlayerSetup component = transform2.GetComponent<MovementTestPlayerSetup>();
				component.Init(this._mainCamera, this._interactionObject, parts, modelSettings, this._isPlayerMovementsEnabled, this._playerIKTargetInitialYaw);
				component.SetupParameters(this._bools, this._bytes, this._floats);
				if (this._animatorPlayer != null)
				{
					this._animatorPlayer.OnAddPlayer(parts);
				}
			}
			else
			{
				LogHelper.Error("Can't find player transform {0}", new object[] { objID });
			}
		}

		private void BakerOnEModelUnbaked(string objID)
		{
			Transform transform = this.FindWaitingPlayer(this._playersRoot, objID);
			if (transform != null)
			{
				this._waitingDeleteModels.Remove(objID);
				Object.Destroy(transform.gameObject);
				this.GenerateNextPlayerPos(-1);
			}
			else
			{
				LogHelper.Error("BakerOnEModelUnbaked: can't found {0} to delete", new object[] { objID });
			}
		}

		private Transform FindWaitingPlayer(Transform root, string playerID)
		{
			for (int i = 0; i < root.childCount; i++)
			{
				Transform child = root.GetChild(i);
				if (child.name == playerID)
				{
					return child;
				}
			}
			return null;
		}

		private Vector3 GenerateNextPlayerPos(int sign = 1)
		{
			return this._prevPlayerPos += (float)sign * this._playersRoot.TransformDirection(new Vector3(this._distBetweenPlayers, 0f, 0f));
		}

		private bool IsButtonUp(string buttonName)
		{
			bool flag;
			try
			{
				flag = Input.GetButtonUp(buttonName);
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		private void Update()
		{
			if (this.IsButtonUp(this._addCommonButtonName) || (this._isOneKeyPressMode && Input.GetKeyUp(277)) || (!this._isOneKeyPressMode && Input.GetKey(277)))
			{
				this.AddPlayer(true);
			}
			if (this.IsButtonUp(this._addAlternativeButtonName))
			{
				this.AddPlayer(false);
			}
			if (this._isDeleteEnabled && this._playersRoot.childCount > 0)
			{
				if ((Input.GetKey(306) || Input.GetKey(305)) & Input.GetKeyUp(127))
				{
					this._baker.ClearMeshes();
					while (this._playersRoot.childCount > 0)
					{
						Transform child = this._playersRoot.GetChild(0);
						child.parent = null;
						Object.Destroy(child.gameObject);
					}
					this._prevPlayerPos = this._playersRoot.position;
				}
				else if ((this._isOneKeyPressMode && Input.GetKeyUp(127)) || (!this._isOneKeyPressMode && Input.GetKey(127)))
				{
					for (int i = this._playersRoot.childCount - 1; i >= 0; i--)
					{
						string name = this._playersRoot.GetChild(i).name;
						if (!this._waitingDeleteModels.Contains(name))
						{
							this._waitingDeleteModels.Add(name);
							this._baker.AskToDeleteModelParts(name);
							break;
						}
					}
				}
			}
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
			gameObject.transform.SetParent(this._waitingQueueRoot, true);
			this._baker.AskToCreateModelParts(gameObject, text, modelSettings, false);
		}

		private void OnDestroy()
		{
			this._baker.EModelUnbaked -= this.BakerOnEModelUnbaked;
			this._baker.EModelReady -= this.BakerOnModelReady;
		}

		[Tooltip("Controller - creator player from the parts")]
		[SerializeField]
		private MeshBakersController _baker;

		[SerializeField]
		private GameObject _playerPrefab;

		[SerializeField]
		private Transform _playersRoot;

		[SerializeField]
		private float _distBetweenPlayers;

		[Tooltip("Leave empty if you want to setup it automatically")]
		[SerializeField]
		private Camera _mainCamera;

		[Tooltip("Could be empty. Used to test interraction with IK object")]
		[SerializeField]
		private InteractionObject _interactionObject;

		[SerializeField]
		private string _addCommonButtonName = "Fire1";

		[SerializeField]
		private string _addAlternativeButtonName = "Fire2";

		[SerializeField]
		private bool _isOneKeyPressMode;

		[SerializeField]
		private float _playerIKTargetInitialYaw;

		[Tooltip("Could be empty. Used to inform external controller about players count changing")]
		[SerializeField]
		private ComplexAnimatorPlayer _animatorPlayer;

		[SerializeField]
		private bool _autoAddFirstPlayer;

		[SerializeField]
		private bool _autoAddDebugPlayers;

		[SerializeField]
		private bool _isDeleteEnabled;

		[SerializeField]
		private bool _isPlayerMovementsEnabled;

		[SerializeField]
		private Transform _holdingFish;

		[SerializeField]
		private TPMMecanimBParameter[] _bools;

		[SerializeField]
		private BytePair[] _bytes;

		[SerializeField]
		private FloatPair[] _floats;

		private DebugCustomizationSettings[] _debugPlayers = new DebugCustomizationSettings[]
		{
			new DebugCustomizationSettings
			{
				head = Faces.M4,
				hat = Hats.BoatCup,
				hair = Hair.M2,
				shirt = Shirts.BoatCup,
				pants = Pants.M1,
				shoes = Shoes.SNEAKERS
			},
			new DebugCustomizationSettings
			{
				head = Faces.F4,
				hat = Hats.BoatCup,
				hair = Hair.F2,
				shirt = Shirts.BoatCup,
				pants = Pants.F1,
				shoes = Shoes.SNEAKERS
			}
		};

		private Vector3 _prevPlayerPos;

		private Transform _waitingQueueRoot;

		private HashSet<string> _waitingDeleteModels = new HashSet<string>();
	}
}
