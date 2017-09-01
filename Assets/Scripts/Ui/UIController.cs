using System.Collections;
using System.Collections.Generic;
using Streams;
using UnityEngine;
using UnityEngine.UI;

public enum MenuScreenState
{
    ModeSelect,
    Retry,
    Pause,
    HighScores,
    None
}

public abstract class MenuScreen : MonoBehaviour
{
    public UIController controller;
    protected DisposableCollector collector = new DisposableCollector();

    public bool isEnabled = false;
    public virtual MenuScreenState stateId { get {return MenuScreenState.None;} }

    public virtual void Show(bool state, GameController gc)
    {
        gameObject.SetActive(state);
        collector.Dispose();
    }
}

public class UIController : MonoBehaviour
{
    public MenuScreen currentState = null;
    private Dictionary<MenuScreenState, MenuScreen> statesCached = new Dictionary<MenuScreenState, MenuScreen>();


    public Button pauseButton;
    public Image  background;
    public Text currentScore;

    public GameObject leftHealthBar, rightHealthBar;
    public Image leftHealthBarFill, rightHealthBarFill;

    public GameController gc;

    public void Awake()
    {
        var screenList = GetComponentsInChildren<MenuScreen>(true);

        foreach (var screen in screenList)
        {
            statesCached.Add(screen.stateId, screen);
            screen.controller = this;
        }

        pauseButton.ClickStream().Listen(() => { SetState(MenuScreenState.Pause); });
        gc.currentMode.Bind(m => { currentScore.gameObject.SetActive(m is NormalGameMode); });
    }

    public void SetState(MenuScreenState state)
    {
        if (currentState != null)
        {
            if (state == currentState.stateId)
                return;
            else
                currentState.Show(false, gc);
        }

        if (state != MenuScreenState.None)
        {
            currentState = statesCached[state];
            currentState.Show(true, gc);
        }
        else
        {
            currentState = null;
        }

        background.gameObject.SetActive(state != MenuScreenState.None);
        gc.paused = state != MenuScreenState.None;
    }
}
