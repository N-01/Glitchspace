using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelectScreen : MenuScreen
{
    public Button normal, vsPlayer, vsComputer, highScores;

    public override MenuScreenState stateId { get { return MenuScreenState.ModeSelect; } }

    public override void Show(bool state, GameController gc)
    {
        base.Show(state, gc);

        if (state)
        {
            collector += normal.OnClickDo(() =>
            {
                gc.SetGameMode(new NormalGameMode());
            });

            collector += vsComputer.OnClickDo(() =>
            {
                gc.SetGameMode(new VsComputer());
            });

            collector += vsPlayer.OnClickDo(() =>
            {
                gc.SetGameMode(new VsPlayer());
            });

            collector += highScores.OnClickDo(() =>
            {
                controller.SetState(MenuScreenState.HighScores);
            });
        }
    }
}
