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

    //Game Objects
    private EnemyFather enemyComum;

    //============================================================================================

    //Variáveis de controle
    private int life = 10;
    private Vector2 moveInput; //Pega o Vector2 para o Input de movimento
    private float vel = 0; //Minha velocidade inicial de movimento
    [SerializeField] private float normalVel; //Define minha velocidade normal de movimento
    [SerializeField] private float slowVel; //Deixa minha velocidade de movimento mais lenta para usar em outros estados
    [SerializeField] private float knockbackForce; // A força do knockback
    [SerializeField] private float knockbackVel; //Minha velocidade durante o knockback
    private int direction = 1; //Descobre a direção em que o PLAYER está olhando [ Direita == 1 | Esquerda == -1 ]
    [SerializeField] private float jumpForce; //Força do pulo
    private bool isGrounded = false; //Controla se estou no chão ou não
    private bool isFalling = false; //Controla se estou caindo
    private bool inAnimation = false; //Controla se estou em alguma animação controlada por estado e não Input

    //============================================================================================

    //Variáveis do LIGHT ATTACK
    private int tapButton = 0; //Quantas vezes o botão de ataque foi pressionado
    private int comboNumber = 0; //Controla em qual golpe do combo estamos [No máximo 2]
    private float comboFirstPunchTimer = 0f; //Timer que pega o tempo da animação do golpe 1 do combo 
    private float comboSecondPunchTimer = 0f; //Timer que pega o tempo da animação do golpe 2 do combo 
    private float firstPunch = 0.4f; //Tempo para colocar timer do primeiro golpe
    private float secondPunch = 0.4f; //Tempo para colocar timer do segundo golpe
    private bool inLightAttack = false; //Identifica se estamos no ataque leve ou não
    [SerializeField] private GameObject hitbox; //Pega a Hitbox do player
    [SerializeField] private Transform hitboxPositionDir; //Pega a posição onde a Hitbox será criada [ Lado direito ]
    [SerializeField] private Transform hitboxPositionEsq; //Pega a posição onde a Hitbox será criada [ Lado Esquerdo ]
    private bool hitboxCreated = false; //Identifica se já foi criada a hitbox
    private int lightDamage = 1;

    //============================================================================================

    //Variáveis do estado de HEAVY ATTACK
    private bool inHeavyAttack = false; //Identifica se está no estado de ataque pesado
    private float heavyAnimTimer = 1.2f; //Pega o tempo da animação do ataque pesado
    private int heavyDamage = 3;

    //============================================================================================

    //Variáveis do estado de DASH
    private bool inDash = false; //Identifica se estou no estado de DASH
    [SerializeField] private float dashVel; //Controla a velocidade do player no DASH
    [SerializeField] private float timerDashDuration; //Duração do DASH
    private float timerDashDurationFixed = 0; //Começa em 0, mas ele depois pega o tempo do timer do DASH e armazena

    //===========================================================================================

    //Variáveis do estado HURT
    private bool inHurtState = false;

    //==========================================================================================

    //Variáveis do estado de DEATH
    private bool inDieState = false;

    //==========================================================================================

    //Variáveis do estado de PARRY (prep bloqueio)
    private bool inParryState = false;
    private bool parryWindow = false;
    private float parryWindowTimer = 0f;
    [SerializeField] private float parryWindowFixedTimer;

    //==========================================================================================

    //Variáveis do estado de PARRY BLOCK
    private bool inParryBlockState = false;


    #endregion

    #region INPUTS

    //Movimento
    private void OnMove(InputValue value)
    {
       
        if (inDash || inDieState || inHurtState || inParryState) return;

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
        if (value.isPressed && isGrounded && !inLightAttack && !inHeavyAttack && !inParryState && !inParryBlockState && !inDash)
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
        //SE apertei o botão e o timer de duração está cheio & Não estou atacando
        if (value.isPressed && timerDashDuration >= timerDashDurationFixed && !inLightAttack && !inHeavyAttack && !inParryBlockState && !inParryState)
        {
            //Estou no Dash
            inDash = true;

            //Animação do Dash
            animator.SetTrigger("isDash");

            //Altera a velocidade do player para o DASH
            vel = dashVel;

            //Entra no estado do Dash
            playerState = state.dash;
        }
    }

    //Ataque leve
    private void OnAttack(InputValue value)
    {
        //SE apertar o botão de ataque & eu ainda não estou no ataque leve & Arrecem vou apertar a primeira vez o botão
        if (value.isPressed && !inLightAttack && tapButton <= 0 && !inHeavyAttack && !inDash && !inParryState && !inParryBlockState)
        {
            inLightAttack = true;

            //Reproduz a animação 1
            animator.SetTrigger("inAttack01");

            //Vai para o estado
            playerState = state.lightAttack;

            tapButton = 1; //Apertei 1x
        }

        //SE apertei pela SEGUNDA VEZ & já estou no ataque leve
        if (value.isPressed && inLightAttack && comboNumber == 1 && comboFirstPunchTimer <= 0.3f && !inHeavyAttack && !inDash)
        {
            tapButton = 2; //Apertei 2x
        }
    }

    //Ataque pesado
    private void OnHeavyAttack(InputValue value)
    {
        //SE apertar o botão de Ataque pesado & não estiver no ar & Não estiver no estado de ataque pesado ainda && Não estiver no ataque leve
        if (value.isPressed && isGrounded && !inHeavyAttack && !inLightAttack && !inDash && !inParryBlockState && !inParryState)
        {
            //Entrei no ataque pesado
            inHeavyAttack = true;

            //Reproduz a animação
            animator.SetTrigger("inHeavyAttack");

            playerState = state.heavyAttack;
        }
    }

    //Parry
    private void OnParry(InputValue value)
    {
        //SE eu apertar o botão & Não estiver caindo, correndo, pulando, atacando
        if (value.isPressed && isGrounded && !isFalling && !inDash && !inHeavyAttack && !inLightAttack && !inDieState)
        {
            animator.SetTrigger("parry");
            playerState = state.parry;
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
        heavyAttack,
        hurt,
        parry,
        parryBlock,
        die

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
            case state.hurt:
                HurtState(); //Vair para o estado de machucado
                break;
            case state.parry:
                ParryState(); //Vair para o estado de prep bloqueio
                break;
            case state.parryBlock:
                ParryBlockState(); //Vair para o estado de bloqueio
                break;
            case state.die:
                DieState(); //Vai para o estado de morto
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
        //Pega a minha direção e aplica o DASH
        rb.linearVelocity = new Vector2(direction * vel, 0f);

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
                if (comboFirstPunchTimer <= 0.1f && !hitboxCreated)
                {
                    GameObject hitboxInst = Instantiate(hitbox, hitboxPositionDir.position * direction, Quaternion.identity);

                    //Criei
                    hitboxCreated = true;

                    //Altero o dano da Hitbox
                    PlayerHitboxController hitboxController = hitboxInst.GetComponent<PlayerHitboxController>();
                    hitboxController.SetDamage(lightDamage);

                    //Depois de um tempo ela se destrói
                    Destroy(hitboxInst, 0.5f);
                }
            }
            else if (direction == -1) // [Esquerda]
            {
                //SE chegou no tempo certo do frame do ataque
                if (comboFirstPunchTimer <= 0.1f && !hitboxCreated)
                {
                    GameObject hitboxInst = Instantiate(hitbox, -hitboxPositionEsq.position * direction, Quaternion.identity);

                    //Criei
                    hitboxCreated = true;

                    //Altero o dano da Hitbox
                    PlayerHitboxController hitboxController = hitboxInst.GetComponent<PlayerHitboxController>();
                    hitboxController.SetDamage(lightDamage);

                    //Depois de um tempo ela se destrói
                    Destroy(hitboxInst, 0.5f);
                }
            }
        }

        //SE esgotou o tempo do primeiro ataque e eu não apertei para o segundo
        if (comboFirstPunchTimer <= 0f && comboNumber == 1 && tapButton < 2)
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
        if (comboFirstPunchTimer <= 0f && comboNumber == 1 && tapButton >= 2)
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

                    //Altero o dano da Hitbox
                    PlayerHitboxController hitboxController = hitboxInst.GetComponent<PlayerHitboxController>();
                    hitboxController.SetDamage(lightDamage);

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

                    //Altero o dano da Hitbox
                    PlayerHitboxController hitboxController = hitboxInst.GetComponent<PlayerHitboxController>();
                    hitboxController.SetDamage(lightDamage);

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

                //Altero o dano da Hitbox
                PlayerHitboxController hitboxController = instHitbox.GetComponent<PlayerHitboxController>();
                hitboxController.SetDamage(heavyDamage);

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

                //Altero o dano da Hitbox
                PlayerHitboxController hitboxController = instHitbox.GetComponent<PlayerHitboxController>();
                hitboxController.SetDamage(heavyDamage);

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

    private void HurtState() //Machucado / sofrendo dano
    {
        inHurtState = true;
        vel = knockbackVel;
        
        //SE ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetTrigger("hurt");

            inAnimation = true;
        }

        //Pego minha animação
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);


        //SE no fim da minha animação de dano, eu saio do estado
        if (animState.IsName("Hurt"))
        {
            if (animState.normalizedTime >= 1f && life > 0)
            {
                inAnimation = false;
                inHurtState = false;
                comboNumber = 0;
                tapButton = 0;
                timerDashDuration = timerDashDurationFixed;
                comboFirstPunchTimer = firstPunch;
                comboSecondPunchTimer = secondPunch;
                inDash = false;
                inHeavyAttack = false;
                inLightAttack = false;
                hitboxCreated = false;
                heavyAnimTimer = 1.2f;
                vel = normalVel;

                playerState = state.idle;
            }

            //SE eu morrer
            if (animState.normalizedTime >= 1f && life <= 0)
            {
                inAnimation = false;
                inHurtState = false;
                comboNumber = 0;
                tapButton = 0;
                timerDashDuration = timerDashDurationFixed;
                comboFirstPunchTimer = firstPunch;
                comboSecondPunchTimer = secondPunch;
                inDash = false;
                inHeavyAttack = false;
                inLightAttack = false;
                hitboxCreated = false;
                heavyAnimTimer = 1.2f;

                playerState = state.die;
            }
        }
        
    }

    private void ParryState() //Prep bloqueio
    {

        inParryState = true;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);


        //Pego minha animação
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);


        //Abro a janela na animação
        if (animState.IsName("Parry"))
        {
            if (animState.normalizedTime >= 0.42857f)
            {
                parryWindow = true;
            }



            /* SE dentro da janela do parry, eu colidir com a hitbox do
             * inimigo eu vou para o estado de ParryBlock (Lógica está nas colisões)
             */
        }


        //SE esgotar o timer da janela do parry, eu saio do estado
        if (parryWindowTimer <= 0f)
        {
            parryWindow = false;
            inAnimation = false;
            parryWindowTimer = parryWindowFixedTimer;
            inParryState = false;
            playerState = state.idle;
        }
    }

    private void ParryBlockState() //Bloqueio
    {
        inParryBlockState = true;

        vel = slowVel;

        //Troco a animação
        if (!inAnimation)
        {
            animator.SetTrigger("parryBlock");

            inAnimation = true;
        }

        //Pego minha animação
        AnimatorStateInfo animatorState = animator.GetCurrentAnimatorStateInfo(0);


        //Minha direção
        if (animatorState.IsName("ParryBlock"))
        {

            if (direction == 1) // [ DIREITA ]
            {
                if (!hitboxCreated && animatorState.normalizedTime >= 0f)
                {
                    //Crio a hitbox na direita
                    GameObject instHitbox = Instantiate(hitbox, hitboxPositionDir.position * direction, Quaternion.identity);

                    //Criei a hitbox
                    hitboxCreated = true;

                    //Altero o dano da Hitbox
                    PlayerHitboxController hitboxController = instHitbox.GetComponent<PlayerHitboxController>();
                    hitboxController.SetDamage(heavyDamage);

                    //Destruo ela
                    Destroy(instHitbox, 0.8f);
                }

            }
            else if (direction == -1) // [ ESQUERDA ]
            {
                if (!hitboxCreated && animatorState.normalizedTime >= 0)
                {
                    //Crio a hitbox
                    GameObject instHitbox = Instantiate(hitbox, -hitboxPositionEsq.position * direction, Quaternion.identity);

                    //Criei a hitbox
                    hitboxCreated = true;

                    //Altero o dano da Hitbox
                    PlayerHitboxController hitboxController = instHitbox.GetComponent<PlayerHitboxController>();
                    hitboxController.SetDamage(heavyDamage);

                    //Destruo ela
                    Destroy(instHitbox, 0.8f);
                }
            }

        }

        if (animatorState.IsName("ParryBlock"))
        {
            //No fim da animação, volto para o estado normal
            if (animatorState.normalizedTime >= 1f)
            {
                vel = normalVel;
                inParryBlockState = false;
                inAnimation = false;
                hitboxCreated = false;
                playerState = state.idle;
            }
        }



    }

    private void DieState() //Morto
    {
        inDieState = true;

        //SE eu ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetTrigger("die");

            inAnimation = true;
        }


        //Pego minha animação
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);


        //SE minha animação chegou ao fim eu me destruo
        if (animState.IsName("Die"))
        {
            if (animState.normalizedTime >= 1f)
            {
                Destroy(gameObject, 1f);
            }
        }
    }



    #endregion

    #region MÉTODOS

    //Verifica se estou em queda livre
    private void IsFalling()
    {
        //SE estou caindo
        if (!isGrounded && rb.linearVelocity.y < 0 && !isFalling && !inDash)
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
        timerDashDuration = Mathf.Clamp(timerDashDuration, 0f, timerDashDurationFixed);
    }

    //Contador de dano
    public void LifeCount(int lifeValue)
    {
        //Perco vida
        life -= lifeValue;
    }

    //Pega a variável de vida do player
    public bool PlayerIsAlive()
    {
        return life > 0;
    }

    //Pega a variável se estou na janela do parry
    public bool InParryWindow()
    {
        return (parryWindow && playerState == state.parry || playerState == state.parryBlock);
    }

    //Prepara para entrar no estado de Hurt
    public void EnterHurtState()
    {
        //Limpo as informações
        comboNumber = 0;
        tapButton = 0;
        timerDashDuration = timerDashDurationFixed;
        comboFirstPunchTimer = firstPunch;
        comboSecondPunchTimer = secondPunch;
        inAnimation = false;
        inDash = false;
        inHeavyAttack = false;
        inLightAttack = false;
        hitboxCreated = false;
        inParryState = false;
        parryWindow = false;
        parryWindowTimer = parryWindowFixedTimer;
        heavyAnimTimer = 1.2f;

        //Limpo as animações
        animator.SetBool("isRunning", false);
        animator.SetBool("isFall", false);
        animator.SetBool("isJumping", false);

        //Fico parado
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        //Vou para o estado de Hurt
        playerState = state.hurt;
    }

    //Knockback
    private void Knockback()
    {
        //Descubro para qual lado devo ser empurrado
        float knockbackDirection = transform.position.x >= enemyComum.transform.position.x ? 1f : -1f;

        //Viro minha sprite para o inimigo
        direction = knockbackDirection > 0 ? -1 : 1;


        //Aplico no meu RB
        rb.linearVelocity = new Vector2(knockbackDirection * knockbackForce, rb.linearVelocity.y);
    }

    //Timers do parry
    private void ParryTimers()
    {
        if (parryWindow) parryWindowTimer -= Time.deltaTime;
        parryWindowTimer = Mathf.Clamp(parryWindowTimer, 0f, parryWindowFixedTimer);
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


    private void OnTriggerEnter2D(Collider2D other)
    {
        //SE não é uma hitbox do inimigo, eu saio
        if (!other.CompareTag("EnemiesHitbox")) return;


        //SE acertei o parry
        if (parryWindow && playerState == state.parry)
        {
            parryWindow = false;
            inParryState = false;
            inAnimation = false;
            parryWindowTimer = parryWindowFixedTimer;
            playerState = state.parryBlock;

            return;
        }

        //SE já estou executando o PARRY BLOCK ou MORTO, eu ignoro
        if (playerState == state.parryBlock || playerState == state.die) return;


        //SE cheguei até aqui, não consegui defender e perdi vida
        EnterHurtState();
        Knockback();
    }

    #endregion

    //Awake (Inicia componentes e variáveis necessárias antes do start)
    private void Awake()
    {
        //Pegando meu RigidBody2D
        rb = GetComponent<Rigidbody2D>();

        //Minha velocidade
        vel = normalVel;

        //Pego o inimigo comum
        enemyComum = FindFirstObjectByType<EnemyFather>();

        //Variáveis de timers
        comboFirstPunchTimer = firstPunch;
        comboSecondPunchTimer = secondPunch;
        timerDashDurationFixed = timerDashDuration;
        parryWindowTimer = parryWindowFixedTimer;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }


    //Em intervalos fixos de tempo Executar lógica relacionada à física.
    private void FixedUpdate()
    {
        //Aplicando ao meu Rb (movimento) SE não estou em DASH
        if (!inDash && !inHurtState && !inDieState && !inParryState) rb.linearVelocity = new Vector2(moveInput.x * vel, rb.linearVelocity.y);
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

        //Timers do PARRY
        ParryTimers();

        #endregion

        #region DEBUGS

        #endregion
    }
}
