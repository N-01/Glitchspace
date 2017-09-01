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
    public Ship firstPlayerShip;
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
        firstPlayerShip = new Ship();
        currentScore.value = 0;
        asteroidSpawner = new AsteroidSpawnerBasic(1f);
        accumulatedDt = 0;
    }

    public override void Start(GameController gc)
    {
        input = new HumanBehavior(firstPlayerShip, 1);
        gc.SpawnEntity(firstPlayerShip, "ship_blue");
        
        spawnCoro = gc.StartCoroutine(asteroidSpawner.Spawn(gc));

        collector += gc.asteroidDestroyed.Listen(() =>
        {
            if(firstPlayerShip.health.value > 0)
                currentScore.value += 10;
        });

        collector += currentScore.Bind(score => gc.uiController.currentScore.text = "Score: " + score);

        gc.uiController.leftHealthBar.SetActive(true);
        gc.uiController.rightHealthBar.SetActive(false);

        collector += firstPlayerShip.health.Bind(h => gc.uiController.leftHealthBarFill.fillAmount = h / 3.0f);
    }

    public override void Finish(GameController gc)
    {
        base.Finish(gc);
        gc.StopCoroutine(spawnCoro);
    }

    public override void Tick(float dt, GameController gc)
    {
        if (firstPlayerShip.health.value <= 0)
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
        enemyShip.angle = 180;
        enemyShip.speed = 8;

        enemyShip.position = new Vector3(5, 0, 0);
    }

    public override void Start(GameController gc)
    {
        playerBehavior = new HumanBehavior(playerShip, 1);
        gc.SpawnEntity(playerShip, "ship_blue");

        enemyBehavior = new AiBehavior(enemyShip);
        gc.SpawnEntity(enemyShip, "ship_red");

        gc.uiController.leftHealthBar.SetActive(true);
        gc.uiController.rightHealthBar.SetActive(true);

        collector += playerShip.health.Bind(h => gc.uiController.leftHealthBarFill.fillAmount = h / 3.0f);
        collector += enemyShip.health.Bind(h => gc.uiController.rightHealthBarFill.fillAmount = h / 3.0f);
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


    [NonSerialized]
    public HumanBehavior firstPlayerBehavior;


    [NonSerialized]
    public HumanBehavior secondPlayerBehavior;

    public VsPlayer()
    {
        Init();
    }

    public override void Init()
    {
        firstPlayerShip = new Ship();
        firstPlayerShip.position = new Vector3(-5, 0, 0);

        secondPlayerShip = new Ship();
        secondPlayerShip.angle = 180;
        secondPlayerShip.position = new Vector3(5, 0, 0);
    }

    public override void Start(GameController gc)
    {
        firstPlayerBehavior = new HumanBehavior(firstPlayerShip, 1);
        gc.SpawnEntity(firstPlayerShip, "ship_blue");

        secondPlayerBehavior = new HumanBehavior(secondPlayerShip, 2);
        gc.SpawnEntity(secondPlayerShip, "ship_red");

        gc.uiController.leftHealthBar.SetActive(true);
        gc.uiController.rightHealthBar.SetActive(true);

        collector += firstPlayerShip.health.Bind(h => gc.uiController.leftHealthBarFill.fillAmount = h / 3.0f);
        collector += secondPlayerShip.health.Bind(h => gc.uiController.rightHealthBarFill.fillAmount = h / 3.0f);
    }

    public override void Tick(float dt, GameController gc)
    {
        if (firstPlayerShip.health.value <= 0 || secondPlayerShip.health.value <= 0)
            gc.uiController.SetState(MenuScreenState.Retry);

        firstPlayerBehavior.Update(gc, dt);
        secondPlayerBehavior.Update(gc, dt);
    }
}