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
    public Slider pregnancySlider;

    [Header("UI")]
    public TMP_Text animalType;
    public GameObject panel;
    public GameObject imageMale;
    public GameObject imageFemale;

    public TextMeshProUGUI stateText;

    [Header("Animal Info")]
    public AnimalNeeds current;

    public AnimalBehaviour currentBehaviour;



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

            healthSlider.maxValue = current.maxHealth;
            healthSlider.value = current.healthLevel;

            staminaSlider.maxValue = current.maxStamina;
            staminaSlider.value = current.staminaLevel;

            float pregnancyValue = current.GetComponent<Mating>()?.GetPregnancyTimer() ?? 0f;
            pregnancySlider.maxValue = 30f;
            pregnancySlider.value = pregnancyValue;

        }

        if (currentBehaviour != null)
        {
            stateText.text = "State: " + currentBehaviour.CurrentState.ToString();
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
        ClearGender();
    }

    public void ShowInfo(Animal animal)
    {
        current = animal.needs;
        currentBehaviour = animal.GetComponent<AnimalBehaviour>();

        animalType.text = animal.species.ToString();
        panel.SetActive(true);

        ShowGender(animal);
        ShowPregnancy(animal);
    }

    public void ShowGender(Animal animal)
    {
        if (animal.IsMale)
        {
            imageMale.SetActive(true);
            imageFemale.SetActive(false);
        }
        else
        {
            imageFemale.SetActive(true);
            imageMale.SetActive(false);
        }
    }

    public void ClearGender()
    {
        imageMale.SetActive(false);
        imageFemale.SetActive(false);
    }

    public void ShowPregnancy(Animal animal)
    {
        if (animal.IsMale)
        {
            pregnancySlider.gameObject.SetActive(false);
        }
        else
        {
            pregnancySlider.gameObject.SetActive(true);
        }
    }


}
