using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Objects")]
    [SerializeField] private GameObject keyPrefab;

    [Header("Game Settings")]
    [SerializeField] private int keysToWin = 3;
    [SerializeField] private float minKeyDistance = 3f;
    [SerializeField] private float tapRange = 20f;
    [SerializeField] private float keyHoverHeight = 0.2f;

    [Header("Spawning Pacing")]
    [SerializeField] private float firstKeyDelay = 4f;
    [SerializeField] private float timeBetweenKeys = 4f;

    [Header("UI References")]
    [SerializeField] private UIBuilder uiBuilder;

    public int KeysCollected { get; private set; }

    private ARPlaneManager planeManager;
    private List<GameObject> spawnedKeys = new List<GameObject>();
    
    public event System.Action OnGameStarted;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        planeManager = FindFirstObjectByType<ARPlaneManager>();

        if (uiBuilder == null)
            uiBuilder = FindObjectOfType<UIBuilder>();
    }

    void Start()
    {
        if (uiBuilder != null && uiBuilder.StartButton != null)
            uiBuilder.StartButton.onClick.AddListener(() => StartGame());

        UpdateHUD();
    }

    private void Update()
    {
        var touch = Touchscreen.current?.primaryTouch;
        bool touched = touch != null && touch.press.wasPressedThisFrame;
        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (touched || clicked)
        {
            Vector2 screenPos = touched ? touch.position.ReadValue() : Mouse.current.position.ReadValue();
            TryTapCollect(screenPos);
        }
    }

#if UNITY_EDITOR
    private List<Vector3> debugPlaneCenters = new List<Vector3>
    {
        new Vector3(-3f, 0f, -3f),
        new Vector3(3f, 0f, -2f),
        new Vector3(-2f, 0f, 3f),
        new Vector3(2.5f, 0f, 3f)
    };

    private bool TryGetSpawnPoint(out Vector3 point, float minDistFromPlayer)
    {
        foreach (var center in debugPlaneCenters)
        {
            Vector3 candidate = center + new Vector3(
                Random.Range(-1f, 1f) * 1.2f, 0f, Random.Range(-1f, 1f) * 1.2f);
            if (FlatDistance(candidate, Camera.main.transform.position) >= minDistFromPlayer)
            {
                point = candidate;
                return true;
            }
        }
        point = Vector3.zero;
        return false;
    }
#endif

    private void TryTapCollect(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        
        if (Physics.SphereCast(ray, 0.25f, out RaycastHit hit, tapRange))
        {
            Debug.Log($"Tap hit: {hit.collider.name}");
            Collectible key = hit.collider.GetComponentInParent<Collectible>();
            if (key != null)
            {
                key.Collect();
                return;
            }
            
            Debug.Log($"Hit {hit.collider.name} at {hit.point} — no Collectible found");
        }
        else
        {
            Debug.Log($"Tap at screen {screenPos} hit nothing in {tapRange}m");
        }
    }

    public void StartGame()
    {
        if (uiBuilder != null)
            uiBuilder.StartPanel.SetActive(false);
        
        KeysCollected = 0;
        spawnedKeys.Clear();

        StartCoroutine(SpawnKeysSequentially());
        
        OnGameStarted?.Invoke();
        
        UpdateHUD();
    }
    
    private IEnumerator SpawnKeysSequentially()
    {
        yield return new WaitForSeconds(firstKeyDelay);

        for (int i = 0; i < keysToWin; i++)
        {
            SpawnKey();
            if (i < keysToWin - 1)
                yield return new WaitForSeconds(timeBetweenKeys);
        }
    }

    private void SpawnKey()
    {
        Vector3 candidate = GetBestSpawnCandidate();
        if (candidate == Vector3.zero) return;
        
        candidate.y += keyHoverHeight;
        
        GameObject key = Instantiate(keyPrefab, candidate, Quaternion.identity);
        spawnedKeys.Add(key);
    }

    private Vector3 GetBestSpawnCandidate()
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector3 pos = Vector3.zero;
            bool valid = false;

            // 1. Try AR Planes
            var planes = planeManager.trackables;
            foreach (var plane in planes)
            {
                Vector3 c = plane.center + new Vector3(
                    Random.Range(-1f, 1f) * plane.size.x * 0.4f, 0,
                    Random.Range(-1f, 1f) * plane.size.y * 0.4f);

                if (FlatDistance(c, Camera.main.transform.position) >= minKeyDistance)
                {
                    pos = c;
                    valid = true;
                    break;
                }
            }
            
#if UNITY_EDITOR
            if (!valid && TryGetSpawnPoint(out pos, minKeyDistance))
            {
                valid = true;
            }
#endif
            
            if (!valid)
            {
                pos = Camera.main.transform.position + Camera.main.transform.forward * 4f;
                pos += new Vector3(Random.Range(-1f, 1f) * 2f, 0, Random.Range(-1f, 1f) * 2f);
                valid = true;
            }
            
            bool tooClose = false;
            foreach (var existingKey in spawnedKeys)
            {
                if (existingKey != null && FlatDistance(pos, existingKey.transform.position) < 2.0f)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose) return pos;
        }

        return Camera.main.transform.position + Camera.main.transform.forward * 4f;
    }

    public void OnKeyCollected()
    {
        KeysCollected++;
        UpdateHUD();

        if (KeysCollected >= keysToWin)
            HandleWin();
    }

    private void UpdateHUD()
    {
        if (uiBuilder == null) return;

        uiBuilder.CountText.text = $"Keys: {KeysCollected}/{keysToWin}";

        int remaining = keysToWin - KeysCollected;
        uiBuilder.ObjectiveText.text = KeysCollected >= keysToWin
            ? "All keys found!"
            : $"Find {remaining} more key{(remaining == 1 ? "" : "s")}";
    }

    private void HandleWin()
    {
        Time.timeScale = 0f;
        
        var spawner = FindObjectOfType<JumpScareSpawner>();
        if (spawner != null) spawner.StopSpawning();

        if (uiBuilder != null)
            uiBuilder.WinPanel.SetActive(true);
    }

    private float FlatDistance(Vector3 a, Vector3 b)
    {
        Vector3 d = a - b; d.y = 0f; return d.magnitude;
    }
}