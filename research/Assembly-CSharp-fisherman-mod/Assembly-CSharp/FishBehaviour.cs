using System;
using System.Collections.Generic;
using ObjectModel;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class FishBehaviour
{
	public FishBehaviour(FishController owner, IFish fishTemplate, GameFactory.RodSlot rodSlot)
	{
		this._owner = owner;
		this.RodSlot = rodSlot;
		this.fishMeshRenderer = this.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		if (this.fishMeshRenderer != null)
		{
			this.fishMaterial = this.fishMeshRenderer.material;
			this.fishMaterials = this.fishMeshRenderer.materials;
			this.furDisplacementPropertyID = Shader.PropertyToID("Displacement");
			for (int i = 0; i < this.fishMaterials.Length; i++)
			{
				if (this.fishMaterials[i].HasProperty(this.furDisplacementPropertyID))
				{
					this.furMaterial = this.fishMaterials[i];
					break;
				}
			}
			this.fishMeshTransform = this.fishMeshRenderer.gameObject.transform;
		}
		this.modelRoot.localScale = this.transform.localScale;
		this.transform.localScale = Vector3.one;
		this.bezierCurve = new BezierCurveWithTorsion(5);
		this.UpdateLength(fishTemplate.Length);
	}

	public FishController Owner
	{
		get
		{
			return this._owner;
		}
	}

	public Transform transform
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.transform;
		}
	}

	public Transform modelRoot
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.modelRoot;
		}
	}

	protected GameObject gameObject
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.gameObject;
		}
	}

	public Transform Mouth
	{
		get
		{
			return this._owner.mouth;
		}
	}

	public Transform Grip
	{
		get
		{
			return this._owner.grip;
		}
	}

	public Transform Gill
	{
		get
		{
			return this._owner.gill;
		}
	}

	public Transform Throat
	{
		get
		{
			return this._owner.throat;
		}
	}

	public PhyObject FishObject
	{
		get
		{
			return this.fishObject;
		}
	}

	public float AttackLure { get; set; }

	public float Mass { get; set; }

	public Guid InstanceGuid { get; set; }

	public int tpmId { get; set; }

	public GameFactory.RodSlot RodSlot { get; private set; }

	public float meshHeadZ { get; protected set; }

	public float meshTailZ { get; protected set; }

	public float fishChordHeadZ { get; protected set; }

	public float fishChordTailZ { get; protected set; }

	public void UpdateLength(float length)
	{
		this.Length = length;
		float num = Mathf.Abs(this.modelRoot.localScale.x / this.modelRoot.localScale.z);
		float num2 = Mathf.Abs(this.modelRoot.localScale.y / this.modelRoot.localScale.z);
		float num3 = length / this._owner.modelSize;
		this.transform.localScale = Vector3.one * num3;
		this.modelRoot.localScale = new Vector3(num, num2, 1f);
		this.bezierCurve.LateralScale = new Vector2(this.modelRoot.localScale.x, this.modelRoot.localScale.y);
	}

	public virtual void Start()
	{
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void LateUpdate()
	{
	}

	public virtual void OnDisable()
	{
	}

	public virtual void Destroy()
	{
		if (this.gameObject != null)
		{
			this.gameObject.SetActive(false);
			Object.Destroy(this.gameObject);
		}
		this._owner = null;
	}

	public void initChord()
	{
		this.fishChord = FishBehaviour.initTransformableBones(this._owner.modelRoot);
		float num;
		float num2;
		FishBehaviour.GetMeshZDimensions(this.fishMeshRenderer.sharedMesh, out num, out num2);
		if (this.fishChord[0].basePosition.z > this.fishChord[this.fishChord.Count - 1].basePosition.z)
		{
			this.meshHeadZ = num2;
			this.meshTailZ = num;
		}
		else
		{
			this.meshHeadZ = num;
			this.meshTailZ = num2;
		}
		this.fishChordHeadZ = this.fishChord[0].basePosition.z;
		this.fishChordTailZ = this.fishChord[this.fishChord.Count - 1].basePosition.z;
	}

	public static void GetMeshZDimensions(Mesh mesh, out float meshMinZ, out float meshMaxZ)
	{
		Vector3[] vertices = mesh.vertices;
		meshMinZ = vertices[0].z;
		meshMaxZ = vertices[0].z;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (meshMinZ > vertices[i].z)
			{
				meshMinZ = vertices[i].z;
			}
			if (meshMaxZ < vertices[i].z)
			{
				meshMaxZ = vertices[i].z;
			}
		}
	}

	public static List<FishBehaviour.ChordNode> initTransformableBones(Transform modelRoot)
	{
		List<FishBehaviour.ChordNode> list = new List<FishBehaviour.ChordNode>();
		Transform transform = modelRoot;
		while (transform != null)
		{
			list.Add(new FishBehaviour.ChordNode
			{
				transform = transform,
				basePosition = modelRoot.parent.InverseTransformPoint(transform.position),
				baseRotation = Quaternion.Inverse(modelRoot.parent.rotation) * transform.rotation
			});
			transform = transform.Find("bone" + list.Count);
		}
		transform = list[list.Count - 1].transform.Find("tail_mid");
		if (transform != null)
		{
			list.Add(new FishBehaviour.ChordNode
			{
				transform = transform,
				basePosition = modelRoot.parent.InverseTransformPoint(transform.position),
				baseRotation = Quaternion.Inverse(modelRoot.parent.rotation) * transform.rotation
			});
			transform = list[list.Count - 1].transform.Find("tail");
			if (transform != null)
			{
				list.Add(new FishBehaviour.ChordNode
				{
					transform = transform,
					basePosition = modelRoot.parent.InverseTransformPoint(transform.position),
					baseRotation = Quaternion.Inverse(modelRoot.parent.rotation) * transform.rotation
				});
			}
		}
		return list;
	}

	public Vector3 curveTransformPoint(Vector3 basePosition)
	{
		float num = Mathf.Min(this.fishChordHeadZ, this.fishChordTailZ);
		float num2 = Mathf.Max(this.fishChordHeadZ, this.fishChordTailZ);
		this.bezierCurve.SetT((basePosition.z - num) / (num2 - num));
		return this.bezierCurve.CurvedCylinderTransform(basePosition);
	}

	protected void updateTransformableBones()
	{
		for (int i = 0; i < this.fishChord.Count; i++)
		{
			this.bezierCurve.SetT(Mathf.Abs(this.fishChord[i].basePosition.z - this.meshHeadZ) / Mathf.Abs(this.meshHeadZ - this.meshTailZ));
			this.fishChord[i].transform.position = this.transform.TransformPoint(this.bezierCurve.CurvedCylinderTransform(this.fishChord[i].basePosition));
			Quaternion quaternion = this.bezierCurve.CurvedCylinderRotation();
			this.fishChord[i].transform.rotation = quaternion * this.transform.rotation;
		}
	}

	public const float BigFishMass = 0.906f;

	public const int FISH_SEGMENTS_COUNT = 5;

	protected FishController _owner;

	public float Length;

	public float Force = 1.5f;

	public float Speed = 1.5f;

	public float Activity = 1f;

	public FishPortrait Portrait;

	public float Stamina = 1f;

	protected BezierCurveWithTorsion bezierCurve;

	protected AbstractFishBody fishObject;

	protected Material fishMaterial;

	protected Material[] fishMaterials;

	protected SkinnedMeshRenderer fishMeshRenderer;

	protected Transform fishMeshTransform;

	protected int furDisplacementPropertyID;

	protected Material furMaterial;

	protected List<FishBehaviour.ChordNode> fishChord;

	public struct ChordNode
	{
		public Transform transform;

		public Vector3 basePosition;

		public Quaternion baseRotation;
	}
}
