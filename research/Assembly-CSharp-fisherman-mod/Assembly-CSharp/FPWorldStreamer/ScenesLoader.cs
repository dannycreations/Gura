using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace FPWorldStreamer
{
	public class ScenesLoader
	{
		public ScenesLoader(int threadsCount)
		{
			this._threadsCount = threadsCount;
			SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.SceneManagerOnSceneLoaded);
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ScenesLoader.SceneLoadedDelegate ESceneNearLoaded = delegate
		{
		};

		public int RegisterManager()
		{
			this._queues.Add(new List<string>());
			return this._queues.Count - 1;
		}

		public bool IsLoading(int managerId, string path)
		{
			return this._inProgress.ContainsKey(path) || this._queues[managerId].Contains(path);
		}

		public void Load(int managerId, string path)
		{
			this._queues[managerId].Add(path);
		}

		public void ClearQueue(int managerId)
		{
			this._queues[managerId].Clear();
		}

		public void Update()
		{
			for (int i = 0; i < this._queues.Count; i++)
			{
				List<string> list = this._queues[i];
				while (list.Count > 0 && this._inProgress.Count < this._threadsCount)
				{
					string text = list[list.Count - 1];
					list.RemoveAt(list.Count - 1);
					this._inProgress[text] = new ScenesLoader.LoadingScene(i, SceneManager.LoadSceneAsync(text, 1));
				}
			}
		}

		private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if (this._inProgress.ContainsKey(scene.path))
			{
				ScenesLoader.LoadingScene loadingScene = this._inProgress[scene.path];
				this._inProgress.Remove(scene.path);
				this.ESceneNearLoaded(loadingScene.ManagerID, scene);
			}
		}

		public void OnDestroy()
		{
			SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.SceneManagerOnSceneLoaded);
		}

		private List<List<string>> _queues = new List<List<string>>();

		private Dictionary<string, ScenesLoader.LoadingScene> _inProgress = new Dictionary<string, ScenesLoader.LoadingScene>();

		private readonly int _threadsCount;

		private class LoadingScene
		{
			public LoadingScene(int managerId, AsyncOperation operation)
			{
				this._managerId = managerId;
				this._operation = operation;
			}

			public int ManagerID
			{
				get
				{
					return this._managerId;
				}
			}

			public float Progress
			{
				get
				{
					return this._operation.progress;
				}
			}

			public bool IsLoaded
			{
				get
				{
					return this._operation.progress == 1f;
				}
			}

			private int _managerId;

			private AsyncOperation _operation;
		}

		public delegate void SceneLoadedDelegate(int managerId, Scene scene);
	}
}
