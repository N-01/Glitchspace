using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShipBehavior
{
	protected Entity puppet = null;
	public float shotDelay = 0.35f, timeUntilCanShoot = 0.5f;

	public void Shoot(int diretion, GameController gc)
	{
		var direction = new Vector3(diretion, 0, 0);
		var left = new Blast
		{
			velocity = direction,
			position = puppet.position + direction * 1.5f + new Vector3(0, 0.5f, 0)
		};
		gc.SpawnEntity(left, "Blast");

		var right = new Blast
		{
			velocity = direction,
			position = puppet.position + direction * 1.5f + new Vector3(0, -0.5f, 0)
		};
		gc.SpawnEntity(right, "Blast");
	}
}

[Serializable]
public class HumanBehavior : ShipBehavior
{
	public int playerId;

	public HumanBehavior(Entity e, int player)
	{
		puppet = e;
		playerId = player;
	}

	public void Update (GameController gc, float dt) {
		puppet.velocity = new Vector3(Input.GetAxis("Horizontal" + playerId), Input.GetAxis("Vertical" + playerId), 0);
		puppet.rotation.x = puppet.velocity.y * 30;

		if (Input.GetButton("Fire" + playerId) && timeUntilCanShoot <= 0)
		{
			timeUntilCanShoot = shotDelay;
			Shoot(playerId == 1 ? 1 : -1, gc);
		}
		else
			timeUntilCanShoot -= dt;
	}
}

[Serializable]
public class AiBehavior : ShipBehavior
{

	public AiBehavior(Entity e)
	{
		puppet = e;
	    shotDelay = 1;
	}

	public List<SerializableVector3> pattern = new List<SerializableVector3>
	{
		new SerializableVector3( 0,  1.0f, 0),
		new SerializableVector3( 0, -2.0f, 0),
		new SerializableVector3( 1,  1,    0),
		new SerializableVector3(-1,  1,    0),
		new SerializableVector3( 0, -1,    0)
	};

	private int counter = 0;

	public void Update(GameController gc, float dt)
	{
		timeUntilCanShoot -= dt;

		if (timeUntilCanShoot <= 0)
		{
			Shoot(-1, gc);
			timeUntilCanShoot = shotDelay;

			puppet.velocity = pattern[counter % pattern.Count];
			puppet.rotation.x = puppet.velocity.y * 10;
			counter++;
		}
	}
}
