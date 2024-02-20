using System;
using UnityEngine;

public class AnimClips : MonoBehaviour
{
	public AnimationClip[] HandsClips
	{
		get
		{
			return this._handsClips;
		}
	}

	public AnimationClip[] ReelClips
	{
		get
		{
			return this._reelClips;
		}
	}

	[SerializeField]
	private AnimationClip[] _handsClips;

	[SerializeField]
	private AnimationClip[] _reelClips;
}
