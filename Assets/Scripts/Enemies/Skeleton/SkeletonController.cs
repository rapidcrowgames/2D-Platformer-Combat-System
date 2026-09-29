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
    private bool inChaseState = false;

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
        if (playerDetected)
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
        if (playerDetected)
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
        inChaseState = true;

        //SE ainda não mudei a animação
        if (!inAnimation)
        {
            animator.SetBool("isRunning", true);

            inAnimation = true;
        }

        //Pego a distância até o player
        float distanceX = transform.position.x - playerTrans.position.x;

        //A direção que devo ir
        float direction = Mathf.Sign(distanceX);

        //Vou na direção do player
        rb.linearVelocity = new Vector2(direction * vel, rb.linearVelocity.y);

        //SE cheguei próximo demais do player, vou para estado de Load Attack

    }

    private void StateLoadAttack()
    {

    }

    private void StateAttack()
    {

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
