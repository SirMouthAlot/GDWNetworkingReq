using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixOutOfBounds : MonoBehaviour
{
    public GameObject puck;
    public Transform puckStartPos;
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Puck")
        {
            if (puckStartPos != null && puck != null)
            {
                puck.transform.position = puckStartPos.position;
                puck.GetComponent<Rigidbody>().Sleep();
                puck.GetComponent<Rigidbody>().WakeUp();
            }
        }

    }
}
