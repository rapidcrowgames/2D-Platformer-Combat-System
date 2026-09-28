using UnityEngine;

public class SkeletonController : EnemyFather
{
    #region VARIÁVEIS
    #endregion

    #region MÉTODOS
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

    }

    private void StateRun()
    {

    }

    private void StateChase()
    {

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
        
    }
}
