using System;
using UnityEngine;

public class AnimationsBank : MonoBehaviour
{
	public AnimationClip[] Clips
	{
		get
		{
			return this._clips;
		}
	}

	[SerializeField]
	private AnimationClip[] _clips;
}
