using System;
using System.Collections;
using DG.Tweening;
using SWS;
using UnityEngine;

public class RapidInputDemo : MonoBehaviour
{
	private void Start()
	{
		this.move = base.GetComponent<splineMove>();
		if (!this.move)
		{
			Debug.LogWarning(base.gameObject.name + " missing movement script!");
			return;
		}
		this.move.speed = 0.01f;
		this.move.StartMove();
		this.move.Pause(0f);
		this.move.speed = 0f;
	}

	private void Update()
	{
		if (this.move.tween == null || !TweenExtensions.IsActive(this.move.tween) || TweenExtensions.IsComplete(this.move.tween))
		{
			return;
		}
		if (Input.GetKeyDown(273))
		{
			if (!TweenExtensions.IsPlaying(this.move.tween))
			{
				this.move.Resume();
			}
			float num = this.currentSpeed + this.addSpeed;
			if (num >= this.topSpeed)
			{
				num = this.topSpeed;
			}
			this.move.ChangeSpeed(num);
			base.StopAllCoroutines();
			base.StartCoroutine("SlowDown");
		}
		this.speedDisplay.text = "YOUR SPEED: " + Mathf.Round(this.move.speed * 100f) / 100f;
		this.timeCounter += Time.deltaTime;
		this.timeDisplay.text = "YOUR TIME: " + Mathf.Round(this.timeCounter * 100f) / 100f;
	}

	private IEnumerator SlowDown()
	{
		yield return new WaitForSeconds(this.delay);
		float t = 0f;
		float rate = 1f / this.slowTime;
		float speed = this.move.speed;
		while (t < 1f)
		{
			t += Time.deltaTime * rate;
			this.currentSpeed = Mathf.Lerp(speed, 0f, t);
			this.move.ChangeSpeed(this.currentSpeed);
			float pitchFactor = this.maxPitch - this.minPitch;
			float pitch = this.minPitch + this.move.speed / this.topSpeed * pitchFactor;
			if (base.GetComponent<AudioSource>())
			{
				base.GetComponent<AudioSource>().pitch = Mathf.SmoothStep(base.GetComponent<AudioSource>().pitch, pitch, 0.2f);
			}
			yield return null;
		}
		yield break;
	}

	public TextMesh speedDisplay;

	public TextMesh timeDisplay;

	public float topSpeed = 15f;

	public float addSpeed = 2f;

	public float delay = 0.05f;

	public float slowTime = 0.5f;

	public float minPitch;

	public float maxPitch = 2f;

	private splineMove move;

	private float currentSpeed;

	private float timeCounter;
}
