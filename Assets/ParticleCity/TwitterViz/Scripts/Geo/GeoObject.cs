using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GeoObject : MonoBehaviour
{
    [SerializeField]
    private double latitude;
    public double Latitude { get { return latitude; } }

    [SerializeField]
    private double longitude;
    public double Longitude { get { return longitude; } }

    [SerializeField]
    private double altitude;
    public double Altitude { get { return altitude; } }

#if UNITY_EDITOR
    private const double GEO_EPSILON = 1e-6;
    private double previousLatitude;
    private double previousLongitude;
    private double previousAltitude;
    private Vector3 previousPosition;
#endif

	void Start () 
    {
		
	}
	
	void Update () 
    {
#if UNITY_EDITOR
        if (hasGeoLocationChanged())
        {
            updateTransformForGeoLocation();
        }    
        else if (hasWorldPositionChanged())
        {
            updateGeoLocationForTransform();
        }

        previousLatitude = latitude;
        previousLongitude = longitude;
        previousAltitude = altitude;
        previousPosition = transform.position;
#endif
    }

    public void SetGeoLocation(double newLatitude, double newLongitude, double newAltitude)
    {
        latitude = newLatitude;
        longitude = newLongitude;
        altitude = newAltitude;
        
        updateTransformForGeoLocation();
    }
    
    public void SetWorldPosition(Vector3 position)
    {
        transform.position = position;
        updateGeoLocationForTransform();
    }

    private void updateTransformForGeoLocation()
    {
        MapModel mapModel = GetComponentInParent<MapModel>();
        if (mapModel == null)
        {
            Debug.LogError("Cannot find MapModel in parents of GeoObject " + name);
            return;
        }
        transform.position = mapModel.EarthToUnityWorld(latitude, longitude, altitude);
    }

    private void updateGeoLocationForTransform()
    {
        MapModel mapModel = GetComponentInParent<MapModel>();
        if (mapModel == null)
        {
            Debug.LogError("Cannot find MapModel in parents of GeoObject " + name);
            return;
        }
        
        mapModel.UnityWorldToEarth(transform.position, out latitude, out longitude, out altitude);
    }

#if UNITY_EDITOR
    private bool hasGeoLocationChanged()
    {
        return Math.Abs(latitude - previousLatitude) > GEO_EPSILON
               || Math.Abs(longitude - previousLongitude) > GEO_EPSILON
               || Math.Abs(altitude - previousAltitude) > GEO_EPSILON;
    }

    private bool hasWorldPositionChanged()
    {
        return transform.position != previousPosition;
    }
#endif
}
