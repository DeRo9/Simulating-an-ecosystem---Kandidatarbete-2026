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

            thirstSlider.maxValue = current.maxThirst;
            thirstSlider.value = current.thirstLevel;

        }
    }


    public void SetMoose(AnimalNeeds newAnimal)
    {
        current = newAnimal;
    }

    public void ClearInfo()
    {
        current = null;
        panel.SetActive(false);
    }

    public void ShowInfo(Animal animal)
    {
        current = animal.needs;
        animalType.text = animal.species.ToString();
        panel.SetActive(true);
    }


}
