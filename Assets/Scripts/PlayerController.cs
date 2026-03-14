using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;
    [HideInInspector] public bool canMove = true;

    Rigidbody2D rb;
    Vector2 movement;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        movement = Vector2.zero;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y = 1;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y = -1;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement.x = -1;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement.x = 1;
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}