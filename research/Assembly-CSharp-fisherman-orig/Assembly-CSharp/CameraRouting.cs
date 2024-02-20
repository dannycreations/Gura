using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraRouting : RoutingBase
{
	public int PathsCount
	{
		get
		{
			return this._paths.Count;
		}
	}

	private void Start()
	{
		this.BuildPaths();
	}

	public void BuildPaths()
	{
		this._paths.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			CameraPathObj component = base.transform.GetChild(i).GetComponent<CameraPathObj>();
			if (component != null)
			{
				this._paths.Add(component);
			}
		}
		this._isActive = false;
	}

	public int CurPathIndex
	{
		get
		{
			return this._curIndex;
		}
	}

	public int CurPointIndex
	{
		get
		{
			return this._curPoint;
		}
	}

	public bool IsActive
	{
		get
		{
			return this._isActive;
		}
	}

	public override void Play()
	{
		this._isActive = true;
		this._curIndex = -1;
		this.StartNewPath();
	}

	public override void Stop()
	{
		base.Fade(false);
		this._isActive = false;
	}

	private bool StartNewPath()
	{
		base.Fade(false);
		int curIndex = this._curIndex;
		this._curIndex++;
		while (this._curIndex < this._paths.Count)
		{
			this._curData = this._paths[this._curIndex].Data;
			if (this._curData.Sections.Length > 0)
			{
				this._curPoint = -1;
				this._transitionLeftTime = 0f;
				this.MoveToTheNextPoint();
				return true;
			}
			this._curIndex++;
		}
		if (curIndex == -1)
		{
			this._isActive = false;
			return false;
		}
		this._curIndex = -1;
		return this.StartNewPath();
	}

	private void MoveToTheNextPoint()
	{
		this._curPoint++;
		if (this._curPoint < this._curData.Sections.Length)
		{
			this._mover = this._curData.Sections[this._curPoint].CreateMover(-this._transitionLeftTime);
			this._wasFadeOutSend = false;
			this._transitionLeftTime = this._curData.Sections[this._curPoint].Duration;
		}
		else
		{
			this.StartNewPath();
		}
	}

	public override Vector3 Position
	{
		get
		{
			return this._mover.Position;
		}
	}

	public override Quaternion Rotation
	{
		get
		{
			return this._mover.Rotation;
		}
	}

	private float Dt
	{
		get
		{
			return Time.deltaTime;
		}
	}

	private void Update()
	{
		if (this._isActive && this._curIndex >= 0)
		{
			this._transitionLeftTime -= this.Dt;
			this._mover.Update(this._mover.Duration - this._transitionLeftTime);
			if (this._transitionLeftTime < this._fadeTime)
			{
				if (!this._wasFadeOutSend)
				{
					this._wasFadeOutSend = true;
					base.Fade(true);
				}
				if (this._transitionLeftTime < 0f)
				{
					this.MoveToTheNextPoint();
				}
			}
		}
	}

	private List<CameraPathObj> _paths = new List<CameraPathObj>();

	private int _curIndex;

	private int _curPoint;

	private float _transitionLeftTime;

	private bool _wasFadeOutSend;

	private bool _isActive;

	private CameraPathData _curData;

	private CameraPathData.Mover _mover;
}
