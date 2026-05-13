using UnityEngine;
using UnityEngine.InputSystem;

public class AnimalInteractor : MonoBehaviour
{
    [Header("UI Panel")]
    public InformationUI informationUI;
    public FreeCamera freeCamera;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()); // Create a ray from the camera to the mouse position
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 40f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Moose") || hit.collider.CompareTag("Wolf") || hit.collider.CompareTag("Bear")) // Check if the clicked object has the "Moose" tag
                {
                    Animal animal = hit.collider.GetComponentInParent<Animal>();
                    if (animal != null)
                    {
                        informationUI.ShowInfo(animal); // Show UI Panel with Moose information

                        if (freeCamera != null)
                        {
                            freeCamera.SetSelectedAnimal(animal.transform);
                        }
                    }
                }
                else
                {
                    informationUI.ClearInfo(); // Clear UI Panel if clicked object is not a Moose
                    if(freeCamera != null ) freeCamera.SetSelectedAnimal(null);
                }

            }
            else
            {
                informationUI.ClearInfo();
            }

        }
    }


}
