using System;
using Mono.Simd;
using Mono.Simd.Math;
using Phy;
using Phy.Verlet;
using SimplexGeometry;
using UnityEngine;

public class FloatingTestRoom : PhyTestRoom
{
	public override void InitSim()
	{
		this.rb = new RigidBody(this.sim, this.BodyMass, this.rootPosition.position, Mass.MassType.Unknown, 0.0125f)
		{
			WaterY = -1000f,
			InertiaTensor = RigidBody.SolidBoxInertiaTensor(this.BodyMass, 2f, 2f, 2f),
			IgnoreEnvForces = true
		};
		this.masses = new ConstraintPointOnRigidBody[4];
		this.masses[0] = new ConstraintPointOnRigidBody(this.sim, this.rb, new Vector3(1f, 1f, 1f), Mass.MassType.Unknown)
		{
			BounceFactor = 0.2f,
			FrictionFactor = 0.1f
		};
		this.masses[1] = new ConstraintPointOnRigidBody(this.sim, this.rb, new Vector3(1f, 1f, -1f), Mass.MassType.Unknown)
		{
			BounceFactor = 0.2f,
			FrictionFactor = 0.1f
		};
		this.masses[2] = new ConstraintPointOnRigidBody(this.sim, this.rb, new Vector3(-1f, 1f, 1f), Mass.MassType.Unknown)
		{
			BounceFactor = 0.2f,
			FrictionFactor = 0.1f
		};
		this.masses[3] = new ConstraintPointOnRigidBody(this.sim, this.rb, new Vector3(-1f, 1f, -1f), Mass.MassType.Unknown)
		{
			BounceFactor = 0.2f,
			FrictionFactor = 0.1f
		};
		this.rootMass = new VerletMass(this.sim, 0.01f, this.rb.Position + Vector3.up, Mass.MassType.Unknown);
		this.sim.Masses.Add(this.rootMass);
		this.springs = new MassToRigidBodySpring[5];
		TriangularBoatHull triangularBoatHull = new TriangularBoatHull(1.25f, 5f, 0.75f, 1.1f, 0.7f, 0f);
		this.boat = new FloatingSimplexComposite(this.sim, this.BodyMass, Vector3.up * 0.25f, Mass.MassType.Boat)
		{
			WaterY = -1000f,
			IgnoreEnvForces = true,
			VolumeBody = triangularBoatHull,
			RotationDamping = 0.5f,
			WaterNoise = 50f,
			LateralWaterResistance = 100f,
			LongitudalWaterResistance = 10f
		};
		this.boat_motor = new PointOnRigidBody(this.sim, this.boat, new Vector3(0f, -0.25f, -1f), Mass.MassType.Unknown);
		this.sim.Masses.Add(this.boat);
		this.sim.Masses.Add(this.boat_motor);
		if (this.BoatConstraints)
		{
			this.sim.Masses.Add(new ConstraintPointOnRigidBody(this.sim, this.boat, triangularBoatHull.BowPoint.AsVector3(), Mass.MassType.Unknown)
			{
				BounceFactor = 0.02f,
				FrictionFactor = 0.3f
			});
			this.sim.Masses.Add(new ConstraintPointOnRigidBody(this.sim, this.boat, triangularBoatHull.BowLeft.AsVector3(), Mass.MassType.Unknown)
			{
				BounceFactor = 0.02f,
				FrictionFactor = 0.3f
			});
			this.sim.Masses.Add(new ConstraintPointOnRigidBody(this.sim, this.boat, triangularBoatHull.BowRight.AsVector3(), Mass.MassType.Unknown)
			{
				BounceFactor = 0.02f,
				FrictionFactor = 0.3f
			});
			this.sim.Masses.Add(new ConstraintPointOnRigidBody(this.sim, this.boat, triangularBoatHull.BowBottom.AsVector3(), Mass.MassType.Unknown)
			{
				BounceFactor = 0.02f,
				FrictionFactor = 0.3f
			});
			this.sim.Masses.Add(new ConstraintPointOnRigidBody(this.sim, this.boat, triangularBoatHull.SternPoint.AsVector3(), Mass.MassType.Unknown)
			{
				BounceFactor = 0.02f,
				FrictionFactor = 0.3f
			});
			this.sim.Masses.Add(new ConstraintPointOnRigidBody(this.sim, this.boat, triangularBoatHull.SternLeft.AsVector3(), Mass.MassType.Unknown)
			{
				BounceFactor = 0.02f,
				FrictionFactor = 0.3f
			});
			this.sim.Masses.Add(new ConstraintPointOnRigidBody(this.sim, this.boat, triangularBoatHull.SternRight.AsVector3(), Mass.MassType.Unknown)
			{
				BounceFactor = 0.02f,
				FrictionFactor = 0.3f
			});
			this.sim.Masses.Add(new ConstraintPointOnRigidBody(this.sim, this.boat, triangularBoatHull.SternBottom.AsVector3(), Mass.MassType.Unknown)
			{
				BounceFactor = 0.02f,
				FrictionFactor = 0.3f
			});
		}
		if (this.BoatRope)
		{
			this.rootPosition.transform.position = this.boat.LocalToWorld(triangularBoatHull.BowPoint.AsVector3());
			this.sim.Connections.Add(new MassToRigidBodySpring(this.rootMass, this.boat, this.SpringConstant, 0.2f, this.SpringFriction, triangularBoatHull.BowPoint.AsVector3()));
		}
		base.InitSim();
	}

	public override void UpdateSim()
	{
		this.boat_motor.WaterMotor = this.boat.Rotation * (Vector3.forward * this.BoatMotor + Vector3.right * this.BoatSteer);
		base.UpdateSim();
	}

	private void Start()
	{
		base.OnStart();
		Vector4f vector4f;
		vector4f..ctor(1f, 2f, 3f, 0f);
		Vector4f vector4f2;
		vector4f2..ctor(4f, 5f, 6f, 0f);
		Debug.Log("Cross3f = " + Vector4fExtensions.Cross(vector4f, vector4f2));
		Debug.Log("Cross = " + Vector3.Cross(vector4f.AsVector3(), vector4f2.AsVector3()));
		if (this.PrismMesh != null)
		{
			this.prism = new TriangularPrism(Vector4fExtensions.right, Vector4fExtensions.left, Vector4fExtensions.down, Vector4fExtensions.right + Vector4fExtensions.forward * new Vector4f(3f), Vector4fExtensions.left + Vector4fExtensions.forward * new Vector4f(3f), Vector4fExtensions.down + Vector4fExtensions.forward * new Vector4f(3f));
			Debug.LogWarning("Volume = " + this.prism.Volume);
			Debug.LogWarning("Barycenter = " + this.prism.Barycenter);
			this.PrismMesh.sharedMesh = this.boat.VolumeBody.GenerateMesh().MakeMesh();
		}
		this.tetra = new Tetrahedron(Vector4fExtensions.right, Vector4fExtensions.left, Vector4fExtensions.forward, Vector4fExtensions.down);
	}

	private void LateUpdate()
	{
		base.OnLateUpdate();
		this.BoatModel.position = this.boat.Position;
		this.BoatModel.rotation = this.boat.Rotation;
		this.PrismMesh.transform.position = this.boat.Position;
		this.PrismMesh.transform.rotation = this.boat.Rotation;
		if (this.boat.VolumeBody.cachedComplexClipBody != null)
		{
			this.Volume = this.boat.VolumeBody.cachedComplexClipBody.Volume;
		}
	}

	private void OnApplicationQuit()
	{
		base.OnQuit();
	}

	public float BodyMass;

	public float SpringConstant;

	public float SpringFriction;

	public MeshFilter PrismMesh;

	public Transform BoatModel;

	public Transform root2position;

	public float Volume;

	public bool BoatConstraints;

	public bool BoatRope;

	public float BoatMotor;

	public float BoatSteer;

	private RigidBody rb;

	private FloatingSimplexComposite boat;

	private PointOnRigidBody boat_motor;

	private ConstraintPointOnRigidBody[] masses;

	private MassToRigidBodySpring[] springs;

	private TriangularPrism prism;

	private Tetrahedron tetra;

	private TriangularBoatHull boat2;
}
