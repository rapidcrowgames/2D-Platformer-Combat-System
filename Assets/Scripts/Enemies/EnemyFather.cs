using UnityEngine;

public class EnemyFather : MonoBehaviour
{
    #region VARIÁVEIS

    //Componentes
    protected Rigidbody2D rb; //Variável que cuida do meu RigidBody
    [SerializeField] protected SpriteRenderer visualRenderer; //Pega o sprite renderer
    [SerializeField] protected Animator animator; //Pega o animator
    protected PlayerController player; //Pega o player em cena

    //TRANSFORMS
    [SerializeField] protected Transform hitboxPosEsq; //Posição esquerda para criar a Hitbox
    [SerializeField] protected Transform hitboxPosDir; //Posição direita para criar a Hitbox

    //GAME OBJECTS
    [SerializeField] protected GameObject hitbox; //Hitbox de ataque do inimigo
    [SerializeField] protected GameObject bloodParty; //Particula de sangue
    protected CameraController screenshake; 

    //Variáveis de controle
    [SerializeField] protected int life; 
    protected float vel = 0f; //Minha velocidade inicial
    protected float normalVel = 2f;
    protected float slowVel = 1.2f;
    protected float chaseVel = 3f;
    protected float knockbackVel = 6f;
    [SerializeField] protected int direction = 1; // [ 1 == DIREITA ] / [ -1 == ESQUERDA ]
    [SerializeField] protected bool playerDetected; //Detecta se o player entrou na zona de detecção
    [SerializeField] protected bool attackZone; //Detecta se o player entrou na zona de ataque
    protected bool inAnimation = false; //Identifica se já está rodando a animação do estado
    protected bool alive = true;
    [SerializeField] protected float knockbackForce;
    protected bool bloodPartyCreated = false; //Verifica se criei a particula de sangue

    protected bool hitstopOn = false;


    #endregion

    #region MÉTODOS

    //Pega a variável para verificar se o player entrou na zona de detecção
    public void SetDetectionZone(bool choice)
    {
        playerDetected = choice;
    }

    public void SetAttackZone(bool choice)
    {
        attackZone = choice;
    }

    //Saber se o inimigo está vivo
    public bool IsAlive()
    {
        return life > 0;
    }

    //Contador da vida
    public void LifeEnemyCount(int life)
    {
        this.life -= life;
    }

    protected void Knockback()
    {
        // Descubro para qual lado devo ser empurrado
        float knockbackDirection =
            transform.position.x >= player.transform.position.x ? 1f : -1f;

        // Viro minha sprite para olhar para o atacante
        direction = knockbackDirection > 0 ? -1 : 1;

        // Aplico a velocidade do Knockback
        rb.linearVelocity = new Vector2(
            knockbackDirection * knockbackForce,
            rb.linearVelocity.y
        );
    }

    #endregion

    #region COLISÕES

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
