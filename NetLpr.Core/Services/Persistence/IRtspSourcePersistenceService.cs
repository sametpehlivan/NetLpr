using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;

namespace NetLpr.Core.Services.Persistence
{
    public interface IRtspSourcePersistenceService
    {
        Task AddAsync(RtspSourceInfo model);
        List<RtspSourceInfo> GetAll();
        Task<RtspSourceInfo?> GetByIdAsync(string id);
        Task UpdateAsync(RtspSourceInfo model);
        Task DeleteAsync(string id);
    }
}
