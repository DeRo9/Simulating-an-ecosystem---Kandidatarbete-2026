using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeSetup : MonoBehaviour
{

    [Header("Amount")]
    public TMP_InputField amountInput;


    private int minAmount = 0;
    private int maxAmount = 600;
    public int amount { get; private set; } = 0;

    private void Start()
    {
        amountInput.onEndEdit.AddListener(OnEditEnd);
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

    public void SetAmount(int newAmount)
    {
        amount = Mathf.Clamp(newAmount, minAmount, maxAmount);
        amountInput.text = amount.ToString();
    }

}
