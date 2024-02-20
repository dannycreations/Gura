using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPM
{
	[RequireComponent(typeof(MB3_MeshBaker))]
	public class MeshBakerPlayerController : MonoBehaviour
	{
		private void Start()
		{
			this._baker = base.GetComponent<MB3_MeshBaker>();
			if (this.FRAMES_BEETWEN_UPDATES > 0)
			{
				this._framesToUpdate = Random.Range(0, 10);
			}
			if (this._warmingUpObjectPrefab != null)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this._warmingUpObjectPrefab);
				gameObject.transform.SetParent(base.transform);
				this.AddModel(gameObject);
				this.GetRenderer().enabled = false;
			}
		}

		public void AddModel(GameObject modelWithMesh)
		{
			this._addOrDeleteObjectCache[0] = modelWithMesh;
			this._bakedObjects.Add(modelWithMesh);
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			this._baker.AddDeleteGameObjects(this._addOrDeleteObjectCache, null, true);
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			this._baker.Apply(null);
			float realtimeSinceStartup3 = Time.realtimeSinceStartup;
			if (this._isLogEnabled)
			{
			}
		}

		public void DelModel(GameObject deletingObject)
		{
			this._bakedObjects.Remove(deletingObject);
			this._addOrDeleteObjectCache[0] = deletingObject;
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			this._baker.AddDeleteGameObjects(null, this._addOrDeleteObjectCache, true);
			this._baker.Apply(null);
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			if (this._isLogEnabled)
			{
			}
		}

		public SkinnedMeshRenderer GetRenderer()
		{
			return this._baker.meshCombiner.targetRenderer as SkinnedMeshRenderer;
		}

		public void Clear()
		{
			if (this._baker != null)
			{
				this._bakedObjects.Clear();
				this._baker.ClearMesh();
			}
		}

		private void Update()
		{
			if (--this._framesToUpdate <= 0)
			{
				this._framesToUpdate = this.FRAMES_BEETWEN_UPDATES;
				this._baker.meshCombiner.UpdateSkinnedMeshApproximateBoundsFromBones();
			}
		}

		private void OnDestroy()
		{
			this.Clear();
			this._baker = null;
			this._warmingUpObjectPrefab = null;
		}

		[SerializeField]
		private bool _isLogEnabled;

		[SerializeField]
		private int FRAMES_BEETWEN_UPDATES = 20;

		[SerializeField]
		private GameObject _warmingUpObjectPrefab;

		private MB3_MeshBaker _baker;

		private int _framesToUpdate;

		private List<GameObject> _bakedObjects = new List<GameObject>();

		private GameObject[] _addOrDeleteObjectCache = new GameObject[1];
	}
}
