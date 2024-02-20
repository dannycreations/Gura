using System;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
	private void Update()
	{
		for (int i = 0; i < this.vortexPair0.Length; i++)
		{
			this.vortexPair0[i].strength = this.strengthVortexPair0;
		}
		for (int j = 0; j < this.vortexPair1.Length; j++)
		{
			this.vortexPair1[j].strength = this.strengthVortexPair1;
		}
		for (int k = 0; k < this.vortexPair2.Length; k++)
		{
			this.vortexPair2[k].strength = this.strengthVortexPair2;
		}
		for (int l = 0; l < this.sideAddFluid.Length; l++)
		{
			this.sideAddFluid[l].strength = this.strengthSideAddFluid;
		}
		this.skimmerClockwise0.Rotate(Vector3.up, Mathf.Min(0.04f, this.strengthVortexPair0 * 20f * Time.deltaTime));
		this.skimmerCounterClockwise0.Rotate(Vector3.up, -Mathf.Min(0.04f, this.strengthVortexPair0 * 20f * Time.deltaTime));
		this.skimmerClockwise1.Rotate(Vector3.up, Mathf.Min(0.04f, this.strengthVortexPair1 * 20f * Time.deltaTime));
		this.skimmerCounterClockwise1.Rotate(Vector3.up, -Mathf.Min(0.04f, this.strengthVortexPair1 * 20f * Time.deltaTime));
		this.skimmerClockwise2.Rotate(Vector3.up, Mathf.Min(0.04f, this.strengthVortexPair2 * 20f * Time.deltaTime));
		this.skimmerCounterClockwise2.Rotate(Vector3.up, -Mathf.Min(0.04f, this.strengthVortexPair2 * 20f * Time.deltaTime));
	}

	[SerializeField]
	private FlowForceField[] vortexPair0;

	[SerializeField]
	private Transform skimmerClockwise0;

	[SerializeField]
	private Transform skimmerCounterClockwise0;

	[SerializeField]
	private float strengthVortexPair0;

	[SerializeField]
	private FlowForceField[] vortexPair1;

	[SerializeField]
	private Transform skimmerClockwise1;

	[SerializeField]
	private Transform skimmerCounterClockwise1;

	[SerializeField]
	private float strengthVortexPair1;

	[SerializeField]
	private FlowForceField[] vortexPair2;

	[SerializeField]
	private Transform skimmerClockwise2;

	[SerializeField]
	private Transform skimmerCounterClockwise2;

	[SerializeField]
	private float strengthVortexPair2;

	[SerializeField]
	private FluidAddField[] sideAddFluid;

	[SerializeField]
	private float strengthSideAddFluid;
}
