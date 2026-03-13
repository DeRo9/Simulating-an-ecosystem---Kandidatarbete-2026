using UnityEngine;
using UnityEngine.EventSystems;



public class Highlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Color startColor;

    private Renderer rend;
    private Material outlineMaterial;

    void Start()
    {
        rend = GetComponent<Renderer>();

        outlineMaterial = rend.materials[1]; // Outline mat is index 1
        outlineMaterial.SetFloat("_outline_scale", 0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null) return;

        if (eventData.pointerEnter.CompareTag("Bear"))
        {
            outlineMaterial.SetFloat("_outline_scale", 2.0f); // Show outline when mouse enters
        } else
        {
            outlineMaterial.SetFloat("_outline_scale", 0.02f); // Show outline when mouse enters
        }

        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outlineMaterial.SetFloat("_outline_scale", 0f); // Dont show outline when mouse exits
    }
}
