using System;
using System.Diagnostics;
using UnityEngine;

public class HidesPanel : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnStart;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnHideFinished;

	private void Start()
	{
		this.ResetPos();
	}

	private void Update()
	{
		if (this.ActionObject.GetComponent<RectTransform>().localPosition != this._position)
		{
			this.ActionObject.GetComponent<RectTransform>().localPosition = this._position;
		}
	}

	private void FixedUpdate()
	{
		if (this._stop)
		{
			return;
		}
		switch (this.ShowDirection)
		{
		case HidesPanel.Direction.RightToLeft:
			this.RightToLeftMoving();
			break;
		case HidesPanel.Direction.LeftToRight:
			this.LeftToRightMoving();
			break;
		case HidesPanel.Direction.DownToUp:
			this.DownToUpMoving();
			break;
		case HidesPanel.Direction.UpToDown:
			this.UpToDownMoving();
			break;
		}
	}

	public void ResetPos()
	{
		this._position = this.ActionObject.GetComponent<RectTransform>().localPosition;
	}

	private void LeftToRightMoving()
	{
		if (this.IsShowing && this._position.x < this.BasePosition.x)
		{
			this._position = new Vector3(Mathf.Min(this._position.x + this.Speed / 50f, this.BasePosition.x), this.BasePosition.y, 0f);
			if (Math.Abs(this._position.x - this.BasePosition.x) < 0.01f)
			{
				this.Finish(true);
			}
		}
		if (!this.IsShowing && this._position.x > this.StartPosition.x)
		{
			this._position = new Vector3(Mathf.Max(this._position.x - this.Speed / 50f, this.StartPosition.x), this.BasePosition.y, 0f);
			if (Math.Abs(this._position.x - this.StartPosition.x) < 0.01f)
			{
				this.Finish(false);
			}
		}
	}

	private void RightToLeftMoving()
	{
		if (this.IsShowing && this._position.x > this.BasePosition.x)
		{
			this._position = new Vector3(Mathf.Max(this._position.x - this.Speed / 50f, this.BasePosition.x), this.BasePosition.y, 0f);
			if (Math.Abs(this._position.x - this.BasePosition.x) < 0.01f)
			{
				this.Finish(true);
			}
		}
		if (!this.IsShowing && this._position.x < this.StartPosition.x)
		{
			this._position = new Vector3(Mathf.Min(this._position.x + this.Speed / 50f, this.StartPosition.x), this.BasePosition.y, 0f);
			if (Math.Abs(this._position.x - this.StartPosition.x) < 0.01f)
			{
				this.Finish(false);
			}
		}
	}

	private void DownToUpMoving()
	{
		if (this.IsShowing && this._position.y > this.BasePosition.y)
		{
			this._position = new Vector3(this.BasePosition.x, Mathf.Max(this._position.y - this.Speed / 50f, this.BasePosition.y), 0f);
			if (Math.Abs(this._position.y - this.BasePosition.y) < 0.01f)
			{
				this.Finish(true);
			}
		}
		if (this.IsShowing && this.BasePosition.y > this._position.y)
		{
			this._position = new Vector3(this.BasePosition.x, Mathf.Min(this._position.y + this.Speed / 50f, this.BasePosition.y), 0f);
			if (Math.Abs(this._position.y - this.BasePosition.y) < 0.01f)
			{
				this.Finish(true);
			}
		}
		if (!this.IsShowing && this._position.y < this.StartPosition.y)
		{
			this._position = new Vector3(this.BasePosition.x, Mathf.Min(this._position.y + this.Speed / 50f, this.StartPosition.y), 0f);
			if (Math.Abs(this._position.y - this.StartPosition.y) < 0.01f)
			{
				this.Finish(false);
			}
		}
	}

	private void UpToDownMoving()
	{
		if (this.IsShowing && this._position.y > this.BasePosition.y)
		{
			this._position = new Vector3(this.BasePosition.x, Mathf.Max(this._position.y - this.Speed / 50f, this.BasePosition.y), this._position.z);
			if (Math.Abs(Math.Abs(this._position.y) - Math.Abs(this.BasePosition.y)) < 0.01f)
			{
				this.Finish(true);
			}
		}
		if (!this.IsShowing && this._position.y < this.StartPosition.y)
		{
			this._position = new Vector3(this.BasePosition.x, Mathf.Min(this._position.y + this.Speed / 50f, this.StartPosition.y), this._position.z);
			if (Math.Abs(Math.Abs(this._position.y) - Math.Abs(this.StartPosition.y)) < 0.01f)
			{
				this.Finish(false);
			}
		}
	}

	public void Show()
	{
		if (this._actionClipShow != null)
		{
			UIAudioSourceListener.Instance.Audio.PlayOneShot(this._actionClipShow, SettingsManager.InterfaceVolume);
		}
		if (!this.ActionObject.activeSelf)
		{
			this.ActionObject.SetActive(true);
		}
		base.transform.localScale = Vector3.one;
		this.ActionObject.GetComponent<RectTransform>().localPosition = this.StartPosition;
		this._position = this.StartPosition;
		this._stop = false;
		this.IsShowing = true;
	}

	public void Hide()
	{
		if (this._actionClipHide != null)
		{
			UIAudioSourceListener.Instance.Audio.PlayOneShot(this._actionClipHide, SettingsManager.InterfaceVolume);
		}
		this._position = this.ActionObject.GetComponent<RectTransform>().localPosition;
		this.IsShowing = false;
		this._stop = false;
	}

	private void Finish(bool isShowing)
	{
		this._stop = true;
		if (isShowing)
		{
			this.CanRun = true;
			if (this.OnStart != null)
			{
				this.OnStart(this, new EventArgs());
			}
		}
		else
		{
			base.transform.localScale = Vector3.zero;
			if (this.NeedDisable)
			{
				this.ActionObject.SetActive(false);
			}
			if (this.OnHideFinished != null)
			{
				this.OnHideFinished(this, new EventArgs());
			}
		}
	}

	[SerializeField]
	private AudioClip _actionClipShow;

	[SerializeField]
	private AudioClip _actionClipHide;

	public GameObject ActionObject;

	public HidesPanel.Direction ShowDirection;

	public Vector3 StartPosition;

	public Vector3 BasePosition;

	public float Speed = 5000f;

	public bool NeedDisable;

	[HideInInspector]
	public bool CanRun;

	[HideInInspector]
	public bool IsShowing;

	private Vector3 _position = Vector3.zero;

	private bool _stop = true;

	public enum Direction
	{
		RightToLeft,
		LeftToRight,
		DownToUp,
		UpToDown
	}
}
