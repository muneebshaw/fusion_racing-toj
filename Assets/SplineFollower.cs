using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    private float distanceAlongSpline;
    private float3 closestPoint;

    private void Awake()
    {
        splineContainer = FindAnyObjectByType<SplineContainer>();

        if (splineContainer == null)
        {
            Debug.LogError("SplineContainer is not assigned in the inspector.");
            return;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(closestPoint, 0.1f);
    }

    internal float GetDistanceAlongSpline()
    {
        // get the closest point on the spline to the current position
        SplineUtility.GetNearestPoint(splineContainer.Spline, transform.position, out float3 _closestPoint, out float _distanceAlongSpline);

        closestPoint = _closestPoint;
        distanceAlongSpline = _distanceAlongSpline;

        if (distanceAlongSpline > 1)
        {
            distanceAlongSpline = 1;
        }
        else if (distanceAlongSpline < 0)
        {
            distanceAlongSpline = 0;
        }

        //Debug.Log($"Distance along spline: {_distanceAlongSpline}");
        return distanceAlongSpline;
    }
}
