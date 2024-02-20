using System;
using UnityEngine;

public abstract class FloatBehaviour : TackleBehaviour
{
	protected FloatBehaviour(FloatController owner, IAssembledRod rodAssembly, RodOnPodBehaviour.TransitionData transitionData = null)
		: base(owner, rodAssembly, transitionData)
	{
		base.transform.localScale = Vector3.one;
		GameObject gameObject = (GameObject)Resources.Load(rodAssembly.HookInterface.Asset, typeof(GameObject));
		this.hookObject = Object.Instantiate<GameObject>(gameObject, new Vector3(0f, -this.leaderLength, 0f), Quaternion.identity);
		base.Hook = this.hookObject.GetComponent<HookController>();
		if (rodAssembly.BaitInterface == null)
		{
			throw new NullReferenceException("No bait on current Rod with float tackle");
		}
		GameObject gameObject2 = (GameObject)Resources.Load(rodAssembly.BaitInterface.Asset, typeof(GameObject));
		this.baitObject = Object.Instantiate<GameObject>(gameObject2, base.Hook.baitAnchor.position, base.Hook.baitAnchor.rotation);
		Transform transform = this.baitObject.transform.Find("bait_root");
		if (transform != null)
		{
			this.baitObject.transform.position -= transform.localPosition;
		}
		this.baitObject.transform.parent = this.hookObject.transform;
		if (this._owner.topLineAnchor == null)
		{
			throw new PrefabConfigException("topLineAnchor is null!");
		}
		if (this._owner.bottomLineAnchor == null)
		{
			throw new PrefabConfigException("bottomLineAnchor is null!");
		}
		this.Size = (this.Owner.waterMark.position - this._owner.bottomLineAnchor.position).magnitude;
	}

	protected FloatController Owner
	{
		get
		{
			return this._owner as FloatController;
		}
	}

	public Transform WaterMark
	{
		get
		{
			return this.Owner.waterMark;
		}
	}

	public override Transform HookAnchor
	{
		get
		{
			return base.Hook.hookAnchor;
		}
	}

	public static Vector3 GetBobberScale(Vector3 bobberPosition)
	{
		Vector3 position = GameFactory.Player.transform.position;
		float magnitude = (position - bobberPosition).magnitude;
		Vector3 vector = Vector3.one;
		if (!float.IsNaN(magnitude))
		{
			float num = Mathf.Lerp(0.3f, 1.3f, (GlobalConsts.BobberScale - 1f) * 3.33333f);
			if (magnitude < 2f)
			{
				vector = Vector3.one;
			}
			else if (magnitude > 20f)
			{
				vector = Vector3.one * 12f * num;
			}
			else
			{
				vector = Vector3.one * (1f + (magnitude - 2f) * 0.6111111f * num);
			}
		}
		return vector;
	}

	public override void Destroy()
	{
		if (this.hookObject != null)
		{
			this.hookObject.SetActive(false);
			Object.Destroy(this.hookObject);
			this.hookObject = null;
		}
		if (this.baitObject != null)
		{
			this.baitObject.SetActive(false);
			Object.Destroy(this.baitObject);
			this.baitObject = null;
		}
		base.Destroy();
	}

	public override void SetActive(bool flag)
	{
		base.SetActive(flag);
		this.hookObject.SetActive(flag);
		this.baitObject.SetActive(flag);
	}

	public virtual float LeaderLength
	{
		get
		{
			return this.leaderLength;
		}
		set
		{
			this.leaderLength = value;
		}
	}

	public float UserSetLeaderLength
	{
		get
		{
			return this._userSetLengthLength;
		}
		set
		{
			this._userSetLengthLength = Mathf.Max(0.1f, value);
		}
	}

	public new float Size { get; private set; }

	protected virtual void OnHookEnterWater()
	{
	}

	public static float GetHookScaling(float hookSize, float hookModelSize)
	{
		float num = hookSize * 0.001f / hookModelSize;
		if (num > 1.25f)
		{
			float num2 = (num - 1.25f) / 2.75f;
			num = num2 * 2.5f + (1f - num2) * 1.25f;
		}
		return num;
	}

	public const int SinkersCount = 3;

	public const float SCALING_MIN_SIZE = 1.25f;

	public const float SCALING_MAX_SIZE = 4f;

	public const float SCALING_NEW_MAX_SIZE = 2.5f;

	protected GameObject hookObject;

	protected GameObject baitObject;

	private const float MinScaleDistance = 2f;

	private const float MaxScaleDistance = 20f;

	private const float MaxScale = 12f;

	private const float ScaleRate = 0.6111111f;

	protected float leaderLength;

	private float _userSetLengthLength;
}
