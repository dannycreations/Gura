using System;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using Phy;
using UnityEngine;

public class HookController : MonoBehaviour
{
	public Hook Hook { get; set; }

	public float Mass { get; private set; }

	public float Windage { get; private set; }

	public float Resistance { get; private set; }

	public TackleBehaviour Tackle { get; set; }

	public float BaitMass { get; private set; }

	public float BaitWindage { get; private set; }

	public bool IsIdle { get; private set; }

	public Transform HandTransform
	{
		get
		{
			Transform transform = null;
			ReelTypes reelType = GameFactory.Player.ReelType;
			if (reelType != ReelTypes.Spinning)
			{
				if (reelType == ReelTypes.Baitcasting)
				{
					transform = GameFactory.Player.LinePositionInRightHand;
				}
			}
			else
			{
				transform = GameFactory.Player.LinePositionInLeftHand;
			}
			return transform;
		}
	}

	public GameObject Bait { get; set; }

	public Bait BaitItem { get; set; }

	public SpringDrivenObject HookObject { get; set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnEnterWater;

	public void Init()
	{
		if (this.Hook.Weight == null)
		{
			throw new DbConfigException("Hook weight is null!");
		}
		if (this.BaitItem.Weight == null)
		{
			throw new DbConfigException("Bait weight is null!");
		}
		if (this.hookAnchor == null)
		{
			throw new PrefabConfigException("hookAnchor is null!");
		}
		if (this.lineAnchor == null)
		{
			throw new PrefabConfigException("lineAnchor is null!");
		}
		this.Mass = (float)this.Hook.Weight.Value;
		this.Windage = this.Hook.Windage;
		this.Resistance = this.Hook.ResistanceForce;
		this.BaitMass = (float)this.BaitItem.Weight.Value;
		this.BaitWindage = this.BaitItem.Windage;
	}

	internal void Start()
	{
		this.fsm = new Fsm<HookController>("Hook", this, true);
		this.fsm.RegisterState<HookFlying>();
		this.fsm.RegisterState<HookFloating>();
		this.fsm.RegisterState<HookHanging>();
		this.fsm.RegisterState<HookShowing>();
		this.fsm.RegisterState<HookHitched>();
		this.fsm.RegisterState<HookHidden>();
		this.fsm.Start<HookHidden>();
	}

	public void OnLateUpdate()
	{
		if (this.fsm == null)
		{
			return;
		}
		this.fsm.Update();
		this.SyncWithSim();
		this.IsIdle = HookController.DetectHookMotion(this.Tackle.BottomLineAnchor.position, this.hookAnchor.position, ref this.idleCycles, ref this.priorShift);
		bool flag = base.transform.position.y > 0f;
		if (this.wasUp && !flag && this.OnEnterWater != null)
		{
			this.OnEnterWater();
		}
		this.wasUp = flag;
	}

	public static bool DetectHookMotion(Vector3 bobberPosition, Vector3 hookPosition, ref int idleCycles, ref float priorShift)
	{
		if (hookPosition.y >= 0f)
		{
			idleCycles = 0;
			return false;
		}
		Vector2 vector;
		vector..ctor(hookPosition.x, hookPosition.z);
		Vector2 vector2;
		vector2..ctor(bobberPosition.x, bobberPosition.z);
		float magnitude = (vector - vector2).magnitude;
		float num = Mathf.Abs(priorShift - magnitude);
		priorShift = magnitude;
		if (num < 0.02f)
		{
			idleCycles++;
			return idleCycles > 10;
		}
		idleCycles = 0;
		return false;
	}

	private void SyncWithSim()
	{
		if (this.HookObject == null)
		{
			base.transform.position = this.Tackle.transform.position;
			base.transform.rotation = this.Tackle.transform.rotation;
			return;
		}
		if (!this.IsFrozen)
		{
			Mass mass = this.HookObject.Masses[0];
			Mass mass2 = this.HookObject.Masses[1];
			Vector3 normalized = (mass.Position - mass2.Position).normalized;
			if (!Mathf.Approximately(normalized.magnitude, 0f))
			{
				base.transform.rotation = Quaternion.FromToRotation(Vector3.up, normalized);
			}
			if (this.Tackle.Rod != null)
			{
				base.transform.position += this.Tackle.Rod.PositionCorrection(mass2, true) - this.lineAnchor.position;
			}
		}
		else if (this.Tackle.Fish != null && this.Tackle.HasEnteredShowingState)
		{
			Vector3 headRight = this.Tackle.Fish.HeadRight;
			this.alignFrozen(this.Tackle.Fish.HeadMass, this.Tackle.KinematicHandLineMass, headRight);
		}
		else if (this.Tackle.UnderwaterItem != null && this.Tackle.HasEnteredShowingState)
		{
			UnderwaterItem1stBehaviour underwaterItem1stBehaviour = this.Tackle.UnderwaterItem.Behaviour as UnderwaterItem1stBehaviour;
			if (underwaterItem1stBehaviour != null)
			{
				this.alignFrozen(underwaterItem1stBehaviour.phyObject.ForHookMass, this.Tackle.KinematicHandLineMass, underwaterItem1stBehaviour.phyObject.ForHookMass.Rotation * Vector3.right);
			}
		}
	}

	private void alignFrozen(Mass hookedMass, Mass directedTowardsMass, Vector3 right)
	{
		Vector3 vector = this.Tackle.Rod.PositionCorrection(hookedMass, this.Tackle.IsShowingComplete);
		Vector3 vector2 = this.Tackle.Rod.PositionCorrection(directedTowardsMass, this.Tackle.IsShowingComplete);
		Vector3 normalized = (vector2 - vector).normalized;
		base.transform.rotation = Quaternion.LookRotation(normalized, right);
		base.transform.rotation = Quaternion.FromToRotation((this.lineAnchor.position - this.hookAnchor.position).normalized, normalized) * base.transform.rotation;
		Vector3 vector3 = vector - this.hookAnchor.position;
		base.transform.position += vector3;
	}

	public void ExchangeFirstMass(Mass newMass)
	{
		this.HookObject.Masses[1] = newMass;
	}

	public bool IsFrozen { get; private set; }

	public Mass Freeze(bool makeKinematic)
	{
		this.IsFrozen = true;
		if (makeKinematic)
		{
			this.HookObject.IsKinematic = true;
		}
		return this.HookObject.Masses.First<Mass>();
	}

	public void Unfreeze()
	{
		this.HookObject.IsKinematic = false;
		this.IsFrozen = false;
	}

	public void RealignMasses()
	{
		Mass mass = this.HookObject.Masses[0];
		Mass mass2 = this.HookObject.Masses[1];
		Spring spring = this.HookObject.Springs[1];
		mass2.Position = mass.Position + Vector3.down * spring.SpringLength;
	}

	public bool IsBaitShown
	{
		get
		{
			return this.isBaitShown;
		}
		set
		{
			this.isBaitShown = value;
			if (this.Bait != null)
			{
				this.Bait.SetActive(value);
				if (this.Tackle.RodSlot != null && this.Tackle.RodSlot.IsInHands)
				{
					GameFactory.Player.ChangeBaitVisibility(value);
				}
			}
		}
	}

	public void SetVisible(bool flag)
	{
		base.gameObject.SetActive(flag);
		this.IsBaitShown = flag;
	}

	public Transform hookAnchor;

	public Transform lineAnchor;

	public Transform baitAnchor;

	public float hookModelSize = 0.008f;

	private Fsm<HookController> fsm;

	private bool wasUp;

	private const int NeededIdleCycles = 10;

	private const float MaxIdleShiftChange = 0.02f;

	private float priorShift;

	private int idleCycles;

	private bool isBaitShown;
}
