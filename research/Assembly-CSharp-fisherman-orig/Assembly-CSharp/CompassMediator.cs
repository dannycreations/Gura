using System;
using UnityEngine;

public class CompassMediator : MonoBehaviour
{
	private void Start()
	{
		this.compassController = base.gameObject.AddComponent<CompassController>();
		this.compassController.Init(GameFactory.PlayerTransform, this.compassView, this.isWindRotation);
		GameObject gameObject = GameObject.Find("3DSky");
		if (gameObject != null)
		{
			this.compassController.ChangeShift(gameObject.transform.eulerAngles.y);
		}
	}

	private CompassController compassController;

	[SerializeField]
	private CompassView compassView;

	[SerializeField]
	private bool isWindRotation = true;
}
