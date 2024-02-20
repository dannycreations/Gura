using System;
using System.Collections.Generic;
using Phy;
using UnityEngine;

public class BoatCollider : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer != GlobalConsts.PhotoModeLayer && (collision.contacts[0].thisCollider == this.BoatMeshCollider || collision.contacts[0].otherCollider == this.BoatMeshCollider))
		{
			this.CurrentCollisions[RigidBody.CollisionHash(collision)] = collision;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.layer != GlobalConsts.PhotoModeLayer && (collision.contacts[0].thisCollider == this.BoatMeshCollider || collision.contacts[0].otherCollider == this.BoatMeshCollider))
		{
			this.CurrentCollisions[RigidBody.CollisionHash(collision)] = collision;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.layer != GlobalConsts.PhotoModeLayer)
		{
			List<int> list = new List<int>();
			foreach (int num in this.CurrentCollisions.Keys)
			{
				if (this.CurrentCollisions[num].gameObject == collision.gameObject)
				{
					list.Add(num);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				this.CurrentCollisions.Remove(list[i]);
			}
		}
	}

	public Dictionary<int, Collision> CurrentCollisions = new Dictionary<int, Collision>();

	public Collider BoatMeshCollider;

	public Mesh CollisionMesh;

	public float InflateWidth;
}
