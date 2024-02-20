using System;
using System.Collections;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public interface IFishSpawner
{
	void EscapeAllFish();

	void Add(IFishController fish);

	void Remove(IFishController fish);

	IList<IFishController> Fish { get; }

	bool IsGamePaused { get; }

	IEnumerator SpawnAttackingFish(Fish template);

	Vector3 GenerateRandomPoint();

	void ShowBobberIndicator(Transform target);

	void HideBobberIndicator();

	List<Box> Shelters { get; set; }

	float CheckDepth(float x, float z);

	event EventHandler<FishCaughtEventArgs> FishCaught;

	FishController SpawnTestFish(Vector3 initialPosition, Vector3 targtPosition);

	void DestroyFishCam();
}
