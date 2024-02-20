using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ConditionalInvoke : MonoBehaviour
{
	public void Invoke()
	{
		int i;
		for (i = 0; i < this.objectToCheck.Length; i++)
		{
			if (this.objectToCheck[i].activeSelf)
			{
				break;
			}
		}
		if (i != this.objectToCheck.Length && this.Invokation != null)
		{
			this.Invokation.Invoke();
		}
		if (i == this.objectToCheck.Length && this.InvokationFailedCondition != null)
		{
			this.InvokationFailedCondition.Invoke();
		}
	}

	public void InvokeDelayed()
	{
		base.StopAllCoroutines();
		base.StartCoroutine("InvokeAfterTime", 0.4f);
	}

	private IEnumerator InvokeAfterTime(float time)
	{
		yield return new WaitForSeconds(time);
		this.Invoke();
		yield break;
	}

	public UnityEvent Invokation;

	public UnityEvent InvokationFailedCondition;

	public GameObject[] objectToCheck;
}
