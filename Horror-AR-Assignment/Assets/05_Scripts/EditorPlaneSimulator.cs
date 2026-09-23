using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class EditorPlaneSimulator : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Vector3 planeCenter = Vector3.zero;
    [SerializeField] private Vector2 planeSize = new Vector2(10f, 10f);

    private void Start()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "SimulatedFloor";
        floor.transform.position = planeCenter;
        Destroy(floor.GetComponent<Collider>());
    }
#endif
}