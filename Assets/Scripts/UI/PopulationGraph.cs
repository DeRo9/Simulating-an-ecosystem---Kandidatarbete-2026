using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopulationGraph : MonoBehaviour
{
    public RectTransform initialBar;
    public RectTransform finalBar;

    public float maxHeight = 200f;

    public TextMeshProUGUI bearInitialText;
    public TextMeshProUGUI finalInitialText;

    [Header("Species Counts")]
    public float initial;
    public float final;
    
    public void UpdateGraph(int initial, int final)
    {
        int maxValue = Mathf.Max(initial, final);

        float initialHeight = (float)initial / maxValue * maxHeight;
        float finalHeight = (float)final / maxValue * maxHeight;

        initialBar.sizeDelta = new Vector2(initialBar.sizeDelta.x, initialHeight);
        finalBar.sizeDelta = new Vector2(finalBar.sizeDelta.x, finalHeight);
        bearInitialText.text = $"Initial: {initial}";
        finalInitialText.text = $"Final: {final}";
    }
}