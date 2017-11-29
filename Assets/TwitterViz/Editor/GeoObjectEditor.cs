using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GeoObject))]
public class GeoObjectEditor : Editor {

    private SerializedProperty latitude;    
    private SerializedProperty longitude;    
    private SerializedProperty altitude;

    void OnEnable()
    {
        latitude = serializedObject.FindProperty("Latitude");
        longitude = serializedObject.FindProperty("Longitude");
        altitude = serializedObject.FindProperty("Altitude");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GeoObject geoObject = target as GeoObject;

        EditorGUI.BeginChangeCheck();
        double newLat = EditorGUILayout.Slider("Latitude", (float)geoObject.Latitude, -90f, 90);
        double newLong =  EditorGUILayout.Slider("Longitude", (float)geoObject.Longitude, -180f, 180f);
        double newAlt = EditorGUILayout.DoubleField("Altitude", geoObject.Altitude);
        if (EditorGUI.EndChangeCheck())
        {
            geoObject.SetGeoLocation(newLat, newLong, newAlt);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
