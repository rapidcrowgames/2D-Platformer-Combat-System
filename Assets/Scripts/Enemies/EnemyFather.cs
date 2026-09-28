using UnityEngine;

public class EnemyFather : MonoBehaviour
{
    #region VARIÁVEIS

    //Componentes
    protected Rigidbody2D rb; //Variável que cuida do meu RigidBody
    [SerializeField] protected SpriteRenderer visualRenderer; //Pega o sprite renderer
    [SerializeField] protected Animator animator; //Pega o animator

    //Variáveis de controle
    protected float vel = 0f; //Minha velocidade inicial
    protected float normalVel = 3f;
    protected float slowVel = 1.2f;
    protected int direction = 1; // [ 1 == DIREITA ] / [ -1 == ESQUERDA ]
    [SerializeField] protected bool playerDetected; //Detecta se o player entrou na zona de detecção

    #endregion

    #region MÉTODOS

    //Pega a variável para verificar se o player entrou na zona de detecção
    public void SetDetectionZone(bool choice)
    {
        playerDetected = choice;
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
