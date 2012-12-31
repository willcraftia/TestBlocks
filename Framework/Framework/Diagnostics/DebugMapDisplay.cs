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

        int textureSize = DefaultMapSize;

        Queue<Texture2D> textures = new Queue<Texture2D>(10);

        public static bool Available
        {
            get { return Instance != null; }
        }

        public static DebugMapDisplay Instance { get; private set; }

        /// <summary>
        /// テクスチャの描画サイズを取得または設定します。
        /// </summary>
        public int TextureSize
        {
            get { return textureSize; }
            set
            {
                if (textureSize < 1) throw new ArgumentOutOfRangeException("value");

                textureSize = value;
            }
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
        }

        /// <summary>
        /// テクスチャ一覧をリセットします。
        /// </summary>
        /// <param name="gameTime">前回の Update が呼び出されてからの経過時間。</param>
        public override void Update(GameTime gameTime)
        {
            // Visible = false の場合、
            // 登録済テクスチャが残骸として残る可能性があるため、ここで破棄。
            textures.Clear();

            base.Update(gameTime);
        }

        /// <summary>
        /// 登録されているテクスチャを描画します。
        /// </summary>
        /// <param name="gameTime">前回の Update が呼び出されてからの経過時間。</param>
        public override void Draw(GameTime gameTime)
        {
            int offsetX = DefaultMapOffsetX;
            int offsetY = DefaultMapOffsetY;
            int width = TextureSize;
            int height = TextureSize;
            var rect = new Rectangle(offsetX, offsetY, width, height);

            while (0 < textures.Count)
            {
                var texture = textures.Dequeue();

                var samplerState = texture.GetPreferredSamplerState();

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, samplerState, null, null);
                spriteBatch.Draw(texture, rect, Color.White);
                spriteBatch.End();

                rect.X += width + offsetX;
                if (GraphicsDevice.Viewport.Width < rect.X + width)
                {
                    rect.X = offsetX;
                    rect.Y += height + offsetY;
                }
            }
        }

        public void Add(Texture2D texture)
        {
            if (texture == null) throw new ArgumentNullException("texture");
            if (texture.IsDisposed) throw new ArgumentException("Texture already disposed.");

            if (Visible && Enabled) textures.Enqueue(texture);
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
    }
}
