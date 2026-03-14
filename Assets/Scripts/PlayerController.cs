using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float mouseSensitivity = 8f;        // ters mouse hassasiyeti
    public WireMinigame wireMinigame;           // Inspector'dan baðla

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

       
        if (wireMinigame != null && wireMinigame.isActive)
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            float mouseDeltaX = mouse.delta.x.ReadValue();

            movement = Vector2.zero;
            // Saða çekince sola, sola çekince saða
            movement.x = -Mathf.Sign(mouseDeltaX) * (Mathf.Abs(mouseDeltaX) > 0.1f ? 1f : 0f);

            // Y hareketi normal klavye ile devam eder
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y = 1;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y = -1;
            }
        }
        else
        {
            // Normal klavye hareketi
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            movement = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y = 1;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y = -1;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement.x = -1;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement.x = 1;
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
