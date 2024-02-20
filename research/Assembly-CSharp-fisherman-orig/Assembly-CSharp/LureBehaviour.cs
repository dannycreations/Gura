using System;
using ObjectModel;
using UnityEngine;

public abstract class LureBehaviour : TackleBehaviour
{
	protected LureBehaviour(LureController owner, IAssembledRod rodAssembly, RodOnPodBehaviour.TransitionData transitionData = null)
		: base(owner, rodAssembly, transitionData)
	{
		base.Hook = null;
		this.leaderHitch = null;
		this.leashHitch = null;
		if (rodAssembly.RodTemplate.IsTails() || rodAssembly.RodTemplate.IsSilicon())
		{
			string text = null;
			if (rodAssembly.BaitInterface != null)
			{
				text = rodAssembly.BaitInterface.Asset;
			}
			if (text != null)
			{
				GameObject gameObject = (GameObject)Resources.Load(text, typeof(GameObject));
				if (gameObject == null)
				{
					throw new PrefabException("Can't instantiate bait '" + text + "' from asset: " + rodAssembly.HookInterface.Asset);
				}
				base.BaitObject = Object.Instantiate<GameObject>(gameObject, this.BaitTopAnchor.position, this.Owner.transform.rotation);
				this.baitController = base.BaitObject.GetComponent<BaitController>();
				base.BaitObject.transform.position += this.BaitShift;
				base.SetForward(base.BaitObject, base.BaitObject.transform.position, this.BaitTopAnchor.position - this.BaitAnchor.position);
				base.BaitObject.transform.parent = this.HookAnchor.transform;
				this._baitRenderer = RenderersHelper.GetRendererForObject(base.BaitObject.transform);
			}
		}
		string text2 = null;
		if (rodAssembly.SinkerInterface != null)
		{
			text2 = rodAssembly.SinkerInterface.Asset;
		}
		if (text2 != null)
		{
			GameObject gameObject2 = (GameObject)Resources.Load(text2, typeof(GameObject));
			if (gameObject2 == null)
			{
				throw new PrefabException("Can't instantiate sinker '" + text2 + "' from asset: " + rodAssembly.HookInterface.Asset);
			}
			this.SinkerObject = Object.Instantiate<GameObject>(gameObject2, base.TopLineAnchor.position, this.Owner.transform.rotation);
			base.Sinker = this.SinkerObject.GetComponent<RigidBodyController>();
			this._sinkerRenderer = this.SinkerObject.transform.GetComponent<MeshRenderer>();
			if (base.RodAssembly.RodTemplate == RodTemplate.TexasRig || base.RodAssembly.RodTemplate == RodTemplate.CarolinaRig)
			{
				RigidBodyController rigidBodyController = null;
				GameObject gameObject3 = (GameObject)Resources.Load("Tackle/Sinkers/RigBead/pRigBead_Red", typeof(GameObject));
				if (gameObject3 != null)
				{
					this.BeadObject = Object.Instantiate<GameObject>(gameObject3, base.Sinker.bottomLineAnchor.position, base.Sinker.transform.rotation);
					rigidBodyController = this.BeadObject.GetComponent<RigidBodyController>();
					this.BeadObject.transform.parent = base.Sinker.transform;
					this._beadRenderer = this.BeadObject.transform.GetComponent<MeshRenderer>();
				}
				if (base.RodAssembly.RodTemplate == RodTemplate.TexasRig)
				{
					Vector3 vector = base.TopLineAnchor.position;
					if (rigidBodyController != null)
					{
						Vector3 vector2 = Vector3.Scale(rigidBodyController.transform.localScale, base.Sinker.transform.localScale);
						vector += Vector3.Scale(rigidBodyController.center.localPosition - rigidBodyController.bottomLineAnchor.localPosition, vector2);
					}
					base.AttachToTackle(this.SinkerObject, this.Owner, vector, Quaternion.identity);
				}
			}
		}
		if (base.IsSwimbait)
		{
			this.SwimbaitOwner = this._owner as SwimbaitController;
		}
	}

	public LureController Owner
	{
		get
		{
			return this._owner as LureController;
		}
	}

	public Lure LureItem { get; protected set; }

	public float LeaderLength { get; set; }

	public float UserSetLeaderLength { get; set; }

	public override Transform HookAnchor
	{
		get
		{
			return this.Owner.hookAnchor;
		}
	}

	public Transform BaitTopAnchor
	{
		get
		{
			if (this.Owner.baitTopAnchor != null)
			{
				return this.Owner.baitTopAnchor;
			}
			return this.Owner.topLineAnchor;
		}
	}

	public Transform BaitAnchor
	{
		get
		{
			if (this.Owner.baitAnchor != null)
			{
				return this.Owner.baitAnchor;
			}
			return this.Owner.bottomLineAnchor;
		}
	}

	public BaitController baitController { get; protected set; }

	public Vector3 BaitShift
	{
		get
		{
			if (this.baitController != null)
			{
				return this.baitController.topLineAnchor.position - this.baitController.bottomLineAnchor.position;
			}
			return Vector3.zero;
		}
	}

	public GameObject SinkerObject { get; protected set; }

	public GameObject BeadObject { get; protected set; }

	public GameObject SmallSinkerObject { get; protected set; }

	public SwimbaitController SwimbaitOwner { get; private set; }

	public override void Destroy()
	{
		if (base.BaitObject != null)
		{
			base.BaitObject.SetActive(false);
			Object.Destroy(base.BaitObject);
			base.BaitObject = null;
		}
		if (this.BeadObject != null)
		{
			this.BeadObject.SetActive(false);
			Object.Destroy(this.BeadObject);
			this.BeadObject = null;
		}
		if (this.SinkerObject != null)
		{
			this.SinkerObject.SetActive(false);
			Object.Destroy(this.SinkerObject);
			this.SinkerObject = null;
		}
		if (this.SmallSinkerObject != null)
		{
			this.SmallSinkerObject.SetActive(false);
			Object.Destroy(this.SmallSinkerObject);
			this.SmallSinkerObject = null;
		}
		base.Destroy();
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (base.BaitObject != null)
		{
			base.BaitObject.SetActive(isActive);
		}
		if (this.BeadObject != null)
		{
			this.BeadObject.SetActive(isActive);
		}
		if (this.SinkerObject != null)
		{
			this.SinkerObject.SetActive(isActive);
		}
		if (this.SmallSinkerObject != null)
		{
			this.SmallSinkerObject.SetActive(isActive);
		}
	}

	public override void SetVisibility(bool isVisible)
	{
		base.SetVisibility(isVisible);
		if (this._sinkerRenderer != null)
		{
			this._sinkerRenderer.enabled = isVisible;
		}
		if (this._beadRenderer != null)
		{
			this._beadRenderer.enabled = isVisible;
		}
	}

	public const float SmallSinkerScale = 0.003f;

	protected SkinnedMeshRenderer _baitRenderer;

	protected MeshRenderer _sinkerRenderer;

	protected MeshRenderer _beadRenderer;
}
