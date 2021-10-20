using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager manager;
    public GameObject loadingCanvas;
    public GameObject gif;

    private void Awake()
    {
        if (manager == null)
        {
            manager = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        init();
        Common.loadingCanvas = loadingCanvas;
    }

    // Update is called once per frame
    void Update()
    {
        if (statequeueflag)
        {
            StartCoroutine(StateChange());
        }
        Updater();
    }

    [ContextMenu("test")]
    void init()
    {
        StateQueue((int)gamestate.Title);
    }
    [SerializeField] gamestate forTest;


    public gamestate GameState
    {
        get
        {
            return Now_GameState;
        }
    }

    gamestate Now_GameState = gamestate.Undefined;
    gamestate Pre_GameState = gamestate.Undefined;
    gamestate Next_GameState = gamestate.Undefined;

    public void StateQueue(int to = -1)
    {
        statequeueflag = true;
        if (to == -1)
        {
            Next_GameState = Pre_GameState;
        }
        else
        {
            Next_GameState = (gamestate)to;
        }
    }
    bool statequeueflag = false;

    SceneVisor Visor;

    IEnumerator StateChange()
    {
        SceneVisor Visor1 = GotVisorOnScene();
        gif.GetComponent<GifPlayer>().index = 0;
        gif.GetComponent<GifPlayer>().StartGif();
        if (SceneManager.GetAllScenes().Length>1) SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1).buildIndex);
        AsyncOperation async = SceneManager.LoadSceneAsync((int)Next_GameState, LoadSceneMode.Additive);
        async.allowSceneActivation = false;
        async.completed += x => SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)Next_GameState));

        statequeueflag = false;

        Pre_GameState = Now_GameState;
        Now_GameState = gamestate.Undefined;
        Debug.Log("Transition…");

        if (Visor1 != null)
        {
            yield return StartCoroutine(Visor1.Finalizer(Next_GameState));
        }
        else
        {
            Debug.Log("Visor null");
        }

        if (Pre_GameState != 0)
        {
            Debug.Log("unload");
           // SceneManager.UnloadSceneAsync((int)Pre_GameState);

        }
        yield return new WaitForSeconds(2);
        async.allowSceneActivation = true;
        loadingCanvas.SetActive(false);
        yield return new WaitUntil(() => SceneManager.GetSceneByBuildIndex((int)Next_GameState).isLoaded);
        gif.GetComponent<GifPlayer>().index = 0;
        SceneVisor Visor2 = GotVisorOnScene();
        if (Visor2 != null)
        {
            yield return StartCoroutine(Visor2.Init(Pre_GameState));
        }
        else
        {
            print("Visor null");
        }
        Visor = Visor2;
        Now_GameState = Next_GameState;
        gif.GetComponent<GifPlayer>().StopGif();

        Debug.Log($"GameState was Changed from {Pre_GameState} to {Now_GameState}");

        yield break;
    }
    void Updater()
    {
        if (Now_GameState != gamestate.Undefined)
        {
            Visor.Updater();
        }
    }

    SceneVisor GotVisorOnScene()
    {
        GameObject @object = GameObject.FindGameObjectWithTag("SceneVisor");
        if (@object != null)
        {
            return @object.GetComponent<SceneVisor>();
        }
        return null;
    }

}






//gamestateとSceneIndexを一致させなければならない
public enum gamestate
{
    Undefined,
    Title,
    Signup,
    Home,
    CompletedCharacters,
    Gacha,
    Live,
    Lesson,
    Ending,
    Gallery,
    Login,
    Result,
    Story,
    GachaUnit
}


