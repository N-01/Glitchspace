using System;
using System.Collections;
using System.Collections.Generic;
using Streams;
using UnityEngine;
using Random = UnityEngine.Random;

public interface IAsteroidSpawner
{
    IEnumerator Spawn(GameController gc);
}

[Serializable]
public class AsteroidSpawnerBasic : IAsteroidSpawner
{
    public float interval;

    [NonSerialized]
    public EmptyStream deadAsteroidStream = new EmptyStream();

    public AsteroidSpawnerBasic(float _interval)
    {
        interval = _interval;
    }

    public IEnumerator Spawn(GameController gc)
    {
        while (true)
        {
            if (!gc.paused)
            {
                int size = Random.Range(1, 4);
                var asteroid = new Asteroid
                {
                    position = new Vector3(GameController.horizontalBoundary, Random.Range(-GameController.verticalBoudnary, GameController.verticalBoudnary), 0),
                    velocity = new Vector3(Random.Range(-5, -7), 0, 0),
                    scale = 1 + size * 0.25f,
                    health = new Cell<int>(size)
                };

                gc.SpawnEntity(asteroid, "Asteroid_" + size);
            }

            yield return new WaitForSeconds(interval);

            //speed thigs up a bit
            interval = Mathf.Max(interval - 0.05f, 0.2f);
        }
    }
}
