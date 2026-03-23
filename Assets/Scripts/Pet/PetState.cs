using UnityEngine;

public class PetState : MonoBehaviour
{
    public int affection = 0;
    public int energy = 100;
    public string mood = "Idle";

    [SerializeField] private float energyTickInterval = 5f;
    private float energyTimer = 0f;

    private void Update()
    {
        energyTimer += Time.deltaTime;

        if (energyTimer >= energyTickInterval)
        {
            energyTimer = 0f;
            energy = Mathf.Max(0, energy - 1);
            UpdateMood();
        }
    }

    public void IncreaseAffection(int amount)
    {
        affection += amount;
        UpdateMood();
    }

    public void UpdateMood()
    {
        if (energy <= 20)
        {
            mood = "Sleepy";
        }
        else if (affection >= 10)
        {
            mood = "Happy";
        }
        else
        {
            mood = "Idle";
        }
    }

    public void ApplySaveData(SaveData data)
    {
        if (data == null) return;

        affection = data.affection;
        energy = data.energy;
        mood = data.mood;
    }
}