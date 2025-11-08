using UnityEngine;

[DefaultExecutionOrder(100)]
public class CornerProbeMini : MonoBehaviour
{
    [SerializeField] LayerMask solid;
    [SerializeField] float rayLen = 0.2f;

    [SerializeField] Vector2 redOffsetL = new(-0.05f, 0f);
    [SerializeField] Vector2 yellowOffsetL = new(-0.10f, 0f);
    [SerializeField] Vector2 redOffsetR = new( 0.05f, 0f);
    [SerializeField] Vector2 yellowOffsetR = new( 0.10f, 0f);

    [SerializeField] Vector2 displacement = new(0.1f, 0f);
    [SerializeField] bool correctionEnabled = true;
    public  bool CorrectionEnabled { get => correctionEnabled; set => correctionEnabled = value; }
    public  void EnableCorrection()  => correctionEnabled = true;
    public  void DisableCorrection() => correctionEnabled = false;

    Rigidbody2D rb; BoxCollider2D col;
    void Awake(){ rb = GetComponent<Rigidbody2D>(); col = GetComponent<BoxCollider2D>(); }

    void FixedUpdate(){
        if (!col || !rb) return;
        if (!correctionEnabled) return;
        if (rb.linearVelocity.y <= 0f) return; // <-- solo cuando sube

        Bounds b = col.bounds; Vector2 top = new(b.center.x, (float)b.max.y);
        Probe(top + redOffsetL, top + yellowOffsetL,  displacement);   // izquierda → +disp
        Probe(top + redOffsetR, top + yellowOffsetR, -displacement);   // derecha  → -disp
    }

    void Probe(Vector2 redO, Vector2 yellowO, Vector2 disp){
        bool redHit = Physics2D.Raycast(redO, Vector2.up, rayLen, solid);
        bool yelHit = Physics2D.Raycast(yellowO, Vector2.up, rayLen, solid);
        if (yelHit && !redHit){
            rb.MovePosition(rb.position + disp);
        }
    }

    void OnDrawGizmosSelected(){
        var c = GetComponent<BoxCollider2D>(); if (!c) return;
        Bounds b = c.bounds; Vector2 top = new(b.center.x, (float)b.max.y);
        Draw(top + redOffsetL, top + yellowOffsetL);
        Draw(top + redOffsetR, top + yellowOffsetR);
    }
    void Draw(Vector2 r, Vector2 y){ Gizmos.color = Color.red; Gizmos.DrawRay(r, Vector2.up * rayLen);
                                     Gizmos.color = Color.yellow; Gizmos.DrawRay(y, Vector2.up * rayLen); }
}
