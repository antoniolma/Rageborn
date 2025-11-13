using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public float speed;

    // 0 = Fire, 1 = Ice, 2 = Elec
    public int TypeSword;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb.MovePosition(rb.position + speed * movement.normalized * Time.fixedDeltaTime);

        // Verifica se esta correndo
        if (moveVertical != 0 || moveHorizontal != 0)
        {
            _animator.SetBool("isRunning", true);
            _animator.SetFloat("LastInputX", moveHorizontal);
            _animator.SetFloat("LastInputY", moveVertical);
        }
        else _animator.SetBool("isRunning", false);

        _animator.SetFloat("InputX", moveHorizontal);
        _animator.SetFloat("InputY", moveVertical);
    }
}
