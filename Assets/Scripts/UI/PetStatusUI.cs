using TMPro;
using UnityEngine;

public class PetStatusUI : MonoBehaviour
{
    [SerializeField] private PetState petState;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Update()
    {
        if (petState == null || statusText == null) return;

        statusText.text =
            $"Affection: {petState.affection}\n" +
            $"Energy: {petState.energy}\n" +
            $"Mood: {petState.mood}";
    }
}