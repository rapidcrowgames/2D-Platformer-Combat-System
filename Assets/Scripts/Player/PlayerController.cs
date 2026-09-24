using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region VARIÁVEIS

    //Componentes do PLAYER
    private Rigidbody2D rb; //Meu RigidBody2D
    [SerializeField] private SpriteRenderer visualRenderer; //Pega a sprite do objeto filho do Player
    [SerializeField] private Animator animator; //Pega o componente Animator do player

    //============================================================================================

    //Variáveis de controle
    private Vector2 moveInput; //Pega o Vector2 para o Input de movimento
    [SerializeField] private float vel; //Minha velocidade de movimento
    private int direction = 1; //Descobre a direção em que o PLAYER está olhando [ Direita == 1 | Esquerda == -1 ]
    [SerializeField] private float jumpForce; //Força do pulo
    private bool isGrounded = false; //Controla se estou no chão ou não
    private bool isFalling = false; //Controla se estou caindo

    //============================================================================================

    //Variáveis do estado de Dash
    private float dashDuration = 1f; //Duração do Dash
    [SerializeField] private float dashSpeed; //Velocidade do Dash
    private float dashTimer = 1f; //Tempo de reload do Dash

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

    //Pulo
    private void OnJump(InputValue value)
    {
        //SE apertar o botão de pulo
        if (value.isPressed && isGrounded)
        {
            //Ele pula
            rb.linearVelocity = new(rb.linearVelocity.x, jumpForce);

            //Vai para o estado de pulo
            playerState = state.jump;

            //Não estou no chão mais
            isGrounded = false;
        }
    }

    //Dash
    private void OnSprint(InputValue value)
    {
        //SE apertar o botão de DASH
        if (value.isPressed)
        {
            //Vou para o estado do Dash
            playerState = state.dash;
        }
    }

    #endregion

    #region MÁQUINA DE ESTADOS e ENUM

    //Enum
    enum state
    {
        idle,
        run,
        jump,
        fall,
        dash

    }

    //Maquina de estados SWITCH

    state playerState = state.idle;
    
    private void StateMachine()
    {
        switch (playerState)
        {
            case state.idle:
                IdleState(); //Vai para o estado de parado
                break;
            case state.run:
                RunState(); //Vai para o estado de movendo
                break;
            case state.jump:
                JumpState(); //Vai para o estado de pulando
                break;
            case state.fall:
                FallState(); //Vai para o estado de caindo / queda livre
                break;
            case state.dash:
                DashState(); //Vai para o estado de esquiva / dash
                break;

        }
    }


    //ESTADOS

    private void IdleState() //Parado
    {
        //Garante que não estou correndo
        animator.SetBool("isRunning", false);

        //SE meu movimento no eixo X for diferente de 0, então vou para o estado de se movendo
        if (moveInput.x != 0)
        {
            //Indica que estou correndo / em movimento
            animator.SetBool("isRunning", true);
            playerState = state.run;
        }
    }

    private void RunState() //Movendo
    {
        //SE meu movimento no eixo X ficar parado, então eu vou para o estado de parado
        if (moveInput.x == 0 && isGrounded)
        {
            //Indica que estou parado
            animator.SetBool("isRunning", false);
            playerState = state.idle;
        }
    }

    private void JumpState() //Pulando
    {
        //Indica que estou pulando
        animator.SetBool("isJumping", true);
    }

    private void FallState() //Caindo
    {
        //Mudo minha animação de queda livre
        animator.SetBool("isFall", true);

        //SE cheguei ao chão, eu não estou mais caindo
        if (isGrounded)
        {
            isFalling = false;
            animator.SetBool("isFall", false);
            animator.SetBool("isJumping", false);
            playerState = state.idle;
        }
    }

    private void DashState() //Dash
    {
        //Pega a direção do player [ Direita ]
        if (direction == 1)
        {

        }

        else if (direction == -1) //[ Esquerda ]
        {

        }
    }

    #endregion

    #region MÉTODOS

    //Verifica se estou em queda livre
    private void IsFalling()
    {
        //SE estou caindo
        if (!isGrounded && rb.linearVelocity.y < 0 && !isFalling)
        {
            //Estou caindo e vou para o estado de queda
            isFalling = true;
            playerState = state.fall;
        }
    }

    #endregion

    #region COLISÕES

    //Collision ENTER
    private void OnCollisionEnter2D(Collision2D other)
    {
        //SE colidir com o chão, indica que estou no chão
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isFalling = false;
        }
    }

    //Collision EXIT
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

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

    //Em intervalos fixos de tempo Executar lógica relacionada à física.
    private void FixedUpdate()
    {
        //Aplicando ao meu Rb (movimento)
        rb.linearVelocity = new Vector2(moveInput.x * vel, rb.linearVelocity.y);
    }

    // Update is called once per frame
    void Update()
    {
        //Verificação de queda livre
        IsFalling();

        //Maquina de estados
        StateMachine();

        #region DEBUGS

        UnityEngine.Debug.Log(playerState);

        #endregion
    }
}
