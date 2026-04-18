using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PopulationGraph : MonoBehaviour
{
    public RectTransform graphContainer;
    public RectTransform pointPrefab;
    public RectTransform linePrefab;

    public TextMeshProUGUI yAxisLabelPrefab;
    public TextMeshProUGUI xAxisLabelPrefab;

    public float graphWidth = 400;
    public float graphHeight = 300;

    public void DrawGraph(List<int> values, float simulationLength)
    {
        int maxValue = 1;

        foreach (int v in values)
        {
            if (v > maxValue) maxValue = v;
        }

        maxValue = Mathf.CeilToInt(maxValue * 1.2f);

        float xSpacing = graphWidth / (values.Count);

        RectTransform lastPoint = null;

        for (int i = 0; i < values.Count; i++)
        {
            float xPos = i * xSpacing;
            float yPos = (values[i] / (float)maxValue) * graphHeight;

            RectTransform point = Instantiate(pointPrefab, graphContainer);
            point.anchoredPosition = new Vector2(xPos, yPos);

            if (lastPoint != null)
            {
                DrawLine(lastPoint.anchoredPosition, point.anchoredPosition);
            }

            lastPoint = point;
        }

        CreateYAxis(maxValue);
        CreateXAxis(values.Count, simulationLength);
    }

    void DrawLine(Vector2 a, Vector2 b)
    {
        RectTransform line = Instantiate(linePrefab, graphContainer);

        Vector2 dir = (b - a).normalized;
        float distance = Vector2.Distance(a, b);

        line.sizeDelta = new Vector2(distance, 3f);
        line.anchoredPosition = a + dir * distance * 0.5f;
        line.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    void CreateYAxis(int maxValue)
    {
        int steps = 5;

        for (int i = 0; i <= steps; i++)
        {
            float normalized = i / (float)steps * 0.95f; 

            TextMeshProUGUI label = Instantiate(yAxisLabelPrefab, graphContainer);
            label.text = Mathf.RoundToInt(maxValue * normalized).ToString();

            label.rectTransform.anchoredPosition = new Vector2(-30, normalized * (graphHeight));
        }
    }

    void CreateXAxis(int valueCount, float simulationLength)
    {
        int steps = 5;

        for (int i = 0; i <= steps; i++)
        {
            float normalized = i / (float)steps * 0.95f;

            TextMeshProUGUI label = Instantiate(xAxisLabelPrefab, graphContainer);

            float time = simulationLength * normalized;
            label.text = time.ToString("0");

            label.rectTransform.anchoredPosition = new Vector2(normalized * (graphWidth) -30, 0);
        }
    }
}