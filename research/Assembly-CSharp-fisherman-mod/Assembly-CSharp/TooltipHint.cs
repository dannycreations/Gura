using System;
using System.Collections.Generic;
using DG.Tweening;
using InControl;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipHint : ManagedHintObject
{
	public string OriginalText { get; set; }

	private void Awake()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		ControlsController.OnBindingsChanged += this.ControlsController_OnBindingsChanged;
		Canvas topmostCanvas = UIHelper.GetTopmostCanvas(this);
		this.isTMP = topmostCanvas != null && topmostCanvas.renderMode == 0;
		this.Text.gameObject.SetActive(!this.isTMP);
		this.TMPText.gameObject.SetActive(this.isTMP);
		if (this.HoverHide == null)
		{
			this.HoverHide = base.GetComponentInChildren<OnHoverHide>();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		ControlsController.OnBindingsChanged -= this.ControlsController_OnBindingsChanged;
	}

	private void ControlsController_OnBindingsChanged()
	{
		this.UpdateTextHint();
	}

	private void OnInputTypeChanged(InputModuleManager.InputType inputType)
	{
		this.UpdateTextHint();
	}

	private void UpdateTextHint()
	{
		this.WidthContentSizeFitter.enabled = true;
		this.HeightContentSizeFitter.enabled = true;
		if (this.hasReplacedContent)
		{
			this.Init(this.OriginalText, this._side);
		}
	}

	public void Init(string text, HintSide side, Vector3 scale)
	{
		this._scale = scale;
		this.Init(text, side);
	}

	public void Init(string text, HintSide side)
	{
		this.OriginalText = text;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		string text2;
		if (TextHintParent.TryReplaceControlsInText(text, out text2, dictionary, true))
		{
			this.hasReplacedContent = true;
		}
		else
		{
			text2 = text;
		}
		Canvas topmostCanvas = UIHelper.GetTopmostCanvas(this);
		this.isTMP = topmostCanvas != null && topmostCanvas.renderMode == 0;
		this.Text.gameObject.SetActive(!this.isTMP);
		this.TMPText.gameObject.SetActive(this.isTMP);
		if (this.isTMP)
		{
			this.TMPText.text = text2;
		}
		else
		{
			this.Text.text = text2;
		}
		this._rt = base.transform as RectTransform;
		this._parent = this._rt.parent as RectTransform;
		Canvas.ForceUpdateCanvases();
		this.WidthContentSizeFitter.enabled = false;
		this.HeightContentSizeFitter.enabled = false;
		this.group.alpha = 0f;
		this.inited = false;
		this._side = side;
	}

	private void Configure(HintSide side)
	{
		Canvas rootCanvas = base.GetComponentInParent<Canvas>().rootCanvas;
		Rect rect = (rootCanvas.transform as RectTransform).rect;
		Rect rect2 = this._parent.rect;
		float height = this._rt.rect.height;
		float width = this._rt.rect.width;
		if (Mathf.Abs(height) < Mathf.Epsilon || Mathf.Abs(width) < Mathf.Epsilon)
		{
			this.WidthContentSizeFitter.enabled = true;
			this.HeightContentSizeFitter.enabled = true;
			this.Init(this.OriginalText, this._side);
			return;
		}
		float height2 = this._parent.rect.height;
		float width2 = this._parent.rect.width;
		float num = (width - 40f) * 0.5f;
		float num2 = (height - 40f) * 0.5f;
		Vector3[] array = new Vector3[4];
		this._parent.GetWorldCorners(array);
		for (int i = 0; i < 4; i++)
		{
			array[i] = (rootCanvas.transform as RectTransform).InverseTransformPoint(array[i]);
		}
		float num3 = -rect.y + array[0].y - height - 12f - 36f;
		float num4 = -rect.x + array[0].x - width - 12f - 64f;
		float num5 = rect.width + rect.x - (array[2].x + width + 12f + 64f);
		float num6 = rect.height + rect.y - (array[2].y + height + 12f + 36f);
		this.tooltipOffset = Vector2.zero;
		this.arrowOffset = Vector2.zero;
		RectTransform rectTransform = null;
		if (side == HintSide.Undefined)
		{
			if (num3 > 0f)
			{
				side = HintSide.Bottom;
			}
			else if (num5 > 0f)
			{
				side = HintSide.Right;
			}
			else if (num6 > 0f)
			{
				side = HintSide.Top;
			}
			else if (num4 > 0f)
			{
				side = HintSide.Left;
			}
		}
		num3 += rect2.height * 0.5f + height * 0.5f + 12f;
		num4 += rect2.width * 0.5f + width * 0.5f + 12f;
		num5 += rect2.width * 0.5f + width * 0.5f + 12f;
		num6 += rect2.height * 0.5f + height * 0.5f + 12f;
		bool flag = num4 > -num;
		bool flag2 = num5 > -num;
		bool flag3 = num6 > -num2;
		bool flag4 = num3 > -num2;
		this._side = side;
		switch (side)
		{
		case HintSide.Top:
			this.ArrowLeft.SetActive(false);
			this.ArrowRight.SetActive(false);
			this.ArrowTop.SetActive(false);
			this.ArrowBottom.SetActive(true);
			rectTransform = this.ArrowBottom.transform as RectTransform;
			if (flag && num4 > 0f)
			{
				num4 = Mathf.Min(num4, 0f);
				this.tooltipOffset = new Vector2(-num4, height2 * 0.5f + height * 0.5f + 12f);
				this.arrowOffset = new Vector2(num4, 0f);
			}
			else if (flag2)
			{
				num5 = Mathf.Min(num5, 0f);
				this.tooltipOffset = new Vector2(num5, height2 * 0.5f + height * 0.5f + 12f);
				this.arrowOffset = new Vector2(-num5, 0f);
			}
			break;
		case HintSide.Bottom:
			this.ArrowLeft.SetActive(false);
			this.ArrowRight.SetActive(false);
			this.ArrowTop.SetActive(true);
			this.ArrowBottom.SetActive(false);
			rectTransform = this.ArrowTop.transform as RectTransform;
			if (flag2 && num4 > 0f)
			{
				num5 = Mathf.Min(num5, 0f);
				this.tooltipOffset = new Vector2(num5, -(height2 * 0.5f + height * 0.5f + 12f));
				this.arrowOffset = new Vector2(-num5, 0f);
			}
			else if (flag && num5 > 0f)
			{
				num4 = Mathf.Min(num4, 0f);
				this.tooltipOffset = new Vector2(-num4, -(height2 * 0.5f + height * 0.5f + 12f));
				this.arrowOffset = new Vector2(num4, 0f);
			}
			else if (num5 < 0f)
			{
				num5 = Mathf.Min(num5, 0f);
				this.tooltipOffset = new Vector2(num5, -(height2 * 0.5f + height * 0.5f + 12f));
				this.arrowOffset = new Vector2(-num5, 0f);
			}
			else if (num4 < 0f)
			{
				num4 = Mathf.Min(num4, 0f);
				this.tooltipOffset = new Vector2(-num4, -(height2 * 0.5f + height * 0.5f + 12f));
				this.arrowOffset = new Vector2(num4, 0f);
			}
			break;
		case HintSide.Left:
			this.ArrowLeft.SetActive(false);
			this.ArrowRight.SetActive(true);
			this.ArrowTop.SetActive(false);
			this.ArrowBottom.SetActive(false);
			rectTransform = this.ArrowRight.transform as RectTransform;
			if (flag3 && num3 > 0f)
			{
				num6 = Mathf.Min(num6, 0f);
				this.tooltipOffset = new Vector2(-(width2 * 0.5f + width * 0.5f + 12f), -num6);
				this.arrowOffset = new Vector2(0f, num6);
			}
			else if (flag4)
			{
				num3 = Mathf.Min(num3, 0f);
				this.tooltipOffset = new Vector2(-(width2 * 0.5f + width * 0.5f + 12f), num3);
				this.arrowOffset = new Vector2(0f, -num3);
			}
			break;
		case HintSide.Right:
			this.ArrowLeft.SetActive(true);
			this.ArrowRight.SetActive(false);
			this.ArrowTop.SetActive(false);
			this.ArrowBottom.SetActive(false);
			rectTransform = this.ArrowLeft.transform as RectTransform;
			if (flag3 && num3 > 0f)
			{
				num6 = Mathf.Min(num6, 0f);
				this.tooltipOffset = new Vector2(width2 * 0.5f + width * 0.5f + 12f, num6);
				this.arrowOffset = new Vector2(0f, -num6);
			}
			else if (flag4)
			{
				num3 = Mathf.Min(num3, 0f);
				this.tooltipOffset = new Vector2(width2 * 0.5f + width * 0.5f + 12f, -num3);
				this.arrowOffset = new Vector2(0f, num3);
			}
			break;
		}
		this._rt.anchoredPosition = this.tooltipOffset;
		rectTransform.anchoredPosition = this.arrowOffset;
		this.inited = true;
	}

	private void LateUpdate()
	{
		if (!this.inited)
		{
			if (this._rt == null)
			{
				this.Init("test test test test", HintSide.Undefined);
			}
			this.Configure(this._side);
		}
	}

	protected new virtual void Show()
	{
		this._rt.localScale = Vector3.zero;
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(this._rt, this._scale, 0.35f), 27);
		Vector2 vector = this.tooltipOffset;
		Vector2 vector2 = vector;
		switch (this._side)
		{
		case HintSide.Top:
			vector2 += Vector2.up * this._rt.rect.height;
			break;
		case HintSide.Bottom:
			vector2 -= Vector2.up * this._rt.rect.height;
			break;
		case HintSide.Left:
			vector2 -= Vector2.right * this._rt.rect.width;
			break;
		case HintSide.Right:
			vector2 += Vector2.right * this._rt.rect.width;
			break;
		}
		this._rt.anchoredPosition = vector2;
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._rt, vector, 0.35f, false), 17);
		ShortcutExtensions.DOFade(this.group, 1f, 0.4f);
		this.showedOnce = true;
		this.HoverHide.isOver = false;
		this.UpdateKeyOutlines();
	}

	public void HideKeyOutlines()
	{
		foreach (KeyValuePair<int, Image> keyValuePair in this.spawnedImages)
		{
			keyValuePair.Value.gameObject.SetActive(false);
		}
	}

	public void UpdateKeyOutlines()
	{
		if (this.Info.Count > 0)
		{
			foreach (KeyValuePair<int, int> keyValuePair in this.Info)
			{
				if (keyValuePair.Key < this.TMPText.textInfo.wordCount)
				{
					TMP_WordInfo tmp_WordInfo = this.TMPText.textInfo.wordInfo[keyValuePair.Key];
					if (!this.spawnedImages.ContainsKey(keyValuePair.Key))
					{
						this.spawnedImages.Add(keyValuePair.Key, Object.Instantiate<Image>(this.buttonOutlinePrefab, (!this.isTMP) ? this.Text.transform : this.TMPText.transform));
					}
					Vector3 bottomLeft = this.TMPText.textInfo.characterInfo[tmp_WordInfo.firstCharacterIndex].bottomLeft;
					Vector3 topRight = this.TMPText.textInfo.characterInfo[tmp_WordInfo.lastCharacterIndex].topRight;
					float scale = this.TMPText.textInfo.characterInfo[tmp_WordInfo.firstCharacterIndex].scale;
					float num = Mathf.Max(40f, (topRight.x - bottomLeft.x) / scale);
					float num2 = Mathf.Max(40f, (topRight.y - bottomLeft.y) / scale);
					if (tmp_WordInfo.firstCharacterIndex == tmp_WordInfo.lastCharacterIndex)
					{
						num = Mathf.Max(num, num2);
						num2 = num;
					}
					Vector3 vector = (bottomLeft + topRight) * 0.5f;
					this.spawnedImages[keyValuePair.Key].gameObject.SetActive(true);
					this.spawnedImages[keyValuePair.Key].rectTransform.sizeDelta = new Vector2(num, num2);
					this.spawnedImages[keyValuePair.Key].rectTransform.anchoredPosition = vector;
				}
			}
		}
	}

	public void Close(Action closeFunc = null)
	{
		this.time = 0f;
		this.showedOnce = false;
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOFade(this.group, 0f, 0.25f), delegate
		{
			if (closeFunc != null)
			{
				closeFunc();
			}
		});
		this.HoverHide.isOver = false;
	}

	protected new virtual void Hide()
	{
		this.Close(null);
	}

	protected override void Update()
	{
		if (this._skipServerScale && base.transform.localScale != Vector3.one)
		{
			base.transform.localScale = Vector3.one;
		}
		if (!this.inited)
		{
			return;
		}
		bool displayed = base.Displayed;
		if (this.time > 2.5f && displayed && !this.showedOnce)
		{
			this.Show();
		}
		if (!displayed)
		{
			this.Hide();
		}
		this.lastCheck = displayed;
		if (displayed)
		{
			this.time += Time.deltaTime;
		}
		if (InputManager.IsInputDeviceActivated(InputModuleManager.InputType.GamePad))
		{
			this.Hide();
		}
	}

	private void OnDisable()
	{
		this.time = 0f;
		this.showedOnce = false;
		this.group.alpha = 0f;
	}

	public Image Background;

	public Text Text;

	public TextMeshProUGUI TMPText;

	public GameObject ArrowLeft;

	public GameObject ArrowRight;

	public GameObject ArrowBottom;

	public GameObject ArrowTop;

	[SerializeField]
	private ContentSizeFitter WidthContentSizeFitter;

	[SerializeField]
	private ContentSizeFitter HeightContentSizeFitter;

	[SerializeField]
	private OnHoverHide HoverHide;

	public const float vertDeadZone = 36f;

	public const float horizDeadZone = 64f;

	public const float timeBeforeShow = 2.5f;

	public const float triangleHeight = 12f;

	public const float triangleWidth = 40f;

	private float time;

	private RectTransform _parent;

	private RectTransform _rt;

	private HintSide _side;

	public Image buttonOutlinePrefab;

	private Dictionary<int, Image> spawnedImages = new Dictionary<int, Image>();

	public Dictionary<int, int> Info = new Dictionary<int, int>();

	private bool isTMP;

	private bool hasReplacedContent;

	private Vector3 _scale = Vector3.one;

	private bool inited;

	private Vector2 tooltipOffset;

	private Vector2 arrowOffset;

	private bool showedOnce;
}
