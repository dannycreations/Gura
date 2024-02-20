using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class TPMFireworkController
	{
		public TPMFireworkController(RodBones bones)
		{
			this._bones = bones;
			this._curObject = null;
		}

		public void OnPutAction()
		{
			if (this._curObject == null)
			{
				return;
			}
			ToolController component = this._curObject.Obj.GetComponent<ToolController>();
			this._curObject.ActionAt = Time.time + component.StartDelay;
			ToolController.InitRigidbody(this._curObject.Obj.transform, this._curObject.Rigitbody);
			this._activeObjects.Enqueue(this._curObject);
			this._curObject = null;
			this._couldBeReEnabled = false;
			this._reEnableAt = Time.time + 2.5f;
		}

		public void SetVisibility(bool flag)
		{
			if (this._curObject != null)
			{
				this._curObject.Obj.SetActive(flag);
			}
		}

		public void ServerUpdate(int itemId, bool isEnabled)
		{
			if (this._reEnableAt > 0f && this._reEnableAt < Time.time)
			{
				this._reEnableAt = -1f;
				this._couldBeReEnabled = true;
			}
			if (isEnabled)
			{
				if (this._couldBeReEnabled && this._curObject == null)
				{
					ItemAssetInfo itemAssetPath = CacheLibrary.AssetsCache.GetItemAssetPath(itemId);
					GameObject gameObject = (GameObject)Resources.Load(itemAssetPath.Asset, typeof(GameObject));
					this._curObject = new TPMFireworkController.Record();
					this._curObject.ItemId = itemId;
					this._curObject.Obj = Object.Instantiate<GameObject>(gameObject);
					ToolController component = this._curObject.Obj.GetComponent<ToolController>();
					this._curObject.Rigitbody = component.rootNode.GetComponent<Rigidbody>();
					this._curObject.Rigitbody.detectCollisions = false;
					component.enabled = false;
					this._curObject.Obj.transform.parent = this._bones.RightHand.transform.parent;
					this._curObject.Obj.transform.localPosition = new Vector3(0.16f, -0.35f, 0f);
					this._curObject.Obj.transform.localRotation = new Quaternion(1f, 0f, 0f, 1f);
					this._isEnabled = true;
				}
			}
			else if (this._isEnabled)
			{
				if (this._curObject != null)
				{
					Object.Destroy(this._curObject.Obj);
					this._curObject = null;
				}
				this._isEnabled = false;
				this._couldBeReEnabled = true;
			}
			if (this._activeObjects.Count > 0 && this._activeObjects.Peek().ActionAt < Time.time)
			{
				TPMFireworkController.Record r = this._activeObjects.Dequeue();
				Firework firework = StaticUserData.FireworkItems.FirstOrDefault((InventoryItem x) => x.ItemId == r.ItemId) as Firework;
				GameObject gameObject2 = (GameObject)Resources.Load(firework.LaunchAsset, typeof(GameObject));
				if (gameObject2 == null)
				{
					throw new PrefabException(string.Format("firework: {0} prefab can't instantiate", firework.LaunchAsset));
				}
				GameObject gameObject3 = Object.Instantiate<GameObject>(gameObject2);
				gameObject3.transform.position = r.Rigitbody.transform.position;
				ToolController component2 = r.Obj.GetComponent<ToolController>();
				r.ActionAt = Time.time + component2.DestroyDelay - component2.StartDelay;
				this._destroyingObjects.Enqueue(r);
			}
			if (this._destroyingObjects.Count > 0 && this._destroyingObjects.Peek().ActionAt < Time.time)
			{
				Object.Destroy(this._destroyingObjects.Dequeue().Obj);
			}
		}

		public void Destroy()
		{
			this._isEnabled = false;
			if (this._curObject != null)
			{
				Object.Destroy(this._curObject.Obj);
				this._curObject = null;
			}
			while (this._activeObjects.Count > 0)
			{
				Object.Destroy(this._activeObjects.Dequeue().Obj);
			}
			while (this._destroyingObjects.Count > 0)
			{
				Object.Destroy(this._destroyingObjects.Dequeue().Obj);
			}
		}

		private bool _isEnabled;

		private bool _couldBeReEnabled = true;

		private RodBones _bones;

		private TPMFireworkController.Record _curObject;

		private Queue<TPMFireworkController.Record> _activeObjects = new Queue<TPMFireworkController.Record>();

		private Queue<TPMFireworkController.Record> _destroyingObjects = new Queue<TPMFireworkController.Record>();

		private float _reEnableAt = -1f;

		private class Record
		{
			public int ItemId { get; set; }

			public GameObject Obj { get; set; }

			public Rigidbody Rigitbody { get; set; }

			public float ActionAt { get; set; }
		}
	}
}
