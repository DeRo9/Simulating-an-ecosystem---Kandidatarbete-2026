using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodSetupPanel : MonoBehaviour
{
    [Header("Info")]
    public string itemName = "Food";

    [Header("Amount")]
    public TMP_InputField amountInput;
    public TextMeshProUGUI amountText;

    
    private int minAmount = 0;
    private int maxAmount = 100;
    public int amount { get; private set; } = 0;

    private void Start()
    {
        UpdateAmountText();
        amountInput.onEndEdit.AddListener(OnEditEnd);
    }

    public void UpdateAmountText()
    {
        if (amountText != null)
            amountText.text = $"Amount of {itemName}:";
    }

    public void Increment()
    {
        amount = Mathf.Clamp(amount + 1, minAmount, maxAmount);
        amountInput.text = amount.ToString();
    }

    public void Decrement()
    {
        amount = Mathf.Clamp(amount - 1, minAmount, maxAmount);
        amountInput.text = amount.ToString();
    }

    public void OnEditEnd(string value)
    {
        if (int.TryParse(value, out int parsed))
        {
            amount = Mathf.Clamp(parsed, minAmount, maxAmount);
            amountInput.text = amount.ToString();
        }
        else
        {
            amount = minAmount;
            amountInput.text = amount.ToString();
        }
    }

}
