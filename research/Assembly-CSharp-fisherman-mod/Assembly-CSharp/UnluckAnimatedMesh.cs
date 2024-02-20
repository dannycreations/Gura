using System;
using UnityEngine;

public class UnluckAnimatedMesh : MonoBehaviour
{
	public void Start()
	{
		this.transformCache = base.transform;
		this.CheckIfMeshHasChanged();
		this.startDelay = Random.Range(this.randomStartDelay.x, this.randomStartDelay.y);
		UnluckAnimatedMesh.updateSeed += 0.0005f;
		if (this.playOnAwake)
		{
			base.Invoke("Play", this.updateInterval + UnluckAnimatedMesh.updateSeed);
		}
		if (UnluckAnimatedMesh.updateSeed >= this.updateInterval)
		{
			UnluckAnimatedMesh.updateSeed = 0f;
		}
		if (this.rendererComponent == null)
		{
			this.GetRequiredComponents();
		}
	}

	public void Play()
	{
		base.CancelInvoke();
		if (this.randomStartFrame)
		{
			this.currentFrame = (float)this.meshCacheCount * Random.value;
		}
		else
		{
			this.currentFrame = 0f;
		}
		this.meshFilter.sharedMesh = this.meshCache[(int)this.currentFrame].sharedMesh;
		base.enabled = true;
		this.RandomizePlaySpeed();
		this.RandomRotate();
	}

	public void RandomRotate()
	{
		if (this.randomRotateX)
		{
			Quaternion localRotation = this.transformCache.localRotation;
			Vector3 eulerAngles = localRotation.eulerAngles;
			eulerAngles.x = (float)Random.Range(0, 360);
			localRotation.eulerAngles = eulerAngles;
			this.transformCache.localRotation = localRotation;
		}
		if (this.randomRotateY)
		{
			Quaternion localRotation2 = this.transformCache.localRotation;
			Vector3 eulerAngles2 = localRotation2.eulerAngles;
			eulerAngles2.y = (float)Random.Range(0, 360);
			localRotation2.eulerAngles = eulerAngles2;
			this.transformCache.localRotation = localRotation2;
		}
		if (this.randomRotateZ)
		{
			Quaternion localRotation3 = this.transformCache.localRotation;
			Vector3 eulerAngles3 = localRotation3.eulerAngles;
			eulerAngles3.z = (float)Random.Range(0, 360);
			localRotation3.eulerAngles = eulerAngles3;
			this.transformCache.localRotation = localRotation3;
		}
	}

	public void GetRequiredComponents()
	{
		this.rendererComponent = base.GetComponent<Renderer>();
	}

	public void RandomizePlaySpeed()
	{
		if (this.playSpeedRandom > 0f)
		{
			this.currentSpeed = Random.Range(this.playSpeed - this.playSpeedRandom, this.playSpeed + this.playSpeedRandom);
		}
		else
		{
			this.currentSpeed = this.playSpeed;
		}
	}

	public void FillCacheArray()
	{
		this.GetRequiredComponents();
		if (this.transformCache == null)
		{
			this.transformCache = base.transform;
		}
		this.meshFilter = this.transformCache.GetComponent<MeshFilter>();
		this.meshCacheCount = this.meshContainerFBX.childCount;
		this.meshCached = this.meshContainerFBX;
		this.meshCache = new MeshFilter[this.meshCacheCount];
		for (int i = 0; i < this.meshCacheCount; i++)
		{
			this.meshCache[i] = this.meshContainerFBX.GetChild(i).GetComponent<MeshFilter>();
		}
		this.currentFrame = (float)this.meshCacheCount * Random.value;
		this.meshFilter.sharedMesh = this.meshCache[(int)this.currentFrame].sharedMesh;
	}

	public void CheckIfMeshHasChanged()
	{
		if (this.meshCached != this.meshContainerFBX && this.meshContainerFBX != null)
		{
			this.FillCacheArray();
		}
	}

	public void Update()
	{
		this.delta = Time.deltaTime;
		this.startDelayCounter += this.delta;
		if (this.startDelayCounter > this.startDelay)
		{
			this.rendererComponent.enabled = true;
			this.Animate();
		}
		if (base.enabled)
		{
			return;
		}
		this.rendererComponent.enabled = false;
	}

	public bool PingPongFrame()
	{
		if (this.pingPongToggle)
		{
			this.currentFrame += this.currentSpeed * this.delta;
		}
		else
		{
			this.currentFrame -= this.currentSpeed * this.delta;
		}
		if (this.currentFrame <= 0f)
		{
			this.currentFrame = 0f;
			this.pingPongToggle = true;
			return true;
		}
		if (this.currentFrame >= (float)this.meshCacheCount)
		{
			this.pingPongToggle = false;
			this.currentFrame = (float)(this.meshCacheCount - 1);
			return true;
		}
		return false;
	}

	public bool NextFrame()
	{
		this.currentFrame += this.currentSpeed * this.delta;
		if (this.currentFrame > (float)(this.meshCacheCount + 1))
		{
			this.currentFrame = 0f;
			if (!this.loop)
			{
				base.enabled = false;
			}
			return true;
		}
		if (this.currentFrame >= (float)this.meshCacheCount)
		{
			this.currentFrame = (float)this.meshCacheCount - this.currentFrame;
			if (!this.loop)
			{
				base.enabled = false;
			}
			return true;
		}
		return false;
	}

	public void RandomizePropertiesAfterLoop()
	{
		if (this.randomSpeedLoop)
		{
			this.RandomizePlaySpeed();
		}
		if (this.randomRotateLoop)
		{
			this.RandomRotate();
		}
	}

	public void Animate()
	{
		if (this.rendererComponent.isVisible)
		{
			if (this.pingPong && this.PingPongFrame())
			{
				this.RandomizePropertiesAfterLoop();
			}
			else if (!this.pingPong && this.NextFrame())
			{
				this.RandomizePropertiesAfterLoop();
			}
			this.meshFilter.sharedMesh = this.meshCache[(int)this.currentFrame].sharedMesh;
		}
	}

	public MeshFilter[] meshCache;

	[HideInInspector]
	public Transform meshCached;

	public Transform meshContainerFBX;

	public float playSpeed = 1f;

	public float playSpeedRandom;

	public bool randomSpeedLoop;

	private float currentSpeed;

	[HideInInspector]
	public float currentFrame;

	[HideInInspector]
	public int meshCacheCount;

	[HideInInspector]
	public MeshFilter meshFilter;

	[HideInInspector]
	public Renderer rendererComponent;

	public float updateInterval = 0.05f;

	public bool randomRotateX;

	public bool randomRotateY;

	public bool randomRotateZ;

	public bool randomStartFrame = true;

	public bool randomRotateLoop;

	public bool loop = true;

	public bool pingPong;

	public bool playOnAwake = true;

	public Vector2 randomStartDelay = new Vector2(0f, 0f);

	private float startDelay;

	private float startDelayCounter;

	public static float updateSeed;

	private bool pingPongToggle;

	public Transform transformCache;

	public float delta;
}
