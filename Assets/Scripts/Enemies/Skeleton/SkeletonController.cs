using UnityEngine;

public class SkeletonController : EnemyFather
{
    #region VARIÁVEIS

    //Estado de IDLE
    private bool inIdleState = false;
    private float idleTimer = 0f; //Tempo no estado
    [SerializeField] private float idleFixedTimer; 

    //Estado de RUN
    private bool inRunState = false;
    private float timerDistance = 0f;
    [SerializeField] private float timerFixedDistance;
    private float timerRunState = 0f;
    [SerializeField] private float timerFixedState;

    //Estado LOAD ATTACK
    private float timerLoadAttack = 0f;
    [SerializeField] private float timerLoadFixedAttack;
    private bool inLoadAttackState = false;

    //Estado ATTACK 1 & 2
    private int comboAttack = 0;
    private bool createHitbox = false;

    #endregion

    #region MÉTODOS

    private void SimpleStateTimers()
    {
        //Timers do estado IDLE
        if (inIdleState) idleTimer -= Time.deltaTime;
        idleTimer = Mathf.Clamp(idleTimer, 0f, idleFixedTimer);



        //Timer do estado RUN
        if (inRunState)
        {
            timerDistance -= Time.deltaTime;
            timerRunState -= Time.deltaTime;

            timerDistance = Mathf.Clamp(timerDistance, 0f, timerFixedDistance);
            timerRunState = Mathf.Clamp(timerRunState, 0f, timerFixedState);
        }



        //Timer do estado LOAD ATTACK
        if (inLoadAttackState) timerLoadAttack -= Time.deltaTime;
        timerLoadAttack = Mathf.Clamp(timerLoadAttack, 0f, timerLoadFixedAttack);


    }

    private void CorrectDirection()
    {
        //Corrigi a sprite de acordo com a direção
        visualRenderer.flipX = direction == -1;

    }

    #endregion

    #region COLISÕES
    #endregion

    #region MÁQUINA DE ESTADOS e ENUM

    //ENUM
    enum state
    {
        idle,
        run,
        chase,
        loadAttack,
        attack,
        attack2,
        hurt,
        die
    }

    //Maquina de estados
    state enemyState = state.idle;

    private void StateMachine()
    {
        switch(enemyState)
        {
            case state.idle:
                StateIdle();
                break;
            case state.run:
                StateRun();
                break;
            case state.chase:
                StateChase();
                break;
            case state.loadAttack:
                StateLoadAttack();
                break;
            case state.attack:
                StateAttack();
                break;
            case state.attack2:
                StateAttack2();
                break;
            case state.hurt:
                StateHurt();
                break;
            case state.die:
                StateDie();
                break;
        }
    }

    //ESTADOS

    private void StateIdle()
    {
        inIdleState = true;
        vel = normalVel;

        //SE ainda não rodou a animação
        if (!inAnimation)
        {
            //Ele roda
            animator.SetBool("isRunning", false);

            inAnimation = true;
        }

        //Zero minha velocidade
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        int chance = UnityEngine.Random.Range(1, 10);

        //SE o timer chegar a 0, eu mudo de estado & tive a chance de mudar
        if (idleTimer <= 0 && chance > 5)
        {
            inAnimation = false;
            inIdleState = false;
            idleTimer = 2f;
            enemyState = state.run;
        }

        else if (idleTimer <= 0 && chance <= 5)
        {
            //Reseto mas sigo no estado
            idleTimer = 2f;
        }

        //SE entrei no raio de detecção, vou para o estado de CHASE
        if (playerDetected && player)
        {
            inAnimation = false;
            inIdleState = false;
            idleTimer = 2f;
            enemyState = state.chase;
        }
    }

    private void StateRun()
    {
        inRunState = true;
        vel = normalVel;

        //SE ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetBool("isRunning", true);

            inAnimation = true;
        }

        //Saio andando para minha direção atual
        rb.linearVelocity = new Vector2(direction * vel, rb.linearVelocity.y);

        //SE o timer de direção chegar a 0, eu inverto minha direção
        if (timerDistance <= 0)
        {
            direction *= -1;

            timerDistance = timerFixedDistance;
        }

        //SE o timer do estado esgotar, eu volto a ficar parado
        if (timerRunState <= 0)
        {
            inRunState = false;
            inAnimation = false;
            animator.SetBool("isRunning", false);
            timerRunState = timerFixedState;
            timerDistance = timerFixedDistance;
            enemyState = state.idle;
        }

        //SE o player entrou na zona de detecção, vou para o estado de CHASE
        if (playerDetected && player)
        {
            inRunState = false;
            inAnimation = false;
            timerRunState = timerFixedState;
            timerDistance = timerFixedDistance;
            enemyState = state.chase;
        }

    }

    private void StateChase()
    {
        vel = chaseVel;

        //SE eu não estiver mais no ZAIO de Detecção, eu saio
        if (!playerDetected)
        {
            inAnimation = false;
            enemyState = state.idle;

            return;
        }

        //SE ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetBool("isRunning", true);

            inAnimation = true;
        }

        //Pego a distância até o player
        float distanceX = player.transform.position.x - transform.position.x;

        //A direção que devo ir
        direction = (int)Mathf.Sign(distanceX);

        //Vou na direção do player
        rb.linearVelocity = new Vector2(direction * vel, rb.linearVelocity.y);

        //SE cheguei próximo demais do player, vou para estado de Load Attack
        if (attackZone)
        {
            inAnimation = false;
            animator.SetBool("isRunning", false);
            enemyState = state.loadAttack;
        }

    }

    private void StateLoadAttack()
    {

        inLoadAttackState = true;

        //Fico parado
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        //SE ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetTrigger("isLoadAttack");

            inAnimation = true;
        }

        //Se o tempo de load zerar, eu troco de estado
        if (timerLoadAttack <= 0)
        {
            inLoadAttackState = false;
            comboAttack = 1;
            inAnimation = false;
            timerLoadAttack = timerLoadFixedAttack;
            enemyState = state.attack;
        }

    }

    private void StateAttack()
    {
        
        vel = slowVel;


        //SE ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetTrigger("attack01");

            inAnimation = true;
        }

        //Pego a animação atual que estou usando
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);

        /* SE estou no primeiro ataque do combo eu crio a hitbox de acordo com minha
         * direção e no tempo correto da animação do golpe 1
        */
        if (comboAttack == 1)
        {

            if (direction == 1) // [ DIREITA ]
            {
                if (!createHitbox && animState.normalizedTime >= 0.6f)
                {
                    GameObject hitboxInst = Instantiate(hitbox, hitboxPosDir.position * direction, Quaternion.identity);

                    createHitbox = true;

                    Destroy(hitboxInst, 0.2f);
                }
            }
            else if (direction == -1) // [ ESQUERDA ]
            {
                if (!createHitbox && animState.normalizedTime >= 0.6f)
                {
                    GameObject hitboxInst = Instantiate(hitbox, -hitboxPosEsq.position * direction, Quaternion.identity);

                    createHitbox = true;

                    Destroy(hitboxInst, 0.2f);
                }
            }




            /* SE o timer da primeira animação chegou ao fim, vou para o próximo combo
             * SE o player ainda estiver dentro da zona de ataque
             * SE ele não estiver eu saio do estado e volto para o CHASE
            */

            if (animState.IsName("Attack01"))
            {
                //Se a animação terminou
                if (animState.normalizedTime >= 1f && attackZone)
                {
                    comboAttack = 2;
                    createHitbox = false;
                    inAnimation = false;
                    enemyState = state.attack2;
                }
                else if (animState.normalizedTime >= 1 && !attackZone)
                {
                    //Saio do estado
                    comboAttack = 0;
                    createHitbox = false;
                    inAnimation = false;
                    enemyState = state.chase;
                }
            }

        }

    }

    private void StateAttack2()
    {
        vel = slowVel;

        //SE ainda não mudei de animação
        if (!inAnimation)
        {
            // Reproduzo a segunda animação
            animator.SetTrigger("attack02");

            inAnimation = true;

            return;
        }


        //Pego minha animação atual
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);

        if (comboAttack >= 2 && animState.IsName("Attack02"))
        {

            if (direction == 1) // [ DIREITA ]
            {
                if (!createHitbox && animState.normalizedTime >= 0.7f)
                {
                    GameObject hitboxInst = Instantiate(hitbox, hitboxPosDir.position * direction, Quaternion.identity);

                    createHitbox = true;

                    Destroy(hitboxInst, 0.5f);
                }
            }
            else if (direction == -1) // [ ESQUERDA ]
            {
                if (!createHitbox && animState.normalizedTime >= 0.7f)
                {
                    GameObject hitboxInst = Instantiate(hitbox, -hitboxPosEsq.position * direction, Quaternion.identity);

                    createHitbox = true;

                    Destroy(hitboxInst, 0.5f);
                }
            }




            //SE minha animação acabou, eu volto para o estado de parado
            if (animState.normalizedTime >= 1f)
            {
                comboAttack = 0;
                inAnimation = false;
                enemyState = state.idle;
                createHitbox = false;
                vel = normalVel;
                    
            }
            

        }
    }

    private void StateHurt()
    {
        //SE ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetTrigger("hurt");

            inAnimation = true;
        }


        //Pego a animação
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);



        //SE a animação chegou ao fim, eu saio do estado
        if (animState.IsName("Hurt"))
        {
            if (animState.normalizedTime >= 1f && life > 0)
            {
                inAnimation = false;
                enemyState = state.idle;
            }

            //SE acabou a vida, ele vai para o estado de DIE
            if (animState.normalizedTime >= 1f && life <= 0)
            {
                inAnimation = false;
                enemyState = state.die;
            }
        }

    }

    private void StateDie()
    {
        //SE ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetTrigger("die");

            inAnimation = true;
        }


        //Pego minha animação
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);


        //Após o fim da animação, eu me destruo
        if (animState.IsName("Die"))
        {
            if (animState.normalizedTime >= 1f)
            {
                Destroy(gameObject, 0.5f);
            }
        }
    }

    #endregion

    //Inicia minhas variáveis e componentes
    private void Awake()
    {
        //Pegando meu RigidBody
        rb = GetComponent<Rigidbody2D>();

        //Define minha velocidade inicial
        vel = slowVel;

        //Pega o player
        player = FindFirstObjectByType<PlayerController>();

        #region TIMERS CONFIG

        idleTimer = idleFixedTimer;
        timerDistance = timerFixedDistance;
        timerRunState = timerFixedState;
        timerLoadAttack = timerLoadFixedAttack;

        #endregion
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    //Atualiza meus movimentos
    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Metodos
        CorrectDirection();

        StateMachine();

        #region TIMERS

        //Timer dos estados
        SimpleStateTimers();

        #endregion

        #region DEBUGS

        UnityEngine.Debug.Log(enemyState);

        #endregion
    }
}
