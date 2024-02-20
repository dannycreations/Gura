using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPM
{
	public class TPMMeshBakerLayer
	{
		public TPMMeshBakerLayer(MB3_MultiMeshBaker baker, IDebugLog logOwner)
		{
			this._baker = baker;
			this._logOwner = logOwner;
		}

		public void AddModel(List<GameObject> modelsWithMesh)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			this._baker.AddDeleteGameObjects(modelsWithMesh.ToArray(), null, true);
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			this._baker.Apply(null);
			float realtimeSinceStartup3 = Time.realtimeSinceStartup;
			if (this._logOwner.IsDebugLogEnabled)
			{
			}
		}

		public void AddModel(GameObject modelWithMesh)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			this._baker.AddDeleteGameObjects(new GameObject[] { modelWithMesh }, null, true);
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			this._baker.Apply(null);
			float realtimeSinceStartup3 = Time.realtimeSinceStartup;
			if (this._logOwner.IsDebugLogEnabled)
			{
			}
		}

		public void DelModel(GameObject deletingObject)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			this._baker.AddDeleteGameObjects(null, new GameObject[] { deletingObject }, true);
			this._baker.Apply(null);
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			if (this._logOwner.IsDebugLogEnabled)
			{
			}
		}

		public void Clear()
		{
			this._baker.ClearMesh();
		}

		private MB3_MultiMeshBaker _baker;

		private IDebugLog _logOwner;
	}
}
