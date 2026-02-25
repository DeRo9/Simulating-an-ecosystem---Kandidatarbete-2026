using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider hungerSlider;
    public Slider thirstSlider;
    public Slider staminaSlider;
    public Slider healthSlider;

    [Header("UI")]
    public TMP_Text animalType;
    public GameObject panel;

    [Header("Animal Info")]
    public AnimalNeeds current;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (current != null)
        {
            hungerSlider.maxValue = current.maxHunger;
            hungerSlider.value = current.hungerLevel;
        }
    }


    public void SetMoose(AnimalNeeds newMoose)
    {
        current = newMoose;
    }

    public void SetType(string type)
    {
        animalType.text = type;
    }

    public void ClearInfo()
    {
        current = null;
        panel.SetActive(false);
    }

    public void ShowInfo(AnimalNeeds moose)
    {
        Debug.Log("SHOW INFO");
        current = moose;
        panel.SetActive(true);
    }


}
