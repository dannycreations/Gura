using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class LurePositionHandler : MonoBehaviour
{
	public void Refresh(LurePositionHandler.LurePositionEnum position, TackleMoveDir moveDir)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		this._newPosition = position;
		this._newMoveDir = moveDir;
	}

	internal void OnEnable()
	{
		if (this._startIcon != null)
		{
			Image component = this._startIcon.GetComponent<Image>();
			component.color = new Color(component.color.r, component.color.g, component.color.b, 0f);
		}
		if (this._currentIcon != null)
		{
			Image component2 = this._currentIcon.GetComponent<Image>();
			component2.color = new Color(component2.color.r, component2.color.g, component2.color.b, 0f);
		}
		this._startIcon = null;
		this._currentIcon = null;
		this._position = LurePositionHandler.LurePositionEnum.Undefined;
		this._newPosition = LurePositionHandler.LurePositionEnum.Undefined;
	}

	public void ShowDragStyleText(string dragStyle, float precision)
	{
		this.Retrieve1.SetActive(false);
		this.Retrieve2.SetActive(false);
		this.Retrieve3.SetActive(false);
		string text;
		if (dragStyle != null)
		{
			if (dragStyle == "Straight Slow")
			{
				text = ScriptLocalization.Get("DragStyleStrightSlow");
				goto IL_DA;
			}
			if (dragStyle == "Straight")
			{
				text = ScriptLocalization.Get("DragStyleStright");
				goto IL_DA;
			}
			if (dragStyle == "Lift&Drop")
			{
				text = ScriptLocalization.Get("DragStyleLiftDrop");
				goto IL_DA;
			}
			if (dragStyle == "Stop&Go")
			{
				text = ScriptLocalization.Get("DragStyleStopGo");
				goto IL_DA;
			}
			if (dragStyle == "Twitching")
			{
				text = ScriptLocalization.Get("DragStyleTwitching");
				goto IL_DA;
			}
		}
		text = string.Empty;
		IL_DA:
		this.DragStyleText.text = text;
		if (!string.IsNullOrEmpty(dragStyle))
		{
			if (precision >= 0f && precision <= 0.33f)
			{
				this.Retrieve1.SetActive(true);
			}
			if (precision > 0.33f && precision <= 0.66f)
			{
				this.Retrieve2.SetActive(true);
			}
			if (precision > 0.66f && precision <= 1f)
			{
				this.Retrieve3.SetActive(true);
			}
		}
	}

	internal void Update()
	{
		this.timer += Time.deltaTime;
		if (this.timer >= this.timerMax && (this._position != this._newPosition || this._currentMoveDir != this._newMoveDir))
		{
			this.timer = 0f;
			if (this._position != this._newPosition)
			{
				if (this._startIcon != null && this._startIcon != this._currentIcon)
				{
					Image component = this._startIcon.GetComponent<Image>();
					component.color = new Color(component.color.r, component.color.g, component.color.b, 0f);
				}
				this._startIcon = this._currentIcon;
				this._position = this._newPosition;
				switch (this._position)
				{
				case LurePositionHandler.LurePositionEnum.OnSurface:
					this._currentIcon = this.OnSurfaceIcon;
					break;
				case LurePositionHandler.LurePositionEnum.NearSurface:
					this._currentIcon = this.NearSurfaceIcon;
					break;
				case LurePositionHandler.LurePositionEnum.Middle:
					this._currentIcon = this.MiddleIcon;
					break;
				case LurePositionHandler.LurePositionEnum.NearBottom:
					this._currentIcon = this.NearBottomIcon;
					break;
				case LurePositionHandler.LurePositionEnum.OnBottom:
					this._currentIcon = this.BottomIcon;
					break;
				}
			}
			this._transStamp = Time.time;
			this._currentMoveDir = this._newMoveDir;
			float num = 0f;
			TackleMoveDir currentMoveDir = this._currentMoveDir;
			if (currentMoveDir != TackleMoveDir.Up)
			{
				if (currentMoveDir == TackleMoveDir.Down)
				{
					num = -15f;
				}
			}
			else
			{
				num = 15f;
			}
			if (this._currentIcon != null)
			{
				RectTransform component2 = this._currentIcon.GetComponent<RectTransform>();
				component2.rotation = Quaternion.Euler(new Vector3(component2.rotation.eulerAngles.x, component2.rotation.eulerAngles.x, num));
			}
		}
		if (this._currentIcon == this._startIcon)
		{
			return;
		}
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		float num2 = (Time.time - this._transStamp) / this._blendingTime;
		float num3 = Mathf.Lerp(0f, 1f, num2);
		if (this._startIcon != null)
		{
			Image component3 = this._startIcon.GetComponent<Image>();
			if (!Mathf.Approximately(component3.color.a, 0f))
			{
				component3.color = new Color(component3.color.r, component3.color.g, component3.color.b, 1f - num3);
			}
		}
		if (this._currentIcon != null)
		{
			Image component4 = this._currentIcon.GetComponent<Image>();
			component4.color = new Color(component4.color.r, component4.color.g, component4.color.b, num3);
			if (component4.color.a >= 0.98f)
			{
				this._startIcon = this._currentIcon;
			}
		}
	}

	public GameObject OnSurfaceIcon;

	public GameObject NearSurfaceIcon;

	public GameObject MiddleIcon;

	public GameObject NearBottomIcon;

	public GameObject BottomIcon;

	public Text DragStyleText;

	public GameObject Retrieve1;

	public GameObject Retrieve2;

	public GameObject Retrieve3;

	private float _fadeDuration;

	private float _blendingTime = 0.25f;

	private float _transStamp;

	private GameObject _currentIcon;

	private GameObject _startIcon;

	private LurePositionHandler.LurePositionEnum _position = LurePositionHandler.LurePositionEnum.OnBottom;

	private LurePositionHandler.LurePositionEnum _newPosition;

	private TackleMoveDir _currentMoveDir;

	private TackleMoveDir _newMoveDir = TackleMoveDir.Forward;

	private float timer;

	private float timerMax = 0.5f;

	public enum LurePositionEnum
	{
		Undefined = -1,
		OnSurface,
		NearSurface,
		Middle,
		NearBottom,
		OnBottom
	}
}
