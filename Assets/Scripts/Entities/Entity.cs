using System;
using System.Collections;
using System.Collections.Generic;
using Streams;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Entity
{
    public SerializableVector3 position, rotation, velocity;

    public float scale = 1;
    public float speed = 10;

    public Cell<int> health = new Cell<int>(1);

    public bool dead = false;

    //return value signifies if collision passed filters
    public virtual bool OnCollision(string otherTag) { return false; }

    public virtual void UpdateBehavior(GameController gc, float dt)
    {
        position += (Vector3)velocity * dt * speed;

        if (position.x >  GameController.horizontalBoundary + 1
        ||  position.x < -GameController.horizontalBoundary - 1
        ||  position.y >  GameController.verticalBoudnary   + 1
        ||  position.y < -GameController.verticalBoudnary   - 1)
        {
            dead = true;
        }
    }
}

[Serializable]
public class Asteroid : Entity
{
    public float rotationSpeed = 1;

    public Asteroid()
    {
        health.value = 3;
        speed = 2;

        rotationSpeed = Random.Range(0.5f, 2.0f);
    }

    public override bool OnCollision(string otherTag)
    {
        if (health.value > 0 && otherTag == "Blast")
            health.value--;
        else
            health.value = 0;

        return true;
    }

    public override void UpdateBehavior(GameController gc, float dt)
    {
        base.UpdateBehavior(gc, dt);
        rotation.x = rotation.x + rotationSpeed * 0.5f;
        rotation.z = rotation.z + rotationSpeed;
    }
}

[Serializable]
public class Blast : Entity
{
    public Blast()
    {
        speed = 20;
    }

    public override bool OnCollision(string otherTag)
    {
        dead = true;
        return true;
    }
}

[Serializable]
public class Ship : Entity
{
    public Ship()
    {
        health.value = 3;
        speed = 12;
    }

    public override void UpdateBehavior(GameController gc, float dt)
    {
        position += (Vector3)velocity * dt * speed;

        position.x = Mathf.Clamp(position.x, -GameController.horizontalBoundary, GameController.horizontalBoundary);
        position.y = Mathf.Clamp(position.y, -GameController.verticalBoudnary, GameController.verticalBoudnary);
    }

    public override bool OnCollision(string otherTag)
    {
        if (health.value > 0 && otherTag != "Ship")
        {
            health.value--;
            return true;
        }

        return false;
    }
}
