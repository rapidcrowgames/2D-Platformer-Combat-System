using UnityEngine;

public class PlayerHitboxController : MonoBehaviour
{
    //Pego o inimigo (Padrões)
    private EnemyFather enemy;
    private int damage = 1;


    #region COLISÕES

    private void OnTriggerEnter2D(Collider2D other)
    {
        //SE minha hitbox colidir com o inimigo comum
        if (other.CompareTag("Enemy"))
        {
            enemy.LifeEnemyCount(damage);
        }
    }

    #endregion

    //Inicia minhas variáveis e componentes
    private void Awake()
    {
        enemy = FindFirstObjectByType<EnemyFather>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
