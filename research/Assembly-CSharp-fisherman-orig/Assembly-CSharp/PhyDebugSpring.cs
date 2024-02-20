using System;
using Phy;
using UnityEngine;

public class PhyDebugSpring : PhyDebugConnection
{
	public override void Init(ConnectionBase phyConnection)
	{
		this.phySpring = phyConnection as Spring;
		base.Init(phyConnection);
		base.name = "Spring_" + this.phySpring.UID;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (this.phySpring != null)
		{
			this.SpringConstant = this.phySpring.SpringConstant;
			this.SpringLength = this.phySpring.SpringLength;
			this.FrictionConstant = this.phySpring.FrictionConstant;
			this.IsRepulsive = this.phySpring.IsRepulsive;
			this.DeltaLength = this.phySpring.DeltaLength;
			float num = 0f;
			if (this.Mass1 != null && this.Mass1.IsTensioned)
			{
				num = 1f;
			}
			if (this.mat != null)
			{
				this.mat.SetColor("_Color", new Color(num, 0f, num));
			}
		}
	}

	public override void OnUpdateReplay(int cUID, int UID1, int UID2, PhyReplay.ReplaySegment.SpringData springData)
	{
		base.OnUpdateReplay(cUID, UID1, UID2, springData);
		this.SpringConstant = springData.Constant;
		this.SpringLength = Mathf.Abs(springData.Length);
		this.IsRepulsive = springData.Length < 0f;
		this.FrictionConstant = springData.Friction;
		this.DeltaLength = (this.Mass1.transform.position - this.Mass2.transform.position).magnitude - this.SpringLength;
	}

	private void Start()
	{
		this.mat = base.gameObject.GetComponent<MeshRenderer>().material;
	}

	private void Update()
	{
		this.OnUpdate();
	}

	private Spring phySpring;

	public float DeltaLength;

	public float SpringConstant;

	public float SpringLength;

	public float FrictionConstant;

	public bool IsRepulsive;

	private Material mat;
}
