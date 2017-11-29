using AmberGarage.Trajen;
using UnityEngine;

public class MapModel : MonoBehaviour {

    [Range(-90, 90)]
    public double RefLatitude;

    [Range(-180, 180)]
    public double RefLongitude;

    public double RefAltitude;

    public string Name {
        get {
            return gameObject.name;
        }
    }

    private Bounds? bounds = null;

    void Start() {
        Transform boundsTransform = transform.Find("Bounds");
        if (boundsTransform != null) {
            BoxCollider collider = boundsTransform.GetComponent<BoxCollider>();
            if (collider != null) {
                bounds = collider.bounds;
            }
        } else {
            bounds = null;
        }
    }

    public bool BoundsContainsPoint(Vector3 point) {
        if (!bounds.HasValue) {
            return true;
        }

        return bounds.Value.Contains(point);
    }

    public Vector3 EarthToLocal(double latitude, double longitude, double altitude)
    {
        double x, y, z;

        GeodeticConverter.geodetic_to_enu(
            latitude, longitude, altitude,
            RefLatitude, RefLongitude, RefAltitude, 
            out x, out z, out y
        );

        return new Vector3((float)x, (float)y, (float)z);
    }

    public Vector3 EarthToUnityWorld(double latitude, double longitude, double altitude)
    {
        Vector3 local = EarthToLocal(latitude, longitude, altitude);
        return transform.TransformPoint(local);
    }

    public void LocalToEarth(Vector3 localPosition, out double latitude, out double longitude, out double altitude)
    {
        GeodeticConverter.enu_to_geodetic(
            localPosition.x, localPosition.z, localPosition.y,
            RefLatitude, RefLongitude, RefAltitude,
            out latitude, out longitude, out altitude  
            );
    }

    public void UnityWorldToEarth(Vector3 position, out double latitude, out double longitude, out double altitude)
    {
        Vector3 local = transform.InverseTransformPoint(position);
        LocalToEarth(local, out latitude, out longitude, out altitude);
    }

    public void GlobalOffsetToDeltaENU(Vector3 globalOffset, out double deltaEast, out double deltaNorth, out double deltaAltitude)
    {
        Vector3 localOffset = transform.InverseTransformVector(globalOffset);
        deltaEast = localOffset.x;
        deltaNorth = localOffset.z;
        deltaAltitude = localOffset.y;
    }
}
