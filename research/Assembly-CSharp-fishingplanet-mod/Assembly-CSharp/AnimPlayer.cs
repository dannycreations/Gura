using System;
using UnityEngine;

public class AnimPlayer : MonoBehaviour
{
	private void Start()
	{
		Shader.SetGlobalFloat("UWIntensityRT", 0.3f);
	}

	private void Update()
	{
	}

	private void PlayAnim(string animName)
	{
		Animation component = this.player.GetComponent<Animation>();
		AnimationState animationState = component[animName];
		animationState.speed *= 2f;
		component.CrossFade(animName, 0.1f);
	}

	public GameObject player;
}
