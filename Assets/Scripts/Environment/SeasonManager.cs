using UnityEngine;

public class SeasonManager : MonoBehaviour
{

    public static SeasonManager Instance { get; private set; }
    public enum Season
    {
        summer,
        winter
    }

    public Season currentSeason;
    public WeatherManager weatherManager;

    private bool percipitation;

    public bool IsSummer => currentSeason == Season.summer;
    public bool IsWinter => currentSeason == Season.winter;

    public bool IsRaining => percipitation && currentSeason == Season.summer;
    public bool IsSnowing => percipitation && currentSeason == Season.winter;


    [Header("Terrain")]
    public Terrain terrain;
    public TerrainLayer grassLayer;
    public TerrainLayer snowLayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        currentSeason = Season.summer;
        percipitation = false;
    }

    public void SetSummer(bool isOn)
    {
        if (isOn)
        {
            currentSeason = Season.summer;
            SetTerrainLayer();

        }
    }

    public void SetWinter(bool isOn)
    {
        if (isOn)
        {
            currentSeason = Season.winter;
            SetTerrainLayer();

        }
    }

    public void SetPercipitation(bool isOn)
    {
        percipitation = isOn;
    }

    private void Update()
    {
        if (!percipitation)
        {
            weatherManager.ChangeWeather(WeatherManager.Weather.sunny);
        }
        else if (currentSeason == Season.summer)
        {
            weatherManager.ChangeWeather(WeatherManager.Weather.rainy);
        }
        else if (currentSeason == Season.winter)
        {
            weatherManager.ChangeWeather(WeatherManager.Weather.snowy);
        }
    }

    private void SetTerrainLayer()
    {
        if (currentSeason == Season.winter)
        {
            terrain.terrainData.terrainLayers = new TerrainLayer[] { snowLayer };
        }
        else
        {
            terrain.terrainData.terrainLayers = new TerrainLayer[] { grassLayer };
        }
    }
}
