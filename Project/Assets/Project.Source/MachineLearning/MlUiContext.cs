using System.Collections.Generic;
using Cinemachine;
using Exanite.Core.Utilities;
using Project.Source.UserInterface;
using UniDi;
using UnityEngine;

namespace Project.Source.MachineLearning
{
    public class MlUiContext : MonoBehaviour
    {
        public Canvas GameCanvas;

        [Header("Entire map camera")]
        public CinemachineVirtualCamera EntireMapCamera;
        public int DefaultEntireMapCameraPriority = 0;
        public int ActiveEntireMapCameraPriority = 1000;

        public SpectateMode SpectateMode = SpectateMode.SpectateSelected;

        [Inject]
        private MlController mlController;

        [Inject]
        private UiContext uiContext;

        private List<MlGameContext> GameContexts => mlController.GameContexts;

        private GameContext GameContext
        {
            get => uiContext.GameContext;
            set => uiContext.GameContext = value;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                SpectateMode = (SpectateMode)MathUtility.Wrap((int)SpectateMode - 1, 0, EnumUtility<SpectateMode>.Max + 1);
                Debug.Log(SpectateMode);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                SpectateMode = (SpectateMode)MathUtility.Wrap((int)SpectateMode + 1, 0, EnumUtility<SpectateMode>.Max + 1);
                Debug.Log(SpectateMode);
            }

            switch (SpectateMode)
            {
                case SpectateMode.SpectateSelected:
                {
                    if (GameContexts.Count == 0)
                    {
                        GameContext = null;

                        break;
                    }

                    if (GameContext == null)
                    {
                        GameContext = GameContexts[0].GameContext;

                        break;
                    }

                    if (UnityEngine.Input.GetKeyDown(KeyCode.A))
                    {
                        var currentIndex = GameContexts.FindIndex(x => x.GameContext == GameContext);
                        currentIndex--;
                        currentIndex = MathUtility.Wrap(currentIndex, 0, GameContexts.Count);

                        GameContext = GameContexts[currentIndex].GameContext;
                    }

                    if (UnityEngine.Input.GetKeyDown(KeyCode.D))
                    {
                        var currentIndex = GameContexts.FindIndex(x => x.GameContext == GameContext);
                        currentIndex++;
                        currentIndex = MathUtility.Wrap(currentIndex, 0, GameContexts.Count);

                        GameContext = GameContexts[currentIndex].GameContext;
                    }

                    break;
                }
                case SpectateMode.ViewEntireMap:
                {
                    GameContext = null;

                    break;
                }
                default: throw ExceptionUtility.NotSupportedEnumValue(SpectateMode);
            }

            GameCanvas.gameObject.SetActive(GameContext != null);
            EntireMapCamera.Priority = SpectateMode == SpectateMode.ViewEntireMap ? ActiveEntireMapCameraPriority : DefaultEntireMapCameraPriority;
        }
    }
}