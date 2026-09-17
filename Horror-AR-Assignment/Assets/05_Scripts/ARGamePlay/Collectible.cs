using UnityEngine;

public class Collectible : MonoBehaviour
{
    public void Collect()
    {
        CollectibleManager manager =
            FindFirstObjectByType<CollectibleManager>();

        if (manager != null)
        {
            manager.CollectItem();
        }

        Destroy(gameObject);
    }
}