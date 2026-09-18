using UnityEngine.InputSystem;
using TMPro;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    [SerializeField] private TMP_Text collectibleText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private int totalCollectibles = 3;

    [Header("Jumpscare")]
    [SerializeField] private GameObject jumpScarePrefab;
    [SerializeField] private float jumpScareDistance = 0.7f;

    private int collectedCount = 0;

    private void Start()
    {
        UpdateText();
    }

    public void CollectItem()
    {
        if (collectedCount >= totalCollectibles)
            return;

        collectedCount++;
        UpdateText();

        if (collectedCount == totalCollectibles)
        {
            ShowJumpScare();
        }
    }

    private void UpdateText()
    {
        collectibleText.text =
            $"Items found: {collectedCount}/{totalCollectibles}";

        if (collectedCount >= totalCollectibles)
        {
            objectiveText.text = "All objects found!";
        }
        else
        {
            objectiveText.text = "Find 3 objects";
        }
    }

    private void ShowJumpScare()
    {
        Camera arCamera = Camera.main;

        if (arCamera == null || jumpScarePrefab == null)
            return;

        Vector3 spawnPosition =
            arCamera.transform.position +
            arCamera.transform.forward * jumpScareDistance;

        GameObject ghost = Instantiate(
            jumpScarePrefab,
            spawnPosition,
            Quaternion.identity);

        // Make it smaller
        ghost.transform.localScale = Vector3.one * 0.15f;

        // Face the camera
        ghost.transform.LookAt(arCamera.transform);

        // Stop physics from moving it
        Rigidbody rb = ghost.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            CollectItem();
        }
    }
#endif
}