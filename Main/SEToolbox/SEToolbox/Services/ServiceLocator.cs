namespace SEToolbox.Services
{
    using System;
    using System.Collections.Generic;
    using Support;

    /// <summary>
    /// A very simple service locator.
    /// </summary>
    internal static class ServiceLocator
    {
        private static readonly Dictionary<Type, ServiceInfo> services = new Dictionary<Type, ServiceInfo>();

        /// <summary>
        /// Registers a service.
        /// </summary>
        public static void Register<TInterface, TImplemention>() where TImplemention : TInterface
        {
            Register<TInterface, TImplemention>(false);
        }

        /// <summary>
        /// Registers a service as a singleton.
        /// </summary>
        public static void RegisterSingleton<TInterface, TImplemention>() where TImplemention : TInterface
        {
            Register<TInterface, TImplemention>(true);
        }

        /// <summary>
        /// Resolves a service.
        /// </summary>
        public static TInterface Resolve<TInterface>()
        {
            return (TInterface)services[typeof(TInterface)].ServiceImplementation;
        }

        /// <summary>
        /// Registers a service.
        /// </summary>
        /// <param name="isSingleton">true if service is Singleton; otherwise false.</param>
        private static void Register<TInterface, TImplemention>(bool isSingleton) where TImplemention : TInterface
        {
            services.Add(typeof(TInterface), new ServiceInfo(typeof(TImplemention), isSingleton));
        }

        /// <summary>
        /// Class holding service information.
        /// </summary>
        private class ServiceInfo
        {
            private readonly Type _serviceImplementationType;
            private object _serviceImplementation;
            private readonly bool _isSingleton;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceInfo"/> class.
            /// </summary>
            /// <param name="serviceImplementationType">Type of the service implementation.</param>
            /// <param name="isSingleton">Whether the service is a Singleton.</param>
            public ServiceInfo(Type serviceImplementationType, bool isSingleton)
            {
                this._serviceImplementationType = serviceImplementationType;
                this._isSingleton = isSingleton;
            }

            /// <summary>
            /// Gets the service implementation.
            /// </summary>
            public object ServiceImplementation
            {
                get
                {
                    if (_isSingleton)
                    {
                        if (_serviceImplementation == null)
                        {
                            _serviceImplementation = CreateInstance(_serviceImplementationType);
                        }

                        return _serviceImplementation;
                    }
                    else
                    {
                        return CreateInstance(_serviceImplementationType);
                    }
                }
            }

            /// <summary>
            /// Creates an instance of a specific type.
            /// </summary>
            /// <param name="type">The type of the instance to create.</param>
            private static object CreateInstance(Type type)
            {
                if (services.ContainsKey(type))
                    return services[type].ServiceImplementation;

                return ReflectionUtil.CreateInstance(type);
            }
        }
    }
}
