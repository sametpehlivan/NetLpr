using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;

namespace NetLpr.Core.Services
{
    public interface ITrackedInfoRepository
    {
        Task AddAsync(TrackedInfo model);
        Task DeleteAsync(string id);
        List<TrackedInfo> GetAll();
    }
}
