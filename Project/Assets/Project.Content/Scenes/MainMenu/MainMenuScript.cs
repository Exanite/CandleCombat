using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private Animator animator = null;

    public string MainSceneName = "Main";

    public void OnClickedPlay()
    {
        animator.SetTrigger("FadeOut");
    }

    public void OnClickedQuit()
    {
        Application.Quit();
    }

    public void OnFadeComplete()
    {
        SceneManager.LoadScene(MainSceneName);
    }

    
}
