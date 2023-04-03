using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spawner : MonoBehaviour
{
	[SerializeField] private float spawnMinTime;
	[SerializeField] private float spawnMaxTime;
	[SerializeField] private bool enableOnAwake = true;

	public bool spawnEnabled { get; set; }

	private float spawnTimer;

	protected void Start()
	{
		// set initial timer
		spawnTimer = Random.Range(spawnMinTime, spawnMaxTime);
		spawnEnabled = enableOnAwake;
	}

	void Update()
	{
		if (!spawnEnabled) return;

		// decrement sapwn timer
		spawnTimer -= Time.deltaTime;
		if (spawnTimer < 0)
		{
			// reset spawn timer and spawn
			spawnTimer = Random.Range(spawnMinTime, spawnMaxTime);
			Spawn();
		}

	}

	public abstract void Spawn();
	public abstract void Clear();

}
