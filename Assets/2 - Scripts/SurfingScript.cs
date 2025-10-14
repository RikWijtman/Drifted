using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SurfingScript : MonoBehaviour
{
    public InventoryUIManager inventoryManager;
    public string surfingLayer = "PlayerSurfing";
    public string landLayer = "PlayerOnLand";
    public Rigidbody2D rb;
    public Tilemap waterTilemap; // Sleep je water tilemap in de Inspector
    public Tilemap landTilemap; // Sleep je land tilemap in de Inspector
    private Animator animator;
    public SpriteRenderer shadowRenderer; // Sleep je shadow SpriteRenderer in de Inspector
    public Sprite shadowSmall;
    public Sprite shadowMedium;

    private bool isSurfing = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        bool canSurf = inventoryManager != null && inventoryManager.hasHorn;

        if (canSurf && Input.GetMouseButtonDown(1))
        {
            if (!isSurfing)
            {
                Vector2 surfPos;
                if (IsNextToWater(out surfPos))
                {
                    isSurfing = true;
                    gameObject.layer = LayerMask.NameToLayer(surfingLayer);
                    transform.position = surfPos;
                    if (animator != null) animator.SetBool("Surfing", true); // Zet animatie aan
                    if (shadowRenderer != null && shadowMedium != null) shadowRenderer.sprite = shadowMedium; // <-- shadow aanpassen
                    Debug.Log("Started Surfing");
                }
                else
                {
                    Debug.Log("Je moet naast water staan om te surfen!");
                }
            }
            else
            {
                Vector2 landPos;
                if (IsNextToLand(out landPos))
                {
                    isSurfing = false;
                    gameObject.layer = LayerMask.NameToLayer(landLayer);
                    transform.position = landPos;
                    if (animator != null) animator.SetBool("Surfing", false); // Zet animatie uit
                    if (shadowRenderer != null && shadowSmall != null) shadowRenderer.sprite = shadowSmall; // <-- shadow aanpassen
                    Debug.Log("Stopped Surfing");
                }
                else
                {
                    Debug.Log("Je moet naast land staan om te stoppen met surfen!");
                }
            }
        }
    }

    // Hulpfunctie: checkt of er water naast de speler is en geeft de positie terug
    private bool IsNextToWater(out Vector2 waterPos)
    {
        float checkRadius = 0.5f;
        LayerMask waterMask = LayerMask.GetMask("Water"); // Zorg dat je water op een aparte layer hebt

        // Check in vier richtingen
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in directions)
        {
            Vector2 checkPos = (Vector2)transform.position + dir * checkRadius;
            Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.2f, waterMask);
            if (hit != null && waterTilemap != null)
            {
                // Vind de tilepositie in de tilemap
                Vector3Int cellPos = waterTilemap.WorldToCell(checkPos);
                Vector3 tileWorldPos = waterTilemap.GetCellCenterWorld(cellPos);

                // Ga een extra stukje richting water (bijv. 0.5f extra)
                Vector2 offset = dir * 0.5f;
                waterPos = new Vector2(tileWorldPos.x, tileWorldPos.y) + offset;
                return true;
            }
        }
        waterPos = Vector2.zero;
        return false;
    }

    private bool IsNextToLand(out Vector2 landPos)
    {
        float checkRadius = 0.8f;
        LayerMask landMask = LayerMask.GetMask("Land"); // Zorg dat je land op een aparte layer hebt

        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in directions)
        {
            Vector2 checkPos = (Vector2)transform.position + dir * checkRadius;
            Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.2f, landMask);
            if (hit != null && landTilemap != null)
            {
                Vector3Int cellPos = landTilemap.WorldToCell(checkPos);
                Vector3 tileWorldPos = landTilemap.GetCellCenterWorld(cellPos);

                // Ga een extra stukje richting land (bijv. 0.5f extra)
                Vector2 offset = dir * 0.5f;
                landPos = new Vector2(tileWorldPos.x, tileWorldPos.y) + offset;
                return true;
            }
        }
        landPos = Vector2.zero;
        return false;
    }
}
