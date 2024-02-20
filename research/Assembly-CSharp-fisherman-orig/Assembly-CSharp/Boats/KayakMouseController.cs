using System;

namespace Boats
{
	public class KayakMouseController : BoatMouseController
	{
		protected override void InitMouseController(CameraController cameraController)
		{
			this._mouseLookController = cameraController.CameraMouseLook.gameObject.GetComponent<KayakFishingMouseController>();
		}
	}
}
