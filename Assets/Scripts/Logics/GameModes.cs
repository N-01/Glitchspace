using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Streams;
using UnityEngine;

[Serializable]
public abstract class GameMode
{
    public abstract void Init();
    public abstract void Start(GameController gc);
    public abstract void Tick(float dt, GameController gc);

    public virtual void Finish(GameController gc)
    {
        gc.Reset();
        collector.Dispose();
    }

    public virtual void Restart(GameController gc)
    {
        Finish(gc);
        Init();
        Start(gc);
    }

    [NonSerialized]
    protected DisposableCollector collector = new DisposableCollector();

    [OnDeserializing]
    public void OnDeserializing(System.Runtime.Serialization.StreamingContext c)
    {
        collector = new DisposableCollector();
    }
}

[Serializable]
public class NormalGameMode : GameMode
{
    public Ship playerShip;
    public Cell<int> currentScore = new Cell<int>(0);
    public AsteroidSpawnerBasic asteroidSpawner;

    [NonSerialized]
    public HumanBehavior input;

    private float accumulatedDt = 0;

    [NonSerialized]
    private Coroutine spawnCoro;

    public NormalGameMode()
    {
        Init();
    }

    public override void Init()
    {
        playerShip = new Ship();
        currentScore.value = 0;
        asteroidSpawner = new AsteroidSpawnerBasic(1f);
        accumulatedDt = 0;
    }

    public override void Start(GameController gc)
    {
        input = new HumanBehavior(playerShip, 1);
        gc.SpawnEntity(playerShip, "ship_blue");
        
        spawnCoro = gc.StartCoroutine(asteroidSpawner.Spawn(gc));

        collector += gc.asteroidDestroyed.Listen(() =>
        {
            if(playerShip.health.value > 0)
                currentScore.value += 10;
        });

        collector += currentScore.Bind(score => gc.uiController.currentScore.text = "Score: " + score);
        collector += gc.uiController.ShowEntityHealth(playerShip, true);
    }

    public override void Tick(float dt, GameController gc)
    {
        if (playerShip.health.value <= 0)
        {
            gc.uiController.SetState(MenuScreenState.Retry);
            return;
        }

        accumulatedDt += dt;

        if (accumulatedDt >= 1)
        {
            currentScore.value += 1;
            accumulatedDt -= 1;
        }

        input.Update(gc, dt);
    }

    public override void Finish(GameController gc)
    {
        base.Finish(gc);
        gc.StopCoroutine(spawnCoro);
    }
}

[Serializable]
public class VsComputer : GameMode
{
    public Ship playerShip;
    public Ship enemyShip;

    [NonSerialized]
    public HumanBehavior playerBehavior;

    [NonSerialized]
    public AiBehavior enemyBehavior;


    public VsComputer()
    {
        Init();
    }

    public override void Init()
    {
        playerShip = new Ship();
        playerShip.position = new Vector3(-5, 0, 0);

        enemyShip = new Ship();
        enemyShip.rotation.z = 180;
        enemyShip.speed = 8;

        enemyShip.position = new Vector3(5, 0, 0);
    }

    public override void Start(GameController gc)
    {
        playerBehavior = new HumanBehavior(playerShip, 1);
        gc.SpawnEntity(playerShip, "ship_blue");

        enemyBehavior = new AiBehavior(enemyShip);
        gc.SpawnEntity(enemyShip, "ship_red");

        collector += gc.uiController.ShowEntityHealth(playerShip, true);
        collector += gc.uiController.ShowEntityHealth(enemyShip, false);
    }

    public override void Tick(float dt, GameController gc)
    {
        if (playerShip.health.value <= 0 || enemyShip.health.value <= 0)
            gc.uiController.SetState(MenuScreenState.Retry);

        playerBehavior.Update(gc, dt);
        enemyBehavior.Update(gc, dt);
    }
}

[Serializable]
public class VsPlayer : GameMode
{
    public Ship firstPlayerShip;
    public Ship secondPlayerShip;

    public AsteroidSpawnerCircular asteroidSpawner;


    [NonSerialized]
    public HumanBehavior firstPlayerBehavior;

    [NonSerialized]
    public HumanBehavior secondPlayerBehavior;

    [NonSerialized]
    private Coroutine spawnCoro;

    public VsPlayer()
    {
        Init();
    }

    public override void Init()
    {
        firstPlayerShip = new Ship();
        firstPlayerShip.position = new Vector3(-5, 0, 0);

        secondPlayerShip = new Ship();
        secondPlayerShip.rotation.z = 180;
        secondPlayerShip.position = new Vector3(5, 0, 0);

        asteroidSpawner = new AsteroidSpawnerCircular(0.5f);
    }

    public override void Start(GameController gc)
    {
        firstPlayerBehavior = new HumanBehavior(firstPlayerShip, 1);
        gc.SpawnEntity(firstPlayerShip, "ship_blue");

        secondPlayerBehavior = new HumanBehavior(secondPlayerShip, 2);
        gc.SpawnEntity(secondPlayerShip, "ship_red");

        gc.uiController.leftHealthBar.SetActive(true);
        gc.uiController.rightHealthBar.SetActive(true);

        collector += gc.uiController.ShowEntityHealth(firstPlayerShip, true);
        collector += gc.uiController.ShowEntityHealth(secondPlayerShip, false);

        spawnCoro = gc.StartCoroutine(asteroidSpawner.Spawn(gc));
    }

    public override void Tick(float dt, GameController gc)
    {
        if (firstPlayerShip.health.value <= 0 || secondPlayerShip.health.value <= 0)
            gc.uiController.SetState(MenuScreenState.Retry);

        firstPlayerBehavior.Update(gc, dt);
        secondPlayerBehavior.Update(gc, dt);
    }

    public override void Finish(GameController gc)
    {
        base.Finish(gc);
        gc.StopCoroutine(spawnCoro);
    }
}