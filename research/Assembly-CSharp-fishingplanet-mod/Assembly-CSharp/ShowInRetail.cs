using System;
using UnityEngine;

public class ShowInRetail : MonoBehaviour
{
	private void Awake()
	{
	}

	[SerializeField]
	private ShowInRetail.State WhatDoingInRetail = ShowInRetail.State.Hide;

	[SerializeField]
	private GameObject[] Objects;

	private enum State
	{
		Show,
		Hide
	}
}
