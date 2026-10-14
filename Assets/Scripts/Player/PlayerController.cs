using System.Collections;
using System.Diagnostics;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PlayerController : MonoBehaviour
{
    #region VARIÁVEIS

    [Header("Componentes")]

    //Componentes do PLAYER
    private Rigidbody2D rb; //Meu RigidBody2D
    [SerializeField] private SpriteRenderer visualRenderer; //Pega a sprite do objeto filho do Player
    [SerializeField] private Animator animator; //Pega o componente Animator do player
    [SerializeField] private AudioSource audioSource; //Pega a "caixa de som" do player

    //============================================================================================

    //Verificação do chão
    [SerializeField] private Transform checkPosition;
    [SerializeField] private float checkRadius;
    [SerializeField] private LayerMask groundLayer;


    //============================================================================================

    [Header("GameObjects")]

    //Game Objects
    private EnemyFather enemyComum;
    [SerializeField] private GameObject bloodPart; //Partícula de sangue
    [SerializeField] private GameObject parryPart; //Partícula do parry

    //============================================================================================

    [Header("HUD")]

    //Variáveis de HUD
    [SerializeField] private Image lifeBar; //Imagem da barra de vida
    [SerializeField] private int lifeMax;
    [SerializeField] private GameObject enemyFoundIcon; //Icone de inimigo selecionado para o Gap closer
    private GameObject enemyFoundIconInst; //Instância do ícone


    //============================================================================================

    [Header("Variáveis de controle")]

    //Variáveis de controle
    private int life = 10;
    private Vector2 moveInput; //Pega o Vector2 para o Input de movimento
    private float vel = 0; //Minha velocidade inicial de movimento
    [SerializeField] private float normalVel; //Define minha velocidade normal de movimento
    [SerializeField] private float slowVel; //Deixa minha velocidade de movimento mais lenta para usar em outros estados
    [SerializeField] private float knockbackForce; // A força do knockback
    [SerializeField] private float knockbackVel; //Minha velocidade durante o knockback
    [SerializeField] private float gapVel; //Velocidade de movimento no Gap Closer
    private int direction = 1; //Descobre a direção em que o PLAYER está olhando [ Direita == 1 | Esquerda == -1 ]
    [SerializeField] private float jumpForce; //Força do pulo
    private bool isGrounded = false; //Controla se estou no chão ou não
    private bool isFalling = false; //Controla se estou caindo
    private bool inAnimation = false; //Controla se estou em alguma animação controlada por estado e não Input
    private bool partCreated = false; //Controla se a partícula foi criada
    private bool isTouchWall = false; //Identifica se estou tocando em alguma parede

    //============================================================================================

    [Header("Áudio e sons")]

    //Variáveis de Áudio
    [SerializeField] private AudioClip[] swordSounds; //Sons da espada no vázio
    [SerializeField] private AudioClip[] swordDamage; //Sons de quando o inimigo me acerta com a espada
    [SerializeField] private AudioClip[] hurtSounds; //Sons do gemido de dor
    [SerializeField] private AudioClip deathSound; //Som de morte do player
    [SerializeField] private AudioClip parrySound; //Som do parry
    [SerializeField] private AudioClip parryBlockSound; //Som do ataque após usar o parry
    [SerializeField] private AudioClip dashSound; //Som do Dash
    [SerializeField] private AudioClip lifeSound; //Som do item de vida


    //============================================================================================

    #region VARIÁVEIS DE ESTADOS

    [Header("Variáveis de estados")]

    //Variáveis do LIGHT ATTACK
    private int tapButton = 0; //Quantas vezes o botão de ataque foi pressionado
    private int comboNumber = 0; //Controla em qual golpe do combo estamos [No máximo 2]
    private float comboFirstPunchTimer = 0f; //Timer que pega o tempo da animação do golpe 1 do combo 
    private float firstPunch = 0.4f; //Tempo para colocar timer do primeiro golpe
    private bool inLightAttack = false; //Identifica se estamos no ataque leve ou não
    [SerializeField] private GameObject hitbox; //Pega a Hitbox do player
    [SerializeField] private Transform hitboxPositionDir; //Pega a posição onde a Hitbox será criada [ Lado direito ]
    [SerializeField] private Transform hitboxPositionEsq; //Pega a posição onde a Hitbox será criada [ Lado Esquerdo ]
    private bool hitboxCreated = false; //Identifica se já foi criada a hitbox
    private int lightDamage = 1;

    //============================================================================================

    //Variáveis do estado de HEAVY ATTACK
    private bool inHeavyAttack = false; //Identifica se está no estado de ataque pesado
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

    //==========================================================================================

    //Variáveis Game Feel
    private bool inHitStop = false;
    private CameraController screenshake;


    //==========================================================================================

    //Variáveis do estado de GAP CLOSER
    private bool inGapCloserState = false;
    [SerializeField] private float enemyGapRadius; //Cria o tamanho do raio de detecção
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float gapStopDistance; //Distancia minima para ele parar de perseguir o inimigo e sair do estado
    private Transform enemyTarget; //Guarda quem é o inimigo que foi detectado

    #endregion


    #endregion

    #region INPUTS

    //Movimento
    private void OnMove(InputValue value)
    {
        if (inDieState) return;
        
        moveInput = value.Get<Vector2>();
    }

    //Pulo
    private void OnJump(InputValue value)
    {
        //SE apertar o botão de pulo
        if (value.isPressed && isGrounded && !inLightAttack && !inHeavyAttack && !inParryState && !inParryBlockState && !inDash && !inHurtState && !inDieState)
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
        if (value.isPressed && timerDashDuration >= timerDashDurationFixed && !inLightAttack && !inHeavyAttack && !inParryBlockState && !inParryState && !inHurtState && !inDieState)
        {
            //Estou no Dash
            inDash = true;

            //Animação do Dash
            animator.SetTrigger("isDash");

            //Som
            PlayerDashSound();

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
        if (value.isPressed && isGrounded && !inLightAttack && tapButton <= 0 && !inHeavyAttack && !inDash && !inParryState && !inParryBlockState && !inHurtState && !inDieState)
        {
            inLightAttack = true;

            hitboxCreated = false;

            //Reproduz a animação 1
            animator.SetTrigger("inAttack01");

            //Vai para o estado
            playerState = state.lightAttack;

            tapButton = 1; //Apertei 1x
            comboNumber = 1;
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
        if (value.isPressed && isGrounded && !inHeavyAttack && !inLightAttack && !inDash && !inParryBlockState && !inParryState && !inHurtState && !inDieState)
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
        if (value.isPressed && isGrounded && !isFalling && !inDash && !inHeavyAttack && !inLightAttack && !inDieState && !inHurtState)
        {
            animator.SetTrigger("parry");

            //Som
            PlayerParrySound();

            playerState = state.parry;
        }
    }

    //Gap closer
    private void OnGap(InputValue value)
    {
        //SE apertei a tecla e existe um inimigo selecionado
        if (value.isPressed &&
            enemyTarget != null &&
            !inDieState &&
            !inDash &&
            !inHurtState &&
            !inHeavyAttack &&
            !isFalling)
        {
            animator.SetTrigger("enterGap");

            //Escondo o ícone quando começo o Gap
            if (enemyFoundIconInst != null)
            {
                enemyFoundIconInst.SetActive(false);
            }

            playerState = state.gapCloser;
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
        gapCloser,
        gapCloserAttack,
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
            case state.gapCloser:
                GapCloserState(); //Vair para o estado de Gap 
                break;
            case state.gapCloserAttack:
                GapCloserAttackState(); //Vair para o estado de Ataque após o Gap
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
        if (moveInput.x != 0 && !isTouchWall)
        {
            //Indica que estou correndo / em movimento
            animator.SetBool("isRunning", true);
            playerState = state.run;
        }


        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
        }
    }

    private void RunState() //Movendo
    {
        //SE meu movimento no eixo X ficar parado, então eu vou para o estado de parado
        if (moveInput.x == 0 || isTouchWall)
        {
            //Indica que estou parado
            animator.SetBool("isRunning", false);
            playerState = state.idle;
        }

        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
        }
    }

    private void JumpState() //Pulando
    {

        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
            return;
        }

        //Indica que estou pulando
        animator.SetBool("isJumping", true);



        //SE já toquei no chão novamente
        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            animator.SetBool("isJumping", false);

            isFalling = false;

            playerState = state.idle;

            return;
        }

       
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
        comboFirstPunchTimer = firstPunch;
        tapButton = 0;
        comboNumber = 0;
        vel = normalVel;
        hitboxCreated = false;
        inLightAttack = false;

        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
        }

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


        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
        }
    }

    private void LightAttackState() //At leve
    {

        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
            return;
        }

        //Quando estou no estado de ataque, minha velocidade diminui
        vel = slowVel;

        //SE apertei pela primeira vez & o timer do PRIMEIRO GOLPE está carregado
        if (comboNumber == 1)
        {

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
                    Destroy(hitboxInst, 0.2f);
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
                    Destroy(hitboxInst, 0.2f);
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

            hitboxCreated = false;

            //Reproduz a animação do segundo golpe
            animator.SetTrigger("inAttack02");
        }

        //Pego a animação do segundo golpe
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);

        if (comboNumber >= 2 && animState.IsName("LightAttack02"))
        {
            //Crio a hitbox na minha frente
            if (direction == 1) // [Direita]
            {
                //SE chegou no tempo certo do frame do ataque
                if (animState.normalizedTime >= 0.33f && !hitboxCreated)
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
                if (animState.normalizedTime >= 0.33f && !hitboxCreated)
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
            if (animState.normalizedTime >= 1f)
            {
                //Eu reseto tudo
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
        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
            return;
        }


        //Deixo minha velocidade de movimento mais lenta
        vel = slowVel;


        //Pego minha animação
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);

        //Pego minha direção para criar a hitbox com base no tempo da minha animação
        if (animState.IsName("HeavyAttack"))
        {
            if (direction == 1) // [ DIREITA ]
            {
                if (!hitboxCreated && animState.normalizedTime >= 0.26f)
                {
                    //Crio a hitbox na direita
                    GameObject instHitbox = Instantiate(hitbox, hitboxPositionDir.position * direction, Quaternion.identity);

                    //Criei a hitbox
                    hitboxCreated = true;

                    //Altero o dano da Hitbox
                    PlayerHitboxController hitboxController = instHitbox.GetComponent<PlayerHitboxController>();
                    hitboxController.SetDamage(heavyDamage);

                    //Destruo ela
                    Destroy(instHitbox, 0.2f);
                }

            }
            else if (direction == -1) // [ ESQUERDA ]
            {
                if (!hitboxCreated && animState.normalizedTime >= 0.26f)
                {
                    //Crio a hitbox
                    GameObject instHitbox = Instantiate(hitbox, -hitboxPositionEsq.position * direction, Quaternion.identity);

                    //Criei a hitbox
                    hitboxCreated = true;

                    //Altero o dano da Hitbox
                    PlayerHitboxController hitboxController = instHitbox.GetComponent<PlayerHitboxController>();
                    hitboxController.SetDamage(heavyDamage);

                    //Destruo ela
                    Destroy(instHitbox, 0.2f);
                }
            }




            //SE a animação acabou eu saio do estado
            if (animState.normalizedTime >= 1f)
            {
                vel = normalVel;
                hitboxCreated = false;
                inHeavyAttack = false;
                playerState = state.idle;
            }
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

            //Som da espada me acertando e do gemido de dor
            SwordDamageSound();
            PlayerDamageSound();

            inAnimation = true;
        }

        //SE ainda não criei a partícula do sangue, eu crio
        if (!partCreated)
        {
            GameObject bloodInst = Instantiate(bloodPart, transform.position, Quaternion.identity);

            partCreated = true;

            Destroy(bloodInst, 2f);
        }

        //Screenshake
        screenshake.Shake(0.2f, 0.1f);


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
                inDash = false;
                inHeavyAttack = false;
                inLightAttack = false;
                hitboxCreated = false;
                partCreated = false;
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
                inDash = false;
                inHeavyAttack = false;
                inLightAttack = false;
                hitboxCreated = false;
                partCreated = false;
                playerState = state.die;
            }
        }
        
    }

    private void ParryState() //Prep bloqueio
    {

        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
            return;
        }

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
            partCreated = false;
            playerState = state.idle;
        }
    }

    private void ParryBlockState() //Bloqueio
    {
        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
            return;
        }

        inParryBlockState = true;

        vel = slowVel;

        //Troco a animação
        if (!inAnimation)
        {
            animator.SetTrigger("parryBlock");

            //Som
            PlayerParryBlockSound();

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
                    Destroy(instHitbox, 0.3f);
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
                    Destroy(instHitbox, 0.3f);
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

    private void GapCloserState() //Avanço / Gap
    {

        //SE o inimigo não existe mais, eu saio
        if (!enemyTarget) return;

        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
            return;
        }


        vel = gapVel;
        inGapCloserState = true;


        //Pega a distância do inimigo
        float enemyDistanceX = enemyTarget.position.x - transform.position.x; 


        //Defino a direção com base na distância
        int gapDistance = 0;

        if (enemyDistanceX > 0)
        {
            gapDistance = 1;
        }
        else if (enemyDistanceX < 0)
        {
            gapDistance = -1;
        }


        //Vou em direção a ele enquanto o timer ainda estiver ativo
        if (gapDistance == 1)
        {
            //Vou para direita
            direction = 1;
            rb.linearVelocity = new Vector2(gapDistance * vel, rb.linearVelocity.y * enemyDistanceX); 
        }
        else if (gapDistance == -1)
        {
            //Vou para direita
            direction = -1;
            rb.linearVelocity = new Vector2(gapDistance * vel, rb.linearVelocity.y * enemyDistanceX);
        }


        //Descubro quanto falta para chegar no inimigo
        float distanceToEnemy = Mathf.Abs(enemyTarget.position.x - transform.position.x);

        
        //No fim da distância eu mudo de estado para aplicar o golpe
        if (distanceToEnemy <= gapStopDistance)
        {
            inGapCloserState = false;
            animator.SetTrigger("gapCloserAttack");
            hitboxCreated = false;
            playerState = state.gapCloserAttack;
        }

    }

    private void GapCloserAttackState()
    {

        //Garante que se eu morri, eu vou para estado de morto
        if (life <= 0)
        {
            playerState = state.die;
            return;
        }

        vel = slowVel;

        //Pego minha animação
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);

        //Pego minha direção e animação
        if (animState.IsName("GapCloserAttack"))
        {
            if (direction == 1)
            {
                if (!hitboxCreated && animState.normalizedTime >= 0)
                {
                    //Crio a hitbox na direita
                    GameObject instHitbox = Instantiate(hitbox, hitboxPositionDir.position * direction, Quaternion.identity);

                    hitboxCreated = true;

                    //Pego o dano do hit
                    PlayerHitboxController hitDamage = instHitbox.GetComponent<PlayerHitboxController>();
                    hitDamage.SetDamage(lightDamage);

                    Destroy(instHitbox, 0.2f);
                }
            }
            else if (direction == -1)
            {
                if (!hitboxCreated && animState.normalizedTime >= 0f)
                {
                    //Crio a hitbox na esquerda
                    GameObject instHitbox = Instantiate(hitbox, -hitboxPositionEsq.position * direction, Quaternion.identity);

                    hitboxCreated = true;

                    //Pego o dano do hit
                    PlayerHitboxController hitDamage = instHitbox.GetComponent<PlayerHitboxController>();
                    hitDamage.SetDamage(lightDamage);

                    Destroy(instHitbox, 0.2f);
                }
            }




            //No fim da animação eu saio do estado
            if (animState.normalizedTime >= 1f)
            {
                vel = normalVel;

                ResetVariables();

                playerState = state.idle;
            }
        }
    }

    private void DieState()
    {
        // Só executa UMA VEZ quando entro no estado de morte
        if (!inDieState)
        {
            inDieState = true;

            // Limpo resíduos dos outros estados
            ResetVariables();

            inParryBlockState = false;
            isFalling = false;
            moveInput = Vector2.zero;

            // Limpo animações de movimento
            animator.SetBool("isRunning", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFall", false);

            // Inicio a morte
            animator.SetTrigger("die");

            PlayerDeathSound();
        }

        // Não me movimento mais
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        AnimatorStateInfo animState =
            animator.GetCurrentAnimatorStateInfo(0);

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

    //Atualiza meus movimentos e sprite no FixedUpdate
    private void UpdateMoves()
    {
        if (inDash || inDieState || inHurtState || inParryState || inGapCloserState) return;

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

    //Reseta algumas variáveis quando entra em estados especificos
    private void ResetVariables()
    {
        comboNumber = 0;
        tapButton = 0;
        timerDashDuration = timerDashDurationFixed;
        comboFirstPunchTimer = firstPunch;
        inAnimation = false;
        inDash = false;
        inHeavyAttack = false;
        inLightAttack = false;
        hitboxCreated = false;
        inParryState = false;
        parryWindow = false;
        inGapCloserState = false;
        parryWindowTimer = parryWindowFixedTimer;
    }

    //Verifica se estou em queda livre
    private void IsFalling()
    {
        //SE estou morto, eu saio do método
        if (inDieState || playerState == state.die || life <= 0) return;
        
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
        ResetVariables();

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
    private void Knockback(float attackerX)
    {
        // Descubro para qual lado devo ser empurrado
        float knockbackDirection =
            transform.position.x >= attackerX ? 1f : -1f;

        // Olho para o lado de onde veio o ataque
        direction = knockbackDirection > 0 ? -1 : 1;

        // Aplico o Knockback
        rb.linearVelocity = new Vector2(
            knockbackDirection * knockbackForce,
            rb.linearVelocity.y
        );
    }

    //Timers do parry
    private void ParryTimers()
    {
        if (parryWindow) parryWindowTimer -= Time.deltaTime;
        parryWindowTimer = Mathf.Clamp(parryWindowTimer, 0f, parryWindowFixedTimer);
    }

    //Faz a checagem do chão
    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(checkPosition.position, checkRadius, groundLayer);
    }

    //Verifica se existe algum inimigo disponível para usar o Gap Closer
    private void CheckGapEnemy()
    {
        //Faço a verificação de inimigos próximos
        Collider2D[] enemyDetected =
            Physics2D.OverlapCircleAll(transform.position, enemyGapRadius, enemyLayer);


        //Inimigo escolhido
        EnemyFather closestEnemy = null;

        //Começa com distância infinita
        float closestDistance = Mathf.Infinity;


        //Passo por todos os inimigos encontrados
        foreach (Collider2D enemyCollider in enemyDetected)
        {
            EnemyFather enemy =
                enemyCollider.GetComponentInParent<EnemyFather>();

            //Se não existe ou está morto, ignoro
            if (enemy == null || !enemy.IsAlive())
            {
                continue;
            }


            //Descubro onde ele está em relação ao Player
            float enemyDistanceX =
                enemy.transform.position.x - transform.position.x;


            //Verifico se ele está na minha frente
            if (enemyDistanceX * direction <= 0)
            {
                continue;
            }


            //Pego a distância sem negativo
            float distance = Mathf.Abs(enemyDistanceX);


            //Se esse inimigo for mais próximo que o anterior
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }


        //Se encontrei algum inimigo válido
        if (closestEnemy != null)
        {
            enemyTarget = closestEnemy.transform;


            //Se ainda não criei o ícone, eu crio
            if (enemyFoundIconInst == null)
            {
                enemyFoundIconInst = Instantiate(enemyFoundIcon);
            }


            //Ativo o ícone
            enemyFoundIconInst.SetActive(true);


            //Coloco o ícone acima do inimigo encontrado
            enemyFoundIconInst.transform.position =
                enemyTarget.position + new Vector3(0f, 0f, 0f);
        }

        //Se não existe nenhum inimigo válido
        else
        {
            enemyTarget = null;

            //Escondo o ícone
            if (enemyFoundIconInst != null)
            {
                enemyFoundIconInst.SetActive(false);
            }
        }
    }



    //======================================= SONS =================================

    //Sendo usado nos frames de todas as animações de ATAQUE
    private void SwordAirSound()
    {
        // * Reproduz o som da espada quando ataca o ar / vázio * //

        if (!isGrounded) return;


        //Pego os áudios de forma aleatória
        int randomSound = UnityEngine.Random.Range(0, swordSounds.Length);


        //Reproduz o som
        audioSource.PlayOneShot(swordSounds[randomSound], 0.6f);
    }  


    //Usado em estado
    private void SwordDamageSound()
    {
        // * Reproduz o som de dano no player quando atingido pela espada do inimigo * //

        //Reproduzo o som
        int randomSound = UnityEngine.Random.Range(0, swordDamage.Length);
        audioSource.PlayOneShot(swordDamage[randomSound], 0.7f);
    }


    //Usado em estado
    private void PlayerDamageSound()
    {
        // * Reproduz o som do gemido de dor do player * //


        //Reproduzo o som
        int randomSound = UnityEngine.Random.Range(0, hurtSounds.Length);
        audioSource.PlayOneShot(hurtSounds[randomSound], 0.4f);
    }


    //Usado em estado
    private void PlayerDeathSound()
    {
        // * Reproduz o som de morte * //


        //Reproduz o som
        audioSource.PlayOneShot(deathSound, 0.5f);
    }


    //Usado em Input
    private void PlayerParrySound()
    {
        // * Reproduz o som do parry * //

        //Reproduz o som
        audioSource.PlayOneShot(parrySound, 0.8f);
    }


    //Usado em estado
    private void PlayerParryBlockSound()
    {
        // * Reproduz o som do contra-ataque após acertar o parry * //

        //Reproduz o som
        audioSource.PlayOneShot(parryBlockSound, 0.8f);
    }

    
    //Usado em Input e no frame de animação do Gap Closer
    private void PlayerDashSound()
    {
        // * Reproduz o som do Dash * //


        //Reproduz o som
        audioSource.PlayOneShot(dashSound, 0.3f);
    }


    //Usado em colisões
    private void LifeItemSound()
    {
        // * Reproduz o som do item de vida * //

        audioSource.PlayOneShot(lifeSound, 1f);
    }



    //======================================= GAME FEEL ===========================

    //HITSTOP
    private IEnumerator HitStop(float duration)
    {
        //Entrei no HitStop
        inHitStop = true;

        //Guardo a velocidade original do tempo
        float originalTimeScale = Time.timeScale;

        //Congelo o jogo
        Time.timeScale = 0f;

        //Guardo uma quantidade de segundos reais
        yield return new WaitForSecondsRealtime(duration);


        //Restauro a velocidade original
        Time.timeScale = originalTimeScale;


        //Sai do hitstop
        inHitStop = false;
    }

    public void PlayHitstop(float duration)
    {
        if (inHitStop) return;

        StartCoroutine(HitStop(duration));
    }


    //======================================= HUD =================================

    //Converte a vida do player em barra
    private void lifeBarUpdate() 
    {
        lifeBar.fillAmount = ((float)life / (float)lifeMax);

        //Mudando a cor da vida
        lifeBar.color = new Color32(201, (byte)(lifeBar.fillAmount * 255), 47, 255);
    }


    #endregion

    #region COLISÕES

    //Collision ENTER
    private void OnCollisionEnter2D(Collision2D other)
    {
        //SE colidir com o chão, indica que estou no chão
        if (other.gameObject.CompareTag("Ground"))
        {
            isFalling = false;
        }


        //Se colidir com a parede, eu reseto minhas váriáveis
        if (other.gameObject.CompareTag("Wall"))
        {
            isTouchWall = true;
        }

    }

    //Collision EXIT
    private void OnCollisionExit2D(Collision2D other)
    {

        //Quando saio da parede
        if (other.gameObject.CompareTag("Wall"))
        {
            isTouchWall = false;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {

        #region VIDA

        //Se eu colidi com a vida
        if (other.CompareTag("Life"))
        {
            //Eu ganho vida SE eu não estiver com a vida cheia
            if (life >= lifeMax) return;

            life += 2;

            //Som
            LifeItemSound();

            //Destruo a vida
            Destroy(other.gameObject);
        }

        #endregion

        #region Enemy / Parry
        //SE não é uma hitbox do inimigo, eu saio
        if (!other.CompareTag("EnemiesHitbox")) return;



        //SE morri, eu saio daqui
        if (life <= 0 || playerState == state.die || inDieState) return;


        //SE acertei o parry
        if (parryWindow && playerState == state.parry)
        {

            //SE ainda não criei a partícula, eu crio
            if (!partCreated)
            {
                GameObject parryPartInst = Instantiate(parryPart, transform.position * direction, Quaternion.identity);

                partCreated = true;

                Destroy(parryPartInst, 4f);
            }


            //hitstop
            PlayHitstop(0.3f);
            inHitStop = true;
            //Screenshake
            screenshake.Shake(0.4f, 0.1f);

            parryWindow = false;
            inParryState = false;
            inAnimation = false;
            partCreated = false;
            inHitStop = false;
            parryWindowTimer = parryWindowFixedTimer;
            playerState = state.parryBlock;

            return;
        }

        //SE já estou executando o PARRY BLOCK ou MORTO, eu ignoro
        if (playerState == state.parryBlock || playerState == state.die) return;


        //SE cheguei até aqui, não consegui defender e perdi vida
        EnterHurtState();

        // Passo a posição X da hitbox que realmente me acertou
        Knockback(other.transform.position.x);

        #endregion
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

        //Pega a câmera
        screenshake = Camera.main.GetComponent<CameraController>();

        //Variáveis de timers
        comboFirstPunchTimer = firstPunch;
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
        //Faço a checagem do chão
        GroundCheck();


        //Atualiza minha movimentação e Sprite 
        UpdateMoves();


        //Procura inimigo próximo para o gap closer
        CheckGapEnemy();


        //Aplicando ao meu Rb (movimento) SE não estou em DASH
        if (!inDash && !inHurtState && !inDieState && !inParryState && !inGapCloserState) rb.linearVelocity = new Vector2(moveInput.x * vel, rb.linearVelocity.y);
    }

    // Update is called once per frame
    void Update()
    {
        //Verificação de queda livre
        IsFalling();

        //Maquina de estados
        StateMachine();

        //HUD
        lifeBarUpdate();

        #region SIMPLE TIMERS

        //Timer do combo do LIGHT ATTACK
        AttackComboTimers();

        //Timers do DASH
        DashTimers();

        //Timers do PARRY
        ParryTimers();


        #endregion

        #region DEBUGS

        UnityEngine.Debug.Log(isGrounded);

        #endregion
    }

    //Draw 
    private void OnDrawGizmosSelected()
    {
        //Checagem do GapCloser
        Gizmos.DrawWireSphere(transform.position, enemyGapRadius);

        //Checagem do chão
        if (checkPosition != null)
        {
            Gizmos.DrawWireSphere(checkPosition.position, checkRadius);
        }
    }
}
