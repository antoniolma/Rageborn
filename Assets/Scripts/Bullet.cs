using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Camera mainCam;
    private Vector3 mousePos;
    private Rigidbody2D rb;
    [SerializeField] private float force;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        Vector3 direction = mousePos - transform.position;
        rb.linearVelocity = new Vector2(direction.x, direction.y).normalized * force;
    
        Vector3 rotation = transform.position - mousePos;
        float rot = Mathf.Atan2(rotation.x, rotation.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot + 90);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
