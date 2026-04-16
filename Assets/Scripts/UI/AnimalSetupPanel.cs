using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class AnimalSetupPanel : MonoBehaviour
{

    public TMP_InputField amountInput;


    private int minAmount = 0;
    private int maxAmount = 50;
    public int amount { get; private set; } = 0;

    private void Start()
    {
        amountInput.onEndEdit.AddListener(OnEditEnd);
    }

    public void Increment()
    {
        amount = Mathf.Clamp(amount + 5, minAmount, maxAmount);
        amountInput.text = amount.ToString();
    }

    public void Decrement()
    {
        amount = Mathf.Clamp(amount - 5, minAmount, maxAmount);
        amountInput.text = amount.ToString();
    }

    public void OnEditEnd(string value)
    {
        if (int.TryParse(value, out int parsed))
        {
            amount = Mathf.Clamp(parsed, minAmount, maxAmount);
            amountInput.text = amount.ToString();
        } else
        {
            amount = minAmount;
            amountInput.text = amount.ToString();
        }
    }

    /*
    public Slider amountSlider;
    public int amount => (int)amountSlider.value;
    */
    


    /*

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

    [Header("Attribute Sliders")]
    public Slider speedSlider;
    public Slider sizeSlider;
    public Slider sightSlider;
    public Slider hearingSlider;

    [Header("Base Attributes")]
    public float baseSpeed;
    public float baseSize;
    public float baseSight;
    public float baseHearing;

    public int amount => (int)amountSlider.value;
    public float updatedSpeed => (int)speedSlider.value;
    public float updatedSize => (int)sizeSlider.value;
    public float updatedSight => (int)sightSlider.value;
    public float updatedHearing => (int)hearingSlider.value;

    private void Start()
    {
        baseSpeed = animal.speed;
        baseSize = animal.size;
        baseSight = animal.sightRange;
        baseHearing = animal.hearingRange;
        attributePanel.SetActive(false);

        SetSlidersToBaseValues();

        UpdateAmountText();
        UpdateSpeedText();
        UpdateSizeText();
        UpdateSightText();
        UpdateHearingText();
    }

    public void SetSlidersToBaseValues() {
        speedSlider.value = baseSpeed;
        sizeSlider.value = baseSize;
        sightSlider.value = baseSight;
        hearingSlider.value = baseHearing;
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
        speedText.text = $"Speed: {speedSlider.value}";
        animal.speed = speedSlider.value;
    }

    public void UpdateSizeText()
    {
        sizeText.text = $"Size: {sizeSlider.value}";
    }

    public void UpdateSightText()
    {
        sightText.text = $"Sight: {sightSlider.value}";
    }

    public void UpdateHearingText()
    {
        hearingText.text = $"Hearing: {hearingSlider.value}";
    }
    */
}
