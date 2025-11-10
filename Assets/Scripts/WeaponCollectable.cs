using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Baseado: https://www.youtube.com/watch?v=s6LxixXc55s
public class WeaponCollectable : MonoBehaviour
{
    [SerializeField] private Sprite fireSwordSprite;
    [SerializeField] private Sprite iceSwordSprite;
    [SerializeField] private Sprite venomSwordSprite;

    private SpriteRenderer swordRenderer;
    [SerializeField] private int TypeSword;

    private void Start()
    {
        swordRenderer = GetComponent<SpriteRenderer>();
        ChangeSprite();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement playerScript = collision.GetComponent<PlayerMovement>();

        // Se encontrou o Player colidindo
        if (playerScript)
        {
            playerScript.TypeSword = TypeSword;
            Destroy(this.gameObject);
        }
    }

    private void ChangeSprite()
    {
        // Fogo
        if (TypeSword == 0) { swordRenderer.sprite = fireSwordSprite; }
        // Ice
        else if (TypeSword == 1) { swordRenderer.sprite = iceSwordSprite; }
        // Venom
        else if (TypeSword == 2) { swordRenderer.sprite = venomSwordSprite; }
    }
}
