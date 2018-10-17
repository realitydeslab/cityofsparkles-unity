using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryUtils
{
    #region Triangle Sampler

    public static Vector3? SampleTriangle(Vector3 p0, Vector3 v1, Vector3 v2)
    {
        for (int retry = 0; retry < 100; retry++)
        {
            // Uniform triangle sampling
            // http://mathworld.wolfram.com/TrianglePointPicking.html  
            float a = Random.value;
            float b = Random.value;

            // Check if the sampled point is inside the triangle
            // http://mathworld.wolfram.com/TriangleInterior.html
            if (a + b > 1)
            {
                continue;
            }

            return p0 + a * v1 + b * v2;
        }

        return null;
    }

    public static Vector3 SampleTriangleEdge(Vector3 p0, Vector3 v1, Vector3 v2)
    {
        float k = Random.value;
        float edge = Random.value;

        if (edge < 0.33333)
        {
            return p0 + k * v1;
        }
        else if (edge < 0.66667)
        {
            return p0 + k * v2;
        }
        else
        {
            return p0 + k * v1 + (1 - k) * v2;
        }
    }

    #endregion

    #region Collider Sampler
    public static Vector3? SampleCollider(Collider collider)
    {
        // TODO: Better sampling

        var bounds = collider.bounds;

        for (int retry = 0; retry < 100; retry++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            var p = new Vector3(x, y, z);
            if (isPointInsideCollider(p, collider))
            {
                return p;
            }
        }

        return null;
    }

    private static bool isPointInsideCollider(Vector3 point, Collider collider)
    {
        // TODO
        var start = new Vector3(0, 10000, 0); // This is defined to be some arbitrary point far away from the collider.

        Vector3 goal = point; // This is the point we want to determine whether or not is inside or outside the collider.
        Vector3 direction = goal - start; // This is the direction from start to goal.
        direction.Normalize();
        var iterations = 0; // If we know how many times the raycast has hit faces on its way to the target and back, we can tell through logic whether or not it is inside.
        Vector3 currentPoint = start;

        int retryCount = 0;
        while (currentPoint != goal && retryCount < 100)
        {// Try to reach the point starting from the far off point.  This will pass through faces to reach its objective.
            retryCount++;
            RaycastHit hit;
            if (Physics.Linecast(currentPoint, goal, out hit))
            {// Progressively move the point forward, stopping everytime we see a new plane in the way.
                iterations++;
                currentPoint = hit.point + (direction / 100.0f); // Move the Point to hit.point and push it forward just a touch to move it through the skin of the mesh (if you don't push it, it will read that same point indefinately).
            }
            else
            {
                currentPoint = goal; // If there is no obstruction to our goal, then we can reach it in one step.
            }
        }

        retryCount = 0;
        while (currentPoint != start && retryCount < 100)
        {// Try to return to where we came from, this will make sure we see all the back faces too.
            retryCount++;
            RaycastHit hit;
            if (Physics.Linecast(currentPoint, start, out hit))
            {
                iterations++;
                currentPoint = hit.point + (-direction / 100.0f);
            }
            else
            {
                currentPoint = start;
            }
        }

        return (iterations % 2 == 1);
    }
    #endregion
}
