using System;
using Phy.Verlet;
using UnityEngine;

public class FishAnimationController : MonoBehaviour, IFishAnimationController
{
	private AbstractFishBody fishBody
	{
		get
		{
			return this.fish.FishObject as AbstractFishBody;
		}
	}

	internal void Start()
	{
		Animation component = base.GetComponent<Animation>();
		this.FishAnimation = base.GetComponent<Animation>();
		this.idleState = component["idle"];
		this.idleState.blendMode = 0;
		this.idleState.weight = 0f;
		this.idleState.layer = 5;
		this.idleState.enabled = true;
		this.idleState.wrapMode = 2;
		this.swimState = component["swim"];
		this.swimState.blendMode = 0;
		this.swimState.weight = 0f;
		this.swimState.layer = 5;
		this.swimState.enabled = true;
		this.swimState.wrapMode = 2;
		this.hangState = component["hang"];
		this.hangState.blendMode = 0;
		this.hangState.weight = 0f;
		this.hangState.layer = 5;
		this.hangState.enabled = true;
		this.hangState.wrapMode = 2;
		this.hangState.speed = 1.4f;
		this.shakeState = component["shake"];
		this.shakeState.blendMode = 0;
		this.shakeState.weight = 0f;
		this.shakeState.layer = 5;
		this.shakeState.enabled = true;
		this.shakeState.wrapMode = 2;
		this.shakeState.speed = 2.8f;
		this.beatState = component["beating"];
		if (this.beatState == null)
		{
			this.beatState = this.shakeState;
		}
		else
		{
			this.beatState.blendMode = 0;
			this.beatState.weight = 0f;
			this.beatState.layer = 5;
			this.beatState.enabled = true;
			this.beatState.wrapMode = 2;
			this.beatState.speed = 2f;
		}
		this.leftState = component["left"];
		this.leftState.blendMode = 0;
		this.leftState.weight = 0f;
		this.leftState.layer = 5;
		this.leftState.enabled = true;
		this.leftState.wrapMode = 2;
		this.leftState.speed = 1f;
		this.rightState = component["right"];
		this.rightState.blendMode = 0;
		this.rightState.weight = 0f;
		this.rightState.layer = 5;
		this.rightState.enabled = true;
		this.rightState.wrapMode = 2;
		this.rightState.speed = 1f;
		this.fish = base.GetComponent<FishController>();
	}

	public void OnUpdate()
	{
		if (this.fishBody == null)
		{
			return;
		}
		this.timeSpent += Time.deltaTime;
		this.speedValue.update(Time.deltaTime);
		this.amplitudeValue.update(Time.deltaTime);
		this.hangValue.update(Time.deltaTime);
		this.shakeValue.update(Time.deltaTime);
		this.beatValue.update(Time.deltaTime);
		if (this.rightValue.value == 0f)
		{
			this.leftValue.update(Time.deltaTime);
		}
		if (this.leftValue.value == 0f)
		{
			this.rightValue.update(Time.deltaTime);
		}
		if (this.playSwimImpulse && this.amplitudeValue.target == this.amplitudeValue.value)
		{
			this.playSwimImpulse = false;
		}
		if (this.playShakeImpulse && this.shakeValue.target == this.shakeValue.value && this.timeSpent > this.shakeState.length)
		{
			this.playShakeImpulse = false;
			this.shakeValue.target = 0f;
			this.fishBody.StopBendStrain();
		}
		if (this.playBeatImpulse && this.beatValue.target == this.beatValue.value && this.timeSpent > this.beatState.length)
		{
			this.playBeatImpulse = false;
			this.beatValue.target = 0f;
		}
		if (this.playLeftTurn && this.leftValue.target == this.leftValue.value)
		{
			this.playLeftTurn = false;
			this.leftValue.target = 0f;
		}
		if (this.playRightTurn && this.rightValue.target == this.rightValue.value)
		{
			this.playRightTurn = false;
			this.rightValue.target = 0f;
		}
		this.swimState.speed = this.speedValue.value * 1.5f;
		float num = Mathf.Max(0.1f, 1f - this.hangValue.value - this.shakeValue.value - this.beatValue.value);
		this.swimState.weight = Mathf.Min(num, Mathf.Max(0.1f, this.amplitudeValue.value * (1f - this.shakeValue.value - this.beatValue.value)));
		this.idleState.weight = num - this.swimState.weight;
		this.hangState.weight = this.hangValue.value;
		this.leftState.weight = this.leftValue.value;
		this.rightState.weight = this.rightValue.value;
		if (this.pullOutFlag && this.hangValue.value == 1f)
		{
			this.shakeState.weight = this.shakeValue.value * 4f;
			this.beatState.weight = this.beatValue.value * 2f;
			this.idleState.weight = 0f;
			this.swimState.weight = 0f;
			this.hangState.weight = 1f;
			this.leftState.weight = 0f;
			this.rightState.weight = 0f;
		}
	}

	public void StartBeating()
	{
		if (this.fishBody == null || this.playGroundBeating)
		{
			return;
		}
		this.beatValue.target = 1f;
		this.beatValue.value = 1f;
		this.fishBody.StartBendStrain(-1f);
		this.FinishShaking();
	}

	public void FinishBeating()
	{
		if (this.fishBody == null || this.playGroundBeating)
		{
			return;
		}
		this.beatValue.target = 0f;
		this.beatValue.value = 0f;
		this.fishBody.StopBendStrain();
	}

	public void StartShaking()
	{
		this.shakeValue.target = 1f;
		this.shakeValue.value = 1f;
		this.FinishBeating();
	}

	public void FinishShaking()
	{
		this.shakeValue.target = 0f;
		this.shakeValue.value = 0f;
	}

	public void StartSwiming()
	{
		this.amplitudeValue.target = 1f;
		this.amplitudeValue.value = 1f;
		this.FinishShaking();
		this.FinishBeating();
	}

	public void SetVisibility(bool flag)
	{
		this._isVisible = flag;
		if (!flag)
		{
			GameFactory.Player.CameraWetness.DisableCameraDrops();
		}
	}

	public void StartCameraDrops()
	{
		CameraWetness cameraWetness = GameFactory.Player.CameraWetness;
		cameraWetness.EnableCameraDrops();
		cameraWetness.DelayedStart();
	}

	public void SetValues(float speed, float amplitude, bool pullOut, bool isShocked, bool turnRight, bool turnLeft)
	{
		if (this.fishBody == null)
		{
			return;
		}
		this.pullOutFlag = pullOut;
		if (pullOut)
		{
			this.speedValue.target = 0f;
			this.amplitudeValue.target = 0f;
			this.hangValue.target = 1f;
			if (!this.playShakeImpulse && !this.playBeatImpulse && !this.playGroundBeating && Random.Range(0f, 1f) < Time.deltaTime * 0.2f * 2f)
			{
				this.playShakeImpulse = true;
				this.shakeValue.target = 1f;
				this.timeSpent = 0f;
				if (this._isVisible)
				{
					this.StartCameraDrops();
				}
				this.fishBody.StartBendStrain(-1f);
			}
			if (this.fishBody.IsLying && this.fishBody.Masses[0].Position.y > -0.15f)
			{
				if (Random.Range(0f, 1f) < Time.deltaTime * 0.2f * 2f)
				{
					if (!this.playGroundBeating)
					{
						this.playGroundBeating = true;
						this.fishBody.StartBendStrain(this.fishBody.BendStiffnessMultiplier);
					}
					else
					{
						this.playGroundBeating = false;
						this.fishBody.StopBendStrain();
					}
				}
			}
			else
			{
				this.playGroundBeating = false;
			}
			return;
		}
		if (isShocked)
		{
			this.speedValue.target = 0f;
			this.amplitudeValue.target = 0f;
			this.hangValue.target = 0f;
			return;
		}
		this.beatValue.target = 0f;
		this.shakeValue.target = 0f;
		this.hangValue.target = 0f;
		this.speedValue.target = speed;
		if (!this.playSwimImpulse)
		{
			if (amplitude == 1f)
			{
				this.playSwimImpulse = true;
				this.amplitudeValue.target = 1f;
			}
			else
			{
				this.amplitudeValue.target = Mathf.Clamp01(amplitude);
			}
		}
		if (!this.playRightTurn && turnRight)
		{
			if (this.playLeftTurn)
			{
				this.playLeftTurn = false;
				this.leftValue.target = 0f;
			}
			this.rightValue.target = 1f;
			this.playRightTurn = true;
		}
		if (!this.playLeftTurn && turnLeft)
		{
			if (this.playRightTurn)
			{
				this.playRightTurn = false;
				this.rightValue.target = 0f;
			}
			this.leftValue.target = 1f;
			this.playLeftTurn = true;
		}
	}

	public void SetAnimation(string anim)
	{
		switch (anim)
		{
		case "idle":
			this.idleState.weight = 1f;
			this.swimState.weight = 0f;
			this.hangState.weight = 0f;
			this.shakeState.weight = 0f;
			this.beatState.weight = 0f;
			this.leftState.weight = 0f;
			this.rightState.weight = 0f;
			break;
		case "swim":
			this.idleState.weight = 0f;
			this.swimState.weight = 1f;
			this.hangState.weight = 0f;
			this.shakeState.weight = 0f;
			this.beatState.weight = 0f;
			this.leftState.weight = 0f;
			this.rightState.weight = 0f;
			break;
		case "hang":
			this.idleState.weight = 0f;
			this.swimState.weight = 0f;
			this.hangState.weight = 1f;
			this.shakeState.weight = 0f;
			this.beatState.weight = 0f;
			this.leftState.weight = 0f;
			this.rightState.weight = 0f;
			break;
		case "shake":
			this.idleState.weight = 0f;
			this.swimState.weight = 0f;
			this.hangState.weight = 0f;
			this.shakeState.weight = 1f;
			this.beatState.weight = 0f;
			this.leftState.weight = 0f;
			this.rightState.weight = 0f;
			break;
		case "beating":
			this.idleState.weight = 0f;
			this.swimState.weight = 0f;
			this.hangState.weight = 0f;
			this.shakeState.weight = 0f;
			this.beatState.weight = 1f;
			this.leftState.weight = 0f;
			this.rightState.weight = 0f;
			break;
		case "left":
			this.idleState.weight = 0f;
			this.swimState.weight = 0f;
			this.hangState.weight = 0f;
			this.shakeState.weight = 0f;
			this.beatState.weight = 0f;
			this.leftState.weight = 1f;
			this.rightState.weight = 0f;
			break;
		case "right":
			this.idleState.weight = 0f;
			this.swimState.weight = 0f;
			this.hangState.weight = 0f;
			this.shakeState.weight = 0f;
			this.beatState.weight = 0f;
			this.leftState.weight = 0f;
			this.rightState.weight = 1f;
			break;
		}
	}

	private AnimationState idleState;

	private AnimationState swimState;

	private AnimationState hangState;

	private AnimationState shakeState;

	private AnimationState beatState;

	private AnimationState leftState;

	private AnimationState rightState;

	internal Animation FishAnimation;

	private float timeSpent;

	private readonly ValueChanger speedValue = new ValueChanger(0f, 0f, 2f, null);

	private readonly ValueChanger amplitudeValue = new ValueChanger(0f, 0f, 2f, null);

	private readonly ValueChanger hangValue = new ValueChanger(0f, 0f, 2f, null);

	private readonly ValueChanger shakeValue = new ValueChanger(0f, 0f, 2f, null);

	private readonly ValueChanger beatValue = new ValueChanger(0f, 0f, 2f, null);

	private readonly ValueChanger leftValue = new ValueChanger(0f, 0f, 10f, null);

	private readonly ValueChanger rightValue = new ValueChanger(0f, 0f, 10f, null);

	private bool playSwimImpulse;

	private bool playShakeImpulse;

	private bool playBeatImpulse;

	private bool playRightTurn;

	private bool playLeftTurn;

	private bool pullOutFlag;

	private bool playGroundBeating;

	private const float ShakeProbability = 0.2f;

	private FishController fish;

	private bool _isVisible = true;
}
