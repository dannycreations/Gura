using System;
using System.Collections.Generic;
using Phy;
using UnityEngine;

public class Plant1stBehaviour : UnderwaterItem1stBehaviour
{
	public Plant1stBehaviour(PlantController controller, FishingRodSimulation sim = null)
		: base(controller)
	{
		this.Sim = sim ?? controller.RodSlot.Sim;
		base.phyObject = new PlantObject(this.Sim);
		this.nodes = new List<Plant1stBehaviour.Node>();
		if (controller != null)
		{
			this.rootTransform = controller.HookTransform;
			if (this.rootTransform == null)
			{
				for (int i = 0; i < controller.transform.childCount; i++)
				{
					Transform child = controller.transform.GetChild(i);
					if (child.name == "hook" || child.name == "branch")
					{
						this.rootTransform = child;
						break;
					}
				}
			}
		}
		float num = this.TakeBranch(this.rootTransform, null, 0f);
		if (base.phyObject.RootDirection == Vector3.zero)
		{
			base.phyObject.RootDirection = Vector3.down;
		}
		if (num > 0f)
		{
			float num2 = this.Controller.MassValue / num;
			foreach (Mass mass in base.phyObject.Masses)
			{
				mass.MassValue *= num2;
			}
		}
		base.phyObject.UpdateSim();
	}

	private PlantController Controller
	{
		get
		{
			return this._owner as PlantController;
		}
	}

	public PlantObject PhyPlant
	{
		get
		{
			return base.phyObject as PlantObject;
		}
	}

	public FishingRodSimulation Sim { get; protected set; }

	public void AddNode(Transform t, Mass m)
	{
		Plant1stBehaviour.Node node = new Plant1stBehaviour.Node
		{
			transform = t,
			mass = m,
			relativeRotation = t.rotation
		};
		this.nodes.Add(node);
	}

	protected float TakeBranch(Transform branchTransform, Mass priorMass = null, float priorLength = 0f)
	{
		Mass mass = new Mass(this.Sim, 0f, branchTransform.position, Mass.MassType.Plant)
		{
			Rotation = branchTransform.rotation,
			Buoyancy = this.Controller.Buoyancy,
			AirDragConstant = this.Controller.AirDragConstant
		};
		base.phyObject.Masses.Add(mass);
		if (this.Controller.AddHitchMass)
		{
			if (priorMass == null)
			{
				Vector3 position = branchTransform.position;
				position.y += 0.004f;
				Mass mass2 = new Mass(this.Sim, 0.01f, position, Mass.MassType.Auxiliary)
				{
					Rotation = branchTransform.rotation,
					Buoyancy = this.Controller.Buoyancy
				};
				base.phyObject.Masses.Add(mass2);
				base.phyObject.ForHookMass = mass2;
				Bend bend = new Bend(mass2, mass, this.Controller.BendConstant, this.Controller.SpringConstant, this.Controller.FrictionConstant, branchTransform.position - position, 0.004f);
				base.phyObject.Connections.Add(bend);
				base.phyObject.RootDirection = Vector3.down;
			}
		}
		else if (priorMass == null)
		{
			if (base.phyObject.ForHookMass == null)
			{
				base.phyObject.ForHookMass = mass;
			}
		}
		else if (priorMass == base.phyObject.ForHookMass)
		{
			base.phyObject.RootDirection = (mass.Position - priorMass.Position).normalized;
		}
		this.AddNode(branchTransform, mass);
		float num = 0f;
		float num2;
		if (branchTransform.childCount < 1)
		{
			mass.AirDragConstant += this.Controller.AirDragConstant;
			num2 = priorLength;
			if (base.phyObject.TailMass == null)
			{
				base.phyObject.TailMass = mass;
			}
		}
		else
		{
			num2 = priorLength * 0.5f;
			for (int i = 0; i < branchTransform.childCount; i++)
			{
				Transform child = branchTransform.GetChild(i);
				float magnitude = (branchTransform.position - child.position).magnitude;
				num2 += magnitude * 0.5f;
				num += this.TakeBranch(child, mass, magnitude);
			}
		}
		mass.MassValue = num2;
		if (priorMass != null)
		{
			Bend bend2 = new Bend(priorMass, mass, this.Controller.BendConstant, this.Controller.SpringConstant, this.Controller.FrictionConstant, branchTransform.position - priorMass.Position, priorLength);
			base.phyObject.Connections.Add(bend2);
		}
		return num + num2;
	}

	public override void LateUpdate()
	{
		if (this.nodes.Count > 0)
		{
			Mass mass = this.nodes[0].mass;
			Vector3 vector = Vector3.zero;
			if (this.Controller.RodSlot.Tackle != null)
			{
				vector = this.Controller.RodSlot.Tackle.HookAnchor.position - mass.Position;
			}
			this._owner.transform.position = base.PositionCorrection(mass) + vector;
			this._owner.transform.rotation = mass.Rotation;
			foreach (Plant1stBehaviour.Node node in this.nodes)
			{
				node.transform.position = base.PositionCorrection(node.mass) + vector;
				node.transform.rotation = node.mass.Rotation * node.relativeRotation;
			}
		}
		base.LateUpdate();
	}

	private const float hitchMassValue = 0.01f;

	private const float hitchLength = 0.004f;

	protected Transform rootTransform;

	protected List<Plant1stBehaviour.Node> nodes;

	protected struct Node
	{
		public Transform transform;

		public Mass mass;

		public Quaternion relativeRotation;
	}
}
