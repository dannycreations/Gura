using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace FPWorldStreamer
{
	public class Streamer : MonoBehaviour
	{
		public LayerData TerrainLayer
		{
			get
			{
				return this._terrainLayer;
			}
		}

		public LayerData TerrainLodLayer
		{
			get
			{
				return this._terrainLODLayer;
			}
		}

		public float FloatingFixDist
		{
			get
			{
				return this._floatingFixDist;
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int> EStartLoading = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ESceneLoaded = delegate
		{
		};

		private void Awake()
		{
			this._scenesLoader = new ScenesLoader(this._threadsCount);
			GameObject gameObject = new GameObject("scenesManager");
			if (this._terrainLODLayer != null)
			{
				this._terrainLOD1 = gameObject.AddComponent<LayerScenes>();
				this._terrainLOD1.Init(this._terrainLODLayer, this._scenesLoader);
				this._terrainLOD1.ESceneLoaded += this.OnSceneLoaded;
			}
			this._terrain = gameObject.AddComponent<LayerScenes>();
			this._terrain.Init(this._terrainLayer, this._scenesLoader);
			this._terrain.ESceneLoaded += this.OnSceneLoaded;
			if (this._terrainLOD1 != null)
			{
				this._terrainLOD1.SetLinkedManager(this._terrain);
			}
			this._maxCellIndex = (int)Mathf.Sqrt((float)this._terrainLayer.SceneNames.Length) - 1;
		}

		private void Start()
		{
			if (this._player == null)
			{
				GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
				if (gameObject != null)
				{
					this._player = gameObject.transform;
				}
				else
				{
					LogHelper.Error("Can't find Player object. Assign it to streamer or add an object with Player tag to scene before Streamer.Start() will be called", new object[0]);
				}
			}
			this.UpdateStaticObjectsPosition();
			this.PrepareNextUnloadingCheck();
		}

		public void TeleportPlayer(Vector3 globalPos, bool isLoadingScreenVisible = false)
		{
			CellPos cellByPosition = LayerHelper.GetCellByPosition(globalPos, this._terrainLayer, default(CellPos));
			if (cellByPosition != this._lastCell)
			{
				this._isLoadingScreenVisible = isLoadingScreenVisible;
				this._player.position = globalPos;
				this._movement = new CellPos(0, 0);
				this.Update();
			}
			else
			{
				Vector3 cellCornerPosition = LayerHelper.GetCellCornerPosition(globalPos, this._terrainLayer, default(CellPos));
				this._player.position = new Vector3(this._player.position.x - cellCornerPosition.x, globalPos.y, this._player.position.z - cellCornerPosition.z);
			}
		}

		public Vector3 GetPlayerGlobalPos()
		{
			return LayerHelper.GetCellCornerPosition(this._movement, this._terrainLayer) + this._player.position;
		}

		private void Update()
		{
			this._scenesLoader.Update();
			CellPos cellPos = this.CalcCurrentCell();
			if (cellPos != this._lastCell)
			{
				this._lastCell = cellPos;
				int num = 0;
				if (this._terrainLOD1 != null)
				{
					num += this.CheckAffectedScenes(this._terrainLOD1);
				}
				num += this.CheckAffectedScenes(this._terrain);
				if (this._isLoadingScreenVisible)
				{
					this._isLoadingScreenVisible = false;
					this.EStartLoading(num);
				}
				this.PrepareNextUnloadingCheck();
			}
			if (this._nextUnloadingCheckAt > 0f && this._nextUnloadingCheckAt < Time.time)
			{
				if (this._terrainLOD1 != null)
				{
					this.RecalcDeloadingIgnores(this._terrainLOD1);
					this._terrainLOD1.CheckScenesToUnload();
				}
				this.RecalcDeloadingIgnores(this._terrain);
				this._terrain.CheckScenesToUnload();
				if (this._terrain.UnloadingIgnoreDir.X != 0 || this._terrain.UnloadingIgnoreDir.Z != 0)
				{
					this.PrepareNextUnloadingCheck();
				}
				else
				{
					this._nextUnloadingCheckAt = -1f;
				}
			}
			if (Mathf.Abs(this._player.position.x) > this._floatingFixDist || Mathf.Abs(this._player.position.z) > this._floatingFixDist)
			{
				int num2 = (int)(this._player.position.x / this._terrainLayer.XSize);
				int num3 = (int)(this._player.position.z / this._terrainLayer.ZSize);
				this._player.position -= new Vector3((float)num2 * this._terrainLayer.XSize, 0f, (float)num3 * this._terrainLayer.ZSize);
				this._movement = cellPos;
				if (this._terrainLOD1 != null)
				{
					this._terrainLOD1.Move(this._movement);
				}
				this._terrain.Move(this._movement);
				this.UpdateStaticObjectsPosition();
			}
		}

		private void UpdateStaticObjectsPosition()
		{
			Vector3 vector = -LayerHelper.GetCellCornerPosition(this._movement, this._terrain.Settings);
			for (int i = 0; i < this._staticObjects.Length; i++)
			{
				this._staticObjects[i].position = vector;
			}
		}

		private void OnSceneLoaded()
		{
			this.ESceneLoaded();
		}

		private int CheckAffectedScenes(LayerScenes manager)
		{
			int loadingCellsR = manager.Settings.LoadingCellsR;
			if (this._lastCell.X >= 0 && this._lastCell.Z >= 0)
			{
				HashSet<int> hashSet = new HashSet<int>();
				int num = Mathf.Max(this._lastCell.X - loadingCellsR, 0);
				int num2 = Mathf.Min(this._lastCell.X + loadingCellsR, this._maxCellIndex);
				int num3 = Mathf.Max(this._lastCell.Z - loadingCellsR, 0);
				int num4 = Mathf.Min(this._lastCell.Z + loadingCellsR, this._maxCellIndex);
				for (int i = num3; i <= num4; i++)
				{
					for (int j = num; j <= num2; j++)
					{
						hashSet.Add(manager.GetCellKey(new CellPos(j, i)));
					}
				}
				this.RecalcDeloadingIgnores(manager);
				return manager.LoadScenes(hashSet, this._lastCell);
			}
			return 0;
		}

		private void RecalcDeloadingIgnores(LayerScenes manager)
		{
			Vector3 vector = this.CalcCurrentCellCornerPosition();
			Vector3 vector2;
			vector2..ctor(vector.x + manager.Settings.XSize, 0f, vector.z + manager.Settings.ZSize);
			manager.UnloadingIgnoreDir = new CellPos(this.CalcDeloadingIgnoreDir(vector, vector2, 0, manager), this.CalcDeloadingIgnoreDir(vector, vector2, 2, manager));
		}

		private int CalcDeloadingIgnoreDir(Vector3 cornerPos, Vector3 cornerPos2, int coordIndex, LayerScenes manager)
		{
			if (Mathf.Abs(this._player.position[coordIndex] - cornerPos[coordIndex]) < manager.Settings.UnloadingBorderDist)
			{
				return -1;
			}
			if (Mathf.Abs(cornerPos2[coordIndex] - this._player.position[coordIndex]) < manager.Settings.UnloadingBorderDist)
			{
				return 1;
			}
			return 0;
		}

		private CellPos CalcCurrentCell()
		{
			return LayerHelper.GetCellByPosition(this._player.position, this._terrainLayer, this._movement);
		}

		private Vector3 CalcCurrentCellCornerPosition()
		{
			return LayerHelper.GetCellCornerPosition(this._player.position, this._terrainLayer, this._movement);
		}

		private void PrepareNextUnloadingCheck()
		{
			this._nextUnloadingCheckAt = Time.time + this._unloadingCheckDelay;
		}

		private void OnDestroy()
		{
			this._terrain.ESceneLoaded -= this.OnSceneLoaded;
			if (this._terrainLOD1 != null)
			{
				this._terrainLOD1.ESceneLoaded -= this.OnSceneLoaded;
			}
		}

		[Conditional("STREAMER_LOG")]
		private void Log(string msg, params object[] args)
		{
			LogHelper.Log(string.Format(msg, args));
		}

		[SerializeField]
		private LayerData _terrainLayer;

		[SerializeField]
		private LayerData _terrainLODLayer;

		[SerializeField]
		private float _floatingFixDist = 550f;

		[SerializeField]
		private float _unloadingCheckDelay = 5f;

		[SerializeField]
		private int _threadsCount = 5;

		[SerializeField]
		private Transform[] _staticObjects;

		[SerializeField]
		private Transform _player;

		private bool _isLoadingScreenVisible = true;

		private CellPos _lastCell = new CellPos(-1, -1);

		private CellPos _movement;

		private int _maxCellIndex;

		private float _nextUnloadingCheckAt = -1f;

		private LayerScenes _terrain;

		private LayerScenes _terrainLOD1;

		private ScenesLoader _scenesLoader;
	}
}
