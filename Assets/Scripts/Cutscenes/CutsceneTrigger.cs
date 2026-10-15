using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class CutsceneTrigger : MonoBehaviour
{

    //Pegando o Director
    [SerializeField] private PlayableDirector director; //Director das cutscenes
    private bool started = false; //Controla se a cutscene já começou ou não.
    private PlayerController player; //Pega o player


    //SE colidir com o trigger da cutscene, ela inicia
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Se eu colidi com alguém que não seja o player eu saio
        if (!other.CompareTag("PlayerOne")) return;


        //SE já esta rodando a cutscene eu evito rodar de novo
        if (started) return;

        started = true;

        //Pego o input do player
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();

        //SE o player input existe, então eu desabilito ele por enquanto
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        player.InCutsceneVariable(true);

        //Inicio a cutscene
        director.Play();
    }

    private void Awake()
    {
        //Pega o player
        player = FindFirstObjectByType<PlayerController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Quando acabar a cutscene, eu chamo o método de encerrar o jogo
        director.stopped += EndGame;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void EndGame(PlayableDirector director)
    {
        //Fecha o jogo na Build
        Application.Quit();

#if UNITY_EDITOR
        //Para o Play Mode enquanto estou testando no Editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
