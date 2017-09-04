using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetryScreen : MenuScreen {
    public Button tryAgain;
    public Button backToMenu;

    public InputField nameInput;
    public Button saveScore;

    public Text finalScore;

    public override MenuScreenState stateId { get { return MenuScreenState.Retry; } }

    public override void Show(bool state, GameController gc)
    {
        base.Show(state, gc);

        if (state)
        {
           collector += tryAgain.ClickStream().Listen(() =>
            {
                controller.SetState(MenuScreenState.None);
                gc.currentMode.value.Restart(gc);
            });

            collector += backToMenu.ClickStream().Listen(() =>
            {
                controller.SetState(MenuScreenState.ModeSelect);
                gc.SetGameMode(null);
            });

            var normalGameMode = gc.currentMode.value as NormalGameMode;

            finalScore.gameObject.SetActive(normalGameMode != null);

            if (normalGameMode != null)
                finalScore.text = "Your score: " + normalGameMode.currentScore.value;

            saveScore.interactable = true;

            collector += saveScore.ClickStream().Listen(() =>
            {
                if (nameInput.textComponent.text.Length > 0)
                {
                    gc.highScores.Add(new HighScore
                    {
                        name = nameInput.textComponent.text,
                        score = normalGameMode.currentScore.value
                    });

                    saveScore.interactable = false;
                }
            });
        }
    }
}
