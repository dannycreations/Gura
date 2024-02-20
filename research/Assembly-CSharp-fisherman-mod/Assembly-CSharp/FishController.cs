using System;
using System.Collections.Generic;
using System.IO;
using ObjectModel;
using Phy;
using TPM;
using UnityEngine;

[ExecuteInEditMode]
public class FishController : MonoBehaviour
{
	public Transform LeftHandRoot
	{
		get
		{
			if (this._leftHandRoot == null)
			{
				this._leftHandRoot = new GameObject("leftHandRoot").transform;
				this._leftHandRoot.parent = base.transform;
				this._leftHandRoot.localScale = Vector3.one;
			}
			return this._leftHandRoot;
		}
	}

	public Transform RightHandRoot
	{
		get
		{
			if (this._rightHandRoot == null)
			{
				this._rightHandRoot = new GameObject("rightHandRoot").transform;
				this._rightHandRoot.parent = base.transform;
				this._rightHandRoot.localScale = Vector3.one;
			}
			return this._rightHandRoot;
		}
	}

	public Transform CenterRoot
	{
		get
		{
			if (this._centerRoot == null)
			{
				this._centerRoot = new GameObject("centerHandRoot").transform;
				this._centerRoot.parent = base.transform;
				this._centerRoot.localScale = Vector3.one;
			}
			return this._centerRoot;
		}
	}

	public Transform _leftHand { get; private set; }

	public Transform _rightHand { get; private set; }

	public float Length
	{
		get
		{
			return (this._behaviour == null) ? 0f : this._behaviour.Length;
		}
	}

	private void OnEnable()
	{
		this.InitHandPoints();
	}

	private void InitHandPoints()
	{
		SkinnedMeshRenderer rendererForObject = RenderersHelper.GetRendererForObject(base.transform);
		this._vertices = rendererForObject.sharedMesh.vertices;
		this._normals = rendererForObject.sharedMesh.normals;
		this._triangles = rendererForObject.sharedMesh.triangles;
		this._spines.Clear();
		this.FillSpines(base.transform);
		this.UpdateHandRootPosition(this.LeftHandRoot, this._leftHandPrc);
		this.UpdateHandRootPosition(this.RightHandRoot, this._rightHandPrc);
		this.UpdateHandRootPosition(this.CenterRoot, this._centerHandPrc);
	}

	private void FillSpines(Transform node)
	{
		if (node.name.IndexOf("bone") == 0)
		{
			this._spines.Add(new FishController.Spine
			{
				LocalPosition = base.transform.InverseTransformPoint(node.position),
				Transform = node
			});
		}
		for (int i = 0; i < node.childCount; i++)
		{
			this.FillSpines(node.GetChild(i));
		}
	}

	public void UpdateHandRootPosition(Transform t, float prc)
	{
		if (this._behaviour != null)
		{
			float num = Mathf.Lerp(this._behaviour.meshHeadZ, this._behaviour.meshTailZ, prc);
			Vector3 bottomVertex = this.GetBottomVertex(num, Mathf.Abs(this._behaviour.meshTailZ - this._behaviour.meshHeadZ) * this._precisionRaiusK);
			t.SetParent(base.transform);
			Vector3 vector;
			vector..ctor(0f, bottomVertex.y, num);
			vector = this._behaviour.curveTransformPoint(vector);
			t.localPosition = vector;
			t.position += Vector3.up * 0.03f;
			t.localScale = Vector3.one;
		}
	}

	private void debugDrawFishBottomOutline(float leftPrc, float rightPrc)
	{
		Vector3 vector = Vector3.zero;
		float num;
		for (int i = 0; i < 100; i++)
		{
			num = Mathf.Lerp(this._behaviour.meshHeadZ, this._behaviour.meshTailZ, (float)i / 100f);
			Vector3 vector2 = this.GetBottomVertex(num, Mathf.Abs(this._behaviour.meshTailZ - this._behaviour.meshHeadZ) * this._precisionRaiusK);
			vector2.z = num;
			vector2 = base.transform.TransformPoint(this._behaviour.curveTransformPoint(vector2));
			if (i > 0)
			{
				Debug.DrawLine(vector, vector2, Color.yellow);
			}
			vector = vector2;
		}
		num = Mathf.Lerp(this._behaviour.meshHeadZ, this._behaviour.meshTailZ, leftPrc);
		Vector3 vector3 = this.GetBottomVertex(num, Mathf.Abs(this._behaviour.meshTailZ - this._behaviour.meshHeadZ) * this._precisionRaiusK);
		vector3.z = num;
		vector3 = this._behaviour.curveTransformPoint(vector3);
		vector3 = base.transform.TransformPoint(vector3);
		Debug.DrawLine(vector3, vector3 + Vector3.down * 0.1f, Color.red);
		num = Mathf.Lerp(this._behaviour.meshHeadZ, this._behaviour.meshTailZ, rightPrc);
		vector3 = this.GetBottomVertex(num, Mathf.Abs(this._behaviour.meshTailZ - this._behaviour.meshHeadZ) * this._precisionRaiusK);
		vector3.z = num;
		vector3 = this._behaviour.curveTransformPoint(vector3);
		vector3 = base.transform.TransformPoint(vector3);
		Debug.DrawLine(vector3, vector3 + Vector3.down * 0.1f, Color.blue);
	}

	public void LeanToHands(Transform leftHand, Transform rightHand)
	{
		Vector3 vector = rightHand.position - leftHand.position;
		Vector3 vector2 = leftHand.position + vector * 0.5f;
		base.transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(vector).eulerAngles.y, 0f);
		if (this._leftHand == null)
		{
			base.transform.position = new Vector3(base.transform.position.x, vector2.y, base.transform.position.z);
			this._leftHand = leftHand;
			this._rightHand = rightHand;
			this._behaviour.initChord();
			this.UpdateLength(this.Length);
			this.UpdateHandRootPosition(this.LeftHandRoot, this._leftHandPrc);
			this.UpdateHandRootPosition(this.RightHandRoot, this._rightHandPrc);
		}
		this._leftHandPrc = Mathf.Abs(base.transform.InverseTransformPoint(this._leftHand.position).z - this._behaviour.meshHeadZ) / Mathf.Abs(this._behaviour.meshHeadZ - this._behaviour.meshTailZ);
		this._rightHandPrc = Mathf.Abs(base.transform.InverseTransformPoint(this._rightHand.position).z - this._behaviour.meshHeadZ) / Mathf.Abs(this._behaviour.meshHeadZ - this._behaviour.meshTailZ);
		this.UpdateHandRootPosition(this.LeftHandRoot, this._leftHandPrc);
		this.UpdateHandRootPosition(this.CenterRoot, this._centerHandPrc);
		this.UpdateHandRootPosition(this.RightHandRoot, this._rightHandPrc);
	}

	public void UpdateLength(float length)
	{
		this._behaviour.UpdateLength(length);
		Vector3 vector = this._leftHand.position - this._rightHand.position;
		vector.y = 0f;
		float num = vector.magnitude * 0.5f;
		float num2 = num / length;
		this._leftHandPrc = Mathf.Abs(base.transform.InverseTransformPoint(this._leftHand.position).z - this._behaviour.meshHeadZ) / Mathf.Abs(this._behaviour.meshHeadZ - this._behaviour.meshTailZ);
		this._rightHandPrc = Mathf.Abs(base.transform.InverseTransformPoint(this._rightHand.position).z - this._behaviour.meshHeadZ) / Mathf.Abs(this._behaviour.meshHeadZ - this._behaviour.meshTailZ);
	}

	private Vector3 GetBottomVertex(float z, float precisionR)
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < this._triangles.Length - 2; i += 3)
		{
			Vector3 vector = this._vertices[this._triangles[i]];
			Vector3 vector2 = this._vertices[this._triangles[i + 1]];
			Vector3 vector3 = this._vertices[this._triangles[i + 2]];
			if (z >= Mathf.Min(new float[] { vector.z, vector2.z, vector3.z }) && z <= Mathf.Max(new float[] { vector.z, vector2.z, vector3.z }))
			{
				if (Math3d.PlaneNormalFrom3Points(vector, vector2, vector3).y < this._bottomVertexNormalThreshold)
				{
					float num3 = 0f;
					float num4 = 0f;
					float num5 = 0f;
					if (z >= Mathf.Min(vector.z, vector2.z) && z <= Mathf.Max(vector.z, vector2.z))
					{
						num3 = Mathf.Lerp(vector.y, vector2.y, (z - vector.z) / (vector2.z - vector.z));
					}
					if (z >= Mathf.Min(vector2.z, vector3.z) && z <= Mathf.Max(vector2.z, vector3.z))
					{
						num4 = Mathf.Lerp(vector2.y, vector3.y, (z - vector2.z) / (vector3.z - vector2.z));
					}
					if (z >= Mathf.Min(vector3.z, vector.z) && z <= Mathf.Max(vector3.z, vector.z))
					{
						num5 = Mathf.Lerp(vector3.y, vector.y, (z - vector3.z) / (vector.z - vector3.z));
					}
					num = Mathf.Min(new float[] { num, num3, num4, num5 });
				}
				else if (Mathf.Max(new float[]
				{
					Mathf.Abs(vector.x),
					Mathf.Abs(vector2.x),
					Mathf.Abs(vector3.x)
				}) > 0.005f)
				{
					num2 = Mathf.Min(new float[] { num2, vector.y, vector2.y, vector3.y });
				}
			}
		}
		float num6 = precisionR * 2f;
		if (Mathf.Approximately(num, 0f))
		{
			return new Vector3(0f, num2, z);
		}
		return new Vector3(0f, num, z);
	}

	private Transform GetNearestSpines(float z)
	{
		int num = 0;
		for (int i = 1; i < this._spines.Count; i++)
		{
			if (Mathf.Abs(this._spines[i].LocalPosition.z - z) < Mathf.Abs(this._spines[num].LocalPosition.z - z))
			{
				num = i;
			}
		}
		return this._spines[num].Transform;
	}

	public PhyObject FishObject
	{
		get
		{
			return this._behaviour.FishObject;
		}
	}

	public FishBehaviour SetBehaviour(UserBehaviours behaviourType, IFish fishTemplate, GameFactory.RodSlot slot, RodOnPodBehaviour.TransitionData td = null)
	{
		if (this._behaviour == null)
		{
			base.name = Path.GetFileName(fishTemplate.Asset);
			if (behaviourType == UserBehaviours.FirstPerson)
			{
				this._behaviour = new Fish1stBehaviour(this, fishTemplate as Fish, slot, td);
				this.PlayOptionalSound();
			}
			else if (behaviourType == UserBehaviours.ThirdPerson)
			{
				this._behaviour = new Fish3rdBehaviour(this, fishTemplate, slot);
			}
			else
			{
				this._behaviour = new FishPhotoBehaviour(this, fishTemplate, slot);
			}
		}
		return this._behaviour;
	}

	private void PlayOptionalSound()
	{
		AudioSource component = base.gameObject.GetComponent<AudioSource>();
		if (component != null)
		{
			component.enabled = true;
			component.Play();
		}
	}

	private void OnDrawGizmos()
	{
		if (this._leftHandRoot != null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(this._leftHandRoot.position, 0.01f);
		}
		if (this._rightHandRoot != null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(this._rightHandRoot.position, 0.01f);
		}
		if (this._centerRoot != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(this._centerRoot.position, 0.01f);
		}
	}

	private void Start()
	{
		this._behaviour.Start();
	}

	private void FixedUpdate()
	{
		this._behaviour.FixedUpdate();
	}

	private void LateUpdate()
	{
		this._behaviour.LateUpdate();
	}

	private void OnDisable()
	{
		if (this._behaviour != null)
		{
			this._behaviour.OnDisable();
		}
	}

	private void OnDestroy()
	{
		this._behaviour = null;
	}

	private void Update()
	{
		this._behaviour.Update();
	}

	private const float BottomVertexNormalThreshold = -0.7f;

	private const float HandToFishGap = 0.03f;

	public Transform root;

	public Transform mouth;

	public Transform gill;

	public Transform tail;

	public Transform modelRoot;

	public Transform grip;

	public Transform throat;

	public Transform[] transformableBones;

	public int updateLengthCounter;

	public float modelSize = 0.24f;

	public float GripRotation;

	public float ShakeMaxBendAngle = 0.62831855f;

	public float ShakeBendReboundPoint = 0.9999f;

	public float ShakeStiffnessMultiplier = 1.5f;

	public float LocomotionWaveAmp = 2f;

	public float LocomotionWaveFreq = 1f;

	public Vector3 LocomotionWaveAxis = Vector3.right;

	public float RollStabilizerMultiplier = 1f;

	public float leftHandLeanDelta;

	public float rightHandLeanDelta;

	[HideInInspector]
	[Range(0f, 1f)]
	[SerializeField]
	private float _leftHandPrc = 0.33f;

	[HideInInspector]
	[Range(0f, 1f)]
	[SerializeField]
	private float _rightHandPrc = 0.66f;

	[Range(0f, 1f)]
	[SerializeField]
	private float _centerHandPrc = 0.5f;

	[SerializeField]
	private float _pitch;

	[SerializeField]
	private Transform _leftHandRoot;

	[SerializeField]
	private Transform _rightHandRoot;

	[SerializeField]
	private float _holdingFishDy = -0.05f;

	private Transform _centerRoot;

	[Range(0.001f, 0.2f)]
	[SerializeField]
	public float _precisionRaiusK = 0.02f;

	[Range(-0.99f, 0f)]
	[SerializeField]
	public float _bottomVertexNormalThreshold = -0.7f;

	private Vector3[] _vertices;

	private Vector3[] _normals;

	private int[] _triangles;

	private List<FishController.Spine> _spines = new List<FishController.Spine>();

	private FishBehaviour _behaviour;

	private struct Spine
	{
		public Vector3 LocalPosition;

		public Transform Transform;
	}
}
