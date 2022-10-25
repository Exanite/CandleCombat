using System;
using Cysharp.Threading.Tasks;
using UniDi;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Source.SceneManagement
{
    /// <summary>
    ///     Used to load scenes
    ///     <para/>
    ///     Note: Only use this class to load scenes, directly loading with
    ///     <see cref="SceneManager"/> will bypass UniDi bindings and
    ///     container parenting
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public const string ParentSceneId = "ParentScene";

        private int internalLoadingCount;
        private bool internalIsLoading;
        
        [Inject] private SceneContextRegistry sceneContextRegistry;

        public bool IsLoading => internalLoadingCount != 0;

        /// <summary>
        ///     Loads the <see cref="Scene"/> using the provided
        ///     <see cref="Scene"/> as its parent
        /// </summary>
        /// <param name="sceneName">
        ///     The name of the <see cref="Scene"/> to load
        /// </param>
        /// <param name="parent">
        ///     The parent of the new <see cref="Scene"/>
        /// </param>
        /// <param name="bindings">
        ///     Bindings to install to the <see cref="DiContainer"/>
        /// </param>
        /// <param name="bindingsLate">
        ///     Late bindings to install to the <see cref="DiContainer"/>, these
        ///     are installed after all other bindings are installed
        /// </param>
        /// <returns>
        ///     The newly loaded <see cref="Scene"/>
        /// </returns>
        public UniTask<Scene> LoadAdditiveScene(
            string sceneName,
            Scene parent,
            LocalPhysicsMode localPhysicsMode,
            Action<DiContainer> bindings = null,
            Action<DiContainer> bindingsLate = null)
        {
            var context = sceneContextRegistry.TryGetSceneContextForScene(parent);

            return LoadAdditiveScene(sceneName, context, localPhysicsMode, bindings, bindingsLate);
        }

        /// <summary>
        ///     Loads the <see cref="Scene"/> using the provided
        ///     <see cref="SceneContext"/> as its parent
        /// </summary>
        /// <param name="sceneName">
        ///     The name of the <see cref="Scene"/> to load
        /// </param>
        /// <param name="parent">
        ///     The parent of the new <see cref="Scene"/>
        /// </param>
        /// <param name="bindings">
        ///     Bindings to install to the <see cref="DiContainer"/>
        /// </param>
        /// <param name="bindingsLate">
        ///     Late bindings to install to the <see cref="DiContainer"/>, these
        ///     are installed after all other bindings are installed
        /// </param>
        /// <returns>
        ///     The newly loaded <see cref="Scene"/>
        /// </returns>
        public async UniTask<Scene> LoadAdditiveScene(
            string sceneName,
            SceneContext parent,
            LocalPhysicsMode localPhysicsMode,
            Action<DiContainer> bindings = null,
            Action<DiContainer> bindingsLate = null)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                throw new ArgumentException($"Failed to load scene. Specified scene '{sceneName}' does not exist.", nameof(sceneName));
            }

            bindings += container =>
            {
                container.Bind<Scene>().WithId(ParentSceneId).To<Scene>().FromInstance(parent.gameObject.scene);
            };

            // Allow only one scene to load at a time
            internalLoadingCount++;
            await UniTask.WaitWhile(() => internalIsLoading);
            internalIsLoading = true;

            PrepareForSceneLoad(parent, bindings, bindingsLate);

            try
            {
                var loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive, localPhysicsMode);
                await SceneManager.LoadSceneAsync(sceneName, loadSceneParameters);

                // LoadSceneAsync does not return the newly loaded scene, this is the only way to get the new scene
                var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

                // Wait for scene to initialize
                await UniTask.Yield();

                return scene;
            }
            finally
            {
                // Prevent dead lock
                internalIsLoading = false;
                internalLoadingCount--;

                Cleanup();
            }
        }

        /// <summary>
        ///     Loads the <see cref="Scene"/> while unloading all other scenes
        /// </summary>
        /// <param name="sceneName">
        ///     The name of the <see cref="Scene"/> to load
        /// </param>
        /// <param name="bindings">
        ///     Bindings to install to the <see cref="DiContainer"/>
        /// </param>
        /// <param name="bindingsLate">
        ///     Late bindings to install to the <see cref="DiContainer"/>, these
        ///     are installed after all other bindings are installed
        /// </param>
        /// <returns>
        ///     The newly loaded <see cref="Scene"/>
        /// </returns>
        public async UniTask<Scene> LoadSingleScene(
            string sceneName,
            LocalPhysicsMode localPhysicsMode,
            Action<DiContainer> bindings = null,
            Action<DiContainer> bindingsLate = null)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                throw new ArgumentException($"Failed to load scene. Specified scene '{sceneName}' does not exist.", nameof(sceneName));
            }

            // Allow only one scene to load at a time
            internalLoadingCount++;
            await UniTask.WaitWhile(() => internalIsLoading);
            internalIsLoading = true;

            PrepareForSceneLoad(null, bindings, bindingsLate);

            try
            {
                var loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Single, localPhysicsMode);
                await SceneManager.LoadSceneAsync(sceneName, loadSceneParameters);

                // LoadSceneAsync does not return the newly loaded scene, this is the only way to get the new scene
                var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

                // Wait for scene to initialize
                await UniTask.Yield();

                return scene;
            }
            finally
            {
                // Prevent dead lock
                internalIsLoading = false;
                internalLoadingCount--;

                Cleanup();
            }
        }

        /// <summary>
        ///     Unloads the provided <see cref="Scene"/>
        /// </summary>
        /// <param name="scene">
        ///     The <see cref="Scene"/> to unload
        /// </param>
        public async UniTask UnloadScene(Scene scene)
        {
            if (SceneManager.sceneCount == 1)
            {
                throw new InvalidOperationException($"Cannot unload the last active scene '{scene.name}'.");
            }

            await SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>
        ///     Prepares the next scene loaded to use the provided parent and
        ///     bindings
        /// </summary>
        private void PrepareForSceneLoad(SceneContext parent, Action<DiContainer> bindings, Action<DiContainer> bindingsLate)
        {
            SceneContext.ParentContainers = parent == null ? null : new[] { parent.Container };

            SceneContext.ExtraBindingsInstallMethod = bindings;
            SceneContext.ExtraBindingsLateInstallMethod = bindingsLate;
        }

        private void Cleanup()
        {
            SceneContext.ParentContainers = null;

            SceneContext.ExtraBindingsInstallMethod = null;
            SceneContext.ExtraBindingsLateInstallMethod = null;
        }
    }
}