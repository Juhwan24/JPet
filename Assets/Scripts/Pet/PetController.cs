using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PetState))]
public class PetController : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private PetState petState;

    private Vector3 mouseDownPosition;
    private bool draggedAfterClick = false;

    private void Start()
    {
        mainCamera = Camera.main;
        petState = GetComponent<PetState>();

        LoadPet();

        Debug.Log("PetController Start");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                draggedAfterClick = false;
                mouseDownPosition = Input.mousePosition;
                dragOffset = transform.position - (Vector3)mouseWorldPos;

                Debug.Log("Drag Start");
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 currentMousePosition = Input.mousePosition;

            if (Vector3.Distance(currentMousePosition, mouseDownPosition) > 10f)
            {
                draggedAfterClick = true;
            }

            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = (Vector3)mouseWorldPos + dragOffset;
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            Debug.Log("Drag End");

            if (!draggedAfterClick)
            {
                OnPetClicked();
            }

            SavePet();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SavePet();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPet();
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            SaveManager.DeleteSave();
        }
    }

    private void OnPetClicked()
    {
        petState.IncreaseAffection(1);
        Debug.Log($"Pet Clicked | Affection: {petState.affection}, Mood: {petState.mood}");
        SavePet();
    }

    private void SavePet()
    {
        SaveManager.Save(petState, transform);
    }

    private void LoadPet()
    {
        SaveData data = SaveManager.Load();

        if (data == null) return;

        petState.ApplySaveData(data);
        transform.position = new Vector3(data.posX, data.posY, data.posZ);

        Debug.Log("Pet data applied.");
    }
}