using System;
using System.Collections.Generic;
using UnityEngine;

public class RotatingBridge : MonoBehaviour
{
	private void Awake()
	{
		this._triggeredBoats = new List<Collider>[]
		{
			new List<Collider>(),
			new List<Collider>()
		};
		SimpleTrigger boatsTrigger = this._boatsTrigger1;
		boatsTrigger.ETriggerEnter = (Action<Collider>)Delegate.Combine(boatsTrigger.ETriggerEnter, new Action<Collider>(this.OnBoatEnter1));
		SimpleTrigger boatsTrigger2 = this._boatsTrigger1;
		boatsTrigger2.ETriggerExit = (Action<Collider>)Delegate.Combine(boatsTrigger2.ETriggerExit, new Action<Collider>(this.OnBoatExit1));
		SimpleTrigger boatsTrigger3 = this._boatsTrigger2;
		boatsTrigger3.ETriggerEnter = (Action<Collider>)Delegate.Combine(boatsTrigger3.ETriggerEnter, new Action<Collider>(this.OnBoatEnter2));
		SimpleTrigger boatsTrigger4 = this._boatsTrigger2;
		boatsTrigger4.ETriggerExit = (Action<Collider>)Delegate.Combine(boatsTrigger4.ETriggerExit, new Action<Collider>(this.OnBoatExit2));
		GroupObjectSelector autoSelector = this._autoSelector;
		autoSelector.EFrozen = (Action)Delegate.Combine(autoSelector.EFrozen, new Action(this.OnReadyForTurn));
		this._immediateTurnTill = Time.time + this._immediateTurnDelay;
	}

	private void Update()
	{
		if (this._immediateTurnTill > 0f && this._immediateTurnTill < Time.time)
		{
			this._immediateTurnTill = -1f;
		}
		if (this._turnStartedAt > 0f)
		{
			float num = Time.time - this._turnStartedAt;
			if (num < this._turnTime)
			{
				float num2 = num / this._turnTime;
				if (this._isBackTurn)
				{
					this._root.transform.localEulerAngles = new Vector3(0f, Mathf.SmoothStep(this._maxAngle, 0f, num2), 0f);
				}
				else
				{
					this._root.transform.localEulerAngles = new Vector3(0f, Mathf.SmoothStep(0f, this._maxAngle, num2), 0f);
				}
			}
			else if (this._contrTurnRequested)
			{
				this._contrTurnRequested = false;
				this._turnStartedAt = Time.time;
				this._isBackTurn = !this._isBackTurn;
			}
			else if (this._isBackTurn)
			{
				this._turnStartedAt = -1f;
				this._root.transform.localEulerAngles = Vector3.zero;
				this._autoSelector.UnFreeze();
			}
			else
			{
				this._turnStartedAt = -1f;
				this._root.transform.localEulerAngles = new Vector3(0f, this._maxAngle, 0f);
			}
		}
	}

	private void OnBoatEnter1(Collider obj)
	{
		this.OnBoatEnter(0, obj);
	}

	private void OnBoatExit1(Collider obj)
	{
		this.OnBoatExit(0, obj);
	}

	private void OnBoatEnter2(Collider obj)
	{
		this.OnBoatEnter(1, obj);
	}

	private void OnBoatExit2(Collider obj)
	{
		this.OnBoatExit(1, obj);
	}

	private void OnBoatEnter(int i, Collider obj)
	{
		this._triggeredBoats[i].Add(obj);
		if (!this._triggeredBoats[1 - i].Contains(obj))
		{
			this._counter++;
			if (this._counter == 1)
			{
				if (this._immediateTurnTill > 0f)
				{
					this._root.transform.localEulerAngles = new Vector3(0f, this._maxAngle, 0f);
				}
				this._autoSelector.FreezeRequest();
			}
		}
	}

	private void OnBoatExit(int i, Collider obj)
	{
		this._triggeredBoats[i].Remove(obj);
		if (this._triggeredBoats[1 - i].Contains(obj))
		{
			this._counter--;
			if (this._counter == 0)
			{
				if (this._turnStartedAt < 0f)
				{
					this._turnStartedAt = Time.time;
					this._isBackTurn = true;
				}
				else if (!this._isBackTurn)
				{
					this._contrTurnRequested = true;
				}
			}
			else if (this._counter < 0)
			{
				LogHelper.Error("{0} Invalid trigger counter {1}", new object[] { base.name, this._counter });
			}
		}
	}

	private void OnReadyForTurn()
	{
		if (this._immediateTurnTill > 0f)
		{
			return;
		}
		if (this._turnStartedAt < 0f)
		{
			this._isBackTurn = false;
			this._turnStartedAt = Time.time;
		}
		else if (this._isBackTurn)
		{
			this._contrTurnRequested = true;
		}
	}

	private void OnDestroy()
	{
		this._triggeredBoats = null;
		SimpleTrigger boatsTrigger = this._boatsTrigger1;
		boatsTrigger.ETriggerEnter = (Action<Collider>)Delegate.Remove(boatsTrigger.ETriggerEnter, new Action<Collider>(this.OnBoatEnter1));
		SimpleTrigger boatsTrigger2 = this._boatsTrigger1;
		boatsTrigger2.ETriggerExit = (Action<Collider>)Delegate.Remove(boatsTrigger2.ETriggerExit, new Action<Collider>(this.OnBoatExit1));
		SimpleTrigger boatsTrigger3 = this._boatsTrigger2;
		boatsTrigger3.ETriggerEnter = (Action<Collider>)Delegate.Remove(boatsTrigger3.ETriggerEnter, new Action<Collider>(this.OnBoatEnter2));
		SimpleTrigger boatsTrigger4 = this._boatsTrigger2;
		boatsTrigger4.ETriggerExit = (Action<Collider>)Delegate.Remove(boatsTrigger4.ETriggerExit, new Action<Collider>(this.OnBoatExit2));
		GroupObjectSelector autoSelector = this._autoSelector;
		autoSelector.EFrozen = (Action)Delegate.Remove(autoSelector.EFrozen, new Action(this.OnReadyForTurn));
	}

	[SerializeField]
	private Transform _root;

	[SerializeField]
	private GroupObjectSelector _autoSelector;

	[SerializeField]
	private float _maxAngle = 90f;

	[SerializeField]
	private float _turnTime = 20f;

	[SerializeField]
	private SimpleTrigger _boatsTrigger1;

	[SerializeField]
	private SimpleTrigger _boatsTrigger2;

	[SerializeField]
	private float _immediateTurnDelay = 0.1f;

	private int _counter;

	private float _immediateTurnTill = -1f;

	private float _turnStartedAt = -1f;

	private bool _isBackTurn;

	private bool _contrTurnRequested;

	private List<Collider>[] _triggeredBoats;
}
