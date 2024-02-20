using System;
using UnityEngine;

[Serializable]
public class TextureAnimation
{
	public void InitTexture()
	{
		this.animatedTex.Init();
		this.mDeltaFrames = this.endAtFrame - this.startAtFrame;
		this.mTimePerFrame = this.animationDuration / (float)(this.mDeltaFrames + 1);
		this.mUVDataThisFrame = this.animatedTex.getUVsForFrame(this.startAtFrame);
	}

	public void Play()
	{
		this.mPlaying = true;
	}

	public void Pause()
	{
		this.mPlaying = false;
	}

	public void Stop()
	{
		this.Pause();
		this.mTimePlayed = 0f;
	}

	public bool Update(float aDeltaTime, Material aMaterial, bool aXFlipped)
	{
		this.UpdateTime(aDeltaTime);
		if (this.mPlaying)
		{
			if (this.mDeltaFrames != 0)
			{
				int num;
				if (this.mDeltaFrames > 0)
				{
					num = Mathf.FloorToInt(this.mTimeToUseForFrameCalc / this.mTimePerFrame);
				}
				else
				{
					num = Mathf.CeilToInt(this.mTimeToUseForFrameCalc / this.mTimePerFrame);
				}
				int num2 = this.startAtFrame + num;
				if (this.mTimePlayed >= this.animationDuration && !this.pingpong)
				{
					this.mUVDataThisFrame = this.animatedTex.getUVsForFrame(this.endAtFrame);
				}
				else
				{
					this.mUVDataThisFrame = this.animatedTex.getUVsForFrame(num2);
				}
			}
			if (aXFlipped)
			{
				Vector2[] array = this.mUVDataThisFrame;
				int num3 = 0;
				array[num3].x = array[num3].x + this.mUVDataThisFrame[1].x;
				this.mUVDataThisFrame[1].x = -this.mUVDataThisFrame[1].x;
			}
			this.updateUVsForMaterial(aMaterial);
		}
		return this.mSendEndedNotification;
	}

	public void updateUVsForMaterial(Material aMaterial)
	{
		aMaterial.SetTextureOffset("_MainTex", this.mUVDataThisFrame[0]);
		aMaterial.SetTextureScale("_MainTex", this.mUVDataThisFrame[1]);
	}

	public bool UpdateTime(float aDeltaTime)
	{
		this.mSendEndedNotification = false;
		if (this.mPlaying)
		{
			this.mTimePlayed += aDeltaTime;
			this.mTimeToUseForFrameCalc = this.mTimePlayed;
			if (this.mTimePlayed > this.animationDuration)
			{
				if (this.loop)
				{
					this.mTimePlayed -= this.animationDuration;
					this.mTimeToUseForFrameCalc -= this.animationDuration;
				}
				else if (this.pingpong)
				{
					float num = this.animationDuration - this.mTimePerFrame * 2f;
					float num2 = this.animationDuration + num;
					if (this.mTimePlayed <= num2)
					{
						this.mTimeToUseForFrameCalc = this.mTimePerFrame + num - (this.mTimePlayed - this.animationDuration);
					}
					else
					{
						this.mTimePlayed -= num2;
						this.mTimeToUseForFrameCalc -= num2;
					}
				}
				else
				{
					this.mTimePlayed = this.animationDuration;
					this.mTimeToUseForFrameCalc = this.animationDuration;
					if (this.goToAnimationOnEnd >= 0)
					{
						this.mSendEndedNotification = true;
					}
				}
			}
		}
		return this.mSendEndedNotification;
	}

	public Texture getTexture()
	{
		this.InitTexture();
		return this.animatedTex.textureWithFrames;
	}

	public AnimatedTexture animatedTex;

	public int startAtFrame;

	public int endAtFrame;

	public float animationDuration = 1f;

	public bool loop = true;

	public bool pingpong;

	public int goToAnimationOnEnd = -1;

	private float mTimePlayed;

	private bool mPlaying;

	private int mDeltaFrames;

	private float mTimePerFrame;

	private Vector2[] mUVDataThisFrame;

	private float mTimeToUseForFrameCalc;

	private bool mSendEndedNotification;
}
