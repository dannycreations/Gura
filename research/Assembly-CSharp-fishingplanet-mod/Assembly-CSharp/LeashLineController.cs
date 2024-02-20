using System;
using System.Globalization;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeashLineController : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	private void OnEnable()
	{
		this.InitMinMaxValues();
	}

	private void InitMinMaxValues()
	{
		Leader leader = ((!(InitRods.Instance != null)) ? null : (InitRods.Instance.ActiveRod.Leader.InventoryItem as Leader));
		bool flag = leader != null && leader.ItemSubType.IsUncuttableLeader();
		this._basicLeaderLength = ((!flag) ? 0f : MeasuringSystemManager.LineLeashLength(leader.LeaderLength));
		this.SliderControl.minValue = (this._minValue = this._basicLeaderLength + MeasuringSystemManager.LineLeashMinLength);
		this.SliderControl.maxValue = (this._maxValue = this._basicLeaderLength + MeasuringSystemManager.LineLeashMaxLength);
		if (InitRods.Instance != null && InitRods.Instance.ActiveRod.Rod.InventoryItem != null)
		{
			this.SliderControl.value = (float)((int)this._basicLeaderLength) + MeasuringSystemManager.LineLeashLength(((Rod)InitRods.Instance.ActiveRod.Rod.InventoryItem).LeaderLength);
		}
		this._value = (int)this.SliderControl.value;
		this.TextControl.text = this.SliderControl.value.ToString(CultureInfo.InvariantCulture);
	}

	public void Initialize(InventoryItem rod)
	{
		bool flag = rod != null;
		InventoryItem inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.GetRodEquipment(rod as Rod).FirstOrDefault((InventoryItem x) => x.ItemType == ItemTypes.Leader);
		this.hasConsumableLeader = inventoryItem != null && inventoryItem.ItemSubType.IsCuttableLeader();
		bool flag2 = flag && (this.hasConsumableLeader || rod.ItemSubType.IsRodWithBobber());
		base.gameObject.SetActive(flag2);
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		this.BobberHandle.SetActive(!this.hasConsumableLeader);
		this.ConsumableLeaderHandler.gameObject.SetActive(this.hasConsumableLeader);
		if (this.hasConsumableLeader)
		{
			this.SliderControl.value = (float)((int)MeasuringSystemManager.LineLeashLength(((Rod)rod).LeaderLength));
			this._value = (int)this.SliderControl.value;
			this.TextControl.text = this.SliderControl.value.ToString(CultureInfo.InvariantCulture);
			this.SliderControl.maxValue = this.SliderControl.value;
			this.ConsumableLeaderHandler.Initialize(((Rod)rod).LeaderLength, 2f);
		}
		else
		{
			this.InitMinMaxValues();
		}
	}

	public void Update()
	{
		this._timer += Time.deltaTime;
		if (Math.Abs((float)this._value - this.SliderControl.value) > 0.01f)
		{
			this._timer = 0f;
			this._isChanged = true;
			this._value = (int)this.SliderControl.value;
			this.TextControl.text = this.SliderControl.value.ToString(CultureInfo.InvariantCulture);
		}
		if (this._isChanged && this._timer > 0.5f)
		{
			this._isChanged = false;
			InventoryItem inventoryItem = base.transform.parent.Find("Content").GetComponent<InventoryItemComponent>().InventoryItem;
			Rod rod = inventoryItem as Rod;
			if (rod != null)
			{
				PhotonConnectionFactory.Instance.Profile.Inventory.SetLeaderLength(rod, MeasuringSystemManager.LineLeashBackLength((float)this._value - this._basicLeaderLength));
				PhotonConnectionFactory.Instance.SetLeaderLength(rod);
				this._timer = 0f;
			}
		}
	}

	public void ShowleashLengthPanel()
	{
		Leader leader = InitRods.Instance.ActiveRod.Leader.InventoryItem as Leader;
		bool flag = leader != null && leader.ItemSubType.IsCuttableLeader();
		if (flag)
		{
			this._messageBox = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.cutLeaderLengthController);
		}
		else
		{
			this._messageBox = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.lineLeashLengthController);
		}
		this._messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this._messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this._messageBox.GetComponent<LineLeashLengthController>().ConfirmActionCalled += delegate(object obj, LineLeashLengthEventArgs e)
		{
			this.SetValue(e.LeashLength);
		};
		this._messageBox.GetComponent<LineLeashLengthController>().Init(this.SliderControl.value, this._minValue, this._maxValue, flag);
	}

	public void IncreaseClick()
	{
		this.SliderControl.value = Mathf.Min(new float[]
		{
			this.SliderControl.value + 1f,
			this._maxValue
		});
	}

	public void DecreaseClick()
	{
		this.SliderControl.value = Mathf.Max(new float[]
		{
			this.SliderControl.value - 1f,
			this._minValue
		});
	}

	public void SetValue(float value)
	{
		if (value < this._minValue)
		{
			value = this._minValue;
		}
		if (value > this._maxValue)
		{
			value = this._maxValue;
		}
		this.SliderControl.value = value;
		if (this.hasConsumableLeader)
		{
			this.ConsumableLeaderHandler.Initialize(MeasuringSystemManager.LineLeashBackLength(value), 2f);
		}
		if (GameFactory.Player != null)
		{
			InventoryItem inventoryItem = base.transform.parent.Find("Content").GetComponent<InventoryItemComponent>().InventoryItem;
			GameFactory.Player.SaveTakeRodRequest(inventoryItem as Rod, false);
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		this.SliderControl.value += (float)((int)eventData.scrollDelta.y);
	}

	public Text TextControl;

	public Slider SliderControl;

	[SerializeField]
	private GameObject BobberHandle;

	[SerializeField]
	private LeaderLineController ConsumableLeaderHandler;

	private int _value;

	private GameObject _messageBox;

	private bool hasConsumableLeader = true;

	private float _basicLeaderLength;

	private float _minValue;

	private float _maxValue;

	private float _timer;

	private bool _isChanged;
}
