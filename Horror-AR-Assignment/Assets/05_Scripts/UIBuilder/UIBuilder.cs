using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIBuilder : MonoBehaviour
{
    [Header("Styling")]
    [SerializeField] private int fontSize = 28;

    public GameObject StartPanel { get; private set; }
    public GameObject WinPanel { get; private set; }
    public TMP_Text CountText { get; private set; }
    public TMP_Text ObjectiveText { get; private set; }
    public Button StartButton { get; private set; }
    public Button RestartButton { get; private set; }

    public Canvas UICanvas { get; private set; }

    private void Awake()
    {
        if (EventSystem.current == null)
        {
            var esGO = new GameObject("EventSystem", 
                typeof(EventSystem), 
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule)); 
            DontDestroyOnLoad(esGO);
        }
        
        var canvasGO = new GameObject("UICanvas");
        UICanvas = canvasGO.AddComponent<Canvas>();
        UICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UICanvas.sortingOrder = 200; 
        UICanvas.overrideSorting = true;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        canvasGO.AddComponent<GraphicRaycaster>();
        
        CountText = CreateText(UICanvas.transform, new Vector2(0, -900), fontSize +10, "Keys: 0/3");
        ObjectiveText = CreateText(UICanvas.transform, new Vector2(0, -840), fontSize + 10, "Find 3 keys");
        
        StartPanel = CreatePanel(UICanvas.transform, "StartPanel", "START GAME");
        StartButton = StartPanel.GetComponentInChildren<Button>(true);

        WinPanel = CreatePanel(UICanvas.transform, "WinPanel", "YOU SURVIVED!");
        RestartButton = WinPanel.GetComponentInChildren<Button>(true);
        WinPanel.SetActive(false);
    }

    private TMP_Text CreateText(Transform parent, Vector2 anchoredPos, int size, string content)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(900, 80);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.text = content;
        text.color = new Color(0.95f, 0.92f, 0.85f);
        return text;
    }

    private GameObject CreatePanel(Transform parent, string name, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var bg = go.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.88f);
        
        var title = CreateText(go.transform, Vector2.zero, 48, label);
        ((RectTransform)title.transform).anchoredPosition = new Vector2(0, 150);
        
        var subtitle = CreateText(go.transform, new Vector2(0, 50), 24, "Find 3 hidden objects and try to survive."); // Objective text
        ((RectTransform)subtitle.transform).anchoredPosition = new Vector2(0, 50);
        
        var btnGo = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(go.transform, false);
        var btnRect = (RectTransform)btnGo.transform;
        btnRect.anchorMin = btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = Vector2.zero;
        btnRect.sizeDelta = new Vector2(420, 120);

        btnGo.GetComponent<Image>().color = new Color(0.42f, 0.29f, 1f);
        var btnText = CreateText(btnGo.transform, Vector2.zero, 34, name == "StartPanel" ? "PLAY" : "RESTART");
        ((RectTransform)btnText.transform).anchoredPosition = Vector2.zero;

        return go;
    }
}