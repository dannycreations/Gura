using System;

namespace Boats
{
	public class ZodiacMouseController : BoatMouseController
	{
		protected override void InitMouseController(CameraController cameraController)
		{
			this._mouseLookController = cameraController.CameraMouseLook.gameObject.GetComponent<ZodiacFishingMouseController>();
			this.FollowBoneController = base.GetComponent<ZodiacFollowBoneMouseController>();
		}
	}
}
