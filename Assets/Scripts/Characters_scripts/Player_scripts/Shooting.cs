using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shooting : MonoBehaviour
{
    // Camera (pode ser atribuída no Inspector; se não, pega Camera.main)
    [SerializeField] private Camera mainCam;

    private Vector3 mousePos;
    [SerializeField] private Transform swordTransform;
    [SerializeField] bool canFire;
    private float fireTime;
    [SerializeField] private float cooldown;

    // Jogador
    private SpriteRenderer playerRnd;

    // Espada do Jogador
    private Transform sword;
    [SerializeField] private PlayerMovement playerMovement;

    // Objects Tiro
    [SerializeField] private GameObject FireBullet;
    [SerializeField] private GameObject IceBullet;
    [SerializeField] private GameObject VenomBullet;

    // Sprites Tiro
    private SpriteRenderer swordRenderer;
    [SerializeField] private Sprite fireSwordSprite;
    [SerializeField] private Sprite iceSwordSprite;
    [SerializeField] private Sprite venomSwordSprite;

    // Config para animacao
    private float swingAngle = 135f;
    [Range(0.01f, 0.9f)] private float forwardFraction = 0.1f; // quanto do cooldown é base para ida (antes do multiplier)
    private float forwardSpeedMultiplier = 4f; // 4x mais rápida que o valor base
    private float forwardEaseExponent = 6f;
    private float backEaseExponent = 2.5f;

    private Coroutine swingCoroutine;

    void Awake()
    {
        // se não atribuída no Inspector, tenta pegar a câmera principal
        if (mainCam == null)
            mainCam = Camera.main;
    }

    void OnEnable()
    {
        // quando uma cena carrega, a câmera pode mudar — reatribui se necessário
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // reatribui Camera.main quando a cena muda (se a câmera atual foi destruída)
        if (mainCam == null)
            mainCam = Camera.main;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Camera (se ainda nulo)
        if (mainCam == null)
            mainCam = Camera.main;

        // Player (pode estar na cena; verifica nulo)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRnd = player.GetComponent<SpriteRenderer>();
            if (playerMovement == null)
                playerMovement = player.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogWarning("[Shooting] Player não encontrado com Tag 'Player' na cena.");
        }

        // Espada (procura child)
        if (swordTransform == null)
        {
            // tenta encontrar um filho chamado "SwordTransform"
            Transform found = transform.Find("SwordTransform");
            if (found != null)
                swordTransform = found;
        }

        sword = swordTransform;
        if (sword != null)
        {
            swordRenderer = sword.GetComponent<SpriteRenderer>();
            if (swordRenderer != null && playerRnd != null)
            {
                swordRenderer.sortingLayerName = playerRnd.sortingLayerName;
                swordRenderer.sortingOrder = playerRnd.sortingOrder;
            }
        }
        else
        {
            Debug.LogWarning("[Shooting] swordTransform não atribuído / não encontrado. Swing e tiros podem não funcionar.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // tenta reatribuir a câmera se ela tiver sido destruída
        if (mainCam == null)
            mainCam = Camera.main;

        // se ainda não há câmera, abranda e evita usar ScreenToWorldPoint
        if (mainCam == null)
        {
            // sem câmera disponível — não processa mira/tiro nesse frame
            return;
        }

        // usa a profundidade da camera para ScreenToWorldPoint (evita z=0 errado)
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Max(0.01f, mainCam.nearClipPlane + 0.01f);
        mousePos = mainCam.ScreenToWorldPoint(mouseScreen);

        // protege transform/position se este objeto tiver sido movido
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        // Verifica qual TypeSword do Player (protege nulo)
        int TypeSword = 0;
        if (playerMovement != null)
            TypeSword = playerMovement.TypeSword;

        // Atirar
        if (Input.GetButton("Fire1") && canFire)
        {
            Shoot(TypeSword);
            SwingAnim();
        }

        if (!canFire && Time.time - fireTime >= GetCurrentCooldown())
        {
            canFire = true;
            fireTime = 0;
        }

        // Atualiza o Sprite da espada (checa nulo)
        UpdateSwordSprite();
    }

    // ============================================================
    // Calcula o cooldown atual baseado no attack speed do PlayerStats
    float GetCurrentCooldown()
    {
        if (PlayerStats.Instance != null)
        {
            // Quanto maior o attackSpeed, menor o cooldown
            float attackSpeed = PlayerStats.Instance.GetTotalAttackSpeed();
            return cooldown / Mathf.Max(0.1f, attackSpeed); // Max evita divisão por zero
        }
        return cooldown;
    }

    // ============================================================
    // Atirar

    void Shoot(int TypeSword)
    {
        if (swordTransform == null)
        {
            Debug.LogWarning("[Shooting] Não é possível atirar: swordTransform é nulo.");
            return;
        }

        canFire = false;
        fireTime = Time.time;

        // Cria um novo "Tiro"
        if (TypeSword == 0)
        {
            // Fire
            if (FireBullet != null)
            {
                GameObject Bullet = Instantiate(FireBullet, swordTransform.position, Quaternion.identity);
                Bullet.transform.rotation = sword.transform.rotation;
                
                // Define o tipo de arma no bullet
                var bulletScript = Bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetWeaponType(WeaponType.Fire);
                }
                
                var sr = Bullet.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = swordRenderer != null ? swordRenderer.sortingLayerName : "Default";
                    sr.sortingOrder = swordRenderer != null ? swordRenderer.sortingOrder + 1 : 0;
                }
                Destroy(Bullet, 3);
            }
        }
        else if (TypeSword == 1)
        {
            // Ice
            if (IceBullet != null)
            {
                GameObject Bullet = Instantiate(IceBullet, swordTransform.position, Quaternion.identity);
                Bullet.transform.rotation = sword.transform.rotation;
                
                // Define o tipo de arma no bullet
                var bulletScript = Bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetWeaponType(WeaponType.Ice);
                }
                
                var sr = Bullet.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = swordRenderer != null ? swordRenderer.sortingLayerName : "Default";
                    sr.sortingOrder = swordRenderer != null ? swordRenderer.sortingOrder + 1 : 0;
                }
                Destroy(Bullet, 3);
            }
        }
        else if (TypeSword == 2)
        {
            // Venom
            if (VenomBullet != null)
            {
                GameObject Bullet = Instantiate(VenomBullet, swordTransform.position, Quaternion.identity);
                Bullet.transform.rotation = sword.transform.rotation;
                
                // Define o tipo de arma no bullet
                var bulletScript = Bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetWeaponType(WeaponType.Venom);
                }
                
                var sr = Bullet.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = swordRenderer != null ? swordRenderer.sortingLayerName : "Default";
                    sr.sortingOrder = swordRenderer != null ? swordRenderer.sortingOrder + 1 : 0;
                }
                Destroy(Bullet, 3);
            }
        }
    }

    // ============================================================
    // Sprite da Espada
    void UpdateSwordSprite()
    {
        if (swordRenderer == null || playerMovement == null) return;

        // Fogo
        if (playerMovement.TypeSword == 0) { swordRenderer.sprite = fireSwordSprite; }
        // Ice
        else if (playerMovement.TypeSword == 1) { swordRenderer.sprite = iceSwordSprite; }
        // Venom
        else if (playerMovement.TypeSword == 2) { swordRenderer.sprite = venomSwordSprite; }
    }

    // ============================================================
    // Animacao

    void SwingAnim()
    {
        if (swordTransform == null) return;
        if (swingCoroutine != null) return;

        // Usa o cooldown atual baseado no attack speed
        float currentCooldown = GetCurrentCooldown();

        // base times
        float baseForwardTime = Mathf.Max(0.01f, currentCooldown * forwardFraction);
        // aplica multiplicador para deixar a ida mais rápida
        float forwardTime = Mathf.Max(0.01f, baseForwardTime / Mathf.Max(0.0001f, forwardSpeedMultiplier));
        float backTime = Mathf.Max(0.01f, currentCooldown - forwardTime);

        float flipSign = 1f;
        if (playerMovement != null)
            flipSign = Mathf.Sign(playerMovement.transform.localScale.x);

        swingCoroutine = StartCoroutine(SwingCoroutine(swingAngle * flipSign, forwardTime, backTime));
    }

    IEnumerator SwingCoroutine(float angleDelta, float forwardTime, float backTime)
    {
        if (swordTransform == null)
        {
            swingCoroutine = null;
            yield break;
        }

        float startZ = NormalizeAngle(swordTransform.localEulerAngles.z);
        float targetZ = startZ - angleDelta;

        // ida — easing com expoente configurável (ease-out com expoente alto => snap)
        float t = 0f;
        while (t < forwardTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / forwardTime);
            float eased = 1f - Mathf.Pow(1f - progress, forwardEaseExponent);
            float z = Mathf.LerpAngle(startZ, targetZ, eased);
            SetLocalEulerZ(swordTransform, z);
            yield return null;
        }
        SetLocalEulerZ(swordTransform, targetZ);

        // volta — easing configurável (mais suave)
        t = 0f;
        while (t < backTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / backTime);
            float eased = 1f - Mathf.Pow(1f - progress, backEaseExponent);
            float z = Mathf.LerpAngle(targetZ, startZ, eased);
            SetLocalEulerZ(swordTransform, z);
            yield return null;
        }
        SetLocalEulerZ(swordTransform, startZ);

        swingCoroutine = null;
    }

    // helper: seta só o eixo Z da localEulerAngles (evita problemas com outras rotações)
    private void SetLocalEulerZ(Transform t, float z)
    {
        Vector3 e = t.localEulerAngles;
        e.z = z;
        t.localEulerAngles = e;
    }

    // normaliza ângulo entre -180 e 180 para evitar saltos estranhos com LerpAngle
    private float NormalizeAngle(float a)
    {
        a = Mathf.Repeat(a + 180f, 360f) - 180f;
        return a;
    }
}