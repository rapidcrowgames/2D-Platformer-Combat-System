using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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
    private float vel = 0; //Minha velocidade inicial de movimento
    [SerializeField] private float normalVel; //Define minha velocidade normal de movimento
    [SerializeField] private float slowVel; //Deixa minha velocidade de movimento mais lenta para usar em outros estados
    private int direction = 1; //Descobre a direção em que o PLAYER está olhando [ Direita == 1 | Esquerda == -1 ]
    [SerializeField] private float jumpForce; //Força do pulo
    private bool isGrounded = false; //Controla se estou no chão ou não
    private bool isFalling = false; //Controla se estou caindo

    //============================================================================================

    //Variáveis do LIGHT ATTACK
    private int tapButton = 0; //Quantas vezes o botão de ataque foi pressionado
    private int comboNumber = 0; //Controla em qual golpe do combo estamos [No máximo 2]
    private float comboFirstPunchTimer = 0f; //Timer que pega o tempo da animação do golpe 1 do combo 
    private float comboSecondPunchTimer = 0f; //Timer que pega o tempo da animação do golpe 2 do combo 
    private float firstPunch = 1.8f; //Tempo para colocar timer do primeiro golpe
    private float secondPunch = 0.5f; //Tempo para colocar timer do segundo golpe
    private bool inLightAttack = false; //Identifica se estamos no ataque leve ou não
    [SerializeField] private GameObject hitbox; //Pega a Hitbox do player
    [SerializeField] private Transform hitboxPositionDir; //Pega a posição onde a Hitbox será criada [ Lado direito ]
    [SerializeField] private Transform hitboxPositionEsq; //Pega a posição onde a Hitbox será criada [ Lado Esquerdo ]
    private bool hitboxCreated = false; //Identifica se já foi criada a hitbox

    //============================================================================================

    //Variáveis do estado de HEAVY ATTACK
    private bool inHeavyAttack = false; //Identifica se está no estado de ataque pesado
    private float heavyAnimTimer = 1.2f; //Pega o tempo da animação do ataque pesado

    //============================================================================================

    //Variáveis do estado de DASH
    private bool inDash = false; //Identifica se estou no estado de DASH
    private float dashVel = 10; //Controla a velocidade do player no DASH
    private float timerDashDuration = 1f; //Duração do DASH


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
        if (value.isPressed && isGrounded && !inLightAttack && !inHeavyAttack)
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
        //Estou no Dash
        inDash = true;

        //Animação do Dash
        animator.SetTrigger("isDash");

        //Entra no estado do Dash
        playerState = state.dash;
    }

    //Ataque leve
    private void OnAttack(InputValue value)
    {
        //SE apertar o botão de ataque & eu ainda não estou no ataque leve & Arrecem vou apertar a primeira vez o botão
        if (value.isPressed && !inLightAttack && tapButton <= 0 && !inHeavyAttack)
        {
            inLightAttack = true;

            //Reproduz a animação 1
            animator.SetTrigger("inAttack01");

            //Vai para o estado
            playerState = state.lightAttack;

            tapButton = 1; //Apertei 1x
        }

        //SE apertei pela SEGUNDA VEZ & já estou no ataque leve
        if (value.isPressed && inLightAttack && comboNumber == 1 && comboFirstPunchTimer <= 1.3f && !inHeavyAttack)
        {
            tapButton = 2; //Apertei 2x
        }
    }

    //Ataque pesado
    private void OnHeavyAttack(InputValue value)
    {
        //SE apertar o botão de Ataque pesado & não estiver no ar & Não estiver no estado de ataque pesado ainda && Não estiver no ataque leve
        if (value.isPressed && isGrounded && !inHeavyAttack && !inLightAttack)
        {
            //Entrei no ataque pesado
            inHeavyAttack = true;

            //Reproduz a animação
            animator.SetTrigger("inHeavyAttack");

            playerState = state.heavyAttack;
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
        dash,
        lightAttack, 
        heavyAttack

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
            case state.lightAttack:
                LightAttackState(); //Vai para o estado de ataque leve
                break;
            case state.heavyAttack:
                HeavyAttackState(); //Vai para o estado de ataque pesado
                break;

        }
    }


    //ESTADOS

    private void IdleState() //Parado
    {
        //Garante que não estou correndo
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isFall", false);

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

        //Quando caio no chão eu reseto o ataque leve e pesado
        comboSecondPunchTimer = secondPunch;
        comboFirstPunchTimer = firstPunch;
        tapButton = 0;
        comboNumber = 0;
        vel = normalVel;
        hitboxCreated = false;
        inLightAttack = false;

    }

    private void DashState() //Dash
    {
        //Altera a velocidade do player para o DASH
        vel = dashVel;

        //Pega a minha direção
        if (direction == 1) // [ DIREITA ]
        {
            //Dou o DASH
            rb.linearVelocity = new Vector2(moveInput.x * vel, rb.linearVelocity.y);
        }
        else if (direction == -1) // [ ESQUERDA ]
        {
            //Dou o DASH
            rb.linearVelocity = new Vector2(-moveInput.x * vel, rb.linearVelocity.y);
        }

        //SE estou no ar
        if (!isGrounded)
        {
            //Desligo a gravidade
            rb.gravityScale = 0f;
        }

        //SE esgotou o timer, eu saio do estado e reseto tudo
        if (timerDashDuration <= 0)
        {
            vel = normalVel;
            inDash = false;
            rb.gravityScale = 1.2f;
            playerState = state.idle;
        }
    }

    private void LightAttackState() //At leve
    {
        //Quando estou no estado de ataque, minha velocidade diminui
        vel = slowVel;

        //SE apertei pela primeira vez & o timer do PRIMEIRO GOLPE está carregado
        if (tapButton == 1)
        {
            //Estou no primeiro golpe do combo
            comboNumber = 1;

            //Crio a hitbox na minha frente
            if (direction == 1) // [Direita]
            {
                //SE chegou no tempo certo do frame do ataque
                if (comboFirstPunchTimer <= 1.3f && !hitboxCreated)
                {
                    GameObject hitboxInst = Instantiate(hitbox, hitboxPositionDir.position * direction, Quaternion.identity);

                    //Criei
                    hitboxCreated = true;

                    //Depois de um tempo ela se destrói
                    Destroy(hitboxInst, 0.5f);
                }
            }
            else if (direction == -1) // [Esquerda]
            {
                //SE chegou no tempo certo do frame do ataque
                if (comboFirstPunchTimer <= 1.3f && !hitboxCreated)
                {
                    GameObject hitboxInst = Instantiate(hitbox, -hitboxPositionEsq.position * direction, Quaternion.identity);

                    //Criei
                    hitboxCreated = true;

                    //Depois de um tempo ela se destrói
                    Destroy(hitboxInst, 0.5f);
                }
            }
        }

        //SE esgotou o tempo do primeiro ataque e eu não apertei para o segundo
        if (comboFirstPunchTimer <= 1 && comboNumber == 1 && tapButton < 2)
        {
            //Eu saio do estado e reseto tudo
            comboFirstPunchTimer = firstPunch;
            tapButton = 0;
            comboNumber = 0;
            hitboxCreated = false;
            inLightAttack = false;
            playerState = state.idle;
            vel = normalVel;
        }


        //=====================================================
        //PARTE 2 (Combo) =====================================
        //=====================================================


        //SE eu apertei o botão pela 2x & já estava no combo 1 && o tempo da animação 1 acabou
        if (comboFirstPunchTimer <= 1 && comboNumber == 1 && tapButton >= 2)
        {
            comboNumber = 2;
            comboSecondPunchTimer = secondPunch;

            hitboxCreated = false;

            //Reproduz a animação do segundo golpe
            animator.SetTrigger("inAttack02");
        }

        if (comboNumber >= 2)
        {
            //Crio a hitbox na minha frente
            if (direction == 1) // [Direita]
            {
                //SE chegou no tempo certo do frame do ataque
                if (comboSecondPunchTimer <= 0.3f && !hitboxCreated)
                {
                    GameObject hitboxInst = Instantiate(hitbox, hitboxPositionDir.position * direction, Quaternion.identity);

                    //Criei
                    hitboxCreated = true;

                    //Depois de um tempo ela se destrói
                    Destroy(hitboxInst, 0.2f);
                }
            }
            else if (direction == -1) // [Esquerda]
            {
                //SE chegou no tempo certo do frame do ataque
                if (comboSecondPunchTimer <= 0.3f && !hitboxCreated)
                {
                    GameObject hitboxInst = Instantiate(hitbox, -hitboxPositionEsq.position * direction, Quaternion.identity);

                    //Criei
                    hitboxCreated = true;

                    //Depois de um tempo ela se destrói
                    Destroy(hitboxInst, 0.2f);
                }
            }


            //SE o tempo da segunda animação acabou
            if (comboSecondPunchTimer <= 0)
            {
                //Eu reseto tudo
                comboSecondPunchTimer = secondPunch;
                comboFirstPunchTimer = firstPunch;
                tapButton = 0;
                comboNumber = 0;
                hitboxCreated = false;
                inLightAttack = false;
                vel = normalVel;
                playerState = state.idle;
            }
        }

        
    }

    private void HeavyAttackState() //At pesado
    {
        //Deixo minha velocidade de movimento mais lenta
        vel = slowVel;

        //Pego minha direção
        if (direction == 1) // [ DIREITA ]
        {
            if (!hitboxCreated && heavyAnimTimer <= 0.7f)
            {
                //Crio a hitbox na direita
                GameObject instHitbox = Instantiate(hitbox, hitboxPositionDir.position * direction, Quaternion.identity);

                //Criei a hitbox
                hitboxCreated = true;

                //Destruo ela
                Destroy(instHitbox, 0.8f);
            }

        }
        else if (direction == -1) // [ ESQUERDA ]
        {
            if (!hitboxCreated && heavyAnimTimer <= 0.7f)
            {
                //Crio a hitbox
                GameObject instHitbox = Instantiate(hitbox, -hitboxPositionEsq.position * direction, Quaternion.identity);

                //Criei a hitbox
                hitboxCreated = true;

                //Destruo ela
                Destroy(instHitbox, 0.8f);
            }
        }

        //Quando a animação acabar, eu saio do estado e reseto tudo
        if (heavyAnimTimer <= 0)
        {
            vel = normalVel;
            hitboxCreated = false;
            inHeavyAttack = false;
            heavyAnimTimer = 1.2f;
            playerState = state.idle;
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

    //Contagem dos timers do combo de ataque leve
    private void AttackComboTimers()
    {
        //SE estou no primeiro ataque do combo, diminuo o timer da primeira animação
        if (comboNumber == 1 && comboFirstPunchTimer > 0) comboFirstPunchTimer -= Time.deltaTime; 

        //SE estou no segundo ataque do combo, diminuo o timer da segunda animação
        if (comboNumber >= 2 && comboSecondPunchTimer > 0) comboSecondPunchTimer -= Time.deltaTime;

        //SE estou no ataque pesado, diminui o timer da animação
        if (inHeavyAttack && heavyAnimTimer > 0) heavyAnimTimer -= Time.deltaTime;
    }

    //Contagem dos timers do DASH
    private void DashTimers()
    {
        //Diminui o timer se estiver no estado de DASH
        if (inDash) timerDashDuration -= Time.deltaTime;

        //Recarrega a duração do Dash & SE não estiver no ar
        if (isGrounded && !inDash)
        {
            timerDashDuration += Time.deltaTime;
        }

        //Limita o timer entre 0 e 2 
        timerDashDuration = Mathf.Clamp(timerDashDuration, 0f, 1f);
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

        //Minha velocidade
        vel = normalVel;

        //Variáveis de timers
        comboFirstPunchTimer = firstPunch;
        comboSecondPunchTimer = secondPunch;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }


    //Em intervalos fixos de tempo Executar lógica relacionada à física.
    private void FixedUpdate()
    {
        //Aplicando ao meu Rb (movimento) SE não estou em DASH
        rb.linearVelocity = new Vector2(moveInput.x * vel, rb.linearVelocity.y);
    }

    // Update is called once per frame
    void Update()
    {
        //Verificação de queda livre
        IsFalling();

        //Maquina de estados
        StateMachine();

        #region SIMPLE TIMERS

        //Timer do combo do LIGHT ATTACK
        AttackComboTimers();

        //Timers do DASH
        DashTimers();

        #endregion

        #region DEBUGS

        UnityEngine.Debug.Log(timerDashDuration);

        #endregion
    }
}
