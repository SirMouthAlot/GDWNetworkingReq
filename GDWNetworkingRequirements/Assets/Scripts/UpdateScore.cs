using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UpdateScore : MonoBehaviour
{
    public Text m_player1Score;
    public Text m_player2Score;
    public Text m_timerDisplay;

    float m_timer;
    public float m_gameLength;

    private void Awake()
    {
        m_timer = m_gameLength;
    }

    private void Update()
    {
        //Update timer
        m_timer -= Time.deltaTime;

        //Check if game is over
        if (m_timer <= 0.0f)
        {
            m_timer = 0.0f;
            //Set end of game state so no more goals can be scored

            //Ends the game
            GameManager.m_instance.EndGame();
        }

        if (m_timerDisplay != null)
        {
            m_timerDisplay.text = m_timer.ToString();
        }

        if (m_player1Score != null)
        {
            string player1Name = (GameManager.m_player1 ? GameManager.m_thisPlayer.name : GameManager.m_otherPlayers[0].name);
            int player1score = (GameManager.m_player1 ? GameManager.m_thisPlayer.score : GameManager.m_otherPlayers[0].score);

            m_player1Score.text = player1Name + ": " + player1score.ToString();
        }

        if (m_player2Score != null)
        {
            string player2Name = (GameManager.m_player2 ? GameManager.m_thisPlayer.name : GameManager.m_otherPlayers[0].name);
            int player2score = (GameManager.m_player2 ? GameManager.m_thisPlayer.score : GameManager.m_otherPlayers[0].score);

            m_player2Score.text = player2Name + ": " + player2score.ToString();
        }
    }
}
