using System.Collections;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    private Camera mainCam;
    private Vector3 mousePos;
    [SerializeField] private Transform swordTransform;
    [SerializeField] bool canFire;
    private float fireTime;
    [SerializeField] private float cooldown;

    // Espada do Jogador
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Camera (para rotacao)
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        // Player
        // GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = GetComponentInParent<PlayerMovement>();

        // Espada
        Transform sword = transform.Find("SwordTransform");
        swordRenderer = sword.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // transform.pos serve como offset, para saber qual longe do centro
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        // Verifica qual TypeSword do Player
        int TypeSword = playerMovement.TypeSword;
        Debug.Log(TypeSword);

        // Atirar
        if (Input.GetButton("Fire1") && canFire)
        {
            Shoot(TypeSword);
            SwingAnim();
        }

        if (!canFire && Time.time - fireTime >= cooldown)
        {
            canFire = true;
            fireTime = 0;
        }

        // Atualiza o Sprite da espada
        UpdateSwordSprite();
    }

    // ============================================================
    // Atirar

    void Shoot(int TypeSword)
    {
        canFire = false;
        fireTime = Time.time;

        // Cria um novo "Tiro"
        if (TypeSword == 0)
        {
            // Fire
            GameObject Bullet = Instantiate(FireBullet, swordTransform.position, Quaternion.identity);
            // Debug.Log("Fire!");
        }
        else if (TypeSword == 1)
        {
            // Ice
            GameObject Bullet = Instantiate(IceBullet, swordTransform.position, Quaternion.identity);
            // Debug.Log("Ice!");
        }
        else if (TypeSword == 2)
        {
            // Venom
            GameObject Bullet = Instantiate(VenomBullet, swordTransform.position, Quaternion.identity);
            // Debug.Log("Venom!");
        }
    }

    // ============================================================
    // Sprite da Espada
    void UpdateSwordSprite()
    {
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

        // base times
        float baseForwardTime = Mathf.Max(0.01f, cooldown * forwardFraction);
        // aplica multiplicador para deixar a ida mais rápida
        float forwardTime = Mathf.Max(0.01f, baseForwardTime / Mathf.Max(0.0001f, forwardSpeedMultiplier));
        float backTime = Mathf.Max(0.01f, cooldown - forwardTime);

        float flipSign = 1f;
        if (playerMovement != null)
            flipSign = Mathf.Sign(playerMovement.transform.localScale.x);

        swingCoroutine = StartCoroutine(SwingCoroutine(swingAngle * flipSign, forwardTime, backTime));
    }

    IEnumerator SwingCoroutine(float angleDelta, float forwardTime, float backTime)
    {
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
