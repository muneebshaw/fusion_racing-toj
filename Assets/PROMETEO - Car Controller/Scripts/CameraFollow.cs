using Fusion;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	private Transform carTransform;
    [Range(1, 10)]
    public float followSpeed = 2;
    [Range(1, 10)]
    public float lookSpeed = 5;
    Vector3 initialCameraPosition;
    Vector3 initialCarPosition;
    Vector3 absoluteInitCameraPosition;

    internal void SetUp(Transform target)
	{
        carTransform = target;
        initialCameraPosition = gameObject.transform.position;
        initialCarPosition = carTransform.position;
        absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
    }

    private void LateUpdate()
    {
        if (!carTransform) return;

        //Look at car
        Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
        Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);

        //Move to car
        Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
        transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        BasicSpawner.LocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    private void OnDisable()
    {
        BasicSpawner.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnLocalPlayerSpawned(PrometeoCarController _carController)
    {
        Debug.Log($"CameraFollow.OnLocalPlayerSpawned {_carController.GetComponent<NetworkObject>().Name}");
        SetUp(_carController.transform.GetChild(0));
    }
}
