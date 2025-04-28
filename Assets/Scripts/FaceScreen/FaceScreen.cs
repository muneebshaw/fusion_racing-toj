using UnityEngine;

public class FaceScreen : MonoBehaviour
{
    private void LateUpdate()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 directionToCamera = cameraPosition - transform.position;
        transform.rotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
    }
}
