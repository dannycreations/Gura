using System;
using System.Linq;
using ObjectModel;
using Phy;
using UnityEngine;

public abstract class FeederBehaviour : TackleBehaviour, IFeederBehaviour, IFloatBehaviour, ITackleBehaviour
{
	protected FeederBehaviour(FeederController owner, IAssembledRod rodAssembly, RodOnPodBehaviour.TransitionData transitionData = null)
		: base(owner, rodAssembly, transitionData)
	{
		base.transform.localScale = Vector3.one;
		GameObject gameObject = (GameObject)Resources.Load(rodAssembly.HookInterface.Asset, typeof(GameObject));
		if (gameObject == null)
		{
			throw new PrefabException(string.Concat(new object[]
			{
				"Can't instantiate hook '",
				rodAssembly.HookInterface,
				"' from asset: ",
				rodAssembly.HookInterface.Asset
			}));
		}
		this.hookObject = Object.Instantiate<GameObject>(gameObject, new Vector3(0f, -this.leaderLength, 0f), Quaternion.identity);
		base.Hook = this.hookObject.GetComponent<HookController>();
		if (rodAssembly.BaitInterface == null)
		{
			throw new NullReferenceException("No bait on current Rod with feeder tackle");
		}
		if (base.Hook.baitAnchor == null)
		{
			throw new PrefabException("Hook.baitAnchor is null, Hook.ItemId = " + rodAssembly.HookInterface.ItemId);
		}
		GameObject gameObject2 = (GameObject)Resources.Load(rodAssembly.BaitInterface.Asset, typeof(GameObject));
		if (gameObject2 == null)
		{
			throw new PrefabException(string.Concat(new object[]
			{
				"Can't instantiate bait '",
				rodAssembly.BaitInterface,
				"' from asset: ",
				rodAssembly.HookInterface.Asset
			}));
		}
		this.baitObject = Object.Instantiate<GameObject>(gameObject2, base.Hook.baitAnchor.position, base.Hook.baitAnchor.rotation);
		Transform transform = this.baitObject.transform.Find("bait_root");
		if (transform != null)
		{
			this.baitObject.transform.position -= transform.localPosition;
		}
		this.baitObject.transform.parent = this.hookObject.transform;
		if (this._owner.topLineAnchor == null)
		{
			throw new PrefabConfigException("topLineAnchor is null!");
		}
		if (this._owner.bottomLineAnchor == null)
		{
			throw new PrefabConfigException("bottomLineAnchor is null!");
		}
		base.Size = (this._owner.topLineAnchor.position - this.Controller.CenterTransform.position).magnitude * 2f;
		this.leaderHitch = null;
		this.leashHitch = null;
		AssembledRod assembledRod = rodAssembly as AssembledRod;
		if (assembledRod != null && assembledRod.Sinker != null)
		{
			if (assembledRod.Sinker.Weight != null)
			{
				this.Controller.MassValue = (float)assembledRod.Sinker.Weight.Value;
			}
			this.Controller.SolidFactor = Mathf.Lerp(this.Controller.MassToSolidFactorLow.y, this.Controller.MassToSolidFactorHigh.y, Mathf.Clamp01((this.Controller.MassValue - this.Controller.MassToSolidFactorLow.x) / (this.Controller.MassToSolidFactorHigh.x - this.Controller.MassToSolidFactorLow.x)));
		}
		this._feederRenderer = base.transform.GetComponentInChildren<Renderer>();
		if (this.Controller.ChumObject != null)
		{
			this._chumRenderer = this.Controller.ChumObject.GetComponentInChildren<Renderer>();
		}
		this._hookRenderer = RenderersHelper.GetRendererForObject(this.hookObject.transform);
		this._baitRenderer = RenderersHelper.GetRendererForObject(this.baitObject.transform);
	}

	public FeederController Controller
	{
		get
		{
			return this._owner as FeederController;
		}
	}

	public float MassValue { get; protected set; }

	public override Transform HookAnchor
	{
		get
		{
			return base.Hook.hookAnchor;
		}
	}

	public virtual RigidBody RigidBody
	{
		get
		{
			return null;
		}
	}

	public virtual FeederTackleObject FeederObject
	{
		get
		{
			return null;
		}
	}

	public virtual float LeaderLength
	{
		get
		{
			return this.leaderLength;
		}
		set
		{
			this.leaderLength = value;
		}
	}

	public float UserSetLeaderLength
	{
		get
		{
			return this._userSetLengthLength;
		}
		set
		{
			this._userSetLengthLength = Mathf.Max(0.1f, value);
		}
	}

	public Feeding[] Feedings { get; protected set; }

	public IChum Chum
	{
		get
		{
			return this._rodAssembly.ChumInterface;
		}
	}

	public IChum[] ChumAll
	{
		get
		{
			return this._rodAssembly.ChumInterfaceAll;
		}
	}

	public bool IsFilled { get; protected set; }

	public virtual bool IsFeederLying
	{
		get
		{
			return false;
		}
	}

	public void SetFilled(bool isFilled = true)
	{
		if (this.Controller.ChumObject != null)
		{
			this.Controller.ChumObject.SetActive(isFilled && this.Chum != null);
		}
		this.FeederObject.OnSetFilled(isFilled);
		if (this.Controller.SecondaryTackleObject != null)
		{
			this.Controller.SecondaryTackleObject.SetActive(!this.FeederObject.FeederDissolved);
		}
		this.IsFilled = isFilled;
	}

	public override void SetVisibility(bool isVisible)
	{
		base.SetVisibility(isVisible);
		this._hookRenderer.enabled = isVisible;
		this._baitRenderer.enabled = isVisible;
		this._feederRenderer.enabled = isVisible;
		if (this._chumRenderer != null)
		{
			this._chumRenderer.enabled = isVisible;
		}
	}

	public virtual bool CheckPitchIsTooShort()
	{
		throw new NotImplementedException();
	}

	public virtual Mass GetKinematicLeaderMass(int index = 0)
	{
		throw new NotImplementedException();
	}

	public void HookSetFreeze(bool freeze)
	{
		if (freeze)
		{
			base.Hook.Freeze(false);
		}
		else
		{
			base.Hook.Unfreeze();
		}
	}

	public Mass GetHookMass(int index)
	{
		if (base.Hook.HookObject != null)
		{
			return base.Hook.HookObject.Masses[index];
		}
		return this.RigidBody;
	}

	public Mass GetBobberMainMass
	{
		get
		{
			return this.phyObject.Masses[1];
		}
	}

	public void SetHookMass(int index, Mass m)
	{
		base.Hook.HookObject.Masses[index] = m;
	}

	public bool HookIsIdle
	{
		get
		{
			return base.Hook.IsIdle;
		}
	}

	public Transform HookTransform
	{
		get
		{
			return base.Hook.transform;
		}
	}

	public Transform BaitTransform
	{
		get
		{
			return base.Hook.Bait.transform;
		}
	}

	public Bait BaitItem
	{
		get
		{
			return base.Hook.BaitItem;
		}
	}

	public override bool IsBaitShown
	{
		get
		{
			return base.Hook.IsBaitShown;
		}
	}

	public override bool IsThrowing
	{
		get
		{
			return this.throwData.IsThrowing;
		}
		set
		{
			this.throwData.IsThrowing = value;
		}
	}

	public override void Destroy()
	{
		if (this.hookObject != null)
		{
			this.hookObject.SetActive(false);
			Object.Destroy(this.hookObject);
			this.hookObject = null;
		}
		if (this.baitObject != null)
		{
			this.baitObject.SetActive(false);
			Object.Destroy(this.baitObject);
			this.baitObject = null;
		}
		if (this.Controller.SecondaryTackleObject != null)
		{
			this.Controller.SecondaryTackleObject.SetActive(false);
			Object.Destroy(this.Controller.SecondaryTackleObject);
			this.Controller.SecondaryTackleObject = null;
		}
		base.Destroy();
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		this.hookObject.SetActive(isActive);
		this.baitObject.SetActive(isActive);
	}

	public override void UpdateTransitionData(RodOnPodBehaviour.TransitionData transitionData)
	{
		base.UpdateTransitionData(transitionData);
		transitionData.solidFactor = this.Controller.SolidFactor;
		transitionData.axialDragFactors = this.Controller.AxialDragFactors;
		transitionData.bounceFactor = this.Controller.BounceFactor;
		transitionData.staticFrictionFactor = this.RigidBody.StaticFrictionFactor;
		transitionData.slidingFrictionFactor = this.RigidBody.SlidingFrictionFactor;
		transitionData.extrudeFactor = this.Controller.ExtrudeFactor;
	}

	private void Instance_OnThrowChumDone(Chum[] chums)
	{
		PhotonConnectionFactory.Instance.OnThrowChumDone -= this.Instance_OnThrowChumDone;
		PhotonConnectionFactory.Instance.OnThrowChumFailed -= this.Instance_OnThrowChumFailed;
		Vector3 position = base.transform.position;
		foreach (Chum chum in chums)
		{
			GameFactory.Player.AddIndependentChum(chum, position, this.RigidBody.Velocity);
		}
	}

	private void Instance_OnThrowChumFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnThrowChumDone -= this.Instance_OnThrowChumDone;
		PhotonConnectionFactory.Instance.OnThrowChumFailed -= this.Instance_OnThrowChumFailed;
	}

	public Feeding[] InitFeeding()
	{
		if (this.ChumAll != null)
		{
			if (this._rodAssembly.RodTemplate == RodTemplate.Spod)
			{
				if (this.ChumAll.Any((IChum c) => !c.WasThrown))
				{
					PhotonConnectionFactory.Instance.OnThrowChumDone += this.Instance_OnThrowChumDone;
					PhotonConnectionFactory.Instance.OnThrowChumFailed += this.Instance_OnThrowChumFailed;
					PhotonConnectionFactory.Instance.ThrowChum(this.ChumAll.Cast<Chum>().ToArray<Chum>());
					foreach (IChum chum in this.ChumAll)
					{
						chum.WasThrown = true;
					}
					return null;
				}
			}
			bool firstFeeding = this.Feedings == null;
			Guid[] prevFeedingIds = new Guid[0];
			if (!firstFeeding)
			{
				prevFeedingIds = this.Feedings.Select((Feeding f) => f.ItemId).ToArray<Guid>();
			}
			this.Feedings = this.ChumAll.Select((IChum c) => new Feeding
			{
				IsNew = ((firstFeeding || !prevFeedingIds.Contains(c.InstanceId.Value)) && !c.WasThrown),
				ItemId = c.InstanceId.Value,
				IsDestroyed = false,
				IsExpired = c.IsExpired
			}).ToArray<Feeding>();
			foreach (Feeding feeding in this.Feedings)
			{
				GameFactory.Player.AddFeeding(feeding);
			}
			foreach (IChum chum2 in this.ChumAll)
			{
				chum2.WasThrown = true;
			}
		}
		return this.Feedings;
	}

	public void UpdateFeeding(Vector3 position)
	{
		if (this.Feedings != null)
		{
			foreach (Feeding feeding in this.Feedings)
			{
				feeding.IsNew = false;
				feeding.Position = new Point3(position.x, position.y, position.z);
				GameFactory.Player.AddFeeding(feeding);
			}
		}
	}

	public void DestroyFeeding()
	{
		if (this.Feedings != null)
		{
			foreach (Feeding feeding in this.Feedings)
			{
				feeding.IsNew = false;
				feeding.IsDestroyed = true;
				GameFactory.Player.AddFeeding(feeding);
			}
			this.Feedings = null;
		}
	}

	public virtual void OnDestroy()
	{
		this._owner = null;
	}

	protected GameObject hookObject;

	protected GameObject baitObject;

	protected float leaderLength;

	private float _userSetLengthLength;

	protected Renderer _feederRenderer;

	protected Renderer _chumRenderer;

	protected SkinnedMeshRenderer _hookRenderer;

	protected SkinnedMeshRenderer _baitRenderer;
}
