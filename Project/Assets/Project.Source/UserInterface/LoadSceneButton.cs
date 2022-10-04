using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Source.UserInterface
{
    public class LoadSceneButton : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

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
}