#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Willcraftia.Xna.Framework.Graphics
{
    public sealed class DepthMapEffect : Effect, IEffectMatrices
    {
        EffectParameter projection;

        EffectParameter view;

        EffectParameter world;

        // I/F
        public Matrix Projection
        {
            get { return projection.GetValueMatrix(); }
            set { projection.SetValue(value); }
        }

        // I/F
        public Matrix View
        {
            get { return view.GetValueMatrix(); }
            set { view.SetValue(value); }
        }

        // I/F
        public Matrix World
        {
            get { return world.GetValueMatrix(); }
            set { world.SetValue(value); }
        }

        public DepthMapEffect(Effect cloneSource)
            : base(cloneSource)
        {
            world = Parameters["World"];
            view = Parameters["View"];
            projection = Parameters["Projection"];

            AssertParameterNotNull(world);
            AssertParameterNotNull(view);
            AssertParameterNotNull(projection);
        }

        void AssertParameterNotNull(EffectParameter parameter)
        {
            if (parameter == null) throw new ArgumentException("Parameter not found.");
        }
    }
}
