using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Persistence.MsSql
{
    public interface IActivityDataProvider
    {
        void CreateActivity(Model.Activity activity);
        void UpdateActivity(Model.Activity activity);
        void DeleteActivity(int id);
        IEnumerable<Model.Activity> ReadAllActivities();
        Model.Activity GetActivityById(int id);
    }
    public class ActivityDataProvider : IActivityDataProvider
    {
        private readonly SurvivalDbContext _dbContext;

        public ActivityDataProvider(SurvivalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void CreateActivity(Model.Activity activity)
        {
            _dbContext.Activities.Add(activity);
            _dbContext.SaveChanges();
        }

        public void UpdateActivity(Model.Activity activity)
        {
            var existingActivity = _dbContext.Activities
                .Include(a => a.Resources)
                .FirstOrDefault(a => a.Id == activity.Id);

            if (existingActivity == null)
                throw new InvalidOperationException($"Activity with ID {activity.Id} not found.");

            existingActivity.Name = activity.Name;
            existingActivity.Duration = activity.Duration;
            existingActivity.TimeOfDay = activity.TimeOfDay;
            existingActivity.HealthImpact = activity.HealthImpact;
            existingActivity.MoodImpact = activity.MoodImpact;
            existingActivity.RequiredHealthStatus = activity.RequiredHealthStatus;
            existingActivity.Resources = activity.Resources;

            _dbContext.SaveChanges();
        }

        public void DeleteActivity(int id)
        {
            var activity = _dbContext.Activities.FirstOrDefault(a => a.Id == id);
            if (activity == null)
                throw new InvalidOperationException($"Activity with ID {id} not found.");

            _dbContext.Activities.Remove(activity);
            _dbContext.SaveChanges();
        }

        public IEnumerable<Model.Activity> ReadAllActivities()
        {
            return _dbContext.Activities
                         .Include(a => a.Resources)
                         .ToList();
        }

        public Model.Activity GetActivityById(int id)
        {
            return _dbContext.Activities.FirstOrDefault(a => a.Id == id);
        }
    }

}
