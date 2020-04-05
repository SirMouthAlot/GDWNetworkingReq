using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirHockeyManager : MonoBehaviour
{
    public GameObject m_puck;
    public GameObject m_player1;
    public GameObject m_player2;
    public Camera m_spectatorCam;
    public Camera m_player1Cam;
    public Camera m_player2Cam;

    private void Awake()
    {
        if (GameManager.m_player2)
        {
            //Player 1 controller should be disabled and not owned
            m_player1Cam.gameObject.SetActive(false);
            m_player1.GetComponent<PlayerController>().enabled = false;
            m_player1.GetComponent<WatchedObject>().m_owned = false;
            m_player1.GetComponent<WatchedObject>().m_performDeadReckoning = true;

            //Player 2 controller should be enabled and owned
            m_player2Cam.gameObject.SetActive(true);
            m_player2.GetComponent<PlayerController>().enabled = true;
            m_player2.GetComponent<WatchedObject>().m_owned = true;
            m_player2.GetComponent<WatchedObject>().m_performDeadReckoning = false;

            //Player 2 does not own puck
            m_puck.GetComponent<WatchedObject>().m_owned = false;

        }
        else if (!GameManager.m_player1 && !GameManager.m_player2)
        {
            //Player 1 cam and controller should be disabled and not owned
            m_player1Cam.gameObject.SetActive(false);
            m_player1.GetComponent<PlayerController>().enabled = false;
            m_player1.GetComponent<WatchedObject>().m_owned = false;

            //Spectator does not own the puck
            m_puck.GetComponent<WatchedObject>().m_owned = false;

            //Spectator cam should be enabled
            m_spectatorCam.gameObject.SetActive(true);
        }
    }
}
