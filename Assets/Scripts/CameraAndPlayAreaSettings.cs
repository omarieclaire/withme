using UnityEngine;

public class CameraAndPlayAreaSettings : MonoBehaviour
{
    [Header("Camera Settings")]
    public int actualCameraResolution = 640;
    public Vector2 RemapCamCoords2GameCoords = new Vector2(1, -1);

    [Header("Play Area Settings")]
    public Transform center;
    public float playAreaSphereSize = 10;
    public float maxDegrees = 210;
    public float minY = 0f;
    public float pushTowardsBottom = 0.5f;
    public float verticalValue = 1;
    public Vector3 maxSize = new Vector3(1, 0, 1);

    public Vector3 SphericalToCartesian(float radius, float polar, float elevation)
    {
        float a = radius * Mathf.Cos(elevation);
        Vector3 outCart = new Vector3();
        outCart.x = a * Mathf.Cos(polar);
        outCart.y = radius * Mathf.Sin(elevation);
        outCart.z = a * Mathf.Sin(polar);
        return outCart;
    }

    private Vector3 CalculatePosition(Vector3 position)
    {
        float nX = position.x / maxSize.x;
        float nZ = position.z / maxSize.z;

        Vector2 nXZ = new Vector2(nX, nZ);
        float l = Mathf.Pow(nXZ.magnitude, pushTowardsBottom);

        float angleAround = Mathf.Atan2(nZ, nX);
        float angleDown = l * (maxDegrees / 360) * 2 * Mathf.PI;

        Vector3 fPosition = SphericalToCartesian(playAreaSphereSize, angleAround, Mathf.PI - angleDown);
        return transform.TransformPoint(fPosition);
    }

    public Vector3 getFinalPosition(Vector3 position)
    {
        return CalculatePosition(position);
    }

    public Vector3 getFinalPositionObjects(Vector3 position)
    {
        Vector3 fPosition = CalculatePosition(position);
        // Apply any additional specific adjustments for objects if needed.
        return fPosition;
    }
}
