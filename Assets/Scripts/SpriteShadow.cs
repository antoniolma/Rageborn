using UnityEngine;

// Baseado em: https://youtu.be/ft4HUL2bFSQ?si=qsoinhGbMvqAtgpg
public class SpriteShadow : MonoBehaviour
{
    [SerializeField] private Vector2 offset;

    private SpriteRenderer sprRndCaster;
    private SpriteRenderer sprRndShadow;

    private Transform transCaster;  // Objeto que tera a sombra
    private Transform transShadow;

    public Material shadowMaterial;
    public Color shadowColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transCaster = transform;
        transShadow = new GameObject().transform;
        transShadow.parent = transCaster;
        transShadow.gameObject.name = "Shadow";

        transShadow.localScale = new Vector3(1.1f, 0.4f, 1f);

        sprRndCaster = GetComponent<SpriteRenderer>();
        sprRndShadow = transShadow.gameObject.AddComponent<SpriteRenderer>();

        sprRndShadow.material = shadowMaterial;
        sprRndShadow.color = shadowColor;

        sprRndShadow.sprite = sprRndCaster.sprite;
        Debug.Log(sprRndCaster.sprite.name);
        sprRndShadow.flipX = true;

        sprRndShadow.sortingLayerName = sprRndCaster.sortingLayerName;
        sprRndShadow.sortingOrder = sprRndCaster.sortingOrder;
    }

    void LateUpdate()
    {
        transShadow.position = new Vector2(
            transCaster.position.x + offset.x,
            transCaster.position.y + offset.y
        );

        sprRndShadow.sprite = sprRndCaster.sprite;
        sprRndShadow.flipX = sprRndCaster.flipX;
    }
}
