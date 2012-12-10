#region Using

using System;
using Willcraftia.Xna.Framework.IO;

#endregion

namespace Willcraftia.Xna.Framework.Assets
{
    public interface IAssetLoader
    {
        AssetManager AssetManager { set; }

        object Load(IResource resource);

        //
        // Unload は、Dispose すべきプロパティを持つならば、その Dispose を行う。
        // あるいは、インスタンスをプールするために、他のアセットへの参照を切り離すなど。
        //
        // アセットに IDisposable を実装する仕組みにしない理由は、
        // 一つに、アセット参照を含むインスタンス全体の Dispose ではないこと、
        // 二つに、他のアセットへの参照を設定している箇所が IAssetLoader.Load であり、
        // ここに Unload 処理をまとめた方が見通しが良いことを挙げることができる。
        //
        // なお、基本的には、個々のアセットについて Unload が呼び出されることはなく、
        // AssetManager の破棄でまとめて呼び出される。
        // ただし、エディタなどで不要となったアセットを即座に解放したい場合など、
        // 特殊な状況下では、個々のアセットについて Unload を呼び出す可能性がある。
        //
        void Unload(IResource resource, object asset);

        void Save(IResource resource, object asset);
    }
}
