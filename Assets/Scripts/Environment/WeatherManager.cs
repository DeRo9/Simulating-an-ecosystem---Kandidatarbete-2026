using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public enum Weather
    {
        sunny,
        rainy,
        snowy
    }

    public Weather currentWeather;
    public GameObject raining;
    public GameObject snowing;


    [Header("Probability precipitation")]
    [UnityEngine.Range(0f, 1f)] public float rainProbability = 0.05f;
    [UnityEngine.Range(0f, 1f)] public float snowProbability = 0.05f;
    public bool precipitationActive = false;

    // Precipitation duration settings
    public float precipTimer = 0f;
    public float precipTimerMin = 40f;
    public float precipTimerMax = 60f;



    private void Start()
    {
        currentWeather = Weather.sunny;
        ApplyWeather();
    }

    private void Update()
    {
        if (!GameManager.GetSimulationStatus()) return;

        if (SeasonManager.GetCurrentSeason() == SeasonManager.Season.summer)
        {
            if (currentWeather == Weather.sunny)
            {
                // Chance to start raining
                if (Random.value < rainProbability * Time.deltaTime)
                {
                    precipTimer = Random.Range(precipTimerMin, precipTimerMax);
                    ChangeWeather(Weather.rainy);
                    precipitationActive = true;
                }
            }
            else if (currentWeather == Weather.rainy)
            {
                precipTimer -= Time.deltaTime;
                if (precipTimer <= 0f)
                {
                    ChangeWeather(Weather.sunny);
                    precipitationActive = false;
                }
            }
        } 
        
        else if (SeasonManager.GetCurrentSeason() == SeasonManager.Season.winter)
        {
            if(currentWeather == Weather.sunny)
            {
                if (Random.value < snowProbability * Time.deltaTime)
                {
                    precipTimer = Random.Range(precipTimerMin, precipTimerMax);
                    ChangeWeather(Weather.snowy);
                    precipitationActive = true;
                }
            } else if (currentWeather == Weather.snowy)
            {
                precipTimer -= Time.deltaTime;
                if(precipTimer <= 0f)
                {
                    ChangeWeather(Weather.sunny);
                    precipitationActive = false;
                }
            }
        }
    }

    public void ChangeWeather(Weather newWeather)
    {
        currentWeather = newWeather;
        raining.SetActive(newWeather == Weather.rainy);
        snowing.SetActive(newWeather == Weather.snowy);
        ApplyWeather();
    }

    private void ApplyWeather()
    {
        if (currentWeather == Weather.rainy)
        {
            snowing.SetActive(false);
            raining.SetActive(true);
            RenderSettings.skybox.SetFloat("_Exposure", 0.5f);
            RenderSettings.skybox.SetColor("_Tint", new Color(0.48f, 0.49f, 0.54f));
            RenderSettings.fogDensity = 0.02f;
        }
        else if (currentWeather == Weather.snowy)
        {
            snowing.SetActive(true);
            raining.SetActive(false);
            RenderSettings.skybox.SetFloat("_Exposure", 0.8f);
            RenderSettings.skybox.SetColor("_Tint", new Color(0.75f, 0.78f, 0.85f));
            RenderSettings.fogDensity = 0.015f;
        }
        else
        {
            snowing.SetActive(false);
            raining.SetActive(false);
            RenderSettings.skybox.SetFloat("_Exposure", 1f);
            RenderSettings.skybox.SetColor("_Tint", Color.white);
            RenderSettings.fogDensity = 0.01f;
        }
    }
}

