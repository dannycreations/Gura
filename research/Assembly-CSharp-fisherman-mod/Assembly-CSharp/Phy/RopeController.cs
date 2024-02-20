using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Phy
{
	public class RopeController : MonoBehaviour
	{
		public RopeController()
		{
			this.SimulationFactory = new Func<ISimulation>(this.CreateCpuSimulation);
		}

		public Func<ISimulation> SimulationFactory { get; set; }

		private ISimulation CreateCpuSimulation()
		{
			RopeSimulation ropeSimulation = new RopeSimulation(new Vector3(0f, 5f, 0f), 0.05f, 4f, 0.001f);
			ropeSimulation.RefreshObjectArrays(true);
			return ropeSimulation;
		}

		public virtual void Awake()
		{
			this.simulation = this.SimulationFactory();
			foreach (Mass mass in this.simulation.Masses)
			{
				GameObject gameObject = GameObject.CreatePrimitive(0);
				SphereCollider component = gameObject.GetComponent<SphereCollider>();
				component.enabled = false;
				gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
				gameObject.transform.position = mass.Position;
				this.RopeVisuals.Add(gameObject);
			}
			this.screenRect = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
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

		public virtual void Update()
		{
			Vector3 vector = Vector3.zero;
			if (Input.GetKey(273))
			{
				vector += Vector3.up * 3f;
			}
			else if (Input.GetKey(274))
			{
				vector += Vector3.down * 3f;
			}
			if (Input.GetKey(276))
			{
				vector += Vector3.back * 3f;
			}
			else if (Input.GetKey(275))
			{
				vector += Vector3.forward * 3f;
			}
			if (Input.GetKey(278))
			{
				vector += Vector3.left * 3f;
			}
			else if (Input.GetKey(279))
			{
				vector += Vector3.right * 3f;
			}
			if (Input.GetKeyDown(32))
			{
				this.TougleLastNode();
			}
			this.simulation.SetVelocity(vector);
			int num = 0;
			foreach (Mass mass in this.simulation.Masses)
			{
				this.RopeVisuals[num].transform.position = mass.Position;
				num++;
			}
			this.connection.position = this.RopeVisuals[0].transform.position;
		}

		public virtual void FixedUpdate()
		{
			this.simulation.Update(Time.deltaTime);
		}

		private void TougleLastNode()
		{
			if (this.isLastNodeHeavy)
			{
				foreach (Mass mass in this.simulation.Masses)
				{
					mass.MassValue = 0.001f;
				}
				foreach (GameObject gameObject in this.RopeVisuals)
				{
					gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
				}
			}
			else
			{
				this.simulation.Masses.Last<Mass>().MassValue = 0.003f;
				this.RopeVisuals.Last<GameObject>().transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
				int num = this.simulation.Masses.Count - 5;
				this.simulation.Masses[num].MassValue = 0.01f;
				this.RopeVisuals[num].transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
				num -= 3;
				this.simulation.Masses[num].MassValue = 0.02f;
				this.RopeVisuals[num].transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
				num--;
				this.simulation.Masses[num].MassValue = 0.03f;
				this.RopeVisuals[num].transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			}
			this.isLastNodeHeavy = !this.isLastNodeHeavy;
		}

		public Transform connection;

		protected ISimulation simulation;

		private const float ConnectionMoveSpeed = 3f;

		private List<GameObject> RopeVisuals = new List<GameObject>();

		private Rect screenRect;

		private bool isLastNodeHeavy;
	}
}
