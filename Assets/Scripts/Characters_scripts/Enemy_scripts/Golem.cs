using UnityEngine;

public class Golem : Enemy
{
    [Header("Golem Tank Stats")]
    [SerializeField] private int armor = 5; // Redução de dano
    
    [Header("Golem Slam Attack")]
    [SerializeField] private float slamRadius = 2.5f;
    [SerializeField] private int slamDamage = 30;
    [SerializeField] private float slamCooldown = 4f;
    [SerializeField] private GameObject slamEffectPrefab;
    [SerializeField] private float chargeWindupTime = 1.2f;
    
    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    
    private bool isChargingSlam = false;
    private float slamChargeStartTime;
    private float lastSlamTime = -999f;
    private Color originalColor;
    
    protected override void Start()
    {
        // 🗿 Stats do Golem - TANQUE LENTO
        damage = 25; // Dano MUITO ALTO
        moveSpeed = 1.5f; // MUITO LENTO
        attackRange = 2f;
        attackCooldown = 3f; // Ataque LENTO
        
        base.Start();
        
        // Golem tem MUITA VIDA
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.SetMaxHealth(150); // Vida MUITO ALTA
        }
        
        if (bodyRenderer == null)
        {
            bodyRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (bodyRenderer != null)
        {
            originalColor = bodyRenderer.color;
        }
    }
    
    protected override void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Se está carregando o slam
        if (isChargingSlam)
        {
            HandleSlamCharge();
            return;
        }
        
        base.Update();
        
        // Tenta fazer slam se disponível e próximo
        if (Time.time >= lastSlamTime + slamCooldown && distanceToPlayer <= slamRadius * 1.2f)
        {
            StartSlamAttack();
        }
    }
    
    private void StartSlamAttack()
    {
        isChargingSlam = true;
        slamChargeStartTime = Time.time;
        lastSlamTime = Time.time;
        
        // Para o movimento
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
        
        Debug.Log("⚡ Golem carregando SLAM PODEROSO!");
    }
    
    private void HandleSlamCharge()
    {
        float chargeProgress = (Time.time - slamChargeStartTime) / chargeWindupTime;
        
        // Efeito visual de charging (pulsa vermelho cada vez mais forte)
        if (bodyRenderer != null)
        {
            float pulse = Mathf.PingPong(Time.time * 8f, 1f);
            float intensity = chargeProgress * pulse;
            bodyRenderer.color = Color.Lerp(originalColor, Color.red, intensity);
        }
        
        // Executa o slam
        if (chargeProgress >= 1f)
        {
            ExecuteSlam();
        }
    }
    
    private void ExecuteSlam()
    {
        isChargingSlam = false;
        
        // Volta à cor normal
        if (bodyRenderer != null)
        {
            bodyRenderer.color = originalColor;
        }
        
        // Retoma movimento
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = false;
        }
        
        // Efeito visual DE SLAM GIGANTE
        if (slamEffectPrefab != null)
        {
            GameObject effect = Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // DANO EM ÁREA - MUITO FORTE!
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slamRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController playerController = hit.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(slamDamage);
                    Debug.Log($"💥 Golem SLAM DEVASTADOR! Dano: {slamDamage}");
                }
            }
        }
        
        // SCREEN SHAKE FORTE
        CameraShake cameraShake = Camera.main?.GetComponent<CameraShake>();
        if (cameraShake != null)
        {
            cameraShake.Shake(0.4f, 0.6f); // Shake forte!
        }
        
        // Toca som de ataque
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        Debug.Log("💥 Golem executou SLAM!");
    }
    
    // 🛡️ GOLEM TEM ARMADURA - REDUZ DANO!
    protected override int ModifyIncomingDamage(int damage)
    {
        int reducedDamage = Mathf.Max(1, damage - armor);
        
        if (damage > reducedDamage)
        {
            Debug.Log($"🛡️ Golem bloqueou {damage - reducedDamage} de dano! (Armadura: {armor})");
        }
        
        return reducedDamage;
    }
    
    protected override void AttackPlayer()
    {
        // Golem usa principalmente o slam
        // Mas pode dar soco se muito perto
        base.AttackPlayer();
    }
    
    void OnDrawGizmosSelected()
    {
        // Desenha o raio do slam
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
        
        // Desenha range de inicio do slam
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, slamRadius * 1.2f);
    }
}