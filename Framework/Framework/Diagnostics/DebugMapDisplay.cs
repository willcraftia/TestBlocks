#region Using

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    public sealed class DebugMapDisplay : DrawableGameComponent
    {
        public const int DefaultMapSize = 128;

        public const int DefaultMapOffsetX = 2;
        
        public const int DefaultMapOffsetY = 2;

        SpriteBatch spriteBatch;

        int mapSize = DefaultMapSize;

        List<Texture2D> maps = new List<Texture2D>(10);

        public static bool Available
        {
            get { return Instance != null; }
        }

        public static DebugMapDisplay Instance { get; private set; }

        /// <summary>
        /// テクスチャ描画領域のサイズを取得または設定します。
        /// </summary>
        public int MapSize
        {
            get { return mapSize; }
            set { mapSize = value; }
        }

        public List<Texture2D> Maps
        {
            get { return maps; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="game">インスタンスを登録する Game。</param>
        public DebugMapDisplay(Game game)
            : base(game)
        {
            game.Components.ComponentAdded += OnComponentAdded;
            game.Components.ComponentRemoved += OnComponentRemoved;

            // 更新処理は行いません。
            Enabled = false;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.LoadContent();
        }

        /// <summary>
        /// 自分自身が GameComponent として登録される時に、自分自身を現在有効な IDebugMap として設定します。
        /// </summary>
        /// <param name="sender">イベントのソース。</param>
        /// <param name="e">イベント データ。</param>
        void OnComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            if (e.GameComponent == this) Instance = this;
        }

        /// <summary>
        /// 自分自身が GameComponent としての登録から解除される時に、
        /// 自分自身が現在有効な IDebugMap ではなくなるように設定します。
        /// </summary>
        /// <param name="sender">イベントのソース。</param>
        /// <param name="e">イベント データ。</param>
        void OnComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            if (e.GameComponent == this) Instance = null;
        }

        /// <summary>
        /// 登録されているテクスチャを描画します。
        /// </summary>
        /// <param name="gameTime">前回の Update が呼び出されてからの経過時間。</param>
        public override void Draw(GameTime gameTime)
        {
            int offsetX = DefaultMapOffsetX;
            int offsetY = DefaultMapOffsetY;
            int width = MapSize;
            int height = MapSize;
            var rect = new Rectangle(offsetX, offsetY, width, height);

            foreach (var map in maps)
            {
                var samplerState = map.GetPreferredSamplerState();

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, samplerState, null, null);
                spriteBatch.Draw(map, rect, Color.White);
                spriteBatch.End();

                rect.X += width + offsetX;
                if (GraphicsDevice.Viewport.Width < rect.X + width)
                {
                    rect.X = offsetX;
                    rect.Y += height + offsetY;
                }
            }

            maps.Clear();
        }
    }
}
