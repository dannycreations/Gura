using System;
using System.Collections.Generic;
using System.Linq;
using BiteEditor.ObjectModel;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class BottomFishingIndicator : FishingIndicatorBase
{
	public float RotationAngle
	{
		get
		{
			return this._indicator.eulerAngles.z;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this._indicatorY0 = this._indicator.anchoredPosition.y;
		this._indicatorCg = this._indicator.GetComponent<CanvasGroup>();
		this._indicatorCg.alpha = 0f;
		this._btmsImages = this._bottomsImages.ToList<CanvasGroup>();
		this._btmsImagesRt = this._btmsImages.Select((CanvasGroup p) => p.GetComponent<RectTransform>()).ToList<RectTransform>();
		this._widthBottom = this._btmsImagesRt[0].rect.width;
		this._widthBottomMove = this._widthBottom + this._widthBottom * 0.5f;
		this._depth.text = string.Empty;
		this._bottomsImagesPos0 = this._btmsImagesRt.Select((RectTransform p) => p.anchoredPosition.x).ToArray<float>();
	}

	private void Start()
	{
		GameFactory.Player.OnPlayerActiveState += this.Player_OnPlayerEnterState;
	}

	private void OnDestroy()
	{
		if (GameFactory.Player != null)
		{
			GameFactory.Player.OnPlayerActiveState -= this.Player_OnPlayerEnterState;
		}
	}

	public override void Hide()
	{
		if (this.InState(BottomFishingIndicator.States.IsActive))
		{
			this._state ^= BottomFishingIndicator.States.IsActive;
		}
		base.Hide();
	}

	public override void Show()
	{
		this._state |= BottomFishingIndicator.States.IsActive;
		this.ResetBottoms();
		base.Show();
	}

	public override bool IsShow
	{
		get
		{
			return this.AlphaFade.IsShow || this.InState(BottomFishingIndicator.States.IsActive);
		}
	}

	public bool IsHidden
	{
		get
		{
			return this.InState(BottomFishingIndicator.States.IsActive) && this.AlphaFade.Alpha <= 0f;
		}
	}

	public void MoveBottom(float speed)
	{
		if (this._bType == SplatMap.LayerName.None)
		{
			return;
		}
		speed *= this._moveBottomSpeedK;
		this._movementSpeedX = speed;
		this.UpdateMovementX();
	}

	public void SetBottomType(SplatMap.LayerName bType, float? animTime = null)
	{
		if (this._bType == bType || (!this.InState(BottomFishingIndicator.States.WasLying) && bType != SplatMap.LayerName.None))
		{
			return;
		}
		this._bType = bType;
		animTime = new float?((animTime == null) ? this._bottomAnim : animTime.Value);
		Sprite sp = this._bottoms.ToList<BottomFishingIndicator.Bottoms>().Find((BottomFishingIndicator.Bottoms p) => p.Type == this._bType).Sp;
		this._btmsImages.ForEach(delegate(CanvasGroup p)
		{
			ShortcutExtensions.DOKill(p, false);
			TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOFade(p, 0f, animTime.Value), delegate
			{
				p.GetComponent<Image>().overrideSprite = sp;
				ShortcutExtensions.DOFade(p, 1f, animTime.Value);
			});
		});
	}

	public void SetAngle(float angle)
	{
		angle %= 360f;
		if (angle < 0f)
		{
			angle += 360f;
		}
		float num = 60f * Time.deltaTime;
		float num2 = angle - this._indicator.eulerAngles.z;
		if (num2 > 180f)
		{
			num2 -= 360f;
		}
		else if (num2 < -180f)
		{
			num2 += 360f;
		}
		if (Mathf.Abs(num2) < num)
		{
			this._indicator.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
		}
		else
		{
			if (num2 < 0f)
			{
				num = -num;
			}
			this._indicator.Rotate(new Vector3(0f, 0f, num));
		}
	}

	public void SetDepth(float depth, float groundH, bool isLying)
	{
		if (!this.InState(BottomFishingIndicator.States.WasLying) && isLying)
		{
			this._state |= BottomFishingIndicator.States.WasLying;
		}
		if (isLying)
		{
			if (this.InState(BottomFishingIndicator.States.PlayerRoll) && GameFactory.Player.Line.IsTensioned && Math.Abs(this._movementSpeedX) > this._movementSpeedXNotActiveHide)
			{
				this.StopHiding();
			}
			else
			{
				this._time += Time.deltaTime;
				if (this._time >= this._notActiveHideTime && this.IsShow)
				{
					base.Hide();
				}
			}
		}
		if (depth >= 0f)
		{
			if (this.InState(BottomFishingIndicator.States.InWater))
			{
				this._state ^= BottomFishingIndicator.States.InWater;
				ShortcutExtensions.DOKill(this._indicatorCg, false);
				ShortcutExtensions.DOFade(this._indicatorCg, 0f, this._indicatorAnim);
			}
			if (this.InState(BottomFishingIndicator.States.WasLying))
			{
				this._state ^= BottomFishingIndicator.States.WasLying;
			}
			if (this.InState(BottomFishingIndicator.States.WasPlayerRoll))
			{
				this._state ^= BottomFishingIndicator.States.WasPlayerRoll;
			}
			this._depth.text = string.Empty;
			return;
		}
		if (!this.InState(BottomFishingIndicator.States.InWater))
		{
			this._state |= BottomFishingIndicator.States.InWater;
			ShortcutExtensions.DOKill(this._indicatorCg, false);
			ShortcutExtensions.DOFade(this._indicatorCg, 1f, this._indicatorAnim);
			if (ShowHudElements.Instance != null)
			{
				if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
				{
					bool flag;
					string icoByActionName = HotkeyIcons.GetIcoByActionName("AddReelClipGamePadPart1", out flag);
					string icoByActionName2 = HotkeyIcons.GetIcoByActionName("AddReelClipGamePadPart2", out flag);
					string text = "<size=24><color=#FFEE44FF>" + icoByActionName + "</color></size>";
					string text2 = "<size=24><color=#FFEE44FF>" + icoByActionName2 + "</color></size>";
					ShowHudElements.Instance.ShowHintCount("HintClipLine", string.Format(ScriptLocalization.Get("HintClipLineController"), text, text2), 1);
				}
				else
				{
					ShowHudElements.Instance.ShowHintCount("HintClipLine", ControlsController.ControlsActions.AddReelClip);
				}
			}
		}
		float num = Math.Abs(groundH * 100f);
		float num2 = Math.Abs(depth * 100f);
		this._height = this._water.anchoredPosition.y - this._bottom.anchoredPosition.y - this._indicatorYoffset;
		float num3 = this._height / num;
		float num4 = num2 * num3;
		this._indicator.anchoredPosition = new Vector2(this._indicator.anchoredPosition.x, -num4);
		if (Mathf.Abs(this._depthValue - depth) >= 0.1f)
		{
			this._depth.gameObject.SetActive(isLying && !this.InState(BottomFishingIndicator.States.WasPlayerRoll));
			this._depth.text = string.Format("{0} {1}", MeasuringSystemManager.LineLength(-depth).ToString("0.0"), MeasuringSystemManager.LineLengthSufix());
			this._depthValue = depth;
		}
	}

	private void StopHiding()
	{
		this._time = 0f;
		if (this.InState(BottomFishingIndicator.States.IsActive))
		{
			base.Show();
		}
	}

	public void ResetBottoms()
	{
		for (int i = 0; i < this._btmsImagesRt.Count; i++)
		{
			this._btmsImagesRt[i].anchoredPosition = new Vector2(this._bottomsImagesPos0[i], this._btmsImagesRt[i].anchoredPosition.y);
		}
		this.SetBottomType(SplatMap.LayerName.None, new float?(0f));
		this._indicator.anchoredPosition = new Vector2(this._indicator.anchoredPosition.x, this._indicatorY0);
	}

	private void UpdateMovementX()
	{
		if (Math.Abs(this._movementSpeedX) < 0.0001f)
		{
			return;
		}
		float num = Time.deltaTime * this._movementSpeedX;
		int num2 = 0;
		int num3 = 0;
		for (int i = 1; i < this._btmsImagesRt.Count; i++)
		{
			if (this._btmsImagesRt[i].anchoredPosition.x > this._btmsImagesRt[num3].anchoredPosition.x)
			{
				num3 = i;
			}
			if (this._btmsImagesRt[i].anchoredPosition.x < this._btmsImagesRt[num2].anchoredPosition.x)
			{
				num2 = i;
			}
		}
		RectTransform rectTransform = this._btmsImagesRt[num2];
		RectTransform rectTransform2 = this._btmsImagesRt[num3];
		if (this._movementSpeedX > 0f)
		{
			this.MoveX(rectTransform2.anchoredPosition.x >= this._widthBottomMove, num, -this._widthBottom, num3, num2);
		}
		else if (this._movementSpeedX < 0f)
		{
			this.MoveX(rectTransform.anchoredPosition.x <= -this._widthBottomMove, num, this._widthBottom, num2, num3);
		}
	}

	private void MoveX(bool move, float xShift, float width, int iCur, int iOther)
	{
		for (int i = 0; i < this._btmsImagesRt.Count; i++)
		{
			RectTransform rectTransform = this._btmsImagesRt[i];
			if (i != iCur || !move)
			{
				this._btmsImagesRt[i].anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + xShift, rectTransform.anchoredPosition.y);
			}
		}
		if (move)
		{
			float num = this._btmsImagesRt[iOther].anchoredPosition.x + width;
			this._btmsImagesRt[iCur].anchoredPosition = new Vector2(num, this._btmsImagesRt[iCur].anchoredPosition.y);
			List<RectTransform> list = this._btmsImagesRt.OrderBy((RectTransform p) => p.anchoredPosition.x).ToList<RectTransform>();
			for (int j = 1; j < list.Count; j++)
			{
				num = list[j - 1].anchoredPosition.x + this._widthBottom;
				list[j].anchoredPosition = new Vector2(num, list[j].anchoredPosition.y);
			}
		}
	}

	private void Player_OnPlayerEnterState(Type state, bool isActive)
	{
		if (state == typeof(PlayerRoll))
		{
			if (isActive)
			{
				if (GameFactory.Player.Line.IsTensioned)
				{
					this.StopHiding();
				}
				if (this.InState(BottomFishingIndicator.States.InWater) && !this.InState(BottomFishingIndicator.States.WasPlayerRoll))
				{
					this._state |= BottomFishingIndicator.States.WasPlayerRoll;
				}
				if (!this.InState(BottomFishingIndicator.States.PlayerRoll))
				{
					this._state |= BottomFishingIndicator.States.PlayerRoll;
				}
			}
			else if (this.InState(BottomFishingIndicator.States.PlayerRoll))
			{
				this._state ^= BottomFishingIndicator.States.PlayerRoll;
			}
		}
	}

	private bool InState(BottomFishingIndicator.States s)
	{
		return (this._state & s) == s;
	}

	[SerializeField]
	private CanvasGroup[] _bottomsImages;

	[SerializeField]
	private RectTransform _indicator;

	[SerializeField]
	private RectTransform _water;

	[SerializeField]
	private RectTransform _bottom;

	[SerializeField]
	private Text _depth;

	[SerializeField]
	private BottomFishingIndicator.Bottoms[] _bottoms;

	[SerializeField]
	private float _bottomAnim = 0.4f;

	[SerializeField]
	private float _indicatorAnim = 0.4f;

	[SerializeField]
	private float _indicatorYoffset = 20f;

	[SerializeField]
	private float _moveBottomSpeedK = 6f;

	[SerializeField]
	private float _notActiveHideTime = 2f;

	[SerializeField]
	private float _movementSpeedXNotActiveHide = 0.5f;

	private SplatMap.LayerName _bType = SplatMap.LayerName.None;

	private float _movementSpeedX;

	private List<CanvasGroup> _btmsImages;

	private List<RectTransform> _btmsImagesRt;

	private float _widthBottom;

	private float _widthBottomMove;

	private CanvasGroup _indicatorCg;

	private float _height;

	private float[] _bottomsImagesPos0;

	private float _indicatorY0;

	private float _depthValue;

	private BottomFishingIndicator.States _state;

	private float _time;

	[Serializable]
	public class Bottoms
	{
		public SplatMap.LayerName Type;

		public Sprite Sp;
	}

	public enum MovementTypes : byte
	{
		None,
		Water,
		Bottom
	}

	[Flags]
	private enum States : byte
	{
		None = 0,
		InWater = 1,
		PlayerRoll = 2,
		IsActive = 4,
		WasLying = 16,
		WasPlayerRoll = 32
	}
}
