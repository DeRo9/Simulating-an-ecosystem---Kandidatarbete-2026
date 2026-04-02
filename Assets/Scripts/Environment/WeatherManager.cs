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


    private void Start()
    {
        currentWeather = Weather.sunny;
        ApplyWeather();
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

