using System;
using UnityEngine;
using UnityEngine.UI;

public class PasswordMaskedInput : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<InputField>().inputType = 2;
		base.GetComponent<Text>().maskable = true;
	}
}
