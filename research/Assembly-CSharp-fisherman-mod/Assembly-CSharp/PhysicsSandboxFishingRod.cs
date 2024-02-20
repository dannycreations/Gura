using System;
using Phy;
using UnityEngine;

public class PhysicsSandboxFishingRod : PhysicsSandbox
{
	protected override void OnStart()
	{
		base.OnStart();
		this.simulation = new FishingRodSimulation("PhysicsSandboxFishingRod", true);
		this.rodSimulation = this.simulation as FishingRodSimulation;
		this.rodSegment = this.rodPrefab.GetComponent<RodController>().segment;
		this.floatController = this.floatPrefab.GetComponent<FloatController>();
		this.hookController = this.hookPrefab.GetComponent<HookController>();
		this.createRod();
		this.rodSimulation.AddLine(this.rodSimulation.CurrentTacklePosition - Vector3.up * 0.3f, 0f);
		this.rodSimulation.AddBobber((this.floatController.waterMark.position - this.floatController.bottomLineAnchor.position).magnitude, (this.hookController.lineAnchor.position - this.hookController.hookAnchor.position).magnitude, this.TackleMass, this.BobberBuoyancy, this.BobberSinkerMass, this.BaitMass, -1f, this.BobberSensitivity, Vector3.down);
		this.rodSimulation.RefreshObjectArrays(true);
	}

	protected override void OnLateUpdatePre()
	{
		base.OnLateUpdatePre();
		this.rodSimulation.ResetTransformCycle();
	}

	private void createRod()
	{
		this.rodSegment.weight = this.RodWeight;
		this.rodSegment.curveTest = this.RodCurveTest;
		this.rodSegment.action = this.RodAction;
		this.rodSegment.progressive = this.RodProgressive;
		this.rodSegment.AngleH = 0f;
		this.rodSegment.AngleV = 0f;
		this.rodSegment.ChainLength = 1;
		Transform transform = this.rodSegment.lastTransform;
		this.rodSegment.rodLength = 0f;
		bool flag = false;
		while (!flag)
		{
			this.rodSegment.ChainLength++;
			this.rodSegment.rodLength += (transform.position - transform.parent.position).magnitude;
			transform = transform.parent;
			flag = transform == this.rodSegment.firstTransform;
		}
		this.rodSegment.Nodes = new Transform[this.rodSegment.ChainLength];
		this.rodSegment.CumulativeLengths = new float[this.rodSegment.ChainLength];
		float num = this.rodSegment.rodLength;
		transform = this.rodSegment.lastTransform;
		for (int i = this.rodSegment.ChainLength - 1; i >= 0; i--)
		{
			this.rodSegment.Nodes[i] = transform;
			this.rodSegment.CumulativeLengths[i] = num;
			num -= (transform.position - transform.parent.position).magnitude;
			transform = transform.parent;
		}
	}

	public GameObject rodPrefab;

	public GameObject floatPrefab;

	public GameObject hookPrefab;

	public GameObject baitPrefab;

	public float TackleMass;

	public float BobberBuoyancy;

	public float BobberSinkerMass;

	public float BobberSensitivity;

	public float BaitMass;

	public float RodWeight;

	public float RodCurveTest;

	public float RodAction;

	public float RodProgressive;

	private FishingRodSimulation rodSimulation;

	private BendingSegment rodSegment;

	private FloatController floatController;

	private HookController hookController;
}
