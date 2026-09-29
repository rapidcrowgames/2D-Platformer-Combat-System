using UnityEngine;

public class AttackZoneController : MonoBehaviour
{
    //Pega componentes
    private EnemyFather enemy;



    #region COLISÕES / DETECÇÕES

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerOne"))
        {
            //Detectei o player
            enemy.SetAttackZone(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
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
