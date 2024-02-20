using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class MeshBakersController : MonoBehaviour, IDebugLog
	{
		public bool IsBakingEnabled
		{
			get
			{
				return this._isBakingEnabled;
			}
		}

		public bool IsDebugLogEnabled
		{
			get
			{
				return this._isDebugLogEnabled;
			}
		}

		public Transform PlayersRoot
		{
			get
			{
				return this._playersRoot;
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event MeshBakersController.ModelReadyDelegate EModelReady = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event MeshBakersController.ModelUnbakedDelegate EModelUnbaked = delegate
		{
		};

		private bool IsTakePartsFromDebugList
		{
			get
			{
				return this._isDebugListOfPartsActive && this._debugListOfPartsToCreate.Count > 0;
			}
		}

		private void Start()
		{
			GameObject gameObject = new GameObject("players");
			this._playersRoot = gameObject.transform;
			this._playersRoot.parent = base.transform;
			this._isBakingEnabled = true;
			if (this.IsTakePartsFromDebugList)
			{
				MeshBakersController.SKELETON_SRC_PART = this._debugListOfPartsToCreate[0];
			}
			if (this._isBakingEnabled && this._warmUpObject)
			{
				this._warmUpObject.SetActive(true);
			}
		}

		public void ClearMeshes()
		{
			this._creatingQueue.Clear();
			this._deletingQueue.Clear();
			foreach (MeshBakerPlayerController meshBakerPlayerController in this._bakers.Values)
			{
				meshBakerPlayerController.Clear();
				Object.DestroyImmediate(meshBakerPlayerController.gameObject);
			}
			this._bakers.Clear();
			this._players.Clear();
		}

		public bool IsPresentPlayer(string playerId)
		{
			return this._creatingQueue.ContainsKey(playerId) || this._deletingQueue.Contains(playerId) || this._players.Contains(playerId);
		}

		public void AskToCreateModelParts(GameObject root, string playerId, TPMCharacterModel modelSettings, bool isSkipPauses = false)
		{
			this._creatingQueue[playerId] = new MeshBakersController.CreatingQueue
			{
				root = root,
				modelSettings = modelSettings,
				isSkipPauses = isSkipPauses
			};
			this._deletingQueue.Remove(playerId);
		}

		private IEnumerator CreateModelParts(GameObject root, string playerID, TPMCharacterModel modelSettings, bool isSkipPauses)
		{
			this._isBusy = true;
			TPMCharacterCustomization factory = TPMCharacterCustomization.Instance;
			HeadRecord headRecord = factory.GetHead(modelSettings.Head);
			string[] prefabs2LoadAsync = new string[7];
			prefabs2LoadAsync[1] = modelSettings.GetShirtPath(headRecord.gender);
			prefabs2LoadAsync[0] = headRecord.headModelPath;
			prefabs2LoadAsync[2] = factory.GetHands(headRecord.gender, modelSettings.GetShirtHandLength(headRecord.gender)).modelPath;
			Shoes curShoes;
			prefabs2LoadAsync[4] = modelSettings.GetShoesPath(headRecord.gender, out curShoes);
			prefabs2LoadAsync[3] = factory.GetPantsPath(modelSettings.Pants, curShoes);
			string hatPath = modelSettings.GetHatPath(headRecord.gender);
			bool isHatPresent = !string.IsNullOrEmpty(hatPath);
			if (isHatPresent)
			{
				prefabs2LoadAsync[6] = hatPath;
			}
			if (modelSettings.Hair != Hair.None)
			{
				HairRecord hair = factory.GetHair(modelSettings.Hair);
				prefabs2LoadAsync[5] = ((!isHatPresent) ? hair.modelPath : hair.modelWithHatPath);
			}
			Dictionary<CustomizedParts, TPMModelLayerSettings> partsMap = new Dictionary<CustomizedParts, TPMModelLayerSettings>();
			MeshBakerPlayerController baker = null;
			this._players.Add(playerID);
			if (this._isBakingEnabled)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this._playerBakerPrefab);
				gameObject.transform.parent = base.transform;
				baker = gameObject.GetComponent<MeshBakerPlayerController>();
				this._bakers[playerID] = baker;
			}
			Dictionary<string, Transform> srcBonesMap = new Dictionary<string, Transform>();
			List<CustomizedParts> orderedParts = new List<CustomizedParts>(prefabs2LoadAsync.Length);
			List<GameObject> meshesToBake = new List<GameObject>(10);
			orderedParts.Add(MeshBakersController.SKELETON_SRC_PART);
			for (int k = 0; k < prefabs2LoadAsync.Length; k++)
			{
				if (k != (int)MeshBakersController.SKELETON_SRC_PART)
				{
					orderedParts.Add((CustomizedParts)k);
				}
			}
			for (int i = 0; i < orderedParts.Count; i++)
			{
				CustomizedParts part = orderedParts[i];
				string path = prefabs2LoadAsync[(int)part];
				if ((!this.IsTakePartsFromDebugList || this._debugListOfPartsToCreate.Contains(part)) && !string.IsNullOrEmpty(path))
				{
					FishWaterTile.DoingRenderWater = false;
					ResourceRequest request = Resources.LoadAsync(path);
					yield return request;
					GameObject prefab = request.asset as GameObject;
					if (prefab == null)
					{
						bool isDefaultPresent = this._defaultParts.ContainsKey(part);
						string message = string.Format("Can't load {0} from path {1}. {2}", part, path, (!isDefaultPresent) ? "Skip creating" : "Load default instead");
						LogHelper.Error(message, new object[0]);
						PhotonConnectionFactory.Instance.PinError(message, "MeshBakerController parts loading");
						if (isDefaultPresent)
						{
							path = this._defaultParts[part][(int)((byte)headRecord.gender)];
							request = Resources.LoadAsync(path);
							yield return request;
							prefab = request.asset as GameObject;
							if (prefab == null)
							{
								message = string.Format("Default prefab path {0} is invalid too", path);
								LogHelper.Error(message, new object[0]);
								PhotonConnectionFactory.Instance.PinError(message, "MeshBakerController parts loading");
							}
						}
					}
					FishWaterTile.DoingRenderWater = true;
					if (prefab != null)
					{
						TPMModelLayerSettings p = this.CreatePartModel(playerID, root, prefab, meshesToBake);
						partsMap[part] = p;
						if (part == CustomizedParts.Head)
						{
							MeshBakersController.SetModelColor(p.gameObject, ColorGroup.SKIN, modelSettings.SkinColor);
							MeshBakersController.SetModelColor(p.gameObject, ColorGroup.EYES, modelSettings.EyeColor);
						}
						else if (part == CustomizedParts.Hands)
						{
							MeshBakersController.SetModelColor(p.gameObject, ColorGroup.SKIN, modelSettings.SkinColor);
						}
						if (!isSkipPauses)
						{
							yield return new WaitForSeconds(this._delayBetweenBaking);
						}
						if (i == 0)
						{
							if (this._bonesCache == null)
							{
								this._bonesCache = new Transform[partsMap[MeshBakersController.SKELETON_SRC_PART].settings[0].renderer.bones.Length];
							}
							srcBonesMap = TransformHelper.BuildSrcBonesMap(partsMap[MeshBakersController.SKELETON_SRC_PART].settings[0].renderer);
						}
						else
						{
							LayerPair[] settings = partsMap[part].settings;
							for (int l = 0; l < settings.Length; l++)
							{
								SkinnedMeshRenderer renderer = settings[l].renderer;
								if (renderer == null)
								{
									LogHelper.Log("Looks like object {0} was destroyed before all parts was baked", new object[] { part });
								}
								else if (renderer.bones == null)
								{
									LogHelper.Error("Can't find bones for layer renderer {0} in {1} object", new object[] { renderer.gameObject, part });
								}
								else
								{
									TransformHelper.CopyBoneTransforms(renderer, srcBonesMap, this._bonesCache);
								}
							}
							for (int m = 0; m < settings.Length; m++)
							{
								SkinnedMeshRenderer renderer2 = settings[m].renderer;
								if (renderer2 != null)
								{
									BonesRemappingHandler component = renderer2.gameObject.GetComponent<BonesRemappingHandler>();
									if (component != null)
									{
										component.OnRemapped();
									}
								}
							}
						}
						if (baker != null)
						{
							for (int j = 0; j < meshesToBake.Count; j++)
							{
								if (!isSkipPauses)
								{
									yield return new WaitForSeconds(this._delayBetweenBaking);
								}
								baker.AddModel(meshesToBake[j]);
							}
							meshesToBake.Clear();
						}
						if (!isSkipPauses)
						{
							yield return new WaitForSeconds(this._delayBetweenPartsCreations);
						}
					}
				}
			}
			this._isBusy = false;
			this.CallModelReady(playerID, partsMap, modelSettings);
			yield break;
		}

		private static void SetModelColor(GameObject model, ColorGroup colorGroup, Color color)
		{
			ModelColorSettings component = model.GetComponent<ModelColorSettings>();
			if (component != null)
			{
				SkinnedMeshRenderer renderer = component.GetRenderer(colorGroup);
				if (renderer != null)
				{
					TPMCharacterCustomization.SetMeshColor(renderer, color);
				}
				else
				{
					LogHelper.Error("Can't find SkinnedMeshRenderer in {0} color group for {1} model", new object[] { colorGroup, model });
				}
			}
			else
			{
				LogHelper.Error("Can't find ModelColorSettings in {0} model", new object[] { model });
			}
		}

		private void CallModelReady(string playerID, Dictionary<CustomizedParts, TPMModelLayerSettings> partsMap, TPMCharacterModel modelSettings)
		{
			this._modelCreationEnabledAt = Time.time + this._delayAfterPlayersCreations;
			if (!this._deletingQueue.Contains(playerID))
			{
				SkinnedMeshRenderer skinnedMeshRenderer = ((!this._bakers.ContainsKey(playerID)) ? null : this._bakers[playerID].GetRenderer());
				this.EModelReady(playerID, partsMap, modelSettings, skinnedMeshRenderer);
			}
		}

		public void AskToDeleteModelParts(string playerID)
		{
			this._playersLoadingTextures.Remove(playerID);
			if (this._creatingQueue.ContainsKey(playerID))
			{
				this._creatingQueue.Remove(playerID);
				this.EModelUnbaked(playerID);
			}
			else if (this._isBusy)
			{
				this._deletingQueue.Add(playerID);
			}
			else if (this._isBakingEnabled)
			{
				if (this._bakers.ContainsKey(playerID))
				{
					this._deletingQueue.Add(playerID);
				}
				else
				{
					LogHelper.Error("Can't find player {0} to delete meshes", new object[] { playerID });
					this.EModelUnbaked(playerID);
				}
			}
			else
			{
				this.EModelUnbaked(playerID);
			}
		}

		private void Update()
		{
			if (this._isBusy)
			{
				return;
			}
			if (this._deletingQueue.Count > 0)
			{
				string text = this._deletingQueue.ElementAt(0);
				this._deletingQueue.Remove(text);
				if (this._bakers.ContainsKey(text))
				{
					this._bakers[text].Clear();
					Object.DestroyImmediate(this._bakers[text].gameObject);
					this._bakers.Remove(text);
				}
				this._players.Remove(text);
				this.EModelUnbaked(text);
			}
			else
			{
				this.ProcessCreatingQueue();
			}
		}

		private void ProcessCreatingQueue()
		{
			if (Time.time > this._modelCreationEnabledAt)
			{
				if (this._creatingQueue.Count > 0)
				{
					string text = this._creatingQueue.Keys.ElementAt(0);
					MeshBakersController.CreatingQueue creatingQueue = this._creatingQueue[text];
					this._creatingQueue.Remove(text);
					base.StartCoroutine(this.CreateModelParts(creatingQueue.root, text, creatingQueue.modelSettings, creatingQueue.isSkipPauses));
				}
				else if (this._playersLoadingTextures.Count > 0)
				{
					string text2;
					Queue<MeshBakersController.ReplaceTextureData> queue;
					for (;;)
					{
						text2 = this._playersLoadingTextures.Keys.ElementAt(0);
						queue = this._playersLoadingTextures[text2];
						if (queue.Count != 0)
						{
							break;
						}
						this._playersLoadingTextures.Remove(text2);
						if (this._playersLoadingTextures.Count == 0)
						{
							return;
						}
					}
					MeshBakersController.ReplaceTextureData replaceTextureData = queue.Dequeue();
					base.StartCoroutine(this.LoadAndReplaceTexture(text2, replaceTextureData.material, replaceTextureData.texturePathes, replaceTextureData.textureName, replaceTextureData.textureIdIndex));
					return;
				}
			}
		}

		private IEnumerator LoadAndReplaceTexture(string playerID, Material material, string[] pathes, string textureName, int textureIdIndex)
		{
			this._isBusy = true;
			ResourceRequest request = null;
			for (int i = 0; i < pathes.Length; i++)
			{
				request = Resources.LoadAsync<Texture2D>(string.Format("{0}/{1}_HQ", pathes[i], textureName));
				yield return request;
				if (request.asset != null)
				{
					break;
				}
			}
			if (this._playersLoadingTextures.ContainsKey(playerID))
			{
				this._modelCreationEnabledAt = Time.time + this._delayBetweenTexturesLoading;
				Texture2D texture2D = request.asset as Texture2D;
				if (texture2D != null)
				{
					material.SetTexture(GlobalConsts.PossibleTextures[textureIdIndex], texture2D);
				}
			}
			this._isBusy = false;
			yield break;
		}

		private void CheckMaterialsForHQReplacing(string playerID, LayerPair layer)
		{
			Material[] materials = layer.renderer.materials;
			string[] array = layer.texturesPath.Split(new char[] { ';' });
			foreach (Material material in materials)
			{
				for (int j = 0; j < GlobalConsts.PossibleTextures.Length; j++)
				{
					Texture texture = material.GetTexture(GlobalConsts.PossibleTextures[j]);
					if (texture != null)
					{
						if (!this._playersLoadingTextures.ContainsKey(playerID))
						{
							this._playersLoadingTextures[playerID] = new Queue<MeshBakersController.ReplaceTextureData>();
						}
						this._playersLoadingTextures[playerID].Enqueue(new MeshBakersController.ReplaceTextureData
						{
							material = material,
							textureIdIndex = j,
							texturePathes = array,
							textureName = texture.name
						});
					}
				}
			}
		}

		private TPMModelLayerSettings CreatePartModel(string playerID, GameObject rootObject, GameObject objToCopy, List<GameObject> meshesToBake)
		{
			if (objToCopy.GetComponent<TPMModelLayerSettings>() != null)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(objToCopy);
				TPMModelLayerSettings component = gameObject.GetComponent<TPMModelLayerSettings>();
				gameObject.name = objToCopy.name;
				LayerPair[] settings = component.settings;
				component.SetLod(this._forceLOD);
				if (this._isBakingEnabled)
				{
					for (int i = 0; i < settings.Length; i++)
					{
						if (settings[i].renderer != null)
						{
							meshesToBake.Add(settings[i].renderer.gameObject);
						}
					}
				}
				else
				{
					for (int j = 0; j < settings.Length; j++)
					{
						if (this._isHQTexturesEnabled && !string.IsNullOrEmpty(settings[j].texturesPath))
						{
							this.CheckMaterialsForHQReplacing(playerID, settings[j]);
						}
					}
				}
				if (gameObject != null)
				{
					gameObject.transform.parent = rootObject.transform;
					gameObject.transform.localPosition = Vector3.zero;
				}
				return component;
			}
			LogHelper.Error("Ignore {0} model without TPMModelLayerSettings component!", new object[] { objToCopy.name });
			return null;
		}

		private void OnDestroy()
		{
			this._warmUpObject = null;
			this._playerBakerPrefab = null;
			this._bakers.Clear();
		}

		[Conditional("UNITY_PS4")]
		[Conditional("UNITY_XBOXONE")]
		public static void PatchRenderers<T>(List<T> renderers) where T : Renderer
		{
			for (int i = 0; i < renderers.Count; i++)
			{
				T t = renderers[i];
				t.shadowCastingMode = 0;
				t.gameObject.layer = 9;
			}
		}

		[Conditional("UNITY_PS4")]
		[Conditional("UNITY_XBOXONE")]
		public static void PatchRenderers<T>(T[] renderers) where T : Renderer
		{
			foreach (T t in renderers)
			{
				t.shadowCastingMode = 0;
				t.gameObject.layer = 9;
			}
		}

		[Conditional("UNITY_PS4")]
		[Conditional("UNITY_XBOXONE")]
		public static void PatchRenderer(Renderer renderer)
		{
			if (renderer != null)
			{
				renderer.shadowCastingMode = 0;
				renderer.gameObject.layer = 9;
			}
		}

		public const int PARTS_LAYER = 9;

		public const int MAX_RENDERERS_COUNT = 10;

		public static CustomizedParts SKELETON_SRC_PART = CustomizedParts.Hands;

		private static string CharacterCustomizationPath = "CharacterCustomizationFactory";

		[SerializeField]
		private float _delayAfterPlayersCreations;

		[SerializeField]
		private float _delayBetweenPartsCreations = 0.1f;

		[SerializeField]
		private float _delayBetweenBaking = 0.15f;

		[SerializeField]
		private float _delayBetweenTexturesLoading = 0.5f;

		[SerializeField]
		private bool _isBakingEnabled;

		[SerializeField]
		private GameObject _warmUpObject;

		[SerializeField]
		private GameObject _playerBakerPrefab;

		private Dictionary<string, MeshBakerPlayerController> _bakers = new Dictionary<string, MeshBakerPlayerController>();

		[SerializeField]
		private LODS _forceLOD;

		[SerializeField]
		private bool _isHQTexturesEnabled;

		[SerializeField]
		private bool _isDebugLogEnabled;

		[SerializeField]
		private bool _isDebugListOfPartsActive;

		[Tooltip("You can select a list of parts to create from model. Clear")]
		[SerializeField]
		private List<CustomizedParts> _debugListOfPartsToCreate;

		private float _modelCreationEnabledAt;

		private Transform _playersRoot;

		private Dictionary<string, Queue<MeshBakersController.ReplaceTextureData>> _playersLoadingTextures = new Dictionary<string, Queue<MeshBakersController.ReplaceTextureData>>();

		private readonly Dictionary<string, MeshBakersController.CreatingQueue> _creatingQueue = new Dictionary<string, MeshBakersController.CreatingQueue>();

		private readonly HashSet<string> _deletingQueue = new HashSet<string>();

		private readonly HashSet<string> _players = new HashSet<string>();

		private readonly Dictionary<CustomizedParts, string[]> _defaultParts = new Dictionary<CustomizedParts, string[]> { 
		{
			CustomizedParts.Shirt,
			new string[] { "Avatar/res/Models/Cloth/pClothMaleShirt_01", "Avatar/res/Models/Cloth/pClothFemaleJacket_01" }
		} };

		private bool _isBusy;

		private Transform[] _bonesCache;

		private class ReplaceTextureData
		{
			public Material material;

			public int textureIdIndex;

			public string[] texturePathes;

			public string textureName;
		}

		public delegate void ModelReadyDelegate(string objID, Dictionary<CustomizedParts, TPMModelLayerSettings> parts, TPMCharacterModel modelSettings, SkinnedMeshRenderer bakedRenderer);

		public delegate void ModelUnbakedDelegate(string objID);

		private struct CreatingQueue
		{
			public GameObject root;

			public TPMCharacterModel modelSettings;

			public bool isSkipPauses;
		}
	}
}
