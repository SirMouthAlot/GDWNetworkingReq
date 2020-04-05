using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public GameObject puck;
    public Transform puckStartPos;
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Puck")
        {
            switch (gameObject.name)
            {
                case "Player1GoalTrigger":
                    if (puckStartPos != null && puck != null)
                    {
                        puck.transform.position = puckStartPos.position;
                        puck.GetComponent<Rigidbody>().Sleep();
                        puck.GetComponent<Rigidbody>().WakeUp();
                    }

                    if (GameManager.m_player2)
                    {
                        GameManager.m_thisPlayer.score++;
                    }
                    else
                    {
                        //Don't handle other player's score
                        
                    }
                    GameManager.m_networkingManager.SendPacketToServer("e;" + GameManager.m_thisPlayer.id.ToString() + ';' + GameManager.m_thisPlayer.score.ToString());
                    break;
                case "Player2GoalTrigger":
                    if (puckStartPos != null && puck != null)
                    {
                        puck.transform.position = puckStartPos.position;
                        puck.GetComponent<Rigidbody>().Sleep();
                        puck.GetComponent<Rigidbody>().WakeUp();
                    }

                    if (GameManager.m_player1)
                    {
                        GameManager.m_thisPlayer.score++;
                    }
                    else
                    {
                        //Don't handle other player's score
                    }
                    GameManager.m_networkingManager.SendPacketToServer("e;" + GameManager.m_thisPlayer.id.ToString() + ';' + GameManager.m_thisPlayer.score.ToString());
                    break;
            }
        }

    }
}
