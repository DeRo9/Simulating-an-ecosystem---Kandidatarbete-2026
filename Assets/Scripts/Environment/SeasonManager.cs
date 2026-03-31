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

    public bool isRaining => percipitation && currentSeason == Season.summer;
    public bool isSnowing => percipitation && currentSeason == Season.winter;

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
        }
    }

    public void SetWinter(bool isOn)
    {
        if (isOn)
        {
            currentSeason = Season.winter;
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
}
