using System;
using UnityEngine;

[Serializable]
public class AnimatedTexture
{
	public void Init()
	{
		this.mOneFrameUstep = 1f / (float)this.numberOfFramesHorizontal;
		this.mOneFrameVstep = 1f / (float)this.numberOfFramesVertical;
	}

	public Vector2[] getUVsForFrame(int aFrameNumber)
	{
		int num = aFrameNumber / this.numberOfFramesHorizontal;
		if (this.needsFlip && aFrameNumber % this.numberOfFramesHorizontal == 0)
		{
			num--;
		}
		int num2 = aFrameNumber - num * this.numberOfFramesHorizontal;
		float num3 = this.mOneFrameUstep * (float)num2;
		float num4 = this.mOneFrameVstep * (float)num;
		Vector2[] array = new Vector2[2];
		if (this.needsFlip)
		{
			array[0] = new Vector2(num3, 1f - num4);
			array[1] = new Vector2(-this.mOneFrameUstep, -this.mOneFrameVstep);
		}
		else
		{
			array[0] = new Vector2(num3, num4);
			array[1] = new Vector2(this.mOneFrameUstep, this.mOneFrameVstep);
		}
		return array;
	}

	public Texture textureWithFrames;

	public int numberOfFramesHorizontal = 1;

	public int numberOfFramesVertical = 1;

	public bool needsFlip = true;

	private float mOneFrameUstep;

	private float mOneFrameVstep;
}
