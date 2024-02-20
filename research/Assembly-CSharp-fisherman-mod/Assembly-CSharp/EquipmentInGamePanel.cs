using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentInGamePanel : MonoBehaviour
{
	public bool IsActive
	{
		get
		{
			return this._canvasGroup.alpha == 1f;
		}
	}

	public void UpdatePanel()
	{
		int num = 0;
		if (this._up.WasPressedMandatory || this._up.WasRepeated || this._upAdditional.WasPressedMandatory || this._upAdditional.WasRepeated)
		{
			num = 1;
		}
		if (this._down.WasPressedMandatory || this._down.WasRepeated || this._downAdditional.WasPressedMandatory || this._downAdditional.WasRepeated)
		{
			num = -1;
		}
		if (this._isInitialization && this._updateCount > 0)
		{
			this.UpdateOneStep((this._updateCount <= 0) ? (-1) : 1, true);
		}
		else if (num != 0 && this._slots.Count > 0)
		{
			this.UpdatePanel(num);
		}
	}

	private void Awake()
	{
		this._canvasGroup = base.GetComponent<CanvasGroup>();
		if (this._rodStandHelp != null)
		{
			this._rodStandHelp.SetActive(false);
			this._actionValue.text = ScriptLocalization.Get("PressCaption");
			this._actionText.text = ScriptLocalization.Get("ControllerRodpodCaption");
			this._actionIco.text = "\ue73f";
		}
	}

	public void HidePanel()
	{
		if (this._tweenSequence != null)
		{
			TweenExtensions.Kill(this._tweenSequence, false);
		}
		this._tweenSequence = TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOFade(this._canvasGroup, 0f, this._fadeDuration), delegate
		{
			for (int i = 0; i < this._slots.Count; i++)
			{
				if (this._slots[i] != null)
				{
					this._slots[i].SetActive(false);
				}
			}
		});
	}

	public int GetSelectedSlotValue()
	{
		int num = 0;
		if (this._slots.Count > this.slot_max_current)
		{
			GameObject gameObject = this._slots[this.slot_max_current];
			if (gameObject == null)
			{
				string text = string.Join(",", this._slots.Select((GameObject p) => (!(p != null)) ? "null" : p.name).ToArray<string>());
				StackTrace stackTrace = new StackTrace();
				string text2 = string.Format("_slots[slot_max_current] == null; slot_max_current:{0} obs:{1}", this.slot_max_current, text);
				PhotonConnectionFactory.Instance.PinError(text2, stackTrace.ToString());
			}
			else
			{
				num = int.Parse(this._slots[this.slot_max_current].name);
			}
		}
		return num;
	}

	public Guid GetSelectedSlotInstanceValue()
	{
		return (this._slots.Count <= this.slot_max_current) ? Guid.Empty : this._guids[this._slots[this.slot_max_current]];
	}

	public void InitPanel(List<SlotContent> slotContent, int centerSlotIndex, CustomPlayerAction up, CustomPlayerAction down, CustomPlayerAction upAdditional, CustomPlayerAction downAdditional, Vector2 deltaSizeThumbnail, Vector2 posImage)
	{
		this._up = up;
		this._down = down;
		this._upAdditional = upAdditional;
		this._downAdditional = downAdditional;
		this._inProgress = false;
		this._updating = false;
		this._updateCount = 0;
		this._values = slotContent;
		this._imagePos = posImage;
		this._imageSize = deltaSizeThumbnail;
		int num = Math.Max(0, slotContent.Count - 1);
		this.slot_max_current = Math.Min(num / 2 + num % 2, 3);
		this._center_displacement = 0;
		this._centerSlotIndex = centerSlotIndex;
		this._loadedSprites = new Sprite[this._values.Count];
		this.loadedCount = 0;
		for (int i = 0; i < this._values.Count; i++)
		{
			base.StartCoroutine(ResourcesHelpers.GetInventoryShortSpriteAsync(this._values[i].ThumbnailId, i, new Action<Sprite, int>(this.OnLoaded)));
		}
	}

	private void OnLoaded(Sprite loadedSprite, int index)
	{
		this._loadedSprites[index] = loadedSprite;
		this.loadedCount++;
		if (this.loadedCount == this._values.Count<SlotContent>())
		{
			this.InitPanel();
		}
	}

	private void InitPanel()
	{
		if (this._slotsPool.Count == 0)
		{
			this.InstantiateSlots();
		}
		this.InitSlots();
		if (this._centerSlotIndex != this.slot_max_current)
		{
			this._isInitialization = true;
			this._inProgress = true;
			this._updateCount = this._centerSlotIndex - this.slot_max_current;
			this.UpdateOneStep((this._updateCount <= 0) ? (-1) : 1, true);
		}
	}

	private void InstantiateSlots()
	{
		this._additionalSlot = Object.Instantiate<GameObject>(this._slotPrefab, this._root.transform);
		this._additionalSlot.SetActive(false);
		this._additionalSlot.transform.localScale = Vector3.one;
		this._additionalSlot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		for (int i = 0; i < 7; i++)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this._slotPrefab, this._root.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(false);
			this._slotsPool.Add(gameObject);
		}
	}

	private void InitSlots()
	{
		this._slots = new List<GameObject>();
		int num = 0;
		while (num < this.slot_max_current * 2 + 1 && num < this._slotsPool.Count)
		{
			this._slots.Add(this._slotsPool[num]);
			num++;
		}
		Vector2 sizeDelta = this._slotPrefab.GetComponent<RectTransform>().sizeDelta;
		for (int i = 0; i < this.slot_max_current; i++)
		{
			this.InitSlot(i, i, i, this.slot_max_current - i - 1, sizeDelta, this._firstAxeDirection);
			if (i + this.slot_max_current + 1 < this._loadedSprites.Length)
			{
				this.InitSlot(i + this.slot_max_current + 1, i + this.slot_max_current + 1, this.slot_max_current - i - 1, i, sizeDelta, this._secondAxeDirection);
			}
			else
			{
				this._slots[i + this.slot_max_current + 1] = null;
			}
		}
		this.InitSlot(this.slot_max_current, this.slot_max_current, this.slot_max_current, 0, sizeDelta, this._secondAxeDirection);
		this._slots[this.slot_max_current].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		if (this._rodStandHelp != null)
		{
			bool flag = RodPodHelper.FindPodOnDoll() != null && (!GameFactory.Player.IsIdle || !RodPodHelper.IsFreeAllRodStands) && RodPodHelper.AnyPodOnGround;
			flag = flag && SettingsManager.InputType == InputModuleManager.InputType.GamePad;
			this._rodStandHelp.SetActive(flag);
		}
		if (this._tweenSequence != null)
		{
			TweenExtensions.Kill(this._tweenSequence, false);
		}
		this._tweenSequence = ShortcutExtensions.DOFade(this._canvasGroup, 1f, this._fadeDuration);
	}

	private void InitSlot(int slotIndex, int spriteIndex, int alphaIndex, int posIndex, Vector2 deltaSize, EquipmentInGamePanel.Direction axe)
	{
		this._slots[slotIndex].SetActive(true);
		this._guids[this._slots[slotIndex]] = this._values[spriteIndex].InstanceValue;
		this._slots[slotIndex].GetComponent<RectTransform>().anchoredPosition = this.GetPosition(posIndex, deltaSize, axe);
		if (this.AvailableSlotBg != null && this.UnavailableSlotBg != null)
		{
			this._slots[slotIndex].GetComponent<Image>().overrideSprite = ((!this._values[spriteIndex].Available) ? this.UnavailableSlotBg : this.AvailableSlotBg);
		}
		Image component = this._slots[slotIndex].transform.Find("Image").GetComponent<Image>();
		component.overrideSprite = this._loadedSprites[spriteIndex];
		component.rectTransform.anchoredPosition = this._imagePos;
		component.rectTransform.sizeDelta = this._imageSize;
		this._slots[slotIndex].transform.Find("Name").GetComponent<TextMeshProUGUI>().text = this._values[spriteIndex].Name;
		this._slots[slotIndex].transform.Find("Count").GetComponent<TextMeshProUGUI>().text = this._values[spriteIndex].Count;
		this._slots[slotIndex].name = this._values[spriteIndex].Value.ToString();
		this.SetAlpha(this._slots[slotIndex], alphaIndex, true);
	}

	private Vector2 GetPosition(int i, Vector2 deltaSize, EquipmentInGamePanel.Direction axeDirection)
	{
		float num = ((axeDirection != EquipmentInGamePanel.Direction.Left && axeDirection != EquipmentInGamePanel.Direction.Right) ? 0f : ((float)(i + 1) * (deltaSize.x + this._spacing)));
		num = ((axeDirection != EquipmentInGamePanel.Direction.Right) ? num : (num * -1f));
		float num2 = ((axeDirection != EquipmentInGamePanel.Direction.Down && axeDirection != EquipmentInGamePanel.Direction.Up) ? 0f : ((float)(i + 1) * (deltaSize.y + this._spacing)));
		num2 = ((axeDirection != EquipmentInGamePanel.Direction.Down) ? num2 : (num2 * -1f));
		return new Vector2(num, num2);
	}

	private void UpdatePanel(int count)
	{
		if (this._inProgress)
		{
			this._updateCount += count;
			return;
		}
		this._inProgress = true;
		this._updateCount = count;
		this.UpdateOneStep((count <= 0) ? (-1) : 1, false);
	}

	private void UpdateOneStep(int direction, bool immediately = false)
	{
		if (this._updateCount == 0 || this._loadedSprites.Length == 1)
		{
			this._inProgress = false;
			this._isInitialization = false;
			return;
		}
		if (this._updating)
		{
			return;
		}
		this._updating = true;
		this._updateCount -= direction;
		if (this._loadedSprites.Length == 2 && this._center_displacement - direction != 0 && this._center_displacement - direction != 1)
		{
			this._updating = false;
			this.UpdateOneStep((this._updateCount <= 0) ? (-1) : 1, false);
			return;
		}
		Vector2 sizeDelta = this._slotPrefab.GetComponent<RectTransform>().sizeDelta;
		Vector2 vector = this.GetPosition(0, sizeDelta, this._firstAxeDirection) * (float)direction;
		Vector2 vector2 = this.GetPosition(0, sizeDelta, this._secondAxeDirection) * (float)(-(float)direction);
		for (int i = 0; i < this.slot_max_current; i++)
		{
			if (this._slots[i] != null)
			{
				Vector2 vector3 = this._slots[i].GetComponent<RectTransform>().anchoredPosition;
				vector3 += vector;
				this.MoveSlot(direction, vector3, i, i - direction, immediately, false);
			}
			if (this._slots[i + this.slot_max_current + 1] != null)
			{
				Vector2 vector3 = this._slots[i + this.slot_max_current + 1].GetComponent<RectTransform>().anchoredPosition;
				vector3 += vector2;
				this.MoveSlot(direction, vector3, i + this.slot_max_current + 1, this.slot_max_current - i - 1 + direction, immediately, false);
			}
		}
		Vector2 vector4 = ((direction != 1) ? vector2 : vector);
		this.MoveSlot(direction, vector4, this.slot_max_current, this.slot_max_current - 1, immediately, true);
	}

	private void MoveSlot(int direction, Vector2 newPosition, int slotIndex, int alphaIndex, bool immediately, bool isCurrentSlot)
	{
		GameObject gameObject = this._slots[slotIndex];
		if (immediately)
		{
			gameObject.GetComponent<RectTransform>().anchoredPosition = newPosition;
			this.SetAlpha(gameObject, alphaIndex, true);
			if (isCurrentSlot)
			{
				this.OnCentralSlotMoved(direction, true);
			}
		}
		else
		{
			Tweener tweener = TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(gameObject.GetComponent<RectTransform>(), newPosition, 0.1f, true), 8);
			if (isCurrentSlot)
			{
				TweenSettingsExtensions.OnComplete<Tweener>(tweener, delegate
				{
					this.OnCentralSlotMoved(direction, false);
				});
			}
			this.SetAlpha(gameObject, alphaIndex, false);
		}
	}

	private void OnCentralSlotMoved(int direction, bool immediately)
	{
		Vector2 sizeDelta = this._slotPrefab.GetComponent<RectTransform>().sizeDelta;
		this.ShiftArray(direction);
		if (this._loadedSprites.Length > 2)
		{
			int num = ((this._center_displacement != 0) ? ((this._center_displacement <= 0) ? (-this._center_displacement) : (this._loadedSprites.Length - this._center_displacement)) : 0);
			if (direction == 1)
			{
				int num2 = ((!(this._slots[this._slots.Count - 1] == null)) ? (this._slots.Count - 1) : (this._slots.Count - 2));
				int num3 = (num + ((this.slot_max_current * 2 + 1 >= this._loadedSprites.Length) ? this._loadedSprites.Length : (this.slot_max_current * 2 + 1))) % this._loadedSprites.Length;
				this.InitSlot(num2, num3, 0, (!(this._slots[this._slots.Count - 1] == null)) ? (this.slot_max_current - 1) : (this.slot_max_current - 2), sizeDelta, this._secondAxeDirection);
			}
			else
			{
				int num4 = ((!(this._slots[0] == null)) ? 0 : 1);
				int num5 = ((num != 0) ? (num - 1) : (this._loadedSprites.Length - 1));
				this.InitSlot(num4, num5, 0, (!(this._slots[0] == null)) ? (this.slot_max_current - 1) : (this.slot_max_current - 2), sizeDelta, this._firstAxeDirection);
			}
		}
		this._center_displacement = (this._center_displacement - direction) % this._loadedSprites.Length;
		this._updating = false;
		if (this._updateCount == 0 || this._updateCount % 35 != 0)
		{
			this.UpdateOneStep((this._updateCount <= 0) ? (-1) : 1, immediately);
		}
	}

	private void SetAlpha(GameObject root, int index, bool immediately)
	{
		CanvasGroup component = root.GetComponent<CanvasGroup>();
		index = this.slot_max_current - index;
		if (immediately)
		{
			component.alpha = ((index < 0 || index >= this._alphas.Length) ? 0f : this._alphas[index]);
		}
		else
		{
			ShortcutExtensions.DOFade(component, (index < 0 || index >= this._alphas.Length) ? 0f : this._alphas[index], 0.1f);
		}
	}

	private void ShiftArray(int direction)
	{
		int num = ((!(this._slots[0] == null) || this._loadedSprites.Length <= 2) ? 0 : 1);
		int num2 = ((!(this._slots[this._slots.Count - 1] == null) || this._loadedSprites.Length <= 2) ? (this._slots.Count - 1) : (this._slots.Count - 2));
		if (direction == 1)
		{
			GameObject gameObject = this._slots[num];
			for (int i = num; i < num2; i++)
			{
				this._slots[i] = this._slots[i + 1];
			}
			this._slots[num2] = gameObject;
		}
		else
		{
			GameObject gameObject2 = this._slots[num2];
			for (int j = num2; j > num; j--)
			{
				this._slots[j] = this._slots[j - 1];
			}
			this._slots[num] = gameObject2;
		}
	}

	[SerializeField]
	private TextMeshProUGUI _actionValue;

	[SerializeField]
	private TextMeshProUGUI _actionText;

	[SerializeField]
	private Text _actionIco;

	[SerializeField]
	private GameObject _rodStandHelp;

	private const float MOVE_DURATION = 0.1f;

	private const int MAX_STEPS_PER_TICK = 35;

	private const int slot_max = 3;

	public Sprite AvailableSlotBg;

	public Sprite UnavailableSlotBg;

	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private GameObject _slotPrefab;

	[SerializeField]
	private EquipmentInGamePanel.Direction _firstAxeDirection;

	[SerializeField]
	private EquipmentInGamePanel.Direction _secondAxeDirection;

	[SerializeField]
	private float _spacing;

	private bool _inProgress;

	private bool _updating;

	private int _updateCount;

	private bool _isInitialization;

	private List<GameObject> _slots = new List<GameObject>();

	private Dictionary<GameObject, Guid> _guids = new Dictionary<GameObject, Guid>();

	private List<GameObject> _slotsPool = new List<GameObject>();

	private readonly float[] _alphas = new float[] { 1f, 0.4f, 0.3f, 0.15f };

	private GameObject _additionalSlot;

	private List<SlotContent> _values;

	private int slot_max_current;

	private int _center_displacement;

	private int _centerSlotIndex;

	private Sprite[] _loadedSprites;

	private CustomPlayerAction _up;

	private CustomPlayerAction _down;

	private CustomPlayerAction _upAdditional;

	private CustomPlayerAction _downAdditional;

	private Tweener _tweenSequence;

	private float _fadeDuration = 0.175f;

	private CanvasGroup _canvasGroup;

	private Vector2 _imagePos;

	private Vector2 _imageSize;

	private int loadedCount;

	public enum Direction
	{
		Left,
		Right,
		Down,
		Up
	}
}
