using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBehavior
{
	protected Entity puppet = null;
	public float shotDelay = 0.35f, timeSinceLastShot = 0;

	public void Shoot(int diretion, GameController gc)
	{
		var direction = new Vector3(diretion, 0, 0);
		var left = new Blast
		{
			velocity = direction,
			position = puppet.position + direction + new Vector3(0, 1, 0)
		};
		gc.SpawnEntity(left, "Blast");

		var right = new Blast
		{
			velocity = direction,
			position = puppet.position + direction + new Vector3(0, -1, 0)
		};
		gc.SpawnEntity(right, "Blast");
	}
}

public class HumanBehavior : ShipBehavior
{
	public int playerId;

	public HumanBehavior(Entity e, int player)
	{
		puppet = e;
		playerId = player;
	}

	public void Update (GameController gc, float dt) {
		puppet.position += new Vector3(Input.GetAxis("Horizontal" + playerId), Input.GetAxis("Vertical" + playerId), 0) * puppet.speed * dt;

		if (Input.GetButton("Fire" + playerId) && timeSinceLastShot <= 0)
		{
			timeSinceLastShot = shotDelay;
			Shoot(playerId == 1 ? 1 : -1, gc);
		}
		else
			timeSinceLastShot -= dt;
	}
}

public class AiBehavior : ShipBehavior
{

	public AiBehavior(Entity e)
	{
		puppet = e;
	}

	public List<Vector3> pattern = new List<Vector3>
	{
		new Vector3(0, 1.0f),
		new Vector3(0, -1.0f),
		new Vector3(0.5f, 0.5f),
		new Vector3(-0.5f, 0),
		new Vector3(0, -1)
	};

	private int counter = 0;

	public void Update(GameController gc, float dt)
	{
		timeSinceLastShot -= dt;

		if (timeSinceLastShot <= 0)
		{
			Shoot(-1, gc);
			timeSinceLastShot = shotDelay;

			puppet.velocity = pattern[counter % pattern.Count];
			counter++;
		}
	}
}
