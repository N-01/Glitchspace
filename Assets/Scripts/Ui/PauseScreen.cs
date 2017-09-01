using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Streams;

public class PauseScreen : MenuScreen {

    public Button continueGame;
    public Button backToMenu;

    public override MenuScreenState stateId { get { return MenuScreenState.Pause; } }

    public override void Show(bool state, GameController gc)
    {
        base.Show(state, gc);

        if (state)
        {
            collector += continueGame.ClickStream().Listen(() =>
            {
                controller.SetState(MenuScreenState.None);
            });

            collector += backToMenu.ClickStream().Listen(() =>
            {
                controller.SetState(MenuScreenState.ModeSelect);
                gc.SetGameMode(null);
            });
        }
    }
}
