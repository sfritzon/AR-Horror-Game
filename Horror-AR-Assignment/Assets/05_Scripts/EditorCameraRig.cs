using UnityEngine;
using UnityEngine.InputSystem;

public class EditorCameraRig : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float lookSpeed = 2f;

    private void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null) return;

        Vector3 dir = Vector3.zero;
        if (kb.wKey.isPressed) dir += transform.forward;
        if (kb.sKey.isPressed) dir -= transform.forward;
        if (kb.dKey.isPressed) dir += transform.right;
        if (kb.aKey.isPressed) dir -= transform.right;
        transform.position += dir * (moveSpeed * Time.deltaTime);

        if (mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue() * (lookSpeed * 0.1f);
            Vector3 euler = transform.eulerAngles;
            euler.y += delta.x;
            euler.x -= delta.y;
            euler.x = Mathf.Clamp(euler.x % 360f > 180f
                ? euler.x - 360f : euler.x, -89f, 89f);
            transform.eulerAngles = euler;
        }
    }
#endif
}