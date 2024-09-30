using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("PlayerController Start called."); // Debugging the Start method
    }

    void Update()
    {
        // Input handling
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        Debug.Log("PlayerController Update called. Movement: " + movement); // Debugging the Update method
    }

    void FixedUpdate()
    {
        // Movement physics
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        Debug.Log("PlayerController FixedUpdate called. Position: " + rb.position); // Debugging the FixedUpdate method
    }

}
