#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class SceneManagerMonitor
    {
        public event EventHandler BeginDrawScene = delegate { };

        public event EventHandler EndDrawScene = delegate { };

        public event EventHandler BeginDrawSceneOcclusionQuery = delegate { };

        public event EventHandler EndDrawSceneOcclusionQuery = delegate { };

        public event EventHandler BeginDrawSceneRendering = delegate { };

        public event EventHandler EndDrawSceneRendering = delegate { };

        SceneManager sceneManager;

        public int TotalSceneObjectCount { get; set; }

        public int VisibleSceneObjectCount { get; set; }

        public int OccludedSceneObjectCount { get; set; }

        public int RenderedSceneObjectCount
        {
            get { return VisibleSceneObjectCount - OccludedSceneObjectCount; }
        }

        public ShadowMapMonitor ShadowMap { get; internal set; }

        public ShadowSceneMonitor ShadowScene { get; internal set; }

        public SssmMonitor ScreenSpaceShadow { get; internal set; }

        public SceneManagerMonitor(SceneManager sceneManager)
        {
            if (sceneManager == null) throw new ArgumentNullException("sceneManager");

            this.sceneManager = sceneManager;
        }

        internal void OnBeginDrawScene()
        {
            BeginDrawScene(sceneManager, EventArgs.Empty);
        }

        internal void OnEndDrawScene()
        {
            EndDrawScene(sceneManager, EventArgs.Empty);
        }

        internal void OnBeginDrawSceneOcclusionQuery()
        {
            BeginDrawSceneOcclusionQuery(sceneManager, EventArgs.Empty);
        }

        internal void OnEndDrawSceneOcclusionQuery()
        {
            EndDrawSceneOcclusionQuery(sceneManager, EventArgs.Empty);
        }

        internal void OnBeginDrawSceneRendering()
        {
            BeginDrawSceneRendering(sceneManager, EventArgs.Empty);
        }

        internal void OnEndDrawSceneRendering()
        {
            EndDrawSceneRendering(sceneManager, EventArgs.Empty);
        }
    }
}
