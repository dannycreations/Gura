using System;
using UnityEngine;

public class PlantTestRoom : PhyTestRoom
{
	public override void InitSim()
	{
		UnderwaterItem1stBehaviour underwaterItem1stBehaviour = this.Controller.Init(0, Vector3.zero, UserBehaviours.FirstPerson, null, this.sim) as UnderwaterItem1stBehaviour;
		this.rootMass = underwaterItem1stBehaviour.phyObject.ForHookMass;
		base.InitSim();
	}

	private void Start()
	{
		base.OnStart();
	}

	private void LateUpdate()
	{
		base.OnLateUpdate();
	}

	private void OnApplicationQuit()
	{
		base.OnQuit();
	}

	public UnderwaterItemController Controller;

	public float MassValue;
}
