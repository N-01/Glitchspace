using System;
using System.Collections;
using System.Collections.Generic;
using Streams;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class AsteroidSpawnerBasic
{
    public float interval;
    public float speed = 5;

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
                    velocity = new Vector3(Random.Range(-1, -1.2f) * speed, 0, 0),
                    scale = 0.5f + size * 0.25f,
                    health = new Cell<int>(size)
                };

                gc.SpawnEntity(asteroid, "Asteroid_" + size);

                //speed thigs up a bit
                speed += 0.05f;
                interval = Mathf.Max(interval - 0.015f, 0.1f);
            }

            yield return new WaitForSeconds(interval);
        }
    }
}

[Serializable]
public class AsteroidSpawnerTopDown
{
    public float interval;
    public float speed = 5;

    [NonSerialized]
    public EmptyStream deadAsteroidStream = new EmptyStream();

    public AsteroidSpawnerTopDown(float _interval)
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
                    position = new Vector3(Random.Range(-GameController.horizontalBoundary, GameController.horizontalBoundary), GameController.verticalBoudnary, 0),
                    velocity = new Vector3(0, -1, 0) * speed,
                    scale = 0.5f + size * 0.25f,
                    health = new Cell<int>(size)
                };

                gc.SpawnEntity(asteroid, "Asteroid_" + size);

                //speed thigs up a bit
                speed += 0.025f;
                interval = Mathf.Max(interval - 0.015f, 0.2f);
            }

            yield return new WaitForSeconds(interval);
        }
    }
}
