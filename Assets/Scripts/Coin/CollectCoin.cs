using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectCoin : MonoBehaviour
{

    // When we collide with the coin and collect it, we make an API request to the server to call our blockchain method
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SendTransaction.instance.PlayerCollectedToken();

            Destroy(this.gameObject);
        }
    }

}
