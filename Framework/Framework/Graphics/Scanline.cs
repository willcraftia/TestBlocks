#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Willcraftia.Xna.Framework.Diagnostics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class Scanline : PostProcessor
    {
        public const string MonitorProcess = "Scanline.Process";

        Effect effect;

        EffectParameter density;

        EffectParameter brightness;

        public float Brightness { get; set; }

        public Scanline(SpriteBatch spriteBatch, Effect effect)
            : base(spriteBatch)
        {
            if (effect == null) throw new ArgumentNullException("effect");

            this.effect = effect;
            density = effect.Parameters["Density"];
            brightness = effect.Parameters["Brightness"];
            Brightness = 0.75f;
        }

        public override void Process(IPostProcessorContext context)
        {
            Monitor.Begin(MonitorProcess);

            var height = context.Destination.Height;

            density.SetValue(height * MathHelper.PiOver2);
            brightness.SetValue(Brightness);

            GraphicsDevice.SetRenderTarget(context.Destination);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, effect);
            SpriteBatch.Draw(context.Source, context.Destination.Bounds, Color.White);
            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            Monitor.End(MonitorProcess);
        }
    }
}
