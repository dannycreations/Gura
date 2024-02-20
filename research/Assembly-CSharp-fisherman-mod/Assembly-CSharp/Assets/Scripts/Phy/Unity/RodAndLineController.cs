using System;
using System.Collections.Generic;
using System.Linq;
using Phy;
using UnityEngine;

namespace Assets.Scripts.Phy.Unity
{
	public class RodAndLineController : MonoBehaviour
	{
		internal void Awake()
		{
			this.simulation = new RodAndLineSimulation(new Vector3(0f, 3f, 0f), 5f, 0.25f, 0.15f, 3f, 0.1f, 0.0005f);
			this.simulation.RefreshObjectArrays(true);
			int num = 0;
			foreach (Mass mass in this.simulation.Masses)
			{
				GameObject gameObject;
				if (num < this.simulation.RodSegCount)
				{
					gameObject = GameObject.CreatePrimitive(3);
				}
				else
				{
					gameObject = GameObject.CreatePrimitive(0);
				}
				if (num < this.simulation.RodSegCount)
				{
					float num2 = Mathf.Max(0.75f * mass.MassValue / 0.15f, 0.01f);
					gameObject.transform.localScale = new Vector3(num2, num2, num2);
				}
				else
				{
					gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
				}
				gameObject.transform.position = mass.Position;
				gameObject.transform.rotation = mass.Rotation;
				this.visuals.Add(gameObject);
				num++;
			}
			this.screenRect = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
		}

		internal void OnGUI()
		{
			GUILayout.BeginArea(this.screenRect);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Move: Arrows, Rotate: A,D,W,S. Num keyboard +/- increase/decrease line length. Space to add weight on the end.", new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			foreach (KeyValuePair<string, float> keyValuePair in PerfStats.Stats)
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				GUILayout.Label(keyValuePair.Key + ":" + keyValuePair.Value.ToString("#0.000"), new GUILayoutOption[0]);
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label(string.Concat(new object[]
			{
				"Masses/Connections simulated: ",
				this.simulation.Masses.Count,
				"/",
				this.simulation.Connections.Count
			}), new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Line length: " + this.simulation.CurrentLineLength, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		internal void Update()
		{
			Vector3 zero = Vector3.zero;
			this.simulation.SetVelocity(zero);
			Vector3 zero2 = Vector3.zero;
			this.simulation.SetAngularVelocity(zero2);
			int num = this.simulation.Masses.Count - this.visuals.Count;
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					GameObject gameObject = GameObject.CreatePrimitive(0);
					gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
					this.visuals.Insert(this.simulation.FirstLineMassIndex, gameObject);
				}
			}
			else if (num < 0)
			{
				for (int j = 0; j < -num; j++)
				{
					Object.DestroyObject(this.visuals[this.simulation.FirstLineMassIndex]);
					this.visuals.RemoveAt(this.simulation.FirstLineMassIndex);
				}
			}
			int num2 = 0;
			foreach (Mass mass in this.simulation.Masses)
			{
				this.visuals[num2].transform.position = mass.Position;
				this.visuals[num2].transform.rotation = mass.Rotation;
				num2++;
			}
		}

		internal void FixedUpdate()
		{
			if (this.isSimulationStopped)
			{
				return;
			}
			this.simulation.Update(Time.deltaTime);
		}

		private void TougleLastNode()
		{
			if (this.isLastNodeHeavy)
			{
				this.simulation.Masses.Last<Mass>().MassValue = 0.001f;
				this.visuals.Last<GameObject>().transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
				int num = (int)((float)this.simulation.Masses.Count * 0.75f);
				this.simulation.Masses[num].MassValue = 0.001f;
				this.visuals[num].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			}
			else
			{
				this.simulation.Masses.Last<Mass>().MassValue = 0.5f;
				this.visuals.Last<GameObject>().transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				int num2 = (int)((float)this.simulation.Masses.Count * 0.75f);
				this.simulation.Masses[num2].MassValue = 0.2f;
				this.visuals[num2].transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			}
			this.isLastNodeHeavy = !this.isLastNodeHeavy;
		}

		private RodAndLineSimulation simulation;

		private const float ConnectionMoveSpeed = 1.5f;

		private const float ConnectionRotationSpeed = 20f;

		private const float ChangeLineLengthSpeed = 1f;

		private const float rodMass = 0.15f;

		private readonly List<GameObject> visuals = new List<GameObject>();

		private Rect screenRect;

		private bool isLastNodeHeavy;

		private bool isSimulationStopped;
	}
}
