using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class AnimalHearing : MonoBehaviour
{
    Animal animal;
    protected List <Animal> animalsInRange = new List<Animal>();

    public Animal HeardAnimal {get;protected set;}
    public bool HeardSomething => HeardAnimal != null;
    [SerializeField] float minHearingSpeed = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
      animal = GetComponent <Animal>();
      var sc = GetComponent<SphereCollider>();
      sc.isTrigger = true;
      if (animal != null) sc.radius = animal.hearingRange;   
    }

    void OnValidate(){
        animal = GetComponent <Animal>();
    }
    //checks if an animal has entered the range
    void OnTriggerEnter(Collider other){ 
        var a = other.GetComponentInParent<Animal>();
        if (a == null) return;
        if (a == animal) return;

        if (!animalsInRange.Contains(a)){
            animalsInRange.Add(a);
        }
    }
    // check if the animal has left the range
    void OnTriggerExit(Collider other){
        var a = other.GetComponentInParent<Animal>();
        if (a == null) return;
        animalsInRange.Remove(a);

    }
    // Update is called once per frame
    void Update()
    {
        DetectMovement();
    }
    protected virtual void DetectMovement() //Checks which animal it heard and which one is the closest
    {
        HeardAnimal = null;
        float dist = Mathf.Infinity;
        for (int i = animalsInRange.Count - 1; i >= 0 ; i --){
            var a = animalsInRange [i];
            if (a == null){animalsInRange.RemoveAt(i);continue;}
            if (!a.isMoving) continue;
            if(a.currentSpeed < minHearingSpeed) continue;

            float d = Vector3.Distance(transform.position,a.transform.position);
            if(d < dist){
                dist = d;
                HeardAnimal = a;
            }
        }
    }
}
