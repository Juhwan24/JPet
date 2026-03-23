using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PetState))]
public class PetVisualController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private PetState petState;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        petState = GetComponent<PetState>();
    }

    private void Update()
    {
        switch (petState.mood)
        {
            case "Happy":
                spriteRenderer.color = Color.green;
                break;
            case "Sleepy":
                spriteRenderer.color = Color.blue;
                break;
            default:
                spriteRenderer.color = Color.white;
                break;
        }
    }
}