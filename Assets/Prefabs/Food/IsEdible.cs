using UnityEngine;

public interface IsEdible
{
    float Consume();
    bool CanBeEatenBy(Species species);
}
