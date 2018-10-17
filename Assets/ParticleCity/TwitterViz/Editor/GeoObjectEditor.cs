using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GeoObject))]
public class GeoObjectEditor : Editor {

    private SerializedProperty latitude;    
    private SerializedProperty longitude;    
    private SerializedProperty altitude;

    public override void OnInspectorGUI()
    {
        GeoObject geoObject = target as GeoObject;

        EditorGUI.BeginChangeCheck();
        double newLat = EditorGUILayout.DoubleField("Latitude", geoObject.Latitude);
        double newLong =  EditorGUILayout.DoubleField("Longitude", geoObject.Longitude);
        double newAlt = EditorGUILayout.DoubleField("Altitude", geoObject.Altitude);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(geoObject, "Change Geo Location");
            geoObject.SetGeoLocation(newLat, newLong, newAlt);
            EditorUtility.SetDirty(geoObject);
        }
    }
}
