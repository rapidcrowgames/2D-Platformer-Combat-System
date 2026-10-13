using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneController : MonoBehaviour
{

    //Pega a cena que quero mudar
    [SerializeField] private int nextSceneNumber; //Número da cena


    //Muda de cena
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Se colidir com o player
        if (other.CompareTag("PlayerOne"))
        {
            SceneManager.LoadScene(nextSceneNumber);
        }
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
