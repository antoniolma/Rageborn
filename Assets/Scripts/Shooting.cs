using UnityEngine;

public class Shooting : MonoBehaviour
{
    private Camera mainCam;
    private Vector3 mousePos;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform swordTransform;
    [SerializeField] bool canFire;
    private float fireTime;
    [SerializeField] private float cooldown;
    [SerializeField] private Animator _animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // transform.pos serve como offset, para saber qual longe do centro
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        // Atirar
        if (Input.GetButton("Fire1") && canFire)
        {
            Shoot();
            SwingAnim();
        }

        if (!canFire && Time.time - fireTime >= cooldown)
        {
            canFire = true;
            fireTime = 0;
        }
    }

    void SwingAnim()
    {
        ;
    }
    
    void Shoot()
    {
        canFire = false;
        fireTime = Time.time;

        // Cria um novo "Tiro"
        GameObject Bullet = Instantiate(bullet, swordTransform.position, Quaternion.identity);
    }
}
