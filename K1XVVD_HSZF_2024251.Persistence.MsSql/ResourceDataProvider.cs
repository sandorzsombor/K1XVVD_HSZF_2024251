using K1XVVD_HSZF_2024251.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Persistence.MsSql
{
    public interface IResourceDataProvider
    {
        void CreateResource(Resource resource);
        void UpdateResource(Resource resource);
        void DeleteResource(int id);
        IEnumerable<Resource> ReadAllResources();
        Resource GetResourceById(int id);
    }

    public class ResourceDataProvider : IResourceDataProvider
    {
        private readonly SurvivalDbContext _dbContext;

        public ResourceDataProvider(SurvivalDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void CreateResource(Resource resource)
        {
            _dbContext.Resources.Add(resource);
            _dbContext.SaveChanges();
        }
        public void UpdateResource(Resource resource)
        {
            var existingResource = _dbContext.Resources.FirstOrDefault(r => r.Id == resource.Id);
            if (existingResource == null)
                throw new InvalidOperationException($"Resource with ID {resource.Id} not found.");

            existingResource.Name = resource.Name;
            existingResource.Type = resource.Type;
            existingResource.Quantity = resource.Quantity;
            existingResource.Activities = resource.Activities;

            _dbContext.SaveChanges();
        }
        public void DeleteResource(int id)
        {
            var resource = _dbContext.Resources.FirstOrDefault(r => r.Id == id);
            if (resource == null)
                throw new InvalidOperationException($"Resource with ID {id} not found.");

            _dbContext.Resources.Remove(resource);
            _dbContext.SaveChanges();
        }
        public IEnumerable<Resource> ReadAllResources()
        {
            return _dbContext.Resources.ToList();
        }
        public Resource GetResourceById(int id)
        {
            return _dbContext.Resources.FirstOrDefault(r => r.Id == id);
        }
    }
}
