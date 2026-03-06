using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodSetupPanel : MonoBehaviour
{
    [Header("Info")]
    public string itemName = "Food";

    [Header("Amount")]
    public Slider amountSlider;
    public TextMeshProUGUI amountText;

    // Read-only property for other scripts to query
    public int amount => (int)amountSlider.value;

    private void Start()
    {
        UpdateAmountText();
    }

    public void UpdateAmountText()
    {
        if (amountText != null)
            amountText.text = $"Amount of {itemName}: {amount}";
    }
}
