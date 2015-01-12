using System;

namespace d60.Cirqus.Config.Configurers
{
    public abstract class ConfigurationBuilder : IRegistrar
    {
        readonly IRegistrar _registrar;

        protected ConfigurationBuilder(IRegistrar registrar)
        {
            _registrar = registrar;
        }

        //public IRegistrar Registrar
        //{
        //    get { return _registrar; }
        //}

        /// <summary>
        /// Registers a factory method for the given service
        /// </summary>
        public void Register<TService>(Func<ResolutionContext, TService> serviceFactory)
        {
            _registrar.Register(serviceFactory);
        }

        /// <summary>
        /// Registers a specific instance (which by definition is not a decorator)
        /// </summary>
        public void RegisterInstance<TService>(TService instance, bool multi = false)
        {
            _registrar.RegisterInstance(instance, multi);
        }

        /// <summary>
        /// Registers a factory method for decorating the given type
        /// </summary>
        public void Decorate<TService>(Func<ResolutionContext, TService> serviceFactory)
        {
            _registrar.Decorate(serviceFactory);
        }

        /// <summary>
        /// Checks whether the given service type has a registration. Optionally checks whether a primary (i.e. non-decorator) is present.
        /// </summary>
        public bool HasService<TService>(bool checkForPrimary = false)
        {
            return _registrar.HasService<TService>(checkForPrimary);
        }
    }

    public abstract class ConfigurationBuilder<TService> : ConfigurationBuilder
    {
        protected ConfigurationBuilder(IRegistrar registrar) : base(registrar) {}
    }
}