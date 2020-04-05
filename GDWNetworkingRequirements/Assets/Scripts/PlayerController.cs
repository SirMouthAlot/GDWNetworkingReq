using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Side
{
    FIRST_PLAYER = 0,
    SECOND_PLAYER = 1
}


public class PlayerController : MonoBehaviour
{
    public Side m_side = Side.FIRST_PLAYER;

    public Rigidbody m_rigidBody;
    public GameObject m_backEdge;
    public GameObject m_boardCenter;

    void MoveTo(Vector3 point)
    {
        //Apply force
        m_rigidBody.MovePosition(point);
    }

    //Late Update is called at the end of a frame
    void LateUpdate()
    {
        //Store our hit for our raycast later
        RaycastHit hit;

        //Get mouse position
        Vector3 mousePosition = Input.mousePosition;
        //Cast a ray from camera towards whatever mouse was pointing at
        Ray rayCast = Camera.main.ScreenPointToRay(mousePosition);

        //Do a raycast
        if (Physics.Raycast(rayCast, out hit))
        {
            //If the object was tagged as playable area
            if (hit.transform.gameObject.CompareTag("PlayableArea"))
            {
                //Check which side this player controller is on
                switch (m_side)
                {
                    case Side.FIRST_PLAYER:
                        if (hit.point.z < m_boardCenter.transform.position.z
                            && hit.point.z > m_backEdge.transform.position.z)
                        {
                            //FIXME: Replace with call to PushTowards function
                            //gameObject.transform.position = hit.point;
                            MoveTo(hit.point);
                        }
                        break;
                    case Side.SECOND_PLAYER:
                        if (hit.point.z > m_boardCenter.transform.position.z
                            && hit.point.z < m_backEdge.transform.position.z)
                        {
                            //FIXME: Replace with call to PushTowards function
                            MoveTo(hit.point);
                        }
                        break;
                    default:
                        //Do nothing
                        break;
                }
            }
        }

    }
}
