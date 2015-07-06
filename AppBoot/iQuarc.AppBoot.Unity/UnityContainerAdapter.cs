﻿using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

namespace iQuarc.AppBoot.Unity
{
	internal sealed class UnityContainerAdapter : IDependencyContainer, IDisposable
	{
		private static readonly Dictionary<Lifetime, Func<ServiceInfo, LifetimeManager>> lifetimeManagers
			= new Dictionary<Lifetime, Func<ServiceInfo, LifetimeManager>>
			  {
				  {Lifetime.Instance, s => new PerResolveLifetimeManager()},
				  {Lifetime.AlwaysNew, s => new TransientLifetimeManager()},
				  {Lifetime.Application, s => new ContainerControlledLifetimeManager()},
			  };

		private readonly IUnityContainer container;
		private readonly IServiceLocator serviceLocator;

		public UnityContainerAdapter()
		{
			container = new UnityContainer();
			container.AddExtension(new DisposablesContainerExtension());
            serviceLocator = new UnityServiceLocator(container);
		}

	    private UnityContainerAdapter(IUnityContainer child)
	    {
	        this.container = child;
		    this.container.AddExtension(new DisposablesContainerExtension());
            serviceLocator = new UnityServiceLocator(child);
	    }

		public IServiceLocator AsServiceLocator
		{
			get { return serviceLocator; }
		}

		public void RegisterService(ServiceInfo service)
		{
			LifetimeManager lifetime = GetLifetime(service);
			container.RegisterType(service.From, service.To, service.ContractName, lifetime, new InjectionMember[] {});
		}

		private static LifetimeManager GetLifetime(ServiceInfo srv)
		{
			Func<ServiceInfo, LifetimeManager> factory = lifetimeManagers[srv.InstanceLifetime];
			return factory(srv);
		}

		public void RegisterInstance<T>(T instance)
		{
			container.RegisterInstance(instance);
		}

        public IDependencyContainer CreateChildContainer()
	    {
	        IUnityContainer child = container.CreateChildContainer();
            return new UnityContainerAdapter(child);
	    }

	    public void Dispose()
	    {
	        container.Dispose();

	        IDisposable serviceLocatorAsDisposable = serviceLocator as IDisposable;
	        if (serviceLocatorAsDisposable != null)
	            serviceLocatorAsDisposable.Dispose();
	    }
	}
}