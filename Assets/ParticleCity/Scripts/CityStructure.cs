using ParticleCities;
using UnityEngine;

public class CityStructure : MonoBehaviour
{
    private static CityStructure instance;
    public static CityStructure Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CityStructure>();
            }

            return instance;
        }
    }

    [Header("Debug")]
    public HeightMap HeightMap;

    public MapModel MapModel;

    void Awake()
    {
        HeightMap = GetComponentInChildren<HeightMap>();
        MapModel = GetComponent<MapModel>();
    }
}
