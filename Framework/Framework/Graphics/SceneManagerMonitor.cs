﻿#region Using

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

        public int TotalSceneObjectCount { get; internal set; }

        public int VisibleSceneObjectCount { get; internal set; }

        public int OccludedSceneObjectCount { get; internal set; }

        public int RenderedSceneObjectCount
        {
            get { return VisibleSceneObjectCount - OccludedSceneObjectCount; }
        }

        public ShadowMap.ShadowMapMonitor ShadowMap { get; internal set; }

        public Sssm.SssmMonitor Sssm { get; internal set; }

        public Ssao.SsaoMonitor Ssao { get; internal set; }

        public Edge.EdgeMonitor Edge { get; internal set; }

        public Bloom.BloomMonitor Bloom { get; internal set; }

        public Dof.DofMonitor Dof { get; internal set; }

        public ColorOverlap.ColorOverlapMonitor ColorOverlap { get; internal set; }

        public Monochrome.MonochromeMonitor Monochrome { get; internal set; }

        internal SceneManagerMonitor(SceneManager sceneManager)
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
