using System;
using Phy;
using UnityEngine;

public abstract class UnderwaterItemController0 : MonoBehaviour
{
	public Mass AttachmentMass { get; protected set; }

	public PhyObject phyObject { get; protected set; }

	public abstract void CreateSim(ConnectedBodiesSystem sim = null);

	public virtual void SyncWithSim()
	{
		if (this.phyNodes != null && this.phyObject != null)
		{
			for (int i = 0; i < this.phyNodes.Length; i++)
			{
				if (this.phyObject.Masses.Count <= i)
				{
					break;
				}
				if (this.phyNodes[i] != null)
				{
					this.phyNodes[i].transform.position = this.phyObject.Masses[i].Position;
					this.phyNodes[i].transform.rotation = this.phyObject.Masses[i].Rotation;
				}
			}
		}
	}

	private void Start()
	{
		if (!this.ExternalCreateSim)
		{
			this.CreateSim(null);
		}
	}

	private void Update()
	{
	}

	internal void LateUpdate()
	{
		this.SyncWithSim();
	}

	public bool ExternalCreateSim;

	public float TotalMass;

	public Transform root;

	public Transform hook;

	protected Transform[] phyNodes;

	protected Quaternion[] initialRotations;
}
