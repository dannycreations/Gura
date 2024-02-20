using System;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB2_MeshBakerLODTestHarness : MonoBehaviour
{
	private void Start()
	{
		MB2_MeshBakerLODTestHarness.harness = this;
		this.manager = MB2_LODManager.Manager();
		this.manager.checkScheduler.FORCE_CHECK_EVERY_FRAME = true;
		this.cam = base.GetComponent<MB2_LODCamera>();
		this.manager.LOG_LEVEL = MB2_LogLevel.trace;
		this.lod.LOG_LEVEL = MB2_LogLevel.trace;
	}

	private void FixedUpdate()
	{
		if (this.currentTest != null && this.currentTest.whenToAct == MB2_MeshBakerLODTestHarness.Test.When.fixedUpdate)
		{
			this.currentTest.DoActions();
		}
	}

	private void Update()
	{
		if (this.currentTest != null && this.currentTest.whenToAct == MB2_MeshBakerLODTestHarness.Test.When.update)
		{
			this.currentTest.DoActions();
		}
		for (int i = 0; i < this.manager.bakers.Length; i++)
		{
			this.manager.bakers[i].baker.LOG_LEVEL = MB2_LogLevel.trace;
		}
	}

	private void LateUpdate()
	{
		if (this.currentTest != null)
		{
			this.currentTest.CheckStateBetweenUpdateAndBake();
		}
		if (this.currentTest != null && this.currentTest.whenToAct == MB2_MeshBakerLODTestHarness.Test.When.lateUpdate)
		{
			this.currentTest.DoActions();
		}
	}

	private void OnPreRender()
	{
		if (this.currentTest != null && this.currentTest.whenToAct == MB2_MeshBakerLODTestHarness.Test.When.preRender)
		{
			this.currentTest.DoActions();
		}
	}

	private void OnPostRender()
	{
		if (this.currentTest != null)
		{
			this.currentTest.CheckStateAfterBake();
		}
		if (this.currentTest != null && this.currentTest.whenToAct == MB2_MeshBakerLODTestHarness.Test.When.postRender)
		{
			this.currentTest.DoActions();
		}
		this.currentTest = null;
		if (this.testNum >= this.tests.Length)
		{
			Debug.Log("Done testing");
			return;
		}
		Debug.Log(string.Concat(new object[]
		{
			"fr=",
			Time.frameCount,
			" ======= starting test ",
			this.testNum
		}));
		this.currentTest = this.tests[this.testNum++];
		this.currentTest.SetupTest(this.lod, this.cam, this.manager);
	}

	public static MB2_MeshBakerLODTestHarness harness;

	private MB2_LODManager manager;

	private MB2_LODCamera cam;

	public MB2_LOD lod;

	private MB2_MeshBakerLODTestHarness.Test[] tests = new MB2_MeshBakerLODTestHarness.Test[]
	{
		new MB2_MeshBakerLODTestHarness.Test(2, true, false, true, MB2_LODOperation.toAdd, 2, 4, true, false, MB2_LODOperation.none, 2, 2, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(3, true, true, true, MB2_LODOperation.update, 3, 2, true, false, MB2_LODOperation.none, 3, 3, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(1, true, true, true, MB2_LODOperation.update, 1, 3, true, false, MB2_LODOperation.none, 1, 1, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(4, true, true, true, MB2_LODOperation.delete, 4, 1, false, false, MB2_LODOperation.none, 4, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(2, false, false, true, MB2_LODOperation.toAdd, 2, 4, false, true, MB2_LODOperation.toAdd, 2, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(4, true, false, false, MB2_LODOperation.none, 4, 4, false, false, MB2_LODOperation.none, 4, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(2, false, false, true, MB2_LODOperation.toAdd, 2, 4, false, true, MB2_LODOperation.toAdd, 2, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(0, false, false, false, MB2_LODOperation.none, 0, 0, false, false, MB2_LODOperation.none, 0, 0, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(4, false, false, false, MB2_LODOperation.none, 4, 4, false, false, MB2_LODOperation.none, 4, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(2, false, false, true, MB2_LODOperation.toAdd, 2, 4, false, true, MB2_LODOperation.toAdd, 2, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(0, false, false, false, MB2_LODOperation.none, 0, 0, false, false, MB2_LODOperation.none, 0, 0, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(3, true, false, true, MB2_LODOperation.toAdd, 3, 0, true, false, MB2_LODOperation.none, 3, 3, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(4, true, true, true, MB2_LODOperation.delete, 4, 3, false, false, MB2_LODOperation.none, 4, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(2, true, false, true, MB2_LODOperation.toAdd, 2, 4, true, false, MB2_LODOperation.none, 2, 2, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(4, false, true, true, MB2_LODOperation.delete, 4, 2, true, true, MB2_LODOperation.delete, 4, 2, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(3, true, true, true, MB2_LODOperation.update, 3, 2, true, false, MB2_LODOperation.none, 3, 3, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(0, false, true, true, MB2_LODOperation.delete, 0, 3, true, true, MB2_LODOperation.delete, 0, 3, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(4, true, true, true, MB2_LODOperation.delete, 4, 3, false, false, MB2_LODOperation.none, 4, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(4, true, false, false, MB2_LODOperation.none, 4, 4, false, false, MB2_LODOperation.none, 4, 4, MB2_MeshBakerLODTestHarness.Test.Action.disable, MB2_MeshBakerLODTestHarness.Test.When.lateUpdate, null),
		new MB2_MeshBakerLODTestHarness.Test(1, true, false, false, MB2_LODOperation.none, 4, 4, false, false, MB2_LODOperation.none, 4, 4, MB2_MeshBakerLODTestHarness.Test.Action.enable, MB2_MeshBakerLODTestHarness.Test.When.lateUpdate, null),
		new MB2_MeshBakerLODTestHarness.Test(1, true, false, true, MB2_LODOperation.toAdd, 1, 4, true, false, MB2_LODOperation.none, 1, 1, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(4, false, true, true, MB2_LODOperation.delete, 4, 1, true, true, MB2_LODOperation.delete, 4, 1, MB2_MeshBakerLODTestHarness.Test.Action.disable, MB2_MeshBakerLODTestHarness.Test.When.lateUpdate, null),
		new MB2_MeshBakerLODTestHarness.Test(1, true, true, true, MB2_LODOperation.delete, 4, 1, false, false, MB2_LODOperation.none, 4, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null),
		new MB2_MeshBakerLODTestHarness.Test(2, true, false, true, MB2_LODOperation.toAdd, 2, 4, true, false, MB2_LODOperation.none, 2, 2, MB2_MeshBakerLODTestHarness.Test.Action.enable, MB2_MeshBakerLODTestHarness.Test.When.fixedUpdate, null),
		new MB2_MeshBakerLODTestHarness.Test(3, true, true, true, MB2_LODOperation.update, 1, 2, true, false, MB2_LODOperation.none, 1, 1, MB2_MeshBakerLODTestHarness.Test.Action.custom, MB2_MeshBakerLODTestHarness.Test.When.fixedUpdate, new MB2_MeshBakerLODTestHarness.ActionForceToLevel(1)),
		new MB2_MeshBakerLODTestHarness.Test(4, true, true, false, MB2_LODOperation.none, 1, 1, true, false, MB2_LODOperation.none, 1, 1, MB2_MeshBakerLODTestHarness.Test.Action.custom, MB2_MeshBakerLODTestHarness.Test.When.preRender, new MB2_MeshBakerLODTestHarness.ActionForceToLevel(-1)),
		new MB2_MeshBakerLODTestHarness.Test(3, false, true, true, MB2_LODOperation.update, 3, 1, true, true, MB2_LODOperation.delete, 4, 1, MB2_MeshBakerLODTestHarness.Test.Action.destroy, MB2_MeshBakerLODTestHarness.Test.When.preRender, null),
		new MB2_MeshBakerLODTestHarness.Test(3, true, true, true, MB2_LODOperation.delete, 4, 1, false, false, MB2_LODOperation.none, 4, 4, MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When.update, null)
	};

	private MB2_MeshBakerLODTestHarness.Test currentTest;

	private int testNum;

	private class Test
	{
		public Test()
		{
		}

		public Test(int level, bool doBake, bool int_inCombined, bool int_inQueue, MB2_LODOperation int_action, int int_nextIdx, int int_currentIdx, bool fin_inCombined, bool fin_inQueue, MB2_LODOperation fin_action, int fin_nextIdx, int fin_currentIdx, MB2_MeshBakerLODTestHarness.Test.Action act = MB2_MeshBakerLODTestHarness.Test.Action.none, MB2_MeshBakerLODTestHarness.Test.When when = MB2_MeshBakerLODTestHarness.Test.When.update, MB2_MeshBakerLODTestHarness.CustomAction a = null)
		{
			this.level = level;
			this.doBake = doBake;
			this.int_inCombined = int_inCombined;
			this.int_inQueue = int_inQueue;
			this.int_action = int_action;
			this.int_currentIdx = int_currentIdx;
			this.int_nextIdx = int_nextIdx;
			this.fin_inCombined = fin_inCombined;
			this.fin_inQueue = fin_inQueue;
			this.fin_action = fin_action;
			this.fin_currentIdx = fin_currentIdx;
			this.fin_nextIdx = fin_nextIdx;
			this.act = act;
			this.whenToAct = when;
			this.customAction = a;
		}

		public void SetupTest(MB2_LOD targ, MB2_LODCamera cam, MB2_LODManager m)
		{
			this.target = targ;
			this.camera = cam;
			float num = this.distances[this.level];
			Debug.Log(string.Concat(new object[]
			{
				"fr=",
				Time.frameCount,
				" PreRender SetupTest moving to dist ",
				num
			}));
			Vector3 position = cam.transform.position;
			position.z = num;
			cam.transform.position = position;
			this.manager = m;
			this.manager.baking_enabled = this.doBake;
		}

		public void CheckStateBetweenUpdateAndBake()
		{
			Debug.Log("fr=" + Time.frameCount + " CheckStateBetweenUpdateAndBake");
			this.target.CheckState(this.int_inCombined, this.int_inQueue, this.int_action, this.int_nextIdx, this.int_currentIdx);
		}

		public void CheckStateAfterBake()
		{
			Debug.Log("fr=" + Time.frameCount + " CheckStateAfterBake");
			this.target.CheckState(this.fin_inCombined, this.fin_inQueue, this.fin_action, this.fin_nextIdx, this.fin_currentIdx);
		}

		public void DoActions()
		{
			Debug.Log(string.Concat(new object[]
			{
				"fr=",
				Time.frameCount,
				" DoActions ",
				this.act,
				" ",
				this.whenToAct
			}));
			if (this.act == MB2_MeshBakerLODTestHarness.Test.Action.activate)
			{
				MB2_Version.SetActive(this.target.gameObject, true);
			}
			if (this.act == MB2_MeshBakerLODTestHarness.Test.Action.deactivate)
			{
				MB2_Version.SetActive(this.target.gameObject, false);
			}
			if (this.act == MB2_MeshBakerLODTestHarness.Test.Action.enable)
			{
				this.target.enabled = true;
			}
			if (this.act == MB2_MeshBakerLODTestHarness.Test.Action.disable)
			{
				this.target.enabled = false;
			}
			if (this.act == MB2_MeshBakerLODTestHarness.Test.Action.destroy)
			{
				MB2_LODManager.Manager().LODDestroy(this.target);
			}
			if (this.act == MB2_MeshBakerLODTestHarness.Test.Action.custom)
			{
				this.customAction.DoAction();
			}
		}

		public MB2_LOD target;

		public MB2_LODCamera camera;

		public MB2_LODManager manager;

		public int level;

		public bool doBake;

		public MB2_MeshBakerLODTestHarness.Test.Action act;

		public MB2_MeshBakerLODTestHarness.Test.When whenToAct = MB2_MeshBakerLODTestHarness.Test.When.update;

		public float[] distances = new float[] { 10f, 30f, 70f, 150f, 250f };

		public bool int_inCombined;

		public bool int_inQueue;

		public MB2_LODOperation int_action;

		public int int_currentIdx;

		public int int_nextIdx;

		public bool fin_inCombined;

		public bool fin_inQueue;

		public MB2_LODOperation fin_action;

		public int fin_currentIdx;

		public int fin_nextIdx;

		public MB2_MeshBakerLODTestHarness.CustomAction customAction;

		public enum Action
		{
			none,
			disable,
			enable,
			destroy,
			activate,
			deactivate,
			custom
		}

		public enum When
		{
			fixedUpdate,
			update,
			lateUpdate,
			preRender,
			postRender
		}
	}

	private interface CustomAction
	{
		void DoAction();
	}

	private class ActionForceToLevel : MB2_MeshBakerLODTestHarness.CustomAction
	{
		public ActionForceToLevel(int l)
		{
			this.level = l;
		}

		public void DoAction()
		{
			MB2_MeshBakerLODTestHarness.harness.lod.forceToLevel = this.level;
			Debug.Log("ActionForceToLevel forcing to " + this.level);
		}

		private int level;
	}
}
