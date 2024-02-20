using System;
using UnityEngine;

public class Player3dController : MonoBehaviour
{
	private void Awake()
	{
		GameObject gameObject = base.gameObject.transform.Find("userName").gameObject;
		this.label = gameObject.GetComponent<TextMesh>();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void setUserName(string name)
	{
		this.label.text = name;
	}

	public void onNewTransform(Vector3 newPosition, Quaternion newRotation)
	{
		float magnitude = (newPosition - base.transform.position).magnitude;
		if (magnitude > 0.0001f)
		{
			base.transform.Translate(0f, 0f, magnitude);
		}
		else
		{
			base.transform.rotation = newRotation;
		}
	}

	private TextMesh label;

	private const float PRECISION = 0.0001f;
}
