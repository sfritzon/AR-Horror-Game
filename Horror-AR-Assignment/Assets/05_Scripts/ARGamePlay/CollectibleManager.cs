using UnityEngine.InputSystem;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CollectibleManager : MonoBehaviour
{
    [SerializeField] private TMP_Text collectibleText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private int totalCollectibles = 3;
    [SerializeField] private GameObject startPanel;
    

    [Header("Win UI")]
    [SerializeField] private GameObject winPanel;
    [Header("Jumpscare")]
    [SerializeField] private GameObject jumpScarePrefab;
    [SerializeField] private float jumpScareDistance = 0.8f;
    [SerializeField] private AudioClip jumpScareSound;

    private int collectedCount = 0;

    private void Start()
    {
        UpdateText();

        collectibleText.gameObject.SetActive(false);
        objectiveText.gameObject.SetActive(false);
    }
    public void StartGame()
    {
        startPanel.SetActive(false);

        collectibleText.gameObject.SetActive(true);
        objectiveText.gameObject.SetActive(true);
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
        
        if (jumpScareSound != null)
        {
            AudioSource.PlayClipAtPoint(
                jumpScareSound,
                arCamera.transform.position);
        }

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
        StartCoroutine(MoveJumpscare(ghost, arCamera));
    }
    private IEnumerator MoveJumpscare(GameObject ghost, Camera arCamera)
    
    {
        float duration = 0.4f;
        float timer = 0f;

        Vector3 startPosition = ghost.transform.position;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            Vector3 targetPosition =
                arCamera.transform.position +
                arCamera.transform.forward * 0.2f;

            ghost.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                timer / duration);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        Destroy(ghost);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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