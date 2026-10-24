using Castle.Core.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Model
{
    public class Activity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public string TimeOfDay { get; set; }
        public int HealthImpact { get; set; }
        public int MoodImpact { get; set; }
        public string RequiredHealthStatus { get; set; }
        public string RequiredSkills { get; set; }
        public virtual ICollection<Resource> Resources { get; set; }

        public Activity()
        {
            Resources = new HashSet<Resource>();
        }
    }

}
