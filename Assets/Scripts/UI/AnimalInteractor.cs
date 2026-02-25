using UnityEngine;
using UnityEngine.InputSystem;

public class AnimalInteractor : MonoBehaviour
{
    [Header("UI Panel")]
    public InformationUI informationUI;


    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()); // Create a ray from the camera to the mouse position
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Moose")) // Check if the clicked object has the "Moose" tag
                {
                    Debug.Log("Moose clicked!");
                    AnimalNeeds needs = hit.collider.GetComponentInParent<AnimalNeeds>();
                    if (needs != null)
                    {
                        informationUI.ShowInfo(needs); // Show UI Panel with Moose information
                    }
                }
                else
                {
                    informationUI.ClearInfo(); // Clear UI Panel if clicked object is not a Moose
                }

            }

        }
    }


}
