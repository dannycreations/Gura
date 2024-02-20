using System;
using UnityEngine;

public class PhyLatencyTest : MonoBehaviour
{
	private void Start()
	{
		this.simPosition = base.transform.position;
	}

	private void Update()
	{
		if (this.Concurrent)
		{
			this.concurrent_Update();
		}
		else
		{
			this.single_Update();
		}
	}

	private void single_Update()
	{
		PhyLatencyTest.FrameState state = this.State;
		if (state != PhyLatencyTest.FrameState.Input)
		{
			if (state != PhyLatencyTest.FrameState.Simulate)
			{
				if (state == PhyLatencyTest.FrameState.Render)
				{
					if (this.renderTimer < this.RenderTime)
					{
						this.renderTimer += Time.deltaTime;
					}
					else
					{
						this.renderPosition = this.simPosition;
						this.s_Render();
						this.renderTimer = 0f;
						this.State = PhyLatencyTest.FrameState.Input;
					}
				}
			}
			else if (this.simTimer < this.SimTime)
			{
				this.simTimer += Time.deltaTime;
			}
			else
			{
				this.s_Simulate();
				this.simTimer = 0f;
				this.State = PhyLatencyTest.FrameState.Render;
			}
		}
		else
		{
			this.s_Input();
			this.State = PhyLatencyTest.FrameState.Simulate;
		}
	}

	private void concurrent_Update()
	{
		PhyLatencyTest.FrameState state = this.State;
		if (state != PhyLatencyTest.FrameState.Simulate)
		{
			if (state != PhyLatencyTest.FrameState.Input)
			{
				if (state == PhyLatencyTest.FrameState.Render)
				{
					if (this.renderTimer < this.RenderTime)
					{
						this.renderTimer += Time.deltaTime;
					}
					else
					{
						this.s_Render();
						this.s_Simulate();
						this.renderTimer = 0f;
						this.State = PhyLatencyTest.FrameState.Input;
					}
				}
			}
			else
			{
				this.s_Input();
				this.renderPosition = this.simPosition;
				this.State = PhyLatencyTest.FrameState.Render;
			}
		}
		else
		{
			this.State = PhyLatencyTest.FrameState.Input;
		}
	}

	private void s_Input()
	{
		this.input = Vector3.zero;
		if (Input.GetKey(273))
		{
			this.input.y = this.input.y + 1f;
		}
		if (Input.GetKey(274))
		{
			this.input.y = this.input.y - 1f;
		}
		if (Input.GetKey(275))
		{
			this.input.x = this.input.x + 1f;
		}
		if (Input.GetKey(276))
		{
			this.input.x = this.input.x - 1f;
		}
	}

	private void s_Simulate()
	{
		this.simPosition += this.input * this.Speed * Time.deltaTime;
	}

	private void s_Render()
	{
		base.transform.position = this.renderPosition;
	}

	public PhyLatencyTest.FrameState State;

	public float SimTime;

	public float SimTimeQuant;

	public float RenderTime;

	public float Speed;

	public bool Concurrent;

	private Vector3 simPosition;

	private Vector3 renderPosition;

	private Vector3 input;

	private float simTimer;

	private float renderTimer;

	private int simIterCount;

	public enum FrameState
	{
		Input,
		Simulate,
		Render
	}
}
