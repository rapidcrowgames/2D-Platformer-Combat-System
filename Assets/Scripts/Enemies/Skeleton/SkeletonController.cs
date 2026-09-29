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

    //Estado de CHASE
    [SerializeField] private Transform playerTrans;

    //Estado LOAD ATTACK
    private float timerLoadAttack = 0f;
    [SerializeField] private float timerLoadFixedAttack;
    private bool inLoadAttackState = false;

    //Estado ATTACK
    private bool inAttackState = false;
    private int comboAttack = 0;
    private float firstAttackTimer = 0f;
    private float secondAttackTimer = 0f;
    [SerializeField] private float firstAttackFixedTimer;
    [SerializeField] private float secondAttackFixedTimer;
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
        float distanceX = playerTrans.position.x - transform.position.x;

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

        //SE o player não existe, eu saio
        if (!player)
        {
            inLoadAttackState = false;
            timerLoadAttack = timerLoadFixedAttack;
            enemyState = state.idle;
            return;
        }

        //SE ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetTrigger("isLoadAttack");

            inAnimation = true;
        }


        //SE o timer for <= 0 eu vou atacar
        if (timerLoadAttack <= 0)
        {
            inLoadAttackState = false;
            inAnimation = false;
            timerLoadAttack = timerLoadFixedAttack;
            enemyState = state.attack;
        }
    }

    private void StateAttack()
    {
        inAttackState = true;

        //SE ainda não mudei a animação
        if (!inAnimation && comboAttack <= 0)
        {
            animator.SetTrigger("attack01");

            inAnimation = true;
        }


        if (comboAttack <= 0) comboAttack = 1;



        /* SE estou no primeiro ataque do combo eu crio a hitbox de acordo com minha
         * direção e no tempo correto da animação do golpe 1
        */
        if (comboAttack == 1)
        {

            if (direction == 1) // [ DIREITA ]
            {
                if (!createHitbox && firstAttackTimer <= 0.18)
                {
                    GameObject hitboxInst = Instantiate(hitbox, hitboxPosDir.position * direction, Quaternion.identity);

                    createHitbox = true;

                    Destroy(hitboxInst, 0.1f);
                }
            }
            else if (direction == -1) // [ ESQUERDA ]
            {
                if (!createHitbox && firstAttackTimer <= 0.18)
                {
                    GameObject hitboxInst = Instantiate(hitbox, -hitboxPosEsq.position * direction, Quaternion.identity);

                    createHitbox = true;

                    Destroy(hitboxInst, 0.1f);
                }
            }


            /* SE o timer da primeira animação chegou ao fim, vou para o próximo combo
             * SE o player ainda estiver dentro da zona de ataque
            */
            if (firstAttackTimer <= 0 && attackZone)
            {
                comboAttack = 2;
            }

            else if (firstAttackTimer <= 0 && !attackZone)
            {
                comboAttack = 0;
                inAnimation = false;
                firstAttackTimer = firstAttackFixedTimer;
                inAttackState = false;
                enemyState = state.chase;
            }
        }

        /* SE o estou no segundo golpe do combo, eu desfiro um golpe mais forte que
         * afasta o player para mais longe
         */

    }

    private void StateHurt()
    {

    }

    private void StateDie()
    {

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
