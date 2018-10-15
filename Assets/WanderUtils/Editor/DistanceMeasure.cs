using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DistanceMeasure 
{
    [MenuItem("Tools/Wander Utils/Measure Distance")]
    public static void MeasureDistance()
    {
        Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel);
        if (transforms.Length != 2)
        {
            Debug.LogError("Select exactly two objects to measure their distance. ");
            return;
        }

        Vector3 delta = transforms[1].position - transforms[0].position;
        Debug.Log(string.Format("From {0} to {1}: Distance = {2}, Squared = {6}, Delta = ({3}, {4}, {5})", 
            transforms[0].gameObject.name,
            transforms[1].gameObject.name,
            delta.magnitude,
            delta.x,
            delta.y,
            delta.z,
            delta.sqrMagnitude));
    }
}
