using UnityEngine;

public class Collectible : MonoBehaviour
{
    public void Collect()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnKeyCollected();
        Destroy(gameObject);
    }
}