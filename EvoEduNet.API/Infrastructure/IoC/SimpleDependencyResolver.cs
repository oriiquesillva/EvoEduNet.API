using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace EvoEduNet.API.Infrastructure.IoC
{
    /// <summary>
    /// Implementação desacoplada e leve de IDependencyResolver para ASP.NET Web API 2.
    /// Suporta injeção de dependência limpa sem sobrecarga de bibliotecas pesadas.
    /// </summary>
    public class SimpleDependencyResolver : IDependencyResolver
    {
        private readonly ConcurrentDictionary<Type, Func<object>> _factories;

        public SimpleDependencyResolver()
        {
            _factories = new ConcurrentDictionary<Type, Func<object>>();
        }

        private SimpleDependencyResolver(ConcurrentDictionary<Type, Func<object>> factories)
        {
            _factories = factories;
        }

        public void Register<TService>(Func<TService> factory) where TService : class
        {
            _factories[typeof(TService)] = factory;
        }

        public void Register(Type serviceType, Func<object> factory)
        {
            _factories[serviceType] = factory;
        }

        public object GetService(Type serviceType)
        {
            if (_factories.TryGetValue(serviceType, out var factory))
            {
                return factory();
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var service = GetService(serviceType);
            return service != null ? new[] { service } : new object[0];
        }

        public IDependencyScope BeginScope()
        {
            return new SimpleDependencyResolver(_factories);
        }

        public void Dispose()
        {
        }
    }
}
