using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikerSounds : MonoBehaviour
{
    public AudioClip strikerHitPuck;

    private void OnCollisionEnter(Collision collision)
    {
        //Check info on the colliding objects
        if (collision.gameObject.tag == "Puck")
        {
            //Plays wall hit sound at the first point of contact (LOUD)
            AudioSource.PlayClipAtPoint(strikerHitPuck, collision.contacts[0].point, 4.0f);
        }
    }
}
