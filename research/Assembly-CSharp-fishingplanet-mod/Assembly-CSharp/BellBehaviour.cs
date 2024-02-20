using System;
using System.Collections.Generic;
using ObjectModel;
using Phy;
using UnityEngine;

public abstract class BellBehaviour
{
	protected BellBehaviour(BellController controller, GameFactory.RodSlot rodSlot, IAssembledRod rodAssembly = null)
	{
		this.assembledRod = rodAssembly;
		this.RodSlot = rodSlot;
		this.RodBellMass = null;
		this.IsSound = false;
		this.IsVoice = false;
		this.isHighSensitivity = false;
		this.owner = controller;
		if (this.owner != null)
		{
			this.localRotationOne = this.owner.SpringOne.localRotation;
			this.localRotationTwo = this.owner.SpringTwo.localRotation;
			this._renderers = RenderersHelper.GetAllRenderersForObject<SkinnedMeshRenderer>(this.owner.transform);
			Bell bell = null;
			if (this.assembledRod != null)
			{
				bell = this.assembledRod.BellInterface as Bell;
			}
			if (bell != null && bell.Loudness > 0f && this.owner.Clips.Length > 0)
			{
				this.audio = this.owner.GetComponent<AudioSource>();
				this.IsSound = this.audio != null;
				if (this.IsSound)
				{
					this.audio.volume = bell.Loudness;
					this.sensitivity = 1f;
					if (bell.Sensitivity > 0f)
					{
						this.sensitivity = bell.Sensitivity;
					}
					this.ballVelocity = Vector3.zero;
					this.bellCounter = 0;
					this.tipValuePrev = 0f;
					this.jinglePeriod = 0f;
					this.isJingle = false;
					this.twitchLevels = new float[this.owner.Clips.Length];
					this.CalculateSensitivity(this.owner.NormalSensitivity);
				}
			}
		}
	}

	public ConnectedBodiesSystem Sim { get; protected set; }

	public GameObject gameObject
	{
		get
		{
			return (!(this.owner != null)) ? null : this.owner.gameObject;
		}
	}

	public Transform transform
	{
		get
		{
			return (!(this.owner != null)) ? null : this.owner.transform;
		}
	}

	public GameFactory.RodSlot RodSlot { get; private set; }

	public BellController Controller
	{
		get
		{
			return this.owner;
		}
	}

	public Mass RodBellMass { get; protected set; }

	protected RodBehaviour rodBehaviour
	{
		get
		{
			return this.RodSlot.Rod;
		}
	}

	public bool IsSound { get; protected set; }

	public bool IsVoice { get; protected set; }

	public bool IsJingle
	{
		get
		{
			return this.isJingle;
		}
	}

	public virtual void Init()
	{
		if (this.rodBehaviour != null)
		{
			if (this.RodBellMass != null)
			{
				this.tBezier = this.rodBehaviour.GetTParam(this.rodBehaviour.transform.InverseTransformPoint(this.RodBellMass.Position).z);
			}
			else if (this.rodBehaviour.RodPointsCount > 0 && this.Controller.BellPosFromTip >= 0 && this.Controller.BellPosFromTip <= this.rodBehaviour.RodPointsCount)
			{
				this.tBezier = (float)(this.rodBehaviour.RodPointsCount - this.Controller.BellPosFromTip) / (float)this.rodBehaviour.RodPointsCount;
			}
		}
	}

	public void CalculateSensitivity(float sensitivityFactor)
	{
		float num = 0.03f;
		if (sensitivityFactor > 0f)
		{
			num /= sensitivityFactor * this.sensitivity;
		}
		for (int i = 0; i < this.twitchLevels.Length; i++)
		{
			float num2 = 1f;
			if (i < this.owner.FactorClips.Length)
			{
				num2 = this.owner.FactorClips[i] * num;
			}
			this.twitchLevels[i] = num2;
			if (i == 0 || num2 < this.minTwitchLevel)
			{
				this.minTwitchLevel = num2;
			}
		}
	}

	public void Voice(bool isVoice = true)
	{
		this.IsVoice = isVoice;
	}

	public void HighSensitivity(bool isHigh = true)
	{
		if (this.IsVoice && isHigh != this.isHighSensitivity)
		{
			this.isHighSensitivity = isHigh;
			if (isHigh)
			{
				this.CalculateSensitivity(this.owner.HighSensitivity);
			}
			else
			{
				this.CalculateSensitivity(this.owner.NormalSensitivity);
			}
		}
	}

	public void SetPhyBell(Mass rodBellMass)
	{
		this.RodBellMass = rodBellMass;
	}

	public void SetVisibility(bool flag)
	{
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in this._renderers)
		{
			skinnedMeshRenderer.enabled = flag;
		}
	}

	public void SetOpaque(float prc)
	{
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in this._renderers)
		{
			skinnedMeshRenderer.material.SetFloat("_CharacterOpaque", prc);
		}
	}

	private void Jingle(float averageTwitch)
	{
		if (this.IsSound && this.IsVoice)
		{
			int num = -1;
			float num2 = 0f;
			for (int i = 0; i < this.twitchLevels.Length; i++)
			{
				float num3 = this.twitchLevels[i];
				if (averageTwitch > num3 && num3 > num2)
				{
					num2 = num3;
					num = i;
				}
			}
			if (num >= 0)
			{
				this.audio.clip = this.Controller.Clips[num];
				this.audio.Play();
			}
		}
	}

	public void Show(bool isShow = true)
	{
		this.gameObject.SetActive(isShow);
	}

	public void OnLateUpdate()
	{
		if (this.rodBehaviour != null || this.RodBellMass != null)
		{
			Vector3 vector;
			Quaternion quaternion;
			if (this.rodBehaviour != null)
			{
				vector = this.rodBehaviour.GetBezierPoint(this.tBezier);
				quaternion = this.rodBehaviour.GetBezierRotation(this.tBezier);
			}
			else
			{
				vector = this.RodBellMass.Position;
				quaternion = this.RodBellMass.Rotation;
			}
			this.Controller.transform.position = vector;
			this.Controller.transform.rotation = quaternion;
		}
	}

	public void SoundUpdate()
	{
		if (this.IsSound)
		{
			float num = 0f;
			if (this.rodBehaviour != null)
			{
				num = this.rodBehaviour.RodObject.TipDivergence;
			}
			float num2 = Mathf.Abs(num - this.tipValuePrev) / Time.deltaTime;
			if (num2 > this.minTwitchLevel)
			{
				this.bellCounter++;
				this.averageTwitch += num2;
			}
			else if (this.bellCounter > 0)
			{
				this.bellCounter--;
			}
			if (this.isJingle)
			{
				this.jinglePeriod -= Time.deltaTime;
				if (this.jinglePeriod < 0f)
				{
					this.isJingle = false;
				}
			}
			if (!this.isJingle && this.bellCounter > 0 && this.bellCounter > this.Controller.JingleCountThreshold)
			{
				if (this.gameObject.activeSelf)
				{
					if (this.bellCounter > 1)
					{
						this.averageTwitch /= (float)this.bellCounter;
					}
					this.Jingle(this.averageTwitch);
				}
				this.isJingle = true;
				this.jinglePeriod = Random.Range(this.Controller.MinJinglePeriod, this.Controller.MaxJinglePeriod);
				this.bellCounter = 0;
				this.averageTwitch = 0f;
			}
			this.tipValuePrev = num;
		}
	}

	public virtual void Destroy()
	{
		if (this.owner != null)
		{
			this.gameObject.SetActive(false);
			Object.Destroy(this.gameObject);
		}
		this.owner = null;
		this.RodSlot = null;
	}

	protected BellController owner;

	protected IAssembledRod assembledRod;

	protected Quaternion localRotationOne;

	protected Quaternion localRotationTwo;

	protected AudioSource audio;

	protected List<SkinnedMeshRenderer> _renderers;

	protected bool isHighSensitivity;

	protected const float tipValueFactor = 0.03f;

	protected float sensitivity = 1f;

	protected float[] twitchLevels;

	protected float minTwitchLevel;

	private Vector3 ballVelocity;

	private float tipValuePrev;

	private int bellCounter;

	private float jinglePeriod;

	private bool isJingle;

	private float averageTwitch;

	protected float tBezier = 0.9f;
}
