using TMPro;
using UnityEngine;

public class SimulationUI : MonoBehaviour
{
    [Header("FPS Text")]
    [SerializeField] TextMeshProUGUI FPS;
    private float fps;

    [Header("Count Animals Text")]
    [SerializeField] TextMeshProUGUI Wolfves;
    [SerializeField] TextMeshProUGUI Bears;
    [SerializeField] TextMeshProUGUI Moose;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FPS.SetText("FPS: {0}", 0);
        InvokeRepeating("UpdateFPS", 1, 1);

        Wolfves.SetText("Wolfs: {0}", 0);
        Bears.SetText("Bears: {0}", 0);
        Moose.SetText("Moose: {0}", 0);
    }

    void Update()
    {
        UpdateWolves();
        UpdateBears();
        UpdateMoose();
    }

    void UpdateWolves()
    {
        Wolfves.SetText("Wolfs: {0}", CountWolves());
    }

    void UpdateBears()
    {
        Bears.SetText("Bears: {0}", CountBears());
    }
    void UpdateMoose()
    {
        Moose.SetText("Moose: {0}", CountMoose());
    }

    void UpdateFPS()
    {
        fps = (int)(1f / Time.unscaledDeltaTime);
        FPS.SetText("FPS: {0}", fps);
    }

    int CountWolves()
    {
        return FindObjectsByType<Wolf>(FindObjectsSortMode.None).Length;
    }

    int CountBears()
    {
        return FindObjectsByType<Bear>(FindObjectsSortMode.None).Length;
    }

    int CountMoose()
    {
        return FindObjectsByType<Moose>(FindObjectsSortMode.None).Length;
    }
}
