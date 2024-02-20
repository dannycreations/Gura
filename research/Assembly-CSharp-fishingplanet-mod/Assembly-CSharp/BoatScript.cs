using System;
using UnityEngine;

public class BoatScript : MonoBehaviour
{
	private void Start()
	{
		Shader.SetGlobalFloat("UWIntensityRT", 1f);
	}

	private void Update()
	{
		Vector3 vector;
		vector..ctor(0f, Mathf.Sin(Time.time * 80f) * 0.01f, 0f);
		Quaternion quaternion = default(Quaternion);
		quaternion.eulerAngles = new Vector3(0f, Mathf.Sin(Time.time * 2.667f * this.speedMul + this.phase) * this.ampl * 0.7f, Mathf.Sin(Time.time * 2.667f * this.speedMul - 1.57f) * this.ampl * 0.45f);
		Quaternion quaternion2 = default(Quaternion);
		quaternion2.eulerAngles = new Vector3(0f, Mathf.Sin(Time.time * 2.667f * this.speedMul - 1.57f) * this.ampl * 0.7f, 0f);
		this.mainCamera.transform.localRotation = quaternion;
		base.transform.localRotation = quaternion2;
	}

	private void PlayAnim(string animName)
	{
		Animation component = this.hands.GetComponent<Animation>();
		Animation component2 = this.sleeves.GetComponent<Animation>();
		AnimationState animationState = component[animName];
		AnimationState animationState2 = component2[animName];
		animationState.speed = 0.3f;
		animationState2.speed = 0.3f;
		component.CrossFade(animName, 0.1f);
		component2.CrossFade(animName, 0.1f);
	}

	private void PlayAnimSlow(string animName)
	{
		Animation component = this.hands.GetComponent<Animation>();
		Animation component2 = this.sleeves.GetComponent<Animation>();
		AnimationState animationState = component[animName];
		AnimationState animationState2 = component2[animName];
		animationState.speed = 0.1f;
		animationState2.speed = 0.1f;
		component.CrossFade(animName, 0.1f);
		component2.CrossFade(animName, 0.1f);
	}

	private void PlayAnimSpeedDown(string animName)
	{
		Animation component = this.hands.GetComponent<Animation>();
		Animation component2 = this.sleeves.GetComponent<Animation>();
		AnimationState animationState = component[animName];
		AnimationState animationState2 = component2[animName];
		animationState.speed *= 0.4f;
		animationState2.speed *= 0.4f;
	}

	private void PlayAnimSpeedUp(string animName)
	{
		Animation component = this.hands.GetComponent<Animation>();
		Animation component2 = this.sleeves.GetComponent<Animation>();
		AnimationState animationState = component[animName];
		AnimationState animationState2 = component2[animName];
		animationState.speed *= 1.7f;
		animationState2.speed *= 1.7f;
	}

	private void PlayAnimReverse(string animName)
	{
		Animation component = this.hands.GetComponent<Animation>();
		Animation component2 = this.sleeves.GetComponent<Animation>();
		AnimationState animationState = component[animName];
		AnimationState animationState2 = component2[animName];
		animationState.speed = -0.3f;
		animationState2.speed = -0.3f;
	}

	private void PlayAnimReverseSlow(string animName)
	{
		Animation component = this.hands.GetComponent<Animation>();
		Animation component2 = this.sleeves.GetComponent<Animation>();
		AnimationState animationState = component[animName];
		AnimationState animationState2 = component2[animName];
		animationState.speed = -0.1f;
		animationState2.speed = -0.1f;
	}

	public GameObject hands;

	public GameObject sleeves;

	public GameObject mainCamera;

	public float speedMul = 2f;

	public float phase;

	public float ampl = 2f;
}
