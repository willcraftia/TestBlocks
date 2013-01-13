#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    /// <summary>
    /// テクスチャを画面へ一覧表示するためのクラスです。
    /// 描画の最中に生成される中間テクスチャの確認のためなどに利用します。
    /// このクラスのインスタンスは、
    /// ゲーム コンポーネントとして登録された時点でシングルトンとして振舞います。
    /// なお、複数のゲーム インスタンスをアプリケーションに存在させて、
    /// 各ゲーム インスタンスへこのクラスのインスタンスを設定することは想定していません。
    /// </summary>
    public sealed class TextureDisplay : DrawableGameComponent
    {
        public const int MapOffsetX = 2;
        
        public const int MapOffsetY = 2;

        /// <summary>
        /// ゲーム コンポーネントとして登録されたインスタンス。
        /// </summary>
        static TextureDisplay instance;

        SpriteBatch spriteBatch;

        int textureSize = 128;

        Queue<Texture2D> textures = new Queue<Texture2D>(10);

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
        /// 既にゲーム コンポーネントとして登録されたインスタンスがある場合、
        /// 例外が発生してインスタンスの生成に失敗します。
        /// </summary>
        /// <param name="game">ゲーム インスタンス。</param>
        public TextureDisplay(Game game)
            : base(game)
        {
            if (instance != null) throw new InvalidOperationException("Instance already exists.");

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
            int offsetX = MapOffsetX;
            int offsetY = MapOffsetY;
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

        /// <summary>
        /// テクスチャを登録します。
        /// </summary>
        /// <param name="texture"></param>
        [Conditional("DEBUG"), Conditional("TRACE")]
        public static void Add(Texture2D texture)
        {
            if (texture == null) throw new ArgumentNullException("texture");
            if (texture.IsDisposed) throw new ArgumentException("Texture already disposed.");

            if (instance != null && instance.Visible && instance.Enabled)
                instance.textures.Enqueue(texture);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.LoadContent();
        }

        /// <summary>
        /// ゲーム コンポーネントとしてインスタンスが登録される時に、
        /// 静的シングルトン フィールドへ自身を設定します。
        /// </summary>
        /// <param name="sender">イベントのソース。</param>
        /// <param name="e">イベント データ。</param>
        void OnComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            if (e.GameComponent == this) instance = this;
        }

        /// <summary>
        /// ゲーム コンポーネントとしての登録が解除される時に、
        /// 静的シングルトン フィールドから自身を削除します。
        /// </summary>
        /// <param name="sender">イベントのソース。</param>
        /// <param name="e">イベント データ。</param>
        void OnComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            if (e.GameComponent == this) instance = null;
        }
    }
}
