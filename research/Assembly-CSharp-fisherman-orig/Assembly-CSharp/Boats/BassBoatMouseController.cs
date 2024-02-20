using System;

namespace Boats
{
	public class BassBoatMouseController : BoatMouseController
	{
		protected override void InitMouseController(CameraController cameraController)
		{
			this._mouseLookController = cameraController.CameraMouseLook.gameObject.GetComponent<BassBoatFishingMouseController>();
		}
	}
}
