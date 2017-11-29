using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private const double GEO_EPSILON = 1e-6;
    private double previousLatitude;
    private double previousLongitude;
    private double previousAltitude;

	void Start () 
    {
		
	}
	
	void Update () 
    {
	    	
	}

    public void SetGeoLocation(double newLatitude, double newLongitude, double newAltitude)
    {
        latitude = newLatitude;
        longitude = newLongitude;
        altitude = newAltitude;

        MapModel mapModel = GetComponentInParent<MapModel>();
        if (mapModel == null)
        {
            Debug.LogError("Cannot find MapModel in parents of GeoObject " + name);
            return;
        }
        transform.position = mapModel.EarthToUnityWorld(latitude, longitude, altitude);
    }

    public void SetWorldPosition(Vector3 position)
    {
        transform.position = position;
        UpdateGeoLocationForTransform();
    }

    public void UpdateGeoLocationForTransform()
    {
        MapModel mapModel = GetComponentInParent<MapModel>();
        if (mapModel == null)
        {
            Debug.LogError("Cannot find MapModel in parents of GeoObject " + name);
            return;
        }
        
        mapModel.UnityWorldToEarth(transform.position, out latitude, out longitude, out altitude);
    }
}
