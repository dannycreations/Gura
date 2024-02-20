using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class FirstSelectedObject : MonoBehaviour
{
	private void OnEnable()
	{
		EventSystem.current.SetSelectedGameObject(base.gameObject);
	}
}
