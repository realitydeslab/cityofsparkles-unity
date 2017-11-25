using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(AkAmbient))]
public class InteractiveMusicController : MonoBehaviour
{
    public ParticleCityAnimator Animator;
    public AnimationCurve MIDIForceToIntensity;
    public AnimationCurve MIDIForceToMinorIntensity;

    public event Action<string> AkMarkerTriggered; 

    private AkAmbient akAmbient;
    private float majorIntensity;
    private float minorIntensity;

    public float GetVolumeMeter()
    {
        float meter;
        int type = (int)RTPCValue_type.RTPCValue_GameObject;
        AkSoundEngine.GetRTPCValue("MasterVolume", gameObject, akAmbient.playingId, out meter, ref type);
        return meter;
    }

    void Awake()
    {
	    akAmbient = GetComponent<AkAmbient>();
        akAmbient.m_callbackData = new AkEventCallbackData()
        {
            callbackFlags = {(int)AkCallbackType.AK_MusicPlayStarted, (int)AkCallbackType.AK_MIDIEvent, (int)AkCallbackType.AK_Marker},
            callbackFunc = {"OnAkStart", "OnAkMIDI", "OnAkMarker"},
            callbackGameObj = {gameObject, gameObject, gameObject},
        };

        akAmbient.m_callbackData.uFlags = 0;
        for (int i = 0; i < akAmbient.m_callbackData.callbackFlags.Count; i++)
        {
            akAmbient.m_callbackData.uFlags |= akAmbient.m_callbackData.callbackFlags[i];
        }
    }

	// Use this for initialization
	void Start()
	{
        Debug.Log("Controller start, tid = " + Thread.CurrentThread.ManagedThreadId);
    }

    void OnDestroy()
    {
        Debug.Log("Ak Destroy: " + akAmbient.eventID);
        AkSoundEngine.StopPlayingID(akAmbient.playingId);
    }

    void OnAkStart(AkEventCallbackMsg msg)
    {
        Debug.Log("Ak Start: " + msg.info);
    }

    void OnAkMarker(AkEventCallbackMsg msg)
    {
        AkMarkerCallbackInfo info = (AkMarkerCallbackInfo) msg.info;       
        Debug.Log("Ak Marker: " + info.strLabel);

        if (AkMarkerTriggered != null)
        {
            AkMarkerTriggered(info.strLabel);
        }
    }

    void OnAkMIDI(AkEventCallbackMsg msg)
    {
        AkMIDIEventCallbackInfo info = (AkMIDIEventCallbackInfo) msg.info;

        float pitch = info.byParam1;
        float force = info.byParam2;

        if (info.byType == 144)
        {
            if (pitch < 50)
            {
                float target = MIDIForceToIntensity.Evaluate(force / 128.0f);
                majorIntensity = Mathf.Lerp(majorIntensity, target, 0.8f);
                minorIntensity = 0;
                // Debug.Log("Major: " + target);
            }
            else
            {
                float target = MIDIForceToMinorIntensity.Evaluate(pitch / 128.0f);
                minorIntensity = Mathf.Lerp(minorIntensity, target, 0.1f);
                // Debug.Log("Minor: " + target);
            }
            Animator.GlobalIntensity = majorIntensity + minorIntensity;
        }
        if (pitch < 50 && info.byType == 144)
        {
        }
    }
}
