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
    public bool precipitationActive = false;

    // Rain duration settings
    public float rainTimer = 0f;
    public float rainTimerMin = 40f;
    public float rainTimerMax = 60f;



    private void Start()
    {
        currentWeather = Weather.sunny;
        ApplyWeather();
    }

    private void Update()
    {
        if (!GameManager.GetSimulationStatus()) return;

        if (currentWeather == Weather.sunny)
        {
            // Chance to start raining
            if (Random.value < rainProbability * Time.deltaTime) {
                rainTimer = Random.Range(rainTimerMin, rainTimerMax);
                ChangeWeather(Weather.rainy);
                precipitationActive = true;
            }
        } else if (currentWeather == Weather.rainy)
        {
            rainTimer -= Time.deltaTime;
            if (rainTimer <= 0f)
            {
                ChangeWeather(Weather.sunny);
                precipitationActive = false;
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

