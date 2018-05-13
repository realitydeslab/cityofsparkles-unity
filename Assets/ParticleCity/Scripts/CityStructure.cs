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
    public DensityMap DensityMap;

    void Awake()
    {
        HeightMap = GetComponentInChildren<HeightMap>();
        DensityMap = GetComponentInChildren<DensityMap>();
        MapModel = GetComponent<MapModel>();
    }
}
