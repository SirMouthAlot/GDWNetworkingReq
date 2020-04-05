using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData
{
    public string name;
    public int id;
    public int score;
}


public class GameManager : MonoBehaviour
{
    public static GameManager m_instance;
    public static NetworkingManager m_networkingManager;

    public static List<PlayerData> m_otherPlayers = new List<PlayerData>();
    public static PlayerData m_thisPlayer;

    public static bool m_player1 = false;
    public static bool m_player2 = false;
    public static int m_clientSize = 0;
    public static int m_userSize = 0;
    public bool loadedGame = false;

    public static float m_sendInterval = 0.1f;

    private void Awake()
    {
        m_instance = this;

        //Loads menu screen
        SceneManager.LoadSceneAsync((int)SceneIndexes.MENU_SCREEN, LoadSceneMode.Additive);
    }

    private void AddOtherPlayers()
    {
        ClientConnectionData temp = NetworkingManager.clients[m_clientSize];
        PlayerData newPlayer = new PlayerData();
        newPlayer.id = temp.id;
        newPlayer.name = temp.name;
        newPlayer.score = 0;

        m_otherPlayers.Add(newPlayer);
        m_clientSize++;
    }

    private void Update()
    {
        if (m_networkingManager != null)
        {
            //Add the new client to the players list
            if (NetworkingManager.clients.Count > m_clientSize)
            {
                AddOtherPlayers();
            }
            if ((NetworkingManager.user != null) && (m_thisPlayer == null))
            {
                ClientConnectionData temp = NetworkingManager.user;
                PlayerData thisPlayer = new PlayerData();
                thisPlayer.id = temp.id;
                thisPlayer.name = temp.name;
                thisPlayer.score = 0;

                if (thisPlayer.id == 0)
                {
                    m_player1 = true;
                }
                else if (thisPlayer.id == 1)
                {
                    m_player2 = true;
                }
                else if (thisPlayer.id > 1)
                {
                    //This player is a spectator
                }

                m_thisPlayer = thisPlayer;
                m_userSize++;
            }

            if ((m_userSize + m_clientSize) >= 2 && !loadedGame)
            {
                LoadGame();
                loadedGame = true;
            }
        }
    }

    public void LoadGame()
    {
        //Unloads menu scene
        SceneManager.UnloadSceneAsync((int)SceneIndexes.MENU_SCREEN);
        //Loads the main game scene
        SceneManager.LoadSceneAsync((int)SceneIndexes.MAIN_GAME, LoadSceneMode.Additive);
    }

    public void EndGame()
    {
        //Unloads menu scene
        SceneManager.UnloadSceneAsync((int)SceneIndexes.MAIN_GAME);
        //Loads end scene
        SceneManager.LoadSceneAsync((int)SceneIndexes.END_SCREEN, LoadSceneMode.Additive);
    }
}
