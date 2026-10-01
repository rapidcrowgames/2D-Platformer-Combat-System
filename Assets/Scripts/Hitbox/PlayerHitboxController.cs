using UnityEngine;

public class PlayerHitboxController : MonoBehaviour
{
    //Pego o inimigo (Padrões)
    private int damage;


    #region MÉTODOS

    //Pega a variável Damage
    public void SetDamage(int damageValue)
    {
        damage = damageValue;
    }

    #endregion

    #region COLISÕES

    private void OnTriggerEnter2D(Collider2D other)
    {
        //SE minha hitbox colidir com o inimigo comum
        if (other.CompareTag("Enemy"))
        {
            EnemyFather enemy = other.GetComponentInParent<EnemyFather>();

            //SE tiver um inimigo, então aplico o dano
            if (enemy != null)
            {
                enemy.LifeEnemyCount(damage);
            }
        }
    }

    #endregion

    //Inicia minhas variáveis e componentes
    private void Awake()
    {
        
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
