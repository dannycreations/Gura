using System;
using Phy;
using UnityEngine;

public class RigidItem1stBehaviour : UnderwaterItem1stBehaviour
{
	public RigidItem1stBehaviour(RigidItemController controller, FishingRodSimulation sim = null)
		: base(controller)
	{
		this.Sim = sim ?? controller.RodSlot.Sim;
		base.phyObject = new UnderwaterItemObject(PhyObjectType.RigidItem, this.Sim);
		if (this.Controller != null && this.Controller.HookTransform != null && this.Controller.CenterTransform != null)
		{
			Mass mass = new Mass(this.Sim, 0.001f, this.Controller.HookTransform.position, Mass.MassType.Auxiliary)
			{
				Rotation = this.Controller.HookTransform.rotation,
				Buoyancy = this.Controller.Buoyancy
			};
			this.HitchMass = mass;
			base.phyObject.Masses.Add(mass);
			if (base.phyObject.ForHookMass == null)
			{
				base.phyObject.ForHookMass = mass;
			}
			Vector3 vector = this.Controller.HookTransform.localPosition - this.Controller.CenterTransform.localPosition;
			float magnitude = vector.magnitude;
			RigidBody rigidBody = new RigidBody(this.Sim, this.Controller.MassValue, this.Controller.CenterTransform.position, Mass.MassType.RigidItem, 0.0125f)
			{
				Rotation = this.Controller.CenterTransform.rotation,
				Buoyancy = this.Controller.Buoyancy,
				AirDragConstant = this.Controller.AirDragConstant,
				UnderwaterRotationDamping = 1f
			};
			rigidBody.InertiaTensor = RigidBody.SolidBoxInertiaTensor(this.Controller.MassValue, this.Controller.Width, magnitude * 2f, this.Controller.Depth);
			this.RigidBody = rigidBody;
			base.phyObject.Masses.Add(rigidBody);
			if (base.phyObject.TailMass == null)
			{
				base.phyObject.TailMass = rigidBody;
			}
			MassToRigidBodySpring massToRigidBodySpring = new MassToRigidBodySpring(mass, rigidBody, this.Controller.TorsionalFrictionConstant, (magnitude <= 0.1f) ? 0.1f : magnitude, this.Controller.FrictionConstant, vector);
			base.phyObject.Connections.Add(massToRigidBodySpring);
		}
		base.phyObject.UpdateSim();
	}

	private RigidItemController Controller
	{
		get
		{
			return this._owner as RigidItemController;
		}
	}

	public RigidBody RigidBody { get; protected set; }

	public Mass HitchMass { get; protected set; }

	public FishingRodSimulation Sim { get; protected set; }

	public override void LateUpdate()
	{
		this.Controller.transform.position = base.PositionCorrection(this.HitchMass) + this.Controller.transform.position - this.Controller.HookTransform.position;
		this.Controller.transform.rotation = this.RigidBody.Rotation;
		base.LateUpdate();
	}

	private const float hitchMassValue = 0.001f;

	private const float hitchLineLength = 0.1f;
}
