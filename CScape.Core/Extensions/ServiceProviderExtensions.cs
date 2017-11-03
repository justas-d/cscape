﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core.Extensions
{
    public static class ServiceProviderExtensions
    {
        public sealed class ServiceNotProvidedException : Exception
        {
            public Type ServiceType { get; }

            public ServiceNotProvidedException(Type serviceType) => ServiceType = serviceType;
            public override string ToString() => $"Service not provided: {ServiceType.Name}";
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowOrGet<T>(this IServiceProvider provider) where T : class
        {
            return provider.GetService<T>() ?? throw new ServiceNotProvidedException(typeof(T));
        }
    }
}
