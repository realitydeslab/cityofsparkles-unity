using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ParticleCities;
using UnityEngine;
using WanderUtils;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(AkAmbient))]
public class InteractiveMusicController : MonoBehaviour
{
    private static InteractiveMusicController instance;

    public static InteractiveMusicController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InteractiveMusicController>();
            }

            return instance;
        }
    }

    public AnimationCurve MIDIForceToIntensity;
    public AnimationCurve MIDIForceToMinorIntensity;

    // Track randomizer
    public float TrackRandomizeInterval;

    public event Action<string> AkMarkerTriggered;
    public event Action<string> AkMusicSyncCueTriggered;

    [Header("Debug")]
    public float MajorIntensity;
    public float MinorIntensity;
    public float Density;
    public float AccumulatedForce;

    private AkAmbient akAmbient;
    private List<GameObject> PointsOfInterest = new List<GameObject>();
    private float lastTrackRandomizeTime;

    public float GetVolumeMeter()
    {
        float meter;
        int type = (int)AkQueryRTPCValue.RTPCValue_GameObject; 
        AkSoundEngine.GetRTPCValue("MasterVolume", gameObject, akAmbient.playingId, out meter, ref type);
        return meter;
    }

    public void AddPointOfInterest(GameObject poi)
    {
        if (!PointsOfInterest.Contains(poi))
        {
            PointsOfInterest.Add(poi);
        }
    }

    public void RemovePointOfInterest(GameObject poi)
    {
        PointsOfInterest.Remove(poi);
    }

    public void AddAccumulatedForce(float force)
    {
        AccumulatedForce += force;
    }

    public void ResetAccumulatedForce()
    {
        AccumulatedForce = 0;
    }

    void Awake()
    {
    }

	void Start()
	{
	    akAmbient = GetComponent<AkAmbient>();
	    
        /*
        akAmbient.m_callbackData = new AkEventCallbackData()
        {
            callbackFlags = {(int)AkCallbackType.AK_MusicPlayStarted, (int)AkCallbackType.AK_MIDIEvent, (int)AkCallbackType.AK_Marker, (int)AkCallbackType.AK_MusicSyncUserCue},
            callbackFunc = {"OnAkStart", "OnAkMIDI", "OnAkMarker", "OnAkMusicSyncCue"},
            callbackGameObj = {gameObject, gameObject, gameObject, gameObject},
        };

        akAmbient.m_callbackData.uFlags = 0;
        for (int i = 0; i < akAmbient.m_callbackData.callbackFlags.Count; i++)
        {
            akAmbient.m_callbackData.uFlags |= akAmbient.m_callbackData.callbackFlags[i];
        }
        */
        
        AkSoundEngine.SetState("RichSentimentTest", SentimentSpawnNode.Sentiment.Neutral.ToString());
        Debug.Log("Controller start, tid = " + Thread.CurrentThread.ManagedThreadId);
    }

    void Update()
    {
        Density = CityStructure.Instance.DensityMap.GetDensity(InputManager.Instance.PlayerTransform.position);
        AkSoundEngine.SetRTPCValue("Density", Density);

        if (PointsOfInterest.Count > 0)
        {
            Vector3 poiPos = PointsOfInterest[PointsOfInterest.Count - 1].transform.position;
            Vector3 playerPos = InputManager.Instance.CenterCamera.gameObject.transform.position;
            AkSoundEngine.SetRTPCValue("DistanceToPOI", Vector3.Distance(poiPos, playerPos));
        }

        AkSoundEngine.SetRTPCValue("AccumulatedForce", AccumulatedForce, gameObject);

        updateTrackRandomize();
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

        if (info.byType == AkMIDIEventTypes.NOTE_ON)
        {
            if (pitch < 50)
            {
                float target = MIDIForceToIntensity.Evaluate(force / 128.0f);
                MajorIntensity = Mathf.Lerp(MajorIntensity, target, 0.8f);
                MinorIntensity = 0;
                // Debug.Log("Major: " + target);
            }
            else
            {
                float target = MIDIForceToMinorIntensity.Evaluate(pitch / 128.0f);
                MinorIntensity = Mathf.Lerp(MinorIntensity, target, 0.1f);
                // Debug.Log("Minor: " + target);
            }
            ParticleCity.Current.Animator.GlobalIntensity = MajorIntensity + MinorIntensity;
        }
        if (pitch < 50 /*&& info.byType == 144*/)
        {
        }
    }

    void OnAkMusicSyncCue(AkEventCallbackMsg msg)
    {
        AkMusicSyncCallbackInfo syncInfo = (AkMusicSyncCallbackInfo)msg.info;
        string cue = syncInfo.userCueName;
        Debug.Log("Ak Cue: " + cue);
        if (AkMusicSyncCueTriggered != null)
        {
            AkMusicSyncCueTriggered(cue);
        }
    }

    private void updateTrackRandomize()
    {
        if (Time.time - lastTrackRandomizeTime > TrackRandomizeInterval)
        {
            for (int i = 1; i <= 12; i++)
            {
                string state = (UnityEngine.Random.value > 0.5f) ? "Enabled" : "Disabled";
                AkSoundEngine.SetSwitch("TR" + i, state, gameObject);
            }

            lastTrackRandomizeTime = Time.time;
        }
    }
}
