using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HighScoreEntry
{
    public int score;
    public string playerName;
}

[System.Serializable]
public class HighScores
{
    public HighScores()
    {
        scores[0] = new HighScoreEntry();
        scores[1] = new HighScoreEntry();
        scores[2] = new HighScoreEntry();
        scores[3] = new HighScoreEntry();
        scores[4] = new HighScoreEntry();
    }

    public void SetScoreVal(int index, int score, string name)
    {
        //just a wrapper to make faster
        scores[index].playerName = name;
        scores[index].score = score;
    }

    public void SetScoreVal(int index, HighScoreEntry score)
    {
        scores[index] = score;
    }


    public HighScoreEntry[] scores = new HighScoreEntry[5];
}

public class HighScoreManager : MonoBehaviour
{
    public string filePath = "/Resources/HighScores.json";
    //List of texts
    public List<Text> displayScores = new List<Text>();

    
    List<HighScoreEntry> jsonScoresUnadjusted = new List<HighScoreEntry>();

    //size 5 array wrapper
    HighScores jsonScores = new HighScores();

    private void Awake()
    {
        //get the winning player
        PlayerData winningPlayer = (GameManager.m_thisPlayer.score > GameManager.m_otherPlayers[0].score) ? GameManager.m_thisPlayer : GameManager.m_otherPlayers[0];
        
        string json = "";
        json = File.ReadAllText(Application.dataPath + filePath, System.Text.Encoding.ASCII);

        //check if you can Load in the high scores from the json file
        if (json != "")
        {
            //deserialize the json
            jsonScores = JsonUtility.FromJson<HighScores>(json);
        }
        else
        {
            //if not create the default values
            jsonScores.SetScoreVal(0, 10, "player1");
            jsonScores.SetScoreVal(1, 8, "player2");
            jsonScores.SetScoreVal(2, 6, "player3");
            jsonScores.SetScoreVal(3, 4, "player4");
            jsonScores.SetScoreVal(4, 2, "player5");
        }


        bool added = false;

        //Check if any of the current game people should be in here instead
        for (int i = 0; i < 5; i++)
        {
            if (winningPlayer.score > jsonScores.scores[i].score && !added)
            {
                HighScoreEntry temp = new HighScoreEntry();
                temp.playerName = winningPlayer.name;
                temp.score = winningPlayer.score;

                //Add to unadjusted list
                jsonScoresUnadjusted.Add(temp);

                added = true;
            }
            jsonScoresUnadjusted.Add(jsonScores.scores[i]);
        }

        if (jsonScoresUnadjusted.Count > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                //Puts the unlimited scores (includes the newest player, this will place them in the list properly
                //And if they don't get a high score, they'll be truncated.
                jsonScores.SetScoreVal(i, jsonScoresUnadjusted[i]);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            //Sets up displaying scores
            displayScores[i].text = jsonScores.scores[i].playerName + ": " + jsonScores.scores[i].score.ToString();
        }
    }

    public void QuitGame()
    {
        //If you're the host
        if (GameManager.m_player1)
        {
            //Save the values into a file
            string outputJson = JsonUtility.ToJson(jsonScores);

            //Outputs the json to file
            File.WriteAllText(Application.dataPath + filePath, outputJson);
        }

        Application.Quit();
    }
}
