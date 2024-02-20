using System;
using UnityEngine;

public class AnimatedTextureObject : MonoBehaviour
{
	private void Start()
	{
		this.StartAnimation(this.currentActiveAnimation);
	}

	public bool StartAnimation(int aAnimationIndex)
	{
		if (this.animations.Length > aAnimationIndex)
		{
			this.currentActiveAnimation = aAnimationIndex;
			base.gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", this.animations[aAnimationIndex].getTexture());
			this.animations[aAnimationIndex].updateUVsForMaterial(base.gameObject.GetComponent<Renderer>().material);
			this.animations[aAnimationIndex].Stop();
			this.animations[aAnimationIndex].Play();
			this.mActiveAnimation = true;
		}
		else
		{
			this.mActiveAnimation = false;
		}
		return this.mActiveAnimation;
	}

	private void Update()
	{
		if (this.mActiveAnimation)
		{
			bool flag;
			if (base.GetComponent<Renderer>().isVisible)
			{
				flag = this.animations[this.currentActiveAnimation].Update(Time.deltaTime, base.gameObject.GetComponent<Renderer>().material, this.mXFlipped);
			}
			else
			{
				flag = this.animations[this.currentActiveAnimation].UpdateTime(Time.deltaTime);
			}
			if (flag)
			{
				this.StartAnimation(this.animations[this.currentActiveAnimation].goToAnimationOnEnd);
			}
		}
	}

	public void Play()
	{
		if (this.animations.Length > this.currentActiveAnimation)
		{
			this.animations[this.currentActiveAnimation].Play();
		}
	}

	public void Pause()
	{
		if (this.animations.Length > this.currentActiveAnimation)
		{
			this.animations[this.currentActiveAnimation].Pause();
		}
	}

	public void Stop()
	{
		if (this.animations.Length > this.currentActiveAnimation)
		{
			this.animations[this.currentActiveAnimation].Stop();
			this.mActiveAnimation = false;
		}
	}

	public void setReverseHorizontalDirection(bool to)
	{
		this.mXFlipped = to;
	}

	[SerializeField]
	public TextureAnimation[] animations;

	public int currentActiveAnimation;

	private bool mActiveAnimation;

	private bool mXFlipped;
}
