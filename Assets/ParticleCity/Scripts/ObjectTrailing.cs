using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTrailing : MonoBehaviour
{
    public struct TrailFrame
    {
        public Vector3 Position;
        public Vector3 Velocity;
    }

    public const int TrailLength = 8;
    public float TrailInterval = 0.1f;
    public bool Visualize = false;
    public bool LocalSpace = false;

    public TrailFrame[] Frames;
    private float lastCaptureTime;

    void OnEnable()
    {
        if (Frames == null)
        {
            Frames = new TrailFrame[TrailLength];

            for (int i = 0; i < TrailLength; i++)
            {
                Frames[i] = new TrailFrame
                {
                    Position = LocalSpace ? transform.localPosition : transform.position,
                    Velocity = Vector3.zero
                };
            }
        }
    }
	
	void Update ()
	{
	    if (Time.time - lastCaptureTime > TrailInterval)
	    {
	        for (int i = TrailLength - 1; i >= 1; i--)
	        {
	            Frames[i] = Frames[i - 1];
	        }

	        Frames[0].Velocity = (LocalSpace ? transform.localPosition : transform.position - Frames[0].Position) / Time.deltaTime;
	        Frames[0].Position = LocalSpace ? transform.localPosition : transform.position;

	        lastCaptureTime = Time.time;
	    }
	}

    void OnDrawGizmos()
    {
	    if (Application.isPlaying && Visualize)
	    {
	        for (int i = 0; i < TrailLength; i += 1)
	        {
                Gizmos.DrawWireSphere(Frames[i].Position, 0.2f);
	            Gizmos.DrawRay(Frames[i].Position, -Frames[i].Velocity / 50);
	        }
	    }
    }
}
