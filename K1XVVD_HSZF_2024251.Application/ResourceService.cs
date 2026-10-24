using K1XVVD_HSZF_2024251.Model;
using K1XVVD_HSZF_2024251.Persistence.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Application
{
    public interface IResourceService
    {
        void CreateResource(Resource resource);
        void UpdateResource(Resource resource);
        void DeleteResource(int id);
        IEnumerable<Resource> GetAllResources();
        Resource GetResourceById(int id);
        public event EventHandler<string>? ResourceDepleted;
        public IEnumerable<Resource> GetResources();
    }

    public class ResourceService : IResourceService
    {
        private readonly IResourceDataProvider _resourceDataProvider;
        private readonly IActivityDataProvider _activityDataProvider;
        private readonly List<Resource> _centralWarehouse = new List<Resource>();
        public event EventHandler<string>? ResourceDepleted;

        public ResourceService(IResourceDataProvider resourceDataProvider, IActivityDataProvider activityDataProvider)
        {
            _resourceDataProvider = resourceDataProvider;
            _activityDataProvider = activityDataProvider;
        }

        public void CreateResource(Resource resource)
        {
            if (string.IsNullOrWhiteSpace(resource.Name))
                throw new ArgumentException("Resource name cannot be empty.");

            _resourceDataProvider.CreateResource(resource);
        }

        public void UpdateResource(Resource resource)
        {
            var existingResource = _resourceDataProvider.GetResourceById(resource.Id);
            if (existingResource == null)
                throw new InvalidOperationException($"Resource with ID {resource.Id} does not exist.");

            _resourceDataProvider.UpdateResource(resource);
        }

        public void DeleteResource(int id)
        {
            var existingResource = _resourceDataProvider.GetResourceById(id);
            if (existingResource == null)
                throw new InvalidOperationException($"Resource with ID {id} does not exist.");

            _resourceDataProvider.DeleteResource(id);
        }

        public IEnumerable<Resource> GetAllResources()
        {
            return _resourceDataProvider.ReadAllResources();
        }

        public Resource GetResourceById(int id)
        {
            var resource = _resourceDataProvider.GetResourceById(id);
            if (resource == null)
                throw new InvalidOperationException($"Resource with ID {id} does not exist.");

            return resource;
        }
        public void AddResources(List<Resource> resources)
        {
            foreach (var resource in resources)
            {
                var existingResource = _centralWarehouse.FirstOrDefault(r => r.Name == resource.Name && r.Type == resource.Type);
                if (existingResource != null)
                {
                    existingResource.Quantity += resource.Quantity;
                }
                else
                {
                    _centralWarehouse.Add(new Resource
                    {
                        Name = resource.Name,
                        Type = resource.Type,
                        Quantity = resource.Quantity
                    });
                }
            }
        }
        public void AllocateResourcesForActivity(int activityId)
        {
            var activity = _activityDataProvider.GetActivityById(activityId);
            if (activity == null)
            {
                throw new InvalidOperationException("Activity not found.");
            }

            foreach (var requiredResource in activity.Resources)
            {
                var warehouseResource = _centralWarehouse.FirstOrDefault(r => r.Name == requiredResource.Name && r.Type == requiredResource.Type);
                if (warehouseResource == null || warehouseResource.Quantity < requiredResource.Quantity)
                {
                    ResourceDepleted?.Invoke(this, $"Resource {requiredResource.Name} is depleted or insufficient.");
                    throw new InvalidOperationException($"Insufficient resource: {requiredResource.Name}.");
                }

                warehouseResource.Quantity -= requiredResource.Quantity;
            }
        }
        public IEnumerable<Resource> GetResources()
        {
            return _centralWarehouse;
        }
    }

}
