using Microsoft.Extensions.DependencyInjection;
using SEA.DependencyInjection.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SEA.DependencyInjection.Configuration
{
    public sealed partial class DependencyBuilder
    {
        int ICollection<ServiceDescriptor>.Count => _serviceInfos.Count;

        bool ICollection<ServiceDescriptor>.IsReadOnly => false;

        ServiceDescriptor IList<ServiceDescriptor>.this[int index]
        {
            get => ToServiceDescriptor(_serviceInfos[index]);
            set
            {
                var asServiceCollection = (IServiceCollection)this;
                asServiceCollection.RemoveAt(index);
                asServiceCollection.Insert(index, value);
            }
        }

        int IList<ServiceDescriptor>.IndexOf(ServiceDescriptor item)
        {
            return _serviceInfos.FindIndex(x => IsMatchWithServiceDescriptor(x, item));
        }

        void IList<ServiceDescriptor>.Insert(int index, ServiceDescriptor item)
        {
            EnsureUniqueService(item.ServiceType, Enum.Parse<ServiceScope>(item.Lifetime.ToString()));

            if (item.ImplementationInstance == null && item.ImplementationFactory == null)
            {
                EnsureConstructibleType(item.ImplementationType);
            }

            _serviceInfos.Insert(index, ToServiceInfo(item));
        }

        void IList<ServiceDescriptor>.RemoveAt(int index)
        {
            _serviceInfos.RemoveAt(index);
        }

        void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
        {
            EnsureUniqueService(item.ServiceType, Enum.Parse<ServiceScope>(item.Lifetime.ToString()));

            if (item.ImplementationInstance == null && item.ImplementationFactory == null)
            {
                EnsureConstructibleType(item.ImplementationType);
            }

            _serviceInfos.Add(ToServiceInfo(item));
        }

        void ICollection<ServiceDescriptor>.Clear()
        {
            _serviceInfos.Clear();
        }

        bool ICollection<ServiceDescriptor>.Contains(ServiceDescriptor item)
        {
            return _serviceInfos.Any(x => IsMatchWithServiceDescriptor(x, item));
        }

        void ICollection<ServiceDescriptor>.CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            var items = _serviceInfos.Select(ToServiceDescriptor).ToArray();
            items.CopyTo(array, arrayIndex);
        }

        bool ICollection<ServiceDescriptor>.Remove(ServiceDescriptor item)
        {
            var matchingItem = _serviceInfos.FirstOrDefault(x => IsMatchWithServiceDescriptor(x, item));

            if (matchingItem == null)
            {
                return false;
            }

            _serviceInfos.Remove(matchingItem);
            return true;
        }

        IEnumerator<ServiceDescriptor> IEnumerable<ServiceDescriptor>.GetEnumerator()
        {
            return _serviceInfos.Select(ToServiceDescriptor).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _serviceInfos.Select(ToServiceDescriptor).GetEnumerator();
        }

        private static bool IsMatchWithServiceDescriptor(ServiceInfo serviceInfo, ServiceDescriptor serviceDescriptor)
        {
            return
                serviceInfo.ServiceType == serviceDescriptor.ServiceType &&
                serviceInfo.Instance == serviceDescriptor.ImplementationInstance &&
                serviceInfo.Scope.ToString() == serviceDescriptor.Lifetime.ToString();
        }

        private static ServiceDescriptor ToServiceDescriptor(ServiceInfo serviceInfo)
        {
            if (serviceInfo.Instance != null)
            {
                return new ServiceDescriptor(serviceInfo.ServiceType, serviceInfo.Instance);
            }
            
            if (serviceInfo.CreationFunction != null)
            {
                return
                    new ServiceDescriptor(
                        serviceInfo.ServiceType,
                        x => serviceInfo.CreationFunction.Invoke((IDependencyResolver)x),
                        Enum.Parse<ServiceLifetime>(serviceInfo.Scope.ToString()));
            }

            return
                new ServiceDescriptor(
                    serviceInfo.ServiceType,
                    serviceInfo.ImplementationTypeInfo.Type,
                    Enum.Parse<ServiceLifetime>(serviceInfo.Scope.ToString()));
        }

        private static ServiceInfo ToServiceInfo(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationInstance != null)
            {
                return
                    new ServiceInfo(
                        serviceDescriptor.ServiceType,
                        serviceDescriptor.ImplementationInstance.GetType(),
                        Enum.Parse<ServiceScope>(serviceDescriptor.Lifetime.ToString()),
                        serviceDescriptor.ImplementationInstance,
                        null);
            }

            if (serviceDescriptor.ImplementationFactory != null)
            {
                return
                    new ServiceInfo(
                        serviceDescriptor.ServiceType,
                        null,
                        Enum.Parse<ServiceScope>(serviceDescriptor.Lifetime.ToString()),
                        null,
                        serviceDescriptor.ImplementationFactory);
            }

            return
                new ServiceInfo(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType,
                    Enum.Parse<ServiceScope>(serviceDescriptor.Lifetime.ToString()),
                    null,
                    null);
        }
    }
}
