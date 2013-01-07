﻿#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class EdgeSettings
    {
        public const float DefaultMapScale = 1;

        public const float DefaultEdgeWidth = 1;

        public const float DefaultEdgeIntensity = 20;

        public const float DefaultNormalThreshold = 0.1f;

        public const float DefaultDepthThreshold = 0;

        public const float DefaultNormalSensitivity = 1;

        public const float DefaultDepthSensitivity = 1000;

        float mapScale = DefaultMapScale;

        float edgeWidth = DefaultEdgeWidth;

        float edgeIntensity = DefaultEdgeIntensity;

        float normalThreshold = DefaultNormalThreshold;

        float depthThreshold = DefaultDepthThreshold;

        float normalSensitivity = DefaultNormalSensitivity;

        float depthSensitivity = DefaultDepthSensitivity;

        Vector3 edgeColor = Vector3.Zero;

        // TODO
        float farPlaneDistance = 128;

        /// <summary>
        /// 実スクリーンに対する法線深度マップのスケールを取得または設定します。
        /// </summary>
        public float MapScale
        {
            get { return mapScale; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                mapScale = value;
            }
        }

        public float EdgeWidth
        {
            get { return edgeWidth; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                edgeWidth = value;
            }
        }

        public float EdgeIntensity
        {
            get { return edgeIntensity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                edgeIntensity = value;
            }
        }

        public float NormalThreshold
        {
            get { return normalThreshold; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                normalThreshold = value;
            }
        }

        public float DepthThreshold
        {
            get { return depthThreshold; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                depthThreshold = value;
            }
        }

        public float NormalSensitivity
        {
            get { return normalSensitivity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                normalSensitivity = value;
            }
        }

        public float DepthSensitivity
        {
            get { return depthSensitivity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                depthSensitivity = value;
            }
        }

        public Vector3 EdgeColor
        {
            get { return edgeColor; }
            set { edgeColor = value; }
        }

        /// <summary>
        /// 法線深度マップ描画で使用するカメラの、遠くのビュー プレーンとの距離。
        /// </summary>
        public float FarPlaneDistance
        {
            get { return farPlaneDistance; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                farPlaneDistance = value;
            }
        }
    }
}