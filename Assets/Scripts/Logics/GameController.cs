using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Streams;
using UnityEditor;
using UnityEngine;


public class GameController : MonoBehaviour {

	public SceneController sceneController;

	public static float horizontalBoundary;
	public static float verticalBoudnary;

	public bool paused = false;
	public UIController uiController;

	public List<Entity> entities = new List<Entity>();

	public Cell<GameMode> currentMode = new Cell<GameMode>(null);
	public EmptyStream asteroidDestroyed = new EmptyStream();

	public List<HighScore> highScores = new List<HighScore>();

	void Start () {
		horizontalBoundary = 10 * Camera.main.aspect;
		verticalBoudnary = 10;

		if (File.Exists(Application.persistentDataPath + "/currentMode.dat"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/currentMode.dat", FileMode.Open);

			try
			{
				SetGameMode((GameMode) bf.Deserialize(file));
			}
			finally
			{
				file.Close();
			}
		}
		else
		{
			uiController.SetState(MenuScreenState.ModeSelect);
		}

		if (File.Exists(Application.persistentDataPath + "/highScores.dat"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/highScores.dat", FileMode.Open);

			highScores = (List<HighScore>)bf.Deserialize(file);
			file.Close();
		}
	}

	public void SetGameMode(GameMode mode)
	{
		if(currentMode.value != null)
			currentMode.value.Finish(this);

		if (mode != null)
		{
			mode.Start(this);
			uiController.SetState(MenuScreenState.None);
		}

		currentMode.value = mode;
	}

	void Update ()
	{
		if(paused)
			return;

		float dt = Time.deltaTime;

		if(currentMode.value != null)
			currentMode.value.Tick(dt, this);

		foreach (var entity in entities)
		{
			entity.UpdateBehavior(this, dt);
		}

		sceneController.UpdateViews();

		BringOutTheDead();
	}

	public EntityView SpawnEntity(Entity e, string prefabName)
	{
		entities.Add(e);
		return sceneController.ShowEntity(e, prefabName);
	}

	//clean dead entities without extra allocations
	public void BringOutTheDead()
	{
		int toRemove = 0;

		foreach (var e in entities)
		{
			if (e.health.value <= 0)
			{
				e.dead = true;

				if (e is Asteroid)
					asteroidDestroyed.Send();
			}
		}

		//swap entities so dead ones are at the end
		int backWardsIndex = entities.Count - 1;
		for (int forwardIndex = 0; forwardIndex < backWardsIndex; forwardIndex++)
		{
			if (!entities[forwardIndex].dead)
				continue;

			DestroyEntity(entities[forwardIndex]);
			toRemove++;

			while (entities[backWardsIndex].dead)
			{
				DestroyEntity(entities[backWardsIndex]);
				toRemove++;
				backWardsIndex--;

				if (backWardsIndex == forwardIndex) goto finish;
			}

			entities.Swap(forwardIndex, backWardsIndex);
			backWardsIndex--;
		}

		finish:
		if (toRemove > 0)
			entities.RemoveRange(entities.Count - toRemove, toRemove);
	}

	public void DestroyEntity(Entity e)
	{
		sceneController.Recycle(e);
	}

	void OnApplicationQuit()
	{
		BinaryFormatter bf = new BinaryFormatter();

		if (currentMode.value != null)
		{
			FileStream saveFile = File.Create(Application.persistentDataPath + "/currentMode.dat");

			bf.Serialize(saveFile, currentMode.value);
			saveFile.Close();
		}
		else
		{
			//if you quit to menu voluntarily, don't force into combat again
			File.Delete(Application.persistentDataPath + "/currentMode.dat");
		}

		FileStream scoreFile = File.Create(Application.persistentDataPath + "/highScores.dat");

		bf.Serialize(scoreFile, highScores);
		scoreFile.Close();
	}


	public void Reset()
	{
		foreach (var entity in entities)
		{
			DestroyEntity(entity);
		}

		entities.Clear();
	}
}
