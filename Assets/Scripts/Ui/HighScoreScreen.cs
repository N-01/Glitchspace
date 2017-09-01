using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct HighScore
{
    public string name;
    public int score;
}

public class HighScoreScreen : MenuScreen
{
    public Text list;
    public Button back;

    public override MenuScreenState stateId { get { return MenuScreenState.HighScores; } }

    public override void Show(bool state, GameController gc)
    {
        base.Show(state, gc);

        if (state)
        {
            if(gc.highScores.Count > 0)
                list.text = gc.highScores.OrderByDescending(o => o.score).Select(s => "#" + s.name + " :: " + s.score).Aggregate(((s, s1) => s + "\n" + s1));
            else
                list.text = "none yet";

            collector += back.OnClickDo(() =>
            {
                controller.SetState(MenuScreenState.ModeSelect);
            });
        }
    }
}
