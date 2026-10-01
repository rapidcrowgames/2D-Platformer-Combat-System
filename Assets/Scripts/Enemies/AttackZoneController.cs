using UnityEngine;

public class AttackZoneController : MonoBehaviour
{
    //Pega componentes
    private EnemyFather enemy;
    private PlayerController player;



    #region COLISÕES / DETECÇÕES

    private void OnTriggerEnter2D(Collider2D other) //Entrei na colisão
    {
        if (other.CompareTag("PlayerOne") && player.PlayerIsAlive())
        {
            //Detectei o player
            enemy.SetAttackZone(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other) //Sai da colisão
    {
        if (other.CompareTag("PlayerOne"))
        {
            //Perdi o player de vista
            enemy.SetAttackZone(false);
        }
    }

    #endregion

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyFather>();
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
