using System;
using Mono.Simd;
using Mono.Simd.Math;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class VerletObjectsTest : MonoBehaviour
{
	private void Start()
	{
		this.sim = new VerletObjectsTest.TestSimulation();
		this.Item.CreateSim(this.sim);
		this.DebugTool.OverrideSim = this.sim;
	}

	private void ItemInit()
	{
		this.Item.phyObject.Masses[0].IsKinematic = true;
	}

	private void ItemUpdate()
	{
		if (this.Pivot != null)
		{
			ElasticBranchController elasticBranchController = this.Item as ElasticBranchController;
			for (int i = 0; i < this.Item.phyObject.Connections.Count; i++)
			{
				VerletBend verletBend = this.Item.phyObject.Connections[i] as VerletBend;
				if (verletBend != null)
				{
					verletBend.FrictionConstant = elasticBranchController.Fritcion;
					verletBend.RotationalStiffness = elasticBranchController.RotationalStiffness;
					verletBend.DisplacementStiffness = elasticBranchController.DisplacementStiffness;
					verletBend.NormalStiffness = elasticBranchController.NormalStiffness;
				}
				else
				{
					VerletSpring verletSpring = this.Item.phyObject.Connections[i] as VerletSpring;
					if (verletSpring != null)
					{
						verletSpring.friction.X = elasticBranchController.Fritcion;
					}
				}
			}
			this.Item.phyObject.Masses[0].Position = this.Pivot.position;
			this.Item.phyObject.Masses[0].Rotation = this.Pivot.rotation;
		}
	}

	private void Update()
	{
		if (this.Item.phyObject != null)
		{
			if (!this.itemInitialized)
			{
				this.ItemInit();
				this.itemInitialized = true;
			}
			else
			{
				this.ItemUpdate();
			}
		}
		this.sim.FlowVelocity = this.FlowVelocity.AsVector4f();
		this.sim.Buoyancy = this.Buoyancy;
		this.sim.WaterDrag = this.WaterDrag;
	}

	private void LateUpdate()
	{
		this.sim.Update(Time.deltaTime);
	}

	public Vector3 FlowVelocity;

	public bool WaterDrag;

	public bool Buoyancy;

	public Transform Pivot;

	public UnderwaterItemController0 Item;

	public PhyDebugTool DebugTool;

	private VerletObjectsTest.TestSimulation sim;

	private bool itemInitialized;

	public class TestSimulation : ConnectedBodiesSystem
	{
		public TestSimulation()
			: base("VerletObjectsTest.TestSimulation")
		{
		}

		public override void ApplyForcesToMass(Mass mass)
		{
			if (mass.IsKinematic || mass.IgnoreEnvForces)
			{
				return;
			}
			Vector4f vector4f = Vector4fExtensions.down * mass.MassValue4f * ConnectedBodiesSystem.GravityAcceleration4f;
			float num = mass.Buoyancy + 1f;
			float num2 = 0f;
			if (this.Buoyancy)
			{
				vector4f += Vector4fExtensions.up * (mass.MassValue4f * ConnectedBodiesSystem.GravityAcceleration4f * new Vector4f(num + Mathf.Clamp(num2, -100f, 100f)));
			}
			if (this.WaterDrag)
			{
				Vector4f vector4f2 = mass.Velocity4f - this.FlowVelocity;
				vector4f -= vector4f2 * mass.MassValue4f * ConnectedBodiesSystem.WaterDragConstant4f;
			}
			mass.ApplyForce(vector4f, false);
		}

		public Vector4f FlowVelocity;

		public bool WaterDrag;

		public bool Buoyancy;
	}
}
