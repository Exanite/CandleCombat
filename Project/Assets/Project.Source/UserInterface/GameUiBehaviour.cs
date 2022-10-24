using UniDi;
using UnityEngine;

namespace Project.Source.UserInterface
{
    public class GameUiBehaviour : MonoBehaviour
    {
        [Inject]
        protected UiContext UiContext { get; private set; }
        protected GameContext GameContext => UiContext.GameContext;
    }
}