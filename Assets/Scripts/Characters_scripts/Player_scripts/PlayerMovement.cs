using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    // possivel layer mask para detectar paredes (assign no Inspector)
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float overlapCheckRadius = 0.12f; // ajuste conforme collider do player

    // armazenamento do input entre Update() e FixedUpdate()
    private Vector2 rawInput = Vector2.zero;

    // 0 = Fire, 1 = Ice, 2 = Elec
    public int TypeSword;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Player precisa de um Rigidbody2D!");
        }
        else
        {
            // recomendações de runtime se quiser garantir valores
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            // collision detection: importante para movimento rápido
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            // Interpolation opcional: Smooth movement
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (PlayerStats.Instance != null)
        {
            speed = PlayerStats.Instance.GetTotalSpeed();
        }
    }

    void Update()
    {
        // Read input aqui para garantir responsividade
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        rawInput = new Vector2(moveHorizontal, moveVertical);

        // atualiza speed dinamicamente (se PlayerStats mudar)
        if (PlayerStats.Instance != null)
        {
            speed = PlayerStats.Instance.GetTotalSpeed();
        }

        // animação (mantive seu comportamento)
        if (rawInput.x != 0 || rawInput.y != 0)
        {
            _animator.SetBool("isRunning", true);
            _animator.SetFloat("LastInputX", rawInput.x);
            _animator.SetFloat("LastInputY", rawInput.y);
        }
        else _animator.SetBool("isRunning", false);

        _animator.SetFloat("InputX", rawInput.x);
        _animator.SetFloat("InputY", rawInput.y);
    }

    void FixedUpdate()
    {
        // Se não houver input, nada a fazer
        if (rawInput == Vector2.zero) return;

        // normalize mantém velocidade igual em diagonal
        Vector2 movement = rawInput.normalized;

        // MovePosition respeita colisões quando usado corretamente
        Vector2 targetPos = rb.position + movement * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

        // Debug check: estamos "dentro" de uma parede após o movimento?
        // Isso ajuda a detectar tunneling ou gaps no collider do mapa
        Collider2D hit = Physics2D.OverlapCircle(rb.position, overlapCheckRadius, wallLayerMask);
        if (hit != null)
        {
            Debug.LogWarning($"Player está sobrepondo parede: {hit.name} - empurrando para fora.");
            // tenta empurrar o player para fora na direção oposta do movimento
            Vector2 pushDir = (rb.position - (Vector2)hit.ClosestPoint(rb.position)).normalized;
            if (pushDir == Vector2.zero) pushDir = -movement;
            rb.MovePosition(rb.position + pushDir * 0.1f);
        }
    }

    public void UpdateSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

#if UNITY_EDITOR
    // Visual debug no Scene view
    void OnDrawGizmosSelected()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rb.position, overlapCheckRadius);
        }
    }
#endif
}
