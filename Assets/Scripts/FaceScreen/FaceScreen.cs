using UnityEngine;

public class FaceScreen : MonoBehaviour
{
    private void LateUpdate()
    {
        // Get the camera's position and rotation
        Vector3 cameraPosition = Camera.main.transform.position;
        //Quaternion cameraRotation = Camera.main.transform.rotation;
        // Calculate the direction from the object to the camera
        Vector3 directionToCamera = cameraPosition - transform.position;
        // Set the object's rotation to face the camera
        transform.rotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
        // Optional: Adjust the position slightly to avoid z-fighting
        //transform.position += transform.forward * 0.01f;
    }
}
