using TMPro;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    [SerializeField] private TMP_Text collectibleText;
    [SerializeField] private int totalCollectibles = 3;
    [SerializeField] private GameObject jumpScareImage;

    private int collectedCount = 0;

    private void Start()
    {
        UpdateText();

        if (jumpScareImage != null)
        {
            jumpScareImage.SetActive(false);
        }
    }

    public void CollectItem()
    {
        collectedCount++;
        UpdateText();

        if (collectedCount >= totalCollectibles)
        {
            ShowJumpScare();
        }
    }

    private void UpdateText()
    {
        collectibleText.text =
            $"Items found: {collectedCount}/{totalCollectibles}";
    }

    private void ShowJumpScare()
    {
        if (jumpScareImage != null)
        {
            jumpScareImage.SetActive(true);
        }

        Debug.Log("JUMPSCARE!");
    }
}