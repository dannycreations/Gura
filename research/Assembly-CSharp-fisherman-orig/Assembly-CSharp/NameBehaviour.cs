using System;
using UnityEngine;
using UnityEngine.UI;

public class NameBehaviour : MonoBehaviour
{
	public virtual void Init(string n)
	{
		this.Name.text = n;
	}

	[SerializeField]
	protected Text Name;
}
