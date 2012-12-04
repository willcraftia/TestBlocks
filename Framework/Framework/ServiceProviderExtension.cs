#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework
{
    public static class ServiceProviderExtension
    {
        public static T GetRequiredService<T>(this IServiceProvider serviceProvider)
        {
            return (T) GetRequiredService(serviceProvider, typeof(T));
        }

        public static object GetRequiredService(this IServiceProvider serviceProvider, Type type)
        {
            var result = serviceProvider.GetService(type);
            if (result == null)
                throw new InvalidOperationException(string.Format("The service '{0}' can not be found.", type));

            return result;
        }
    }
}
