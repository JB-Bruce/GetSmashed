using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEdges : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponentInParent<PlayerController>() != null)
        {
            var collidedPlayer = collision.GetComponentInParent<PlayerController>();

            collidedPlayer.Respawn();
        }
    }
}
