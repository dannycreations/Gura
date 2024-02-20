using System;
using System.Collections.Generic;
using Phy;
using UnityEngine;

public class RigidBodyController : MonoBehaviour
{
	private void Start()
	{
		this.currentCollisions = new Dictionary<int, Collision>();
	}

	private void LateUpdate()
	{
		if (GameFactory.GameIsPaused)
		{
			return;
		}
		if (this.RigidBody != null)
		{
			this.RigidBody.UpdateCollisions(this.currentCollisions, GlobalConsts.RigidBodyMask);
			base.transform.rotation = this.RigidBody.Rotation;
			if (GameFactory.Player.Rod != null)
			{
				base.transform.position += GameFactory.Player.Rod.PositionCorrection(this.RigidBody, true) - this.center.position;
			}
			else
			{
				base.transform.position += this.RigidBody.Position - this.center.position;
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject != null && collision.gameObject.layer != GlobalConsts.PhotoModeLayer)
		{
			this.currentCollisions[RigidBody.CollisionHash(collision)] = collision;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject != null && collision.gameObject.layer != GlobalConsts.PhotoModeLayer)
		{
			this.currentCollisions[RigidBody.CollisionHash(collision)] = collision;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject != null && collision.gameObject.layer != GlobalConsts.PhotoModeLayer)
		{
			List<int> list = new List<int>();
			foreach (int num in this.currentCollisions.Keys)
			{
				if (this.currentCollisions[num].gameObject == collision.gameObject)
				{
					list.Add(num);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				this.currentCollisions.Remove(list[i]);
			}
		}
	}

	public float FrictionConstant = 0.01f;

	public float TorsionalFrictionConstant = 0.005f;

	public float Width = 0.1f;

	public float Depth = 0.1f;

	public float Buoyancy = -1f;

	public float BounceFactor;

	public float ExtrudeFactor = 0.01f;

	public float CollisionFrictionFactor = 0.1f;

	public RigidBody RigidBody;

	public Transform center;

	public Transform topLineAnchor;

	public Transform bottomLineAnchor;

	private Dictionary<int, Collision> currentCollisions;
}
