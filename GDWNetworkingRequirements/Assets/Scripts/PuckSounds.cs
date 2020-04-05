using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckSounds : MonoBehaviour
{
    public AudioClip puckHitWall;

    private void OnCollisionEnter(Collision collision)
    {
        //Check info on the colliding objects
        if (collision.gameObject.tag == "Wall")
        {
            //Plays wall hit sound at the first point of contact
            AudioSource.PlayClipAtPoint(puckHitWall, collision.contacts[0].point);
        }
    }
}
