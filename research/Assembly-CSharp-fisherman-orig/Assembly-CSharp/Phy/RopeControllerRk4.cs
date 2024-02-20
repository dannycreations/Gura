using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phy
{
	public class RopeControllerRk4 : MonoBehaviour
	{
		public RopeControllerRk4()
		{
			this.sim = new RopeSimulationRk4(new Vector3(0f, 5f, 0f), 0.05f, 4f, 0.001f);
		}

		public virtual void OnGUI()
		{
			GUILayout.BeginArea(this.screenRect);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Controls: Arrows, Home, End. Space to add weight on the end.", new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		public virtual void FixedUpdate()
		{
			this.sim.Update(Time.deltaTime);
		}

		public Transform connection;

		private RopeSimulationRk4 sim;

		private const float ConnectionMoveSpeed = 3f;

		private List<GameObject> RopeVisuals = new List<GameObject>();

		private Rect screenRect;

		private bool isLastNodeHeavy;
	}
}
