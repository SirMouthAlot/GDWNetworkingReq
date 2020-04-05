using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WatchedObject : MonoBehaviour
{
    //Is this object owned by this client?
    public bool m_owned = false;

    //Should we send 
    public bool m_updatePos = false;
    public bool m_updateRot = false;
    public bool m_updateVel = false;
    public bool m_performDeadReckoning = false;

    //Object id
    public string m_objectID = "00";
    //Compiled id that includes client number
    public string m_finalObjectID;

    //store the position from the last 2 packets
    public Queue<Vector3> positions = new Queue<Vector3>(2);

    Rigidbody rb;

    //
    float m_sendTimer = 0.0f;
    public static float m_sendInterval = 0.0f;

    // Start is called before the first frame update
    void Awake()
    {
        //Gets rigidbody
        rb = GetComponent<Rigidbody>();

        //init queue
        positions.Enqueue(new Vector3(0.0f, 0.0f, 0.0f));
        positions.Enqueue(new Vector3(0.0f, 0.0f, 0.0f));
    }

    private string PackageVector(Vector3 vec, string packetType)
    {
        string temp = packetType + GameManager.m_thisPlayer.id.ToString() +
                ';' + m_finalObjectID + ';' + vec.x.ToString() + ';' +
                    vec.y.ToString() + ';' + vec.z.ToString();

        return temp;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_owned)
        {
            //If the object is NOT owned, just do dead reckoning
            //(Placeholder, the idea is for watched object to have multiple algorithms available)
            m_performDeadReckoning = true;
        }


        if (m_owned)
        {
            m_finalObjectID = "0" + GameManager.m_thisPlayer.id + m_objectID;
        }
        else
        {
            m_finalObjectID = "0" + GameManager.m_otherPlayers[0].id + m_objectID;
        }

        m_sendTimer += Time.deltaTime;

        if (m_owned)
        {
            if (m_sendTimer > m_sendInterval)
            {
                if (m_updatePos)
                {
                    Vector3 pos = gameObject.transform.position;
                    //p;userID;objectID;posX;posy;posZ
                    GameManager.m_networkingManager.SendPacketToServer(PackageVector(pos, "p;"));
                }

                if (m_updateRot)
                {
                    Vector3 rot = gameObject.transform.rotation.eulerAngles;
                    //p;userID;objectID;posX;posy;posZ
                    GameManager.m_networkingManager.SendPacketToServer(PackageVector(rot, "r;"));
                }

                if (m_updateVel)
                {
                    Vector3 vel = gameObject.GetComponent<Rigidbody>().velocity;
                    //p;userID;objectID;posX;posy;posZ
                    GameManager.m_networkingManager.SendPacketToServer(PackageVector(vel, "v;"));
                }

                m_sendTimer = 0.0f;

            }
        }

        if (m_performDeadReckoning)
        {
            DeadReckoning();
        }
    }
    
    public void DeadReckoning()
    {
        //Current position - last position to get change in position
        Vector3 deltaPos = rb.position - positions.ToArray()[0];
        //change in position / change in time to get velocity
        Vector3 vel = deltaPos / Time.deltaTime;
        vel.Normalize();

        //Override the current velocity with this predicted velocity
        rb.velocity = vel * 3.0f;
    }
}
