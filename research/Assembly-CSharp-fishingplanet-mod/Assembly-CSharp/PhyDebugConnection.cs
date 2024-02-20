using System;
using Phy;
using UnityEngine;

public class PhyDebugConnection : MonoBehaviour
{
	public virtual void Init(ConnectionBase phyConnection)
	{
		this.phyConnection = phyConnection;
		base.name = "ConnectionBase";
		this.updateMasses();
		this.OnUpdate();
		this.destroyedFlag = false;
	}

	private void updateMasses()
	{
		if (this.phymass1 != this.phyConnection.Mass1 || this.phymass2 != this.phyConnection.Mass2)
		{
			this.phymass1 = this.phyConnection.Mass1;
			this.phymass2 = this.phyConnection.Mass2;
			this.Mass1 = PhyDebugTool.Instance.FindMass(this.phyConnection.Mass1.UID);
			this.Mass2 = PhyDebugTool.Instance.FindMass(this.phyConnection.Mass2.UID);
			this.MassesStr = ((this.phymass1 == null) ? "null" : this.phymass1.UID.ToString()) + " - " + ((this.phymass2 == null) ? "null" : this.phymass2.UID.ToString());
			this.Mass1IID = this.phyConnection.Mass1.IID;
			this.Mass2IID = this.phyConnection.Mass2.IID;
		}
	}

	public virtual void OnUpdate()
	{
		if (this.phyConnection != null)
		{
			this.updateMasses();
		}
		if (this.Mass1 != null && this.Mass2 != null)
		{
			Vector3 position = this.Mass1.transform.position;
			Vector3 vector = this.Mass2.transform.position;
			if (this.phyConnection is MassToRigidBodySpring)
			{
				vector = (this.phymass2 as RigidBody).LocalToWorld((this.phyConnection as MassToRigidBodySpring).RigidBodyAttachmentLocalPoint);
			}
			if (this.phyConnection is MassToRigidBodyEulerSpring)
			{
				vector = (this.phymass2 as RigidBody).LocalToWorld((this.phyConnection as MassToRigidBodyEulerSpring).RigidBodyAttachmentLocalPoint);
			}
			Vector3 vector2 = position - vector;
			float magnitude = vector2.magnitude;
			vector2 /= magnitude;
			if (Mathf.Approximately(vector2.magnitude, 1f))
			{
				base.transform.rotation = Quaternion.FromToRotation(Vector3.up, vector2);
			}
			base.transform.position = (position + vector) * 0.5f;
			base.transform.localScale = new Vector3(base.transform.localScale.x, magnitude * 0.5f, base.transform.localScale.z);
		}
		else
		{
			base.transform.localScale = Vector3.zero;
		}
		this.destroyedFlag = true;
	}

	public virtual void OnUpdateReplay(int cUID, int UID1, int UID2, PhyReplay.ReplaySegment.SpringData springData)
	{
		if (this.Mass1 == null || this.Mass2 == null || this.Mass1.UID != UID1 || this.Mass2.UID != UID2)
		{
			this.UID = cUID;
			this.Mass1 = PhyDebugTool.Instance.FindMass(UID1);
			this.Mass2 = PhyDebugTool.Instance.FindMass(UID2);
			this.MassesStr = ((!(this.Mass1 != null)) ? "null" : this.Mass1.UID.ToString()) + " - " + ((!(this.Mass2 != null)) ? "null" : this.Mass2.UID.ToString());
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		this.OnUpdate();
	}

	public ConnectionBase phyConnection;

	public int UID;

	public PhyDebugMass Mass1;

	public PhyDebugMass Mass2;

	public string MassesStr;

	private Mass phymass1;

	private Mass phymass2;

	public bool destroyedFlag;

	private int Mass1UID;

	private int Mass2UID;

	public int Mass1IID;

	public int Mass2IID;
}
