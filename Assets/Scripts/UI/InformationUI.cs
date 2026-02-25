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
    public MooseNeeds currentMoose;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentMoose != null)
        {
            hungerSlider.maxValue = currentMoose.maxHunger;
            hungerSlider.value = currentMoose.hungerLevel;
        }
    }


    public void SetMoose(MooseNeeds newMoose)
    {
        currentMoose = newMoose;
    }

    public void SetType(string type)
    {
        animalType.text = type;
    }

    public void ClearInfo()
    {
        currentMoose = null;
        panel.SetActive(false);
    }

    public void ShowInfo(MooseNeeds moose)
    {
        Debug.Log("SHOW INFO");
        currentMoose = moose;
        panel.SetActive(true);
    }


}
