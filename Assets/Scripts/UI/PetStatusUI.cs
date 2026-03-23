using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetStatusUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PetState petState;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Transform followTarget;

    [Header("Follow")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private Vector2 screenOffset = new Vector2(90f, 85f);
    [SerializeField] private float followSmoothness = 12f;
    [SerializeField] private bool clampToScreen = true;

    [Header("Style")]
    [SerializeField] private Color backgroundColor = new Color(0.11f, 0.14f, 0.22f, 0.88f);
    [SerializeField] private Color outlineColor = new Color(1f, 1f, 1f, 0.16f);

    private RectTransform statusRect;
    private RectTransform canvasRect;
    private Canvas parentCanvas;
    private Camera uiCamera;
    private Camera worldCamera;
    private Image backgroundImage;
    private RectTransform backgroundRect;
    private Outline outline;

    private void Awake()
    {
        if (statusText == null)
        {
            statusText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (petState == null)
        {
            petState = FindObjectOfType<PetState>();
        }

        if (followTarget == null && petState != null)
        {
            followTarget = petState.transform;
        }

        if (statusText == null)
        {
            enabled = false;
            return;
        }

        statusRect = statusText.rectTransform;
        parentCanvas = statusText.GetComponentInParent<Canvas>();
        canvasRect = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>() : null;
        uiCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? parentCanvas.worldCamera
            : null;
        worldCamera = Camera.main;

        SetupVisuals();
        RefreshStatusText();
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (petState == null || statusText == null)
        {
            return;
        }

        if (followTarget == null)
        {
            followTarget = petState.transform;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        RefreshStatusText();
        UpdateFollowPosition();
    }

    private void SetupVisuals()
    {
        statusText.richText = true;
        statusText.enableWordWrapping = false;
        statusText.alignment = TextAlignmentOptions.MidlineLeft;
        statusText.fontSize = 20f;
        statusText.margin = new Vector4(20f, 18f, 20f, 18f);
        statusText.color = Color.white;

        statusRect.anchorMin = new Vector2(0.5f, 0.5f);
        statusRect.anchorMax = new Vector2(0.5f, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.sizeDelta = new Vector2(280f, 138f);

        EnsureBackground();
        backgroundImage.color = backgroundColor;

        outline = statusText.GetComponent<Outline>();
        if (outline == null)
        {
            outline = statusText.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;
    }

    private void RefreshStatusText()
    {
        string moodColor = GetMoodColorTag();
        string affectionBar = BuildBar(petState.affection, 10, 5);
        string energyBar = BuildBar(petState.energy, 100, 5);

        statusText.text =
            $"<size=120%><b>{petState.mood.ToUpperInvariant()}</b></size>\n" +
            $"<size=82%><color=#8FA1BD>AFFECTION</color></size>  <b>{petState.affection}</b>\n" +
            $"<color=#FF8FB1>{affectionBar}</color>\n" +
            $"<size=82%><color=#8FA1BD>ENERGY</color></size>  <b>{petState.energy}</b>\n" +
            $"<color=#7EE0C2>{energyBar}</color>";
    }

    private void UpdateFollowPosition()
    {
        if (followTarget == null || statusRect == null || worldCamera == null)
        {
            return;
        }

        Vector3 worldPosition = followTarget.position + worldOffset;
        Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);

        bool visible = screenPosition.z > 0f;
        if (statusText.gameObject.activeSelf != visible)
        {
            statusText.gameObject.SetActive(visible);
        }

        if (backgroundImage != null && backgroundImage.gameObject.activeSelf != visible)
        {
            backgroundImage.gameObject.SetActive(visible);
        }

        if (!visible)
        {
            return;
        }

        float horizontalDirection = screenPosition.x < Screen.width * 0.5f ? 1f : -1f;
        Vector2 desiredOffset = new Vector2(screenOffset.x * horizontalDirection, screenOffset.y);

        if (canvasRect != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, uiCamera, out Vector2 canvasPoint);
            Vector2 targetAnchoredPosition = canvasPoint + desiredOffset;

            if (clampToScreen)
            {
                Vector2 halfSize = statusRect.sizeDelta * 0.5f;
                Rect rect = canvasRect.rect;
                targetAnchoredPosition.x = Mathf.Clamp(targetAnchoredPosition.x, rect.xMin + halfSize.x + 16f, rect.xMax - halfSize.x - 16f);
                targetAnchoredPosition.y = Mathf.Clamp(targetAnchoredPosition.y, rect.yMin + halfSize.y + 16f, rect.yMax - halfSize.y - 16f);
            }

            Vector2 smoothedPosition = Vector2.Lerp(
                statusRect.anchoredPosition,
                targetAnchoredPosition,
                Time.deltaTime * followSmoothness);

            statusRect.anchoredPosition = smoothedPosition;

            if (backgroundRect != null)
            {
                backgroundRect.anchoredPosition = smoothedPosition;
            }

            return;
        }

        Vector3 targetScreenPosition = screenPosition + (Vector3)desiredOffset;
        Vector3 smoothedScreenPosition = Vector3.Lerp(statusRect.position, targetScreenPosition, Time.deltaTime * followSmoothness);
        statusRect.position = smoothedScreenPosition;

        if (backgroundRect != null)
        {
            backgroundRect.position = smoothedScreenPosition;
        }
    }

    private void SnapToTarget()
    {
        if (statusRect == null || followTarget == null || worldCamera == null)
        {
            return;
        }

        Vector3 screenPosition = worldCamera.WorldToScreenPoint(followTarget.position + worldOffset);
        Vector2 desiredOffset = new Vector2(screenOffset.x, screenOffset.y);

        if (canvasRect != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, uiCamera, out Vector2 canvasPoint);
            Vector2 anchoredPosition = canvasPoint + desiredOffset;
            statusRect.anchoredPosition = anchoredPosition;

            if (backgroundRect != null)
            {
                backgroundRect.anchoredPosition = anchoredPosition;
            }
        }
        else
        {
            Vector3 targetPosition = screenPosition + (Vector3)desiredOffset;
            statusRect.position = targetPosition;

            if (backgroundRect != null)
            {
                backgroundRect.position = targetPosition;
            }
        }
    }

    private void EnsureBackground()
    {
        Transform existingBackground = statusRect.parent.Find("StatusCardBackground");
        if (existingBackground == null)
        {
            GameObject backgroundObject = new GameObject("StatusCardBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.SetParent(statusRect.parent, false);
            backgroundRect.SetSiblingIndex(statusRect.GetSiblingIndex());
            backgroundRect.anchorMin = statusRect.anchorMin;
            backgroundRect.anchorMax = statusRect.anchorMax;
            backgroundRect.pivot = statusRect.pivot;
            backgroundRect.sizeDelta = statusRect.sizeDelta;
            backgroundRect.anchoredPosition = statusRect.anchoredPosition;

            backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.raycastTarget = false;
        }
        else
        {
            backgroundRect = existingBackground as RectTransform;
            backgroundImage = existingBackground.GetComponent<Image>();
        }
    }

    private static string BuildBar(int value, int maxValue, int segments)
    {
        float normalized = maxValue <= 0 ? 0f : Mathf.Clamp01((float)value / maxValue);
        int filledSegments = Mathf.RoundToInt(normalized * segments);
        return "[" + new string('#', filledSegments) + new string('-', segments - filledSegments) + "]";
    }

    private string GetMoodColorTag()
    {
        return petState.mood switch
        {
            "Happy" => "#FFD166",
            "Sleepy" => "#8EC5FF",
            _ => "#9AE6B4"
        };
    }
}
