using UnityEngine;

public class EnemyHitboxController : MonoBehaviour
{
    //Pega o player
    private PlayerController player;

    #region COLISÕES

    private void OnTriggerEnter2D(Collider2D other)
    {
        //SE não colidi com o player eu saio
        if (!other.CompareTag("PlayerOne")) return;

        //SE não encontrei o player ou ele já morreu, eu saio
        if (player == null || !player.PlayerIsAlive()) return;

        //SE ele está protegido pelo parry normal
        if (player.InParryWindow()) return;


        //SE cheguei até aqui, dou o dano
        player.LifeCount(1);
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
