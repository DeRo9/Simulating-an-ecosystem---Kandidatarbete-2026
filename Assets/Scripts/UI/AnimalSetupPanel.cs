using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimalSetupPanel : MonoBehaviour
{

    public Animal animal; 
    [Header("Info")]
    public string animalName;

    [Header("Attribute Panel")]
    public GameObject attributePanel;

    [Header("Amount")]
    public Slider amountSlider;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI sizeText;
    public TextMeshProUGUI sightText;
    public TextMeshProUGUI hearingText;



    [Header("Base Attributes")]
    public float baseSpeed;
    public float baseSize;
    public float baseSight;
    public float baseHearing;

    public int amount => (int)amountSlider.value;
    public float updatedSpeed => baseSpeed;
    public float updatedSize => baseSize;
    public float updatedSight => baseSight;
    public float updatedHearing => baseHearing;

    private void Start()
    {
        float speed, size, sight, hearing;
        animal.GetStats(out speed, out size, out sight, out hearing);


        baseSpeed = speed;
        baseSize = size;
        baseSight = sight;
        baseHearing = hearing;

        attributePanel.SetActive(false);

        UpdateAmountText();
        UpdateSpeedText();
        UpdateSizeText();
        UpdateSightText();
        UpdateHearingText();
    }

    public void ToggleAttributePanel()
    {
        attributePanel.SetActive(!attributePanel.activeSelf);
    }

    public void UpdateAmountText()
    {
        amountText.text = $"Amount of {animalName}: {amountSlider.value}";
    }

    public void UpdateSpeedText()
    {
        speedText.text = $"Speed: {baseSpeed}";
    }

    public void UpdateSizeText()
    {
        sizeText.text = $"Size: {baseSize}";
    }

    public void UpdateSightText()
    {
        sightText.text = $"Sight: {baseSight}";
    }

    public void UpdateHearingText()
    {
        hearingText.text = $"Hearing: {baseHearing}";
    }
}
