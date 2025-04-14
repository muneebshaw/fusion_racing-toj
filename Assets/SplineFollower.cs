using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;

    private void Awake()
    {
        if (splineContainer == null)
        {
            Debug.LogError("SplineContainer is not assigned in the inspector.");
            return;
        }
    }

    private void OnDrawGizmos()
    {
        if (splineContainer == null) return;

        // get the closest point on the spline to the current position
        SplineUtility.GetNearestPoint(splineContainer.Spline, transform.position, out float3 closestPoint, out float distanceAlongSpline);

        // show a gizmo at the closest point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(closestPoint, 0.1f);

    }
}
