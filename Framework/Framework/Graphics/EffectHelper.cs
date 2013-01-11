#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public static class EffectHelper
    {
        /// <summary>
        /// SpriteBatch でシェーダ モデル 3 を用いる場合に設定する行列を算出します。
        /// </summary>
        /// <param name="width">テクスチャの幅。</param>
        /// <param name="height">テクスチャの高さ。</param>
        /// <returns>シェーダへ設定する行列。</returns>
        public static Matrix CreateSpriteBatchMatrixTransform(int width, int height)
        {
            Matrix matrixTransform;
            CreateSpriteBatchMatrixTransform(width, height, out matrixTransform);
            return matrixTransform;
        }

        /// <summary>
        /// SpriteBatch でシェーダ モデル 3 を用いる場合に設定する行列を算出します。
        /// </summary>
        /// <param name="width">テクスチャの幅。</param>
        /// <param name="height">テクスチャの高さ。</param>
        /// <param name="matrixTransform">シェーダへ設定する行列。</param>
        public static void CreateSpriteBatchMatrixTransform(int width, int height, out Matrix matrixTransform)
        {
            Matrix projection;
            Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1, out projection);

            Matrix halfPixelOffset;
            Matrix.CreateTranslation(-0.5f, -0.5f, 0, out halfPixelOffset);

            Matrix.Multiply(ref halfPixelOffset, ref projection, out matrixTransform);
        }
    }
}
