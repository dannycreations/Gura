using System;
using Mono.Simd;
using Mono.Simd.Math;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class NewFishPhysicsTest : MonoBehaviour
{
	private void Start()
	{
		Shader.DisableKeyword("FISH_PROCEDURAL_BEND_BYPASS");
		Mesh sharedMesh = this.FishMeshObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
		Vector3[] vertices = sharedMesh.vertices;
		Color[] array = sharedMesh.colors;
		float num = vertices[0].z;
		float num2 = vertices[0].z;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (num > vertices[i].z)
			{
				num = vertices[i].z;
			}
			if (num2 < vertices[i].z)
			{
				num2 = vertices[i].z;
			}
		}
		Debug.Log(vertices.Length);
		array = new Color[vertices.Length];
		for (int j = 0; j < array.Length; j++)
		{
			float num3 = (vertices[j].z - num) / (num2 - num) + 0.001f;
			array[j] = new Color(num3, num3, num3);
		}
		this.FishMeshObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.colors = array;
		this.fishMaterial = this.FishMeshObject.GetComponent<SkinnedMeshRenderer>().material;
		this.fishMaterial.SetFloat("_LateralScale", 2.3f);
		this.bezierCurve = new BezierCurveWithTorsion(5);
		this.bezierCurve.LateralScale = new Vector2(2.3f, 2.3f);
		this.cachedTransBone = this.FishMouth.transform.InverseTransformPoint(this.TransBone.position);
		this.transBoneTParam = (this.FishMeshObject.transform.InverseTransformPoint(this.TransBone.position).z - num) / (num2 - num);
		this.TransBone.transform.parent = this.FishMouth.transform;
		this.sim = new NewFishPhysicsTest.TestSimulation();
		this.body = new VerletFishBody(this.sim, 2f, this.BezierPointsHelpers[0].transform.position, Vector3.right, Vector3.up, Vector3.forward, 0.25f, 5, 2f, 1f, Vector3.right, new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }, new float[] { 0.05f, 0.01f, 0.01f, 0.01f }, new float[] { 0.003f, 0.003f, 0.003f, 0f });
		this.fishKinematicMass = this.body.Masses[0];
		this.fishKinematicMass.IsKinematic = true;
		this.body.BendMaxAngle = this.BendMaxAngle;
		this.body.BendStiffnessMultiplier = 4f;
		this.body.BendReboundPoint = 0.3f;
		this.body.StartBendStrain(-1f);
		this.body.SetLocomotionWaveMult(0f);
		this.body.Masses[0].EnableMotionMonitor("Test_FishMass0");
	}

	private void Update()
	{
		this.sim.FlowVelocity = this.FlowVelocity.AsVector4f();
		this.sim.Buoyancy = this.Buoyancy;
		this.sim.WaterDrag = this.WaterDrag;
		this.body.IgnoreEnvForces = this.IgnoreEnvForces;
		if (this.fishKinematicMass.IsKinematic)
		{
			this.fishKinematicMass.Position = this.BezierPointsHelpers[0].transform.position;
		}
		else
		{
			this.BezierPointsHelpers[0].transform.position = this.fishKinematicMass.Position;
		}
		this.FishObject.transform.position += this.body.Masses[0].Position - this.FishMouth.transform.position;
		for (int i = 0; i < 5; i++)
		{
			this.bezierCurve.RightAxis[i] = this.body.GetSegmentRight(i);
		}
		this.bezierCurve.AnchorPoints[0] = this.FishMouth.transform.InverseTransformPoint(this.body.Masses[0].Position);
		this.bezierCurve.AnchorPoints[1] = this.FishMouth.transform.InverseTransformPoint(this.body.Masses[4].Position);
		this.bezierCurve.AnchorPoints[2] = this.FishMouth.transform.InverseTransformPoint(this.body.Masses[8].Position);
		this.bezierCurve.AnchorPoints[3] = this.FishMouth.transform.InverseTransformPoint(this.body.Masses[12].Position);
		this.bezierCurve.AnchorPoints[4] = this.FishMouth.transform.InverseTransformPoint(this.body.Masses[16].Position);
		this.bezierCurve.AnchorPoints[5] = this.FishMouth.transform.InverseTransformPoint((this.body.Masses[17].Position + this.body.Masses[18].Position + this.body.Masses[19].Position) * 0.3333f);
		this.fishMaterial.SetVector("_BezierP0", this.body.Masses[0].Position - this.FishMouth.transform.position);
		this.fishMaterial.SetVector("_BezierP1", this.body.Masses[4].Position - this.FishMouth.transform.position);
		this.fishMaterial.SetVector("_BezierP2", this.body.Masses[8].Position - this.FishMouth.transform.position);
		this.fishMaterial.SetVector("_BezierP3", this.body.Masses[12].Position - this.FishMouth.transform.position);
		this.fishMaterial.SetVector("_BezierP4", this.body.Masses[16].Position - this.FishMouth.transform.position);
		this.fishMaterial.SetVector("_BezierP5", (this.body.Masses[17].Position + this.body.Masses[18].Position + this.body.Masses[19].Position) * 0.3333f - this.FishMouth.transform.position);
		this.bezierCurve.SetT(this.transBoneTParam);
		this.TransBone.position = this.FishMouth.transform.TransformPoint(this.bezierCurve.CurvedCylinderTransform(this.cachedTransBone));
		this.TransBone.rotation = this.bezierCurve.CurvedCylinderTransformRotation(Quaternion.identity);
		this.body.BendMaxAngle = this.BendMaxAngle;
		if (this.Muscles)
		{
			this.body.StartBendStrain(-1f);
		}
		else
		{
			this.body.StopBendStrain();
		}
		if (!this.Muscles)
		{
			for (int j = 1; j < this.body.joints.Length - 1; j++)
			{
				this.body.joints[j].BendAngle = this.BendAngle;
			}
		}
		if (Input.GetKey(276))
		{
			this.Angle += Time.deltaTime * 3f;
		}
		if (Input.GetKey(275))
		{
			this.Angle -= Time.deltaTime * 3f;
		}
		this.body.RollStabilizer(Vector3.up, 0f);
		this.body.DebugDraw();
		if (Input.GetKeyDown(32))
		{
			this.body.Masses[0].IsKinematic = !this.body.Masses[0].IsKinematic;
		}
		for (int k = 0; k < 5; k++)
		{
			this.fishMaterial.SetVector("_RightAxis" + k, this.body.GetSegmentRight(k));
		}
		this.sim.DebugDraw();
	}

	private void LateUpdate()
	{
		this.sim.Update(Time.deltaTime);
		this.testWave();
	}

	private void testWave()
	{
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < 6; i++)
		{
			float num = Mathf.Sin((float)i - Time.time * 3f) * 0.3f;
			Vector3 vector2 = vector + new Vector3(Mathf.Cos(num), Mathf.Sin(num), 0f);
			Debug.DrawLine(vector, vector2, Color.green);
			vector = vector2;
		}
	}

	public Vector3 FlowVelocity;

	public bool WaterDrag;

	public bool Buoyancy;

	public bool VerticalRollStabilizer;

	public bool Muscles;

	public float BendMaxAngle;

	public float BendAngle;

	public GameObject FishObject;

	public GameObject FishMeshObject;

	public GameObject FishMouth;

	public GameObject[] BezierPointsHelpers;

	public float BallJointStiffness;

	public float Angle;

	public bool IgnoreEnvForces;

	public Transform TransBone;

	private Material fishMaterial;

	private NewFishPhysicsTest.TestSimulation sim;

	private VerletFishBody body;

	private VerletFishBody body2;

	private Mass fishKinematicMass;

	public Vector3 cachedTransBone;

	public float transBoneTParam;

	private BezierCurveWithTorsion bezierCurve;

	public class TestSimulation : ConnectedBodiesSystem
	{
		public TestSimulation()
			: base("TestSimulation")
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
