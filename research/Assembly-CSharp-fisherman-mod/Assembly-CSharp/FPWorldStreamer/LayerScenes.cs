using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPWorldStreamer
{
	public class LayerScenes : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event LayerScenes.SceneActionDelegate ESceneAction = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ESceneLoaded = delegate
		{
		};

		public LayerData Settings
		{
			get
			{
				return this._settings;
			}
		}

		public CellPos UnloadingIgnoreDir { get; set; }

		public void Init(LayerData layerSettings, ScenesLoader scenesLoader)
		{
			this._settings = layerSettings;
			this._scenesLoader = scenesLoader;
			this._scenesLoader.ESceneNearLoaded += this.OnSceneNearLoaded;
			this._idInLoader = this._scenesLoader.RegisterManager();
			int num = (int)Mathf.Sqrt((float)layerSettings.SceneNames.Length);
			this._cellPackBit = this.GetMaxBit(num);
			for (int i = 0; i < layerSettings.SceneNames.Length; i++)
			{
				CellPos cellPositionByItemID = LayerHelper.GetCellPositionByItemID(layerSettings.SceneNames[i]);
				if (this._allScenes.ContainsKey(this.GetCellKey(cellPositionByItemID)))
				{
					LogHelper.Error("Invalid key for {0}, {1}", new object[] { cellPositionByItemID.X, cellPositionByItemID.Z });
				}
				else
				{
					int cellKey = this.GetCellKey(cellPositionByItemID);
					this._allScenes.Add(cellKey, layerSettings.SceneNames[i]);
					this._scenesToKey[layerSettings.SceneNames[i]] = cellKey;
				}
			}
		}

		public void SetLinkedManager(LayerScenes linkedManager)
		{
			if (linkedManager != null)
			{
				linkedManager.ESceneAction += this.LinkedManagerOnSceneAction;
				this._linkedManager = linkedManager;
			}
		}

		private void OnSceneNearLoaded(int managerId, Scene scene)
		{
			if (this._idInLoader == managerId)
			{
				base.StartCoroutine(this.WaitFinishLoading(scene));
			}
		}

		private IEnumerator WaitFinishLoading(Scene scene)
		{
			int cellKey = this._scenesToKey[scene.path];
			this._waitingLoading.Add(cellKey);
			while (!scene.isLoaded)
			{
				yield return new WaitForEndOfFrame();
			}
			this._waitingLoading.Remove(cellKey);
			this.ESceneLoaded();
			this.ESceneAction(cellKey, true);
			this._loaded[cellKey] = SceneManager.GetSceneByPath(this._allScenes[cellKey]);
			this.UpdateScenePosition(cellKey);
			if (this._linkedManagerRequestedActions.ContainsKey(cellKey))
			{
				this.ProcessLinkedSceneAction(cellKey, this._linkedManagerRequestedActions[cellKey]);
			}
			yield break;
		}

		private void LinkedManagerOnSceneAction(int cellKey, bool isLoaded)
		{
			if (this._loaded.ContainsKey(cellKey) && cellKey != this._curUnloadingScene)
			{
				this.ProcessLinkedSceneAction(cellKey, isLoaded);
			}
			else
			{
				this._linkedManagerRequestedActions[cellKey] = isLoaded;
			}
		}

		public int GetCellKey(CellPos pos)
		{
			return (pos.Z << this._cellPackBit) + pos.X;
		}

		public CellPos UnpackCellKey(int cellKey)
		{
			return new CellPos(cellKey & ((1 << this._cellPackBit) - 1), cellKey >> this._cellPackBit);
		}

		public string CellKeyToString(int cellKey)
		{
			CellPos cellPos = this.UnpackCellKey(cellKey);
			return string.Format("({0}x{1})", cellPos.X, cellPos.Z);
		}

		private int GetMaxBit(int n)
		{
			int num = 0;
			while (n > 0)
			{
				n >>= 1;
				num++;
			}
			return num;
		}

		public int LoadScenes(HashSet<int> keys, CellPos curPos)
		{
			this._mustBeLoaded = keys;
			this._curPos = curPos;
			this._toUnload.Clear();
			this._scenesLoader.ClearQueue(this._idInLoader);
			int num = 0;
			foreach (int num2 in keys)
			{
				if (!this._loaded.ContainsKey(num2) && !this._waitingLoading.Contains(num2) && !this._scenesLoader.IsLoading(this._idInLoader, this._allScenes[num2]))
				{
					this._scenesLoader.Load(this._idInLoader, this._allScenes[num2]);
					num++;
				}
			}
			this.CheckScenesToUnload();
			return num;
		}

		public void CheckScenesToUnload()
		{
			int num = this._curPos.X - this._settings.LoadingCellsR;
			int num2 = this._curPos.X + this._settings.LoadingCellsR;
			int num3 = this._curPos.Z - this._settings.LoadingCellsR;
			int num4 = this._curPos.Z + this._settings.LoadingCellsR;
			if (this.UnloadingIgnoreDir.X != 0)
			{
				if (this.UnloadingIgnoreDir.X < 0)
				{
					num = this._curPos.X - (this._settings.LoadingCellsR + 1);
				}
				else
				{
					num2 = this._curPos.X + (this._settings.LoadingCellsR + 1);
				}
			}
			if (this.UnloadingIgnoreDir.Z != 0)
			{
				if (this.UnloadingIgnoreDir.Z < 0)
				{
					num3 = this._curPos.Z - (this._settings.LoadingCellsR + 1);
				}
				else
				{
					num4 = this._curPos.Z + (this._settings.LoadingCellsR + 1);
				}
			}
			foreach (int num5 in this._loaded.Keys)
			{
				if (!this._mustBeLoaded.Contains(num5))
				{
					CellPos cellPos = this.UnpackCellKey(num5);
					if (cellPos.X < num || cellPos.X > num2 || cellPos.Z < num3 || cellPos.Z > num4)
					{
						this.UnloadScene(num5);
					}
				}
			}
		}

		public void Move(CellPos movement)
		{
			this._movement = movement;
			foreach (int num in this._loaded.Keys)
			{
				this.UpdateScenePosition(num);
			}
		}

		private void UpdateScenePosition(int cellKey)
		{
			if (cellKey != this._curUnloadingScene)
			{
				GameObject gameObject = this._loaded[cellKey].GetRootGameObjects()[0];
				CellPos cellPos = this.UnpackCellKey(cellKey);
				gameObject.transform.position = LayerHelper.GetCellCornerPosition(cellPos - this._movement, this._settings);
			}
		}

		private void UnloadScene(int cellKey)
		{
			if (this._allScenes.ContainsKey(cellKey) && this._loaded.ContainsKey(cellKey))
			{
				this._toUnload.Add(cellKey);
				if (!this._isUnloaderBusy)
				{
					base.StartCoroutine(this.SceneUnLoading());
				}
			}
		}

		private IEnumerator SceneUnLoading()
		{
			this._isUnloaderBusy = true;
			while (this._toUnload.Count > 0)
			{
				int cellKey = this._toUnload[this._toUnload.Count - 1];
				this._toUnload.RemoveAt(this._toUnload.Count - 1);
				if (this._loaded.ContainsKey(cellKey))
				{
					this._curUnloadingScene = cellKey;
					this._loaded[cellKey].GetRootGameObjects()[0].SetActive(false);
					AsyncOperation request = SceneManager.UnloadSceneAsync(this._allScenes[cellKey]);
					yield return request;
					this._curUnloadingScene = -1;
					this.ESceneAction(cellKey, false);
					this._loaded.Remove(cellKey);
					this._linkedManagerRequestedActions.Remove(cellKey);
				}
			}
			this._isUnloaderBusy = false;
			yield break;
		}

		private void ProcessLinkedSceneAction(int cellKey, bool isLoaded)
		{
			GameObject gameObject = this._loaded[cellKey].GetRootGameObjects()[0];
			gameObject.SetActive(!isLoaded);
			this._linkedManagerRequestedActions.Remove(cellKey);
		}

		[Conditional("STREAMER_LOG")]
		private void LogList(string title, IEnumerable<int> list)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (int num in list)
			{
				stringBuilder.Append(this.CellKeyToString(num));
				stringBuilder.Append(", ");
			}
		}

		[Conditional("STREAMER_LOG")]
		private void Log(string msg, params object[] args)
		{
			LogHelper.Log("LayerScenes {0}: {1}", new object[]
			{
				this._settings.LayerName,
				string.Format(msg, args)
			});
		}

		private void OnDestroy()
		{
			this._scenesLoader.ESceneNearLoaded -= this.OnSceneNearLoaded;
			if (this._linkedManager != null)
			{
				this._linkedManager.ESceneAction -= this.LinkedManagerOnSceneAction;
			}
		}

		private Dictionary<int, Scene> _loaded = new Dictionary<int, Scene>();

		private List<int> _toUnload = new List<int>();

		private HashSet<int> _mustBeLoaded = new HashSet<int>();

		private HashSet<int> _waitingLoading = new HashSet<int>();

		private int _cellPackBit;

		private Dictionary<int, string> _allScenes = new Dictionary<int, string>();

		private Dictionary<string, int> _scenesToKey = new Dictionary<string, int>();

		private bool _isUnloaderBusy;

		private Dictionary<int, bool> _linkedManagerRequestedActions = new Dictionary<int, bool>();

		private int _curUnloadingScene = -1;

		private LayerData _settings;

		private CellPos _curPos;

		private CellPos _movement;

		private LayerScenes _linkedManager;

		private ScenesLoader _scenesLoader;

		private int _idInLoader;

		public delegate void SceneActionDelegate(int cellKey, bool isLoaded);
	}
}
