using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

[RequireComponent(typeof(PlayerTargetCloser))]
public class RodPodController : MonoBehaviour
{
	public float PlacementDY
	{
		get
		{
			return this._placementDY;
		}
	}

	public int TpmId
	{
		get
		{
			return this._tpmId;
		}
	}

	public void SetTpmId(int id)
	{
		this._tpmId = id;
	}

	public int ItemId
	{
		get
		{
			return this._itemId;
		}
	}

	public void SetItemId(int id)
	{
		this._itemId = id;
	}

	public InventoryItem Item
	{
		get
		{
			return this._item;
		}
	}

	public void SetBoat(Boat boat)
	{
		this._item = boat;
	}

	public void SetItem(RodStand item)
	{
		this._item = item;
		if (this._signals == null || this._signals.Length == 0)
		{
			return;
		}
		for (int i = 0; i < this._signals.Length; i++)
		{
			this._signals[i].SetParams(item.Bell);
		}
	}

	public bool CouldBeTaken
	{
		get
		{
			return this._couldBeTaken;
		}
	}

	public int RodsCount
	{
		get
		{
			return this._pivots.Length;
		}
	}

	public bool IsFree
	{
		get
		{
			return this._occupiedSlots.Count == 0;
		}
	}

	public bool HasFreeSlots
	{
		get
		{
			return this._occupiedSlots.Count < this.RodsCount;
		}
	}

	public bool IsFarEnough(Vector3 position)
	{
		return (base.transform.position - position).magnitude > this._minInterval;
	}

	private bool IsCorrectPosition(Vector3 putPosition)
	{
		bool flag = this._pivots.All((Transform p) => this._targetCloser.IsPossibleToAdjust(p));
		int num;
		if (flag && (this._aboveWaterAdjuster == null || this._aboveWaterAdjuster.IsAdjusted))
		{
			if (this._swivels.All((RodPodSwivel s) => s.IsContactFound) && this._isLegsLanded)
			{
				if (!this._isPunctureLanding)
				{
					num = (this._legs.All((RodPodLeg l) => l.IsContactFound) ? 1 : 0);
				}
				else
				{
					num = 1;
				}
				return num != 0;
			}
		}
		num = 0;
		return num != 0;
	}

	public RodPodPutState PutState
	{
		get
		{
			return this._putState;
		}
	}

	private void Awake()
	{
		this._outlineWidthID = Shader.PropertyToID("_OutlineWidth");
		this._outlineColorID = Shader.PropertyToID("_OutlineColor");
		LogHelper.Log("Create RodPod. Min Interval = {0}", new object[] { this._minInterval });
		this._collider = base.GetComponent<Collider>();
		this._collider.enabled = false;
		Shader shader = Shader.Find("M_Special/Bumped Specular IBL OUT");
		for (int i = 0; i < this._outlinesRoots.Length; i++)
		{
			List<Renderer> allRenderersForObject = RenderersHelper.GetAllRenderersForObject<Renderer>(this._outlinesRoots[i]);
			for (int j = 0; j < allRenderersForObject.Count; j++)
			{
				Renderer renderer = allRenderersForObject[j];
				Material[] materials = renderer.materials;
				for (int k = 0; k < materials.Length; k++)
				{
					materials[k].shader = shader;
				}
				renderer.materials = materials;
				for (int l = 0; l < materials.Length; l++)
				{
					this._outlines.Add(renderer.materials[l]);
				}
			}
		}
		this._slots = new RodPodSlot[this._pivots.Length];
		for (int m = 0; m < this._pivots.Length; m++)
		{
			Transform child = this._pivots[m].GetChild(0);
			RodPodSlot component = child.GetComponent<RodPodSlot>();
			this._slots[m] = component;
			if (this._pivots[m].childCount > 1)
			{
				component.SetupCollider(this._pivots[m].GetChild(1).GetComponent<Collider>());
			}
			component.SetActive(false);
			component.ActivateCollider(false);
		}
		float num = ((this._signals.Length <= 1) ? (RodStandSignalizer.maxPitch - RodStandSignalizer.basePitch) : ((RodStandSignalizer.maxPitch - RodStandSignalizer.basePitch) / (float)(this._signals.Length - 1)));
		for (int n = 0; n < this._signals.Length; n++)
		{
			this._signals[n].Controller = this;
			this._signals[n].MyPitch = RodStandSignalizer.basePitch + num * (float)n;
		}
		this.ActivateInclers(false);
		this._targetCloser = base.GetComponent<PlayerTargetCloser>();
		if (this._forwardLeg != null && this._backwardLeg != null)
		{
			this._legsDist = (this._forwardLeg.GroundTip.position - this._backwardLeg.GroundTip.position).magnitude;
		}
		this._meshRenderers = RenderersHelper.GetAllRenderersForObject<Renderer>(base.transform);
	}

	public void ActivateOutline(bool flag)
	{
		float num = ((!flag) ? 0f : 0.023f);
		for (int i = 0; i < this._outlines.Count; i++)
		{
			this._outlines[i].SetFloat(this._outlineWidthID, num);
		}
	}

	public void SetOutlineColor(Color color)
	{
		for (int i = 0; i < this._outlines.Count; i++)
		{
			this._outlines[i].SetColor(this._outlineColorID, color);
		}
	}

	public void SetActive(bool flag)
	{
		foreach (PodSlotData podSlotData in this._occupiedSlots.Values)
		{
			podSlotData.Reel.SetMuteSounds(!flag);
		}
		base.gameObject.SetActive(flag);
	}

	private void Start()
	{
		this.SetupCloser();
		this.ActivateInclers(false);
	}

	public void SetVisibility(bool flag)
	{
		for (int i = 0; i < this._meshRenderers.Count; i++)
		{
			this._meshRenderers[i].enabled = flag;
		}
	}

	public void SetOpaque(float prc)
	{
		for (int i = 0; i < this._meshRenderers.Count; i++)
		{
			this._meshRenderers[i].material.SetFloat("_CharacterOpaque", prc);
		}
	}

	private void SetupCloser()
	{
		if (GameFactory.Player != null)
		{
			this._targetCloser.Setup(GameFactory.Player.Collider, GameFactory.Player.CameraController.Camera.transform, new UpdatePlayerPositionAndRotationDelegate(GameFactory.Player.OnExternalMove));
			this._targetCloser.EAdjusted += GameFactory.Player.OnPositionAdjusted;
		}
	}

	public void AdjustPlayerPosition(int slot)
	{
		this._targetCloser.AdjustPlayer(this._pivots[slot]);
	}

	public void PutTpmModel(Vector3 pos, float yaw)
	{
		this.UpdatePosition(pos, yaw);
		this._collider.enabled = false;
		this._couldBeTaken = false;
		this.ActivateOutline(false);
		for (int i = 0; i < this._slots.Length; i++)
		{
			this._slots[i].SetActive(false);
			this._slots[i].ActivateCollider(false);
		}
	}

	public bool IsValidSlot(int slotId)
	{
		return slotId >= 0 && slotId < this._pivots.Length;
	}

	public void UpdatePosition(Vector3 putBasePoint, float ownerYaw)
	{
		Vector3? maskedRayContactPoint = Math3d.GetMaskedRayContactPoint(putBasePoint + new Vector3(0f, 10f, 0f), putBasePoint + new Vector3(0f, -10f, 0f), GlobalConsts.GroundObstacleMask);
		this._isLegsLanded = maskedRayContactPoint != null && maskedRayContactPoint.Value.y > this._minHeightToPut;
		Vector3 vector = ((!this._isLegsLanded) ? putBasePoint : maskedRayContactPoint.Value);
		base.transform.rotation = Quaternion.AngleAxis(ownerYaw, Vector3.up);
		base.transform.position = vector + new Vector3(0f, this.PlacementDY, 0f);
		if (!this._isLegsLanded)
		{
			return;
		}
		for (int i = 0; i < this._swivels.Length; i++)
		{
			this._swivels[i].UpdateContactPoint();
		}
		if (this._isPunctureLanding)
		{
			if (this._singleLegTip != null)
			{
				Vector3 vector2 = this._singleLegTip.forward * this._maxPunctureDY;
				RaycastHit maskedRayHit = Math3d.GetMaskedRayHit(this._singleLegTip.position - vector2, this._singleLegTip.position + vector2, GlobalConsts.GroundObstacleMask);
				if (maskedRayHit.transform != null && maskedRayHit.transform.gameObject.layer == GlobalConsts.TerrainLayer)
				{
					Vector3 vector3 = maskedRayHit.point - this._singleLegTip.position;
					base.transform.position += vector3;
					this._isLegsLanded = true;
				}
				else
				{
					this._isLegsLanded = false;
				}
			}
			else if (this._forwardLeg != null && this._backwardLeg != null)
			{
				if (!this._forwardLeg.LandTelescopicLeg(this._maxPunctureDY))
				{
					this._isLegsLanded = false;
					return;
				}
				if (!this._backwardLeg.LandTelescopicLeg(this._maxPunctureDY))
				{
					this._isLegsLanded = false;
					return;
				}
				float num = this._backwardLeg.RodTip.position.y - this._forwardLeg.RodTip.position.y;
				if (num > 0f)
				{
					float num2 = 0f;
					if (num < this._forwardLeg.MaxMove - this._forwardLeg.MovableYMovement)
					{
						this._forwardLeg.MovablePart.position += new Vector3(0f, num, 0f);
					}
					else
					{
						this._forwardLeg.MovablePart.position += new Vector3(0f, this._forwardLeg.MaxMove - this._forwardLeg.MovableYMovement, 0f);
						num = this._backwardLeg.RodTip.position.y - this._forwardLeg.RodTip.position.y;
						float num3 = Mathf.Min(new float[]
						{
							num,
							this._backwardLeg.PressureMaxMove,
							this._backwardLeg.WaterMark.position.y
						});
						num -= num3;
						this._backwardLeg.transform.position -= new Vector3(0f, num3, 0f);
						num2 = Mathf.Atan2(num, this._legsDist) * 57.29578f;
					}
					this._pivots[0].localRotation = Quaternion.AngleAxis(num2, Vector3.right);
					return;
				}
				this._isLegsLanded = false;
			}
		}
		else
		{
			for (int j = 0; j < this._legs.Length; j++)
			{
				this._legs[j].UpdateContactPoint();
			}
		}
	}

	public void UpdateIdle(bool isNotBlocked)
	{
		bool flag = isNotBlocked && this.IsCorrectPosition(base.transform.position);
		if (flag)
		{
			this._putState = RodPodPutState.Good;
		}
		else if (!isNotBlocked)
		{
			this._putState = RodPodPutState.Blocked;
		}
		else
		{
			this._putState = RodPodPutState.ContactNotFound;
		}
		this.SetOutlineColor((!flag) ? this._invalidColor : this._correctColor);
	}

	public void UpdateDeployed(bool isSelected, int slotIndex)
	{
		if (isSelected && slotIndex == -1)
		{
			this.ActivateOutline(true);
			this.SetOutlineColor(this._selectionPodColor);
			this.ActivateInclers(true);
		}
		else
		{
			this.ActivateInclers(false);
			this.ActivateOutline(false);
			for (int i = 0; i < this._slots.Length; i++)
			{
				this.HighlightSlot(i, isSelected && i == slotIndex);
			}
		}
	}

	public void UpdateRodsBeforeSim()
	{
		foreach (PodSlotData podSlotData in this._occupiedSlots.Values)
		{
			podSlotData.Rod.Behaviour.LateUpdateBeforeSim();
		}
	}

	public void UpdateSim()
	{
		foreach (PodSlotData podSlotData in this._occupiedSlots.Values)
		{
			if (!podSlotData.Rod.Behaviour.RodSlot.PendingServerOp)
			{
				podSlotData.Rod.Behaviour.OnLateUpdate();
			}
		}
	}

	public void UpdateRodsAfterSim()
	{
		foreach (PodSlotData podSlotData in this._occupiedSlots.Values)
		{
			podSlotData.Rod.Behaviour.LateUpdateAfterSim();
		}
	}

	private void HighlightSlot(int i, bool flag)
	{
		if (this._slots[i].IsActive)
		{
			this._slots[i].SetSlotVisibililty(flag);
		}
		else if (!this._occupiedSlots.ContainsKey(i))
		{
			LogHelper.Error("Invalid slot {0}", new object[] { i });
		}
		else
		{
			this._occupiedSlots[i].Outline.ActivateOutline(flag);
			if (flag)
			{
				this._occupiedSlots[i].Outline.SetOutlineColor(this._selectionRodColor);
			}
		}
	}

	public void OnPut(bool couldBeTaken = true)
	{
		if (!couldBeTaken)
		{
			this.SetupCloser();
		}
		this._collider.enabled = true;
		this._couldBeTaken = couldBeTaken;
		this.ActivateOutline(false);
		for (int i = 0; i < this._slots.Length; i++)
		{
			this._slots[i].SetActive(true);
			this._slots[i].SetSlotVisibililty(false);
			this._slots[i].ActivateCollider(true);
		}
	}

	public void OnPickUp()
	{
		this._collider.enabled = false;
		this.ActivateInclers(false);
		this.ActivateOutline(true);
		for (int i = 0; i < this._slots.Length; i++)
		{
			this._slots[i].SetActive(false);
			this._slots[i].ActivateCollider(false);
		}
	}

	private void ActivateInclers(bool flag)
	{
		if (this._swing != null)
		{
			this._swing.SetActive(flag);
		}
		if (this._lifter != null)
		{
			this._lifter.SetActive(flag);
		}
	}

	public void PrepareTransitionData(int slot, ReelBehaviour reel, TackleBehaviour tackle, LineBehaviour line, RodOnPodBehaviour.TransitionData transitionData)
	{
		Vector3 vector = this._pivots[slot].rotation * Vector3.forward;
		transitionData.rootPosition = this._pivots[slot].position;
		transitionData.rootRotation = this._pivots[slot].rotation;
		transitionData.tipPosition = this._pivots[slot].position + vector * transitionData.rodLength;
		transitionData.tackleObject = tackle.gameObject;
		transitionData.hookObject = ((!(tackle.Hook != null)) ? null : tackle.Hook.gameObject);
		transitionData.mainLineObject = line.mainLineObject;
		transitionData.lineOnRodObject = line.rodLineObject;
		transitionData.Signalizer = ((this._signals.Length <= slot) ? null : this._signals[slot]);
		transitionData.leaderLineObject = line.leaderLineObject;
	}

	public RodStandSignalizer GetSignalizer(int slot)
	{
		return (this._signals.Length <= slot) ? null : this._signals[slot];
	}

	public void OccupySlot(GameFactory.RodSlot rodSlot, int slot, RodController rod, ReelBehaviour reel, TackleBehaviour tackle, LineBehaviour line, BellBehaviour bell, RodOnPodBehaviour.TransitionData transitionData)
	{
		if (!this.HasFreeSlots)
		{
			LogHelper.Error("No one free slot found", new object[0]);
			return;
		}
		rodSlot.SetRod(rod.Behaviour);
		rodSlot.SetReel(reel);
		rodSlot.SetTackle(tackle);
		rodSlot.SetLine(line);
		rodSlot.SetBell(bell);
		this._slots[slot].SetActive(false);
		rod.transform.parent = this._pivots[slot];
		rod.transform.localPosition = this.GetRodLocalPosition(slot, rod.BackTipDistance);
		rod.transform.localRotation = RodInitialize.GetIdentityRotation(rod.Behaviour.IsBaitcasting);
		rod.Behaviour.ActivateOutline(false);
		this._occupiedSlots.Add(slot, new PodSlotData
		{
			Rod = rod,
			Tackle = tackle,
			Line = line,
			Reel = reel,
			Bell = bell,
			ReelType = reel.ReelType
		});
	}

	private Vector3 GetRodLocalPosition(int slot, float backTipDistance)
	{
		if (GameFactory.Player.IsSailing)
		{
			return Vector3.zero;
		}
		if (this._isBackTipPivot)
		{
			return new Vector3(0f, 0f, backTipDistance);
		}
		if (this._lifter != null)
		{
			return new Vector3(0f, 0f, this._lifter.RodMovement);
		}
		float num = ((slot >= this._swivels.Length) ? 0f : (backTipDistance - this._swivels[slot].DistFromRootToBackTip + 0.16f));
		return new Vector3(0f, 0f, num);
	}

	public Transform CreatePutRodTarget(byte slot, float backTipDistance)
	{
		Transform transform = new GameObject("putRodIKTarget").transform;
		transform.parent = this._pivots[(int)slot];
		transform.localRotation = Quaternion.identity;
		transform.localPosition = this.GetRodLocalPosition((int)slot, backTipDistance);
		return transform;
	}

	public float GetPlayerInteractionHeight(byte slot)
	{
		return this._pivots[(int)slot].transform.position.y + this._targetCloser.Dy;
	}

	public bool IsSlotOccupied(int slot)
	{
		return this._occupiedSlots.ContainsKey(slot);
	}

	public RodPodSlot GetSlotObj(int slot)
	{
		return (slot < 0 || slot >= this._slots.Length) ? null : this._slots[slot];
	}

	public int FindSlotByCollidedTransform(Transform t)
	{
		for (int i = 0; i < this._slots.Length; i++)
		{
			if (this._slots[i].Collider.transform == t)
			{
				return i;
			}
		}
		return -1;
	}

	public void RemoveAllRods()
	{
		while (this._occupiedSlots.Count > 0)
		{
			KeyValuePair<int, PodSlotData> keyValuePair = this._occupiedSlots.First<KeyValuePair<int, PodSlotData>>();
			GameFactory.Player.OnTakeRodFromPod(keyValuePair.Value.Rod.Behaviour.RodOnPodTpmId);
			PhotonConnectionFactory.Instance.GetGameSlot(keyValuePair.Value.Rod.Behaviour.RodSlot.Index).Adapter.FinishGameAction();
			keyValuePair.Value.Rod.Behaviour.Clean();
			Object.Destroy(keyValuePair.Value.Rod.gameObject);
			Object.Destroy(keyValuePair.Value.Reel.gameObject);
			Object.Destroy(keyValuePair.Value.Tackle.gameObject);
			Object.Destroy(keyValuePair.Value.Line.gameObject);
			if (keyValuePair.Value.Bell != null)
			{
				Object.Destroy(keyValuePair.Value.Bell.gameObject);
			}
			this.LeaveSlot(keyValuePair.Key);
		}
	}

	public PodSlotData LeaveSlot(int slot)
	{
		this._slots[slot].SetActive(true);
		PodSlotData podSlotData = this._occupiedSlots[slot];
		podSlotData.Outline.ActivateOutline(false);
		this._occupiedSlots.Remove(slot);
		return podSlotData;
	}

	public int FindSlotByRodSlot(int rodSlot)
	{
		for (int i = 0; i < this._occupiedSlots.Count; i++)
		{
			int num = this._occupiedSlots.Keys[i];
			if (this._occupiedSlots[num].Rod.Behaviour.RodSlot.Index == rodSlot)
			{
				return num;
			}
		}
		return -1;
	}

	public int FindFreeSlot()
	{
		if (this._occupiedSlots.Count == 0 || this._occupiedSlots.Keys[0] != 0)
		{
			return 0;
		}
		for (int i = 0; i < this._occupiedSlots.Count - 1; i++)
		{
			if (this._occupiedSlots.Keys[i + 1] - this._occupiedSlots.Keys[i] != 1)
			{
				return this._occupiedSlots.Keys[i] + 1;
			}
		}
		return this._occupiedSlots.Keys[this._occupiedSlots.Count - 1] + 1;
	}

	public PodSlotData GetOccupiedSlotData(int slot)
	{
		return this._occupiedSlots[slot];
	}

	public void Clean()
	{
		if (!this._isCleaned && GameFactory.Player != null)
		{
			this._targetCloser.EAdjusted -= GameFactory.Player.OnPositionAdjusted;
		}
		this._isCleaned = true;
	}

	private void OnDestroy()
	{
		this.Clean();
	}

	private readonly Color _selectionRodColor = Color.yellow;

	private readonly Color _selectionPodColor = Color.yellow;

	private readonly Color _invalidColor = Color.red;

	private readonly Color _correctColor = Color.green;

	private const float OUTLINE_WIDTH = 0.023f;

	[SerializeField]
	private Transform[] _pivots;

	[SerializeField]
	private RodPodLeg[] _legs;

	[SerializeField]
	private Transform[] _outlinesRoots;

	[SerializeField]
	private RodStandSignalizer[] _signals;

	[SerializeField]
	private Transform[] _hitches;

	[Tooltip("Min distance to another rod pod")]
	[SerializeField]
	private float _minInterval;

	[SerializeField]
	private float _placementDY;

	[SerializeField]
	private float _minHeightToPut = -0.1f;

	[SerializeField]
	private bool _isPunctureLanding;

	[SerializeField]
	private Transform _singleLegTip;

	[SerializeField]
	private float _minPunctureDY = 0.05f;

	[SerializeField]
	private float _maxPunctureDY = 0.3f;

	[SerializeField]
	private bool _isBackTipPivot;

	[Tooltip("Should be in the same order as _pivots!")]
	[SerializeField]
	private RodPodSwivel[] _swivels;

	[SerializeField]
	private AboveWaterTransformAdjuster _aboveWaterAdjuster;

	[SerializeField]
	private RodPodSwing _swing;

	[SerializeField]
	private RodPodLifter _lifter;

	[SerializeField]
	private TelescopicLeg _forwardLeg;

	[SerializeField]
	private TelescopicLeg _backwardLeg;

	private bool _isCleaned;

	private int _tpmId;

	private int _itemId;

	private InventoryItem _item;

	private bool _isBoat;

	private bool _couldBeTaken;

	private bool _isLegsLanded;

	private float _legsDist;

	private SortedList<int, PodSlotData> _occupiedSlots = new SortedList<int, PodSlotData>();

	private List<Material> _outlines = new List<Material>();

	private RodPodSlot[] _slots;

	private RodPodPutState _putState;

	private PlayerTargetCloser _targetCloser;

	private List<Renderer> _meshRenderers;

	private Collider _collider;

	private int _outlineWidthID;

	private int _outlineColorID;
}
