using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTest : MonoBehaviour
{

        // Define the prefab of the projectile
        public GameObject projectilePrefab, player;
        public Transform targetDestination, originTransform;

        // Define the force with which the projectile is thrown
        public float throwForce = 10.0f;

        // Define the reference to the first person controller
        //public FirstPersonController controller;

        // Define the maximum height of the projectile's curve
        public float flyTime = 5.0f;

    private void Start()
    {
        //player = FindObjectOfType<FirstPersonController>().gameObject;
    }
    void Update()
        {
            // Check if the player pressed the throw button
            if (Input.GetMouseButtonDown(0))
            {
                // Get the current position and orientation of the player
                Vector3 playerPosition = transform.position;
                Quaternion playerOrientation = transform.rotation;

                // Calculate the forward direction of the player
                Vector3 forward = playerOrientation * Vector3.forward;

                // Calculate the destination position of the projectile
                Vector3 destination = playerPosition + forward * 100.0f;

                // Instantiate the projectile prefab at the player's position
                GameObject projectile = Instantiate(projectilePrefab, originTransform.position, originTransform.rotation);

                // Get the rigidbody component of the projectile
                Rigidbody rb = projectile.GetComponent<Rigidbody>();

                // Calculate the initial velocity of the projectile
                Vector3 initialVelocity = CalculateInitialVelocity(originTransform.position, targetDestination.position, flyTime);

                // Apply the initial velocity to the rigidbody of the projectile
                rb.velocity = initialVelocity;
            }
        }

/*        // Define the function to calculate the initial velocity of the projectile
        Vector3 CalculateInitialVelocity(Vector3 playerPosition, Vector3 destination, float maxHeight)
        {
            // Calculate the distance between the player and the destination
            float distance = Vector3.Distance(playerPosition, destination);

            // Calculate the initial upward velocity of the projectile
            float upwardVelocity = maxHeight * Physics.gravity.magnitude / (2.0f * distance);

            // Calculate the forward and upward components of the initial velocity
            float forwardVelocity = distance / (2.0f * maxHeight / upwardVelocity);
            float upwardComponent = upwardVelocity;
            float forwardComponent = forwardVelocity;

            // Calculate the initial velocity of the projectile
            Vector3 initialVelocity = new Vector3(forwardComponent, upwardComponent, 0.0f);

            // Return the initial velocity of the projectile
            return initialVelocity;
        }*/

    // Define the function to calculate the initial velocity of the projectile
    Vector3 CalculateInitialVelocity(Vector3 playerPosition, Vector3 destination, float flyingTime)
    {
        // Calculate the distance between the player and the destination
        float distance = Vector3.Distance(playerPosition, destination);
        Vector3 direction = destination - playerPosition;
        float playerX = (destination - playerPosition).x;
        float playerZ = (destination - playerPosition).z;
        Vector2 playerXZ = new Vector2(playerX, playerZ);

        // Calculate the initial upward velocity of the projectile
        //float upwardVelocity = Physics.gravity.magnitude * flyingTime / 2.0f;
        float upwardVelocity = (destination - playerPosition).y / flyingTime + 0.5f * Physics.gravity.magnitude * flyingTime;

        // Calculate the forward and upward components of the initial velocity
        //float forwardVelocity = distance / flyingTime;
        float forwardVelocity = playerXZ.magnitude / flyingTime;
        float upwardComponent = upwardVelocity;
        float forwardComponent = forwardVelocity;

        // Calculate the initial velocity of the projectile
        //Vector3 initialVelocity = originTransform.rotation * new Vector3(forwardComponent, upwardComponent, 0.0f);
        Vector3 initialVelocity = direction.normalized * forwardVelocity;
        initialVelocity.y = upwardComponent;

        // Return the initial velocity of the projectile
        return initialVelocity;
    }

    Vector3 CalculateInitialVelocityAI(Vector3 playerPosition, Vector3 destination, float flyingTime)
    {
        // Calculate the distance between the player and the destination
        float distance = Vector3.Distance(playerPosition, destination);

        // Calculate the initial upward velocity of the projectile
        float upwardVelocity = Physics.gravity.magnitude * flyingTime / 2.0f;

        // Calculate the forward and upward components of the initial velocity
        float forwardVelocity = distance / flyingTime;
        float upwardComponent = upwardVelocity;
        float forwardComponent = forwardVelocity;

        // Calculate the initial velocity of the projectile
        Vector3 initialVelocity = new Vector3(forwardComponent, upwardComponent, 0.0f);

        // Return the initial velocity of the projectile
        return initialVelocity;
    }
}
