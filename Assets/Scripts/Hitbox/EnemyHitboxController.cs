using UnityEngine;

public class EnemyHitboxController : MonoBehaviour
{
    //Pega o player
    private PlayerController player;

    #region COLISÕES

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Se a hitbox do inimigo colidir com o player, eu tiro vida dele
        if (other.CompareTag("PlayerOne") && !player.InParryWindow() && player.PlayerIsAlive())
        {
            player.LifeCount(1);
        }
    }

    #endregion

    //Pega minhas variáveis e componentes
    private void Awake()
    {
        //Pega o player
        player = FindFirstObjectByType<PlayerController>();
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
