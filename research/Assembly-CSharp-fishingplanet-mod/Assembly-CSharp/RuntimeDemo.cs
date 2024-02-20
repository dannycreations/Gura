using System;
using DG.Tweening;
using SWS;
using UnityEngine;
using UnityEngine.Events;

public class RuntimeDemo : MonoBehaviour
{
	private void OnGUI()
	{
		this.DrawExample1();
		this.DrawExample2();
		this.DrawExample3();
		this.DrawExample4();
		this.DrawExample5();
		this.DrawExample6();
	}

	private void DrawExample1()
	{
		GUI.Label(new Rect(10f, 10f, 20f, 20f), "1:");
		string text = "Walker (Path1)";
		string text2 = "Path1 (Instantiation)";
		Vector3 vector;
		vector..ctor(-25f, 0f, 10f);
		if (!this.example1.done && GUI.Button(new Rect(30f, 10f, 100f, 20f), "Instantiate"))
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.example1.walkerPrefab, vector, Quaternion.identity);
			gameObject.name = text;
			GameObject gameObject2 = Object.Instantiate<GameObject>(this.example1.pathPrefab, vector, Quaternion.identity);
			gameObject2.name = text2;
			WaypointManager.AddPath(gameObject2);
			gameObject.GetComponent<splineMove>().SetPath(WaypointManager.Paths[text2], 0f);
			this.example1.done = true;
		}
		if (this.example1.done && GUI.Button(new Rect(30f, 10f, 100f, 20f), "Destroy"))
		{
			Object.Destroy(GameObject.Find(text));
			Object.Destroy(GameObject.Find(text2));
			WaypointManager.Paths.Remove(text2);
			this.example1.done = false;
		}
	}

	private void DrawExample2()
	{
		GUI.Label(new Rect(10f, 30f, 20f, 20f), "2:");
		if (GUI.Button(new Rect(30f, 30f, 100f, 20f), "Switch Path"))
		{
			string name = this.example2.moveRef.pathContainer.name;
			this.example2.moveRef.moveToPath = true;
			if (name == this.example2.pathName1)
			{
				this.example2.moveRef.SetPath(WaypointManager.Paths[this.example2.pathName2], 0f);
			}
			else
			{
				this.example2.moveRef.SetPath(WaypointManager.Paths[this.example2.pathName1], 0f);
			}
		}
	}

	private void DrawExample3()
	{
		GUI.Label(new Rect(10f, 50f, 20f, 20f), "3:");
		if (this.example3.moveRef.tween == null && GUI.Button(new Rect(30f, 50f, 100f, 20f), "Start"))
		{
			this.example3.moveRef.StartMove();
		}
		if (this.example3.moveRef.tween != null && GUI.Button(new Rect(30f, 50f, 100f, 20f), "Stop"))
		{
			this.example3.moveRef.Stop();
		}
		if (this.example3.moveRef.tween != null && GUI.Button(new Rect(30f, 70f, 100f, 20f), "Reset"))
		{
			this.example3.moveRef.ResetToStart();
		}
	}

	private void DrawExample4()
	{
		GUI.Label(new Rect(10f, 90f, 20f, 20f), "4:");
		if (this.example4.moveRef.tween != null && TweenExtensions.IsPlaying(this.example4.moveRef.tween) && GUI.Button(new Rect(30f, 90f, 100f, 20f), "Pause"))
		{
			this.example4.moveRef.Pause(0f);
		}
		if (this.example4.moveRef.tween != null && !TweenExtensions.IsPlaying(this.example4.moveRef.tween) && GUI.Button(new Rect(30f, 90f, 100f, 20f), "Resume"))
		{
			this.example4.moveRef.Resume();
		}
	}

	private void DrawExample5()
	{
		GUI.Label(new Rect(10f, 110f, 20f, 20f), "5:");
		if (GUI.Button(new Rect(30f, 110f, 100f, 20f), "Change Speed"))
		{
			float speed = this.example5.moveRef.speed;
			float num = 1.5f;
			if (speed == num)
			{
				num = 4f;
			}
			this.example5.moveRef.ChangeSpeed(num);
		}
	}

	private void DrawExample6()
	{
		GUI.Label(new Rect(10f, 130f, 20f, 20f), "6:");
		if (!this.example6.done && GUI.Button(new Rect(30f, 130f, 100f, 20f), "Add Event"))
		{
			EventReceiver receiver = this.example6.moveRef.GetComponent<EventReceiver>();
			UnityEvent unityEvent = this.example6.moveRef.events[1];
			unityEvent.RemoveAllListeners();
			unityEvent.AddListener(delegate
			{
				receiver.ActivateForTime(this.example6.target);
			});
			this.example6.done = true;
		}
	}

	public RuntimeDemo.ExampleClass1 example1;

	public RuntimeDemo.ExampleClass2 example2;

	public RuntimeDemo.ExampleClass3 example3;

	public RuntimeDemo.ExampleClass4 example4;

	public RuntimeDemo.ExampleClass5 example5;

	public RuntimeDemo.ExampleClass6 example6;

	[Serializable]
	public class ExampleClass1
	{
		public GameObject walkerPrefab;

		public GameObject pathPrefab;

		public bool done;
	}

	[Serializable]
	public class ExampleClass2
	{
		public splineMove moveRef;

		public string pathName1;

		public string pathName2;
	}

	[Serializable]
	public class ExampleClass3
	{
		public splineMove moveRef;
	}

	[Serializable]
	public class ExampleClass4
	{
		public splineMove moveRef;
	}

	[Serializable]
	public class ExampleClass5
	{
		public splineMove moveRef;
	}

	[Serializable]
	public class ExampleClass6
	{
		public splineMove moveRef;

		public GameObject target;

		public bool done;
	}
}
