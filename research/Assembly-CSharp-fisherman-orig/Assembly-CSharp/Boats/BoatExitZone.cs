using System;
using UnityEngine;

namespace Boats
{
	public class BoatExitZone : MonoBehaviour
	{
		private void OnTriggerEnter(Collider body)
		{
			if (body.tag == "Player")
			{
				GameFactory.Player.SetBoatInExitZone(true);
			}
		}

		private void OnTriggerExit(Collider body)
		{
			if (body.tag == "Player")
			{
				GameFactory.Player.SetBoatInExitZone(false);
			}
		}
	}
}
