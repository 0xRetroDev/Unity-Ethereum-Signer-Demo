using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;


    private void Update()
    {
        // Get the horizontal input axis (left/right arrow keys or A/D keys)
        float horizontalInput = Input.GetAxis("Horizontal");

        // Calculate the new position based on the input
        Vector3 newPosition = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;

        // Update the object's position
        transform.position = newPosition;
    }
}
