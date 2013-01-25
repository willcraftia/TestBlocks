#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public abstract class SceneObject
    {
        /// <summary>
        /// 位置。
        /// </summary>
        public Vector3 PositionWorld;

        /// <summary>
        /// オブジェクトのメッシュ全体を含む BoundingBox。
        /// </summary>
        public BoundingBox BoxWorld;

        /// <summary>
        /// オブジェクトのメッシュ全体を含む BoundingSphere。
        /// </summary>
        public BoundingSphere SphereWorld;

        string name;

        public string Name
        {
            get { return name; }
            set
            {
                // TODO
                // コンストラクタで確定させたい。

                if (value == null) throw new ArgumentNullException("value");

                if (Parent != null)
                {
                    if (Parent.Objects.Contains(value))
                        throw new ArgumentException("Name duplicate.", "value");

                    // 今の名前での登録を解除。
                    if (name != null) Parent.Objects.Remove(this);
                }

                name = value;

                // 新たな名前で再登録。
                if (Parent != null) Parent.Objects.Add(this);
            }
        }

        public SceneNode Parent { get; internal set; }

        /// <summary>
        /// 可視か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (可視の場合)、false (それ以外の場合)。
        /// </value>
        public bool Visible { get; set; }

        /// <summary>
        /// 半透明か否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (半透明の場合)、false (それ以外の場合)。
        /// </value>
        public bool Translucent { get; set; }

        /// <summary>
        /// オクルージョン カリングされたか否かを示す値を取得または設定します。
        /// </summary>
        /// <value>
        /// true (オクルージョン カリングされた場合)、false (それ以外の場合)。
        /// </value>
        public bool Occluded { get; protected set; }

        protected SceneObject()
        {
            Visible = true;
        }

        /// <summary>
        /// オクルージョン クエリを用いる場合、サブクラスでオーバライドし、
        /// オクルージョン クエリを適切に更新するように実装します。
        /// このメソッドは、各 Draw メソッドの前に呼び出されます。
        /// </summary>
        public virtual void UpdateOcclusion() { }

        /// <summary>
        /// 自身が管理するエフェクトを用いて描画します。
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// シーン マネージャが指定したエフェクトを用いて描画します。
        /// </summary>
        /// <param name="effect"></param>
        public abstract void Draw(Effect effect);
    }
}
