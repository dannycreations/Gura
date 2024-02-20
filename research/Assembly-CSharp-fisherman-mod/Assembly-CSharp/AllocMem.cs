using System;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class AllocMem : MonoBehaviour
{
	public void Start()
	{
		base.useGUILayout = false;
	}

	public void OnGUI()
	{
		if (!this.show || (!Application.isPlaying && !this.showInEditor))
		{
			return;
		}
		int num = GC.CollectionCount(0);
		if (this.lastCollectNum != (float)num)
		{
			this.lastCollectNum = (float)num;
			this.delta = Time.realtimeSinceStartup - this.lastCollect;
			this.lastCollect = Time.realtimeSinceStartup;
			this.lastDeltaTime = Time.deltaTime;
			this.collectAlloc = this.allocMem;
		}
		this.allocMem = (int)GC.GetTotalMemory(false);
		this.peakAlloc = ((this.allocMem <= this.peakAlloc) ? this.peakAlloc : this.allocMem);
		if (Time.realtimeSinceStartup - this.lastAllocSet > 0.3f)
		{
			int num2 = this.allocMem - this.lastAllocMemory;
			this.lastAllocMemory = this.allocMem;
			this.lastAllocSet = Time.realtimeSinceStartup;
			if (num2 >= 0)
			{
				this.allocRate = num2;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Currently allocated      ");
		stringBuilder.Append(((float)this.allocMem / 1000000f).ToString("0"));
		stringBuilder.Append("mb\n");
		stringBuilder.Append("Peak allocated           ");
		stringBuilder.Append(((float)this.peakAlloc / 1000000f).ToString("0"));
		stringBuilder.Append("mb (last\tcollect ");
		stringBuilder.Append(((float)this.collectAlloc / 1000000f).ToString("0"));
		stringBuilder.Append(" mb)\n");
		stringBuilder.Append("Allocation rate          ");
		stringBuilder.Append(((float)this.allocRate / 1000000f).ToString("0.0"));
		stringBuilder.Append("mb\n");
		stringBuilder.Append("Collection frequency     ");
		stringBuilder.Append(this.delta.ToString("0.00"));
		stringBuilder.Append("s\n");
		stringBuilder.Append("Last collect delta\t      ");
		stringBuilder.Append(this.lastDeltaTime.ToString("0.000"));
		stringBuilder.Append("s (");
		stringBuilder.Append((1f / this.lastDeltaTime).ToString("0.0"));
		stringBuilder.Append(" fps)");
		if (this.showFPS)
		{
			stringBuilder.Append("\n" + (1f / Time.deltaTime).ToString("0.0") + " fps");
		}
		GUI.Box(new Rect(5f, 5f, 310f, (float)(80 + ((!this.showFPS) ? 0 : 16))), string.Empty);
		GUI.Label(new Rect(10f, 5f, 1000f, 200f), stringBuilder.ToString());
	}

	public bool show = true;

	public bool showFPS;

	public bool showInEditor;

	private float lastCollect;

	private float lastCollectNum;

	private float delta;

	private float lastDeltaTime;

	private int allocRate;

	private int lastAllocMemory;

	private float lastAllocSet = -9999f;

	private int allocMem;

	private int collectAlloc;

	private int peakAlloc;
}
