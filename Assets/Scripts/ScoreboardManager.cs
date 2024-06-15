using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardManager : MonoBehaviour
{

    [Header("Scoreboard UI")]
    [SerializeField] private GameObject playerHolder;
    [SerializeField] private GameObject playerRowPrefab;
    [SerializeField] private GameObject scorePrefab;

    [Header("Scoreboard Frame UI")]
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject rowHolder;
    [SerializeField] private HorizontalLayoutGroup frameLayoutGroup;
    int maxRounds;
    int pinCount;

    public void Configure(int maxRounds, int pinCount) {
        this.maxRounds = maxRounds;
        this.pinCount = pinCount;
        foreach(Transform child in rowHolder.transform) {
            Destroy(child.gameObject);
        }

        for(int i = 0; i < maxRounds; i++) {
            GameObject row = Instantiate(rowPrefab, rowHolder.transform);
            TextMeshProUGUI roundText = row.GetComponentInChildren<TextMeshProUGUI>();
            roundText.text = (i + 1).ToString();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(frameLayoutGroup.GetComponent<RectTransform>());
    }

    public void UpdateScoreboardUI(List<Player> players, int currentPlayer) {

        foreach (Transform child in playerHolder.transform) {
            Destroy(child.gameObject);
        }

        foreach (Player player in players) {
 
            GameObject playerRow = Instantiate(playerRowPrefab, playerHolder.transform);
            playerRow.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = player.name;
            playerRow.transform.Find("Name").GetComponent<TextMeshProUGUI>().color = currentPlayer == players.IndexOf(player) ? Color.yellow : Color.white;

            GameObject scoreList = playerRow.transform.Find("Scores").gameObject;

            int finalScore = -1;

            for(int i = 0; i < player.scores.Count; i++) {

                Score score = player.scores[i];

                GameObject scoreRow = Instantiate(scorePrefab, scoreList.transform);

                string firstThrowText = "";
                string secondThrowText = "";
                string bonusThrowText = "";
                string totalText = "";

                if(score.firstThrow == -1) {
                    firstThrowText = " ";
                } else if (score.firstThrow == 0) {
                    firstThrowText = "-";
                } else if (score.firstThrow == pinCount) {
                    firstThrowText = "X";
                } else {
                    firstThrowText = score.firstThrow.ToString();
                }

                if(score.secondThrow == -1) {
                    secondThrowText = " ";
                } else if (score.secondThrow == 0) {
                    secondThrowText = "-";
                } else if (score.secondThrow == pinCount) {
                    secondThrowText = i == (maxRounds - 1) && score.firstThrow == pinCount ? "X" : "/";
                } else {
                    secondThrowText = score.firstThrow + score.secondThrow == pinCount ? "/" : score.secondThrow.ToString();
                }

                if(score.bonusThrow == -1) {
                    bonusThrowText = " ";
                } else if (score.bonusThrow == 0) {
                    bonusThrowText = "-";
                } else if (score.bonusThrow == pinCount) {
                    bonusThrowText = "X";
                } else {
                    bonusThrowText = score.secondThrow + score.bonusThrow == pinCount ? "/" : score.bonusThrow.ToString();
                }

                totalText = score.totalScore == -1 ? " " : score.totalScore.ToString();

                scoreRow.transform.Find("FirstThrow").GetComponent<TextMeshProUGUI>().text = firstThrowText;
                scoreRow.transform.Find("SecondThrow").GetComponent<TextMeshProUGUI>().text = secondThrowText;
                scoreRow.transform.Find("Total").GetComponent<TextMeshProUGUI>().text = totalText;
                scoreRow.transform.Find("BonusThrow").GetComponent<TextMeshProUGUI>().text = bonusThrowText;

                if(score.totalScore != -1 && i == (maxRounds - 1)) {
                    finalScore = score.totalScore;
                }
            }

            playerRow.transform.Find("FinalScore").GetComponent<TextMeshProUGUI>().text = finalScore == -1 ? " " : finalScore.ToString();

        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(playerHolder.GetComponent<RectTransform>());

    }

}
