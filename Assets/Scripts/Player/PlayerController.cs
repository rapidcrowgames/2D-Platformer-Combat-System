using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region VARIÁVEIS

    //Componentes do PLAYER
    private Rigidbody2D rb; //Meu RigidBody2D
    [SerializeField] private SpriteRenderer visualRenderer; //Pega a sprite do objeto filho do Player

    //Variáveis de controle
    private Vector2 moveInput; //Pega o Vector2 para o Input de movimento
    [SerializeField] private float vel; //Minha velocidade de movimento
    private int direction = 1; //Descobre a direção em que o PLAYER está olhando [ Direita == 1 | Esquerda == -1 ]

    #endregion

    #region INPUTS
    
    //Movimento
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        //SE estiver indo para esquerda, eu viro para esquerda a SPRITE
        if (moveInput.x < 0)
        {
            direction = -1;
        }

        else if (moveInput.x > 0) //Se tiver indo para a direita
        {
            direction = 1;
        }

        //Atualizo a posição correta da sprite no eixo X
        visualRenderer.flipX = direction == -1;
    }

    #endregion

    #region MÉTODOS
    #endregion

    //Awake (Inicia componentes e variáveis necessárias antes do start)
    private void Awake()
    {
        //Pegando meu RigidBody2D
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        //Aplicando ao meu Rb (movimento)
        rb.linearVelocity = new Vector2(moveInput.x * vel, rb.linearVelocity.y);
    }

    // Update is called once per frame
    void Update()
    {
        #region DEBUGS
        #endregion
    }
}
