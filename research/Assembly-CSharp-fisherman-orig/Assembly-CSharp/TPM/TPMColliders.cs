using System;
using UnityEngine;

namespace TPM
{
	public class TPMColliders : MonoBehaviour
	{
		public void Init(Transform bonesRoot)
		{
			TPMFullIKDebugSettings iksettings = TPMCharacterCustomization.Instance.IKSettings;
			this._colliders[0] = this.CreateCollider(bonesRoot.Find(iksettings.bodyIKSettings.head), 0.3f, 0.1f, new Vector3(0f, 0.1f, 0f));
			this._colliders[1] = this.CreateCollider(bonesRoot.Find(iksettings.spine[0]), 1.4f, 0.3f, new Vector3(0f, -0.15f, 0.03f));
		}

		private Collider CreateCollider(Transform root, float height, float r, Vector3 center)
		{
			CapsuleCollider capsuleCollider = root.gameObject.AddComponent<CapsuleCollider>();
			capsuleCollider.center = center;
			capsuleCollider.height = height;
			capsuleCollider.radius = r;
			capsuleCollider.isTrigger = true;
			capsuleCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			return capsuleCollider;
		}

		public Vector3? GetHitPoint(Ray ray, float dist)
		{
			for (int i = 0; i < this._colliders.Length; i++)
			{
				RaycastHit raycastHit;
				if (this._colliders[i].Raycast(ray, ref raycastHit, dist))
				{
					return new Vector3?(raycastHit.point);
				}
			}
			return null;
		}

		[SerializeField]
		private Collider[] _colliders = new Collider[2];
	}
}
