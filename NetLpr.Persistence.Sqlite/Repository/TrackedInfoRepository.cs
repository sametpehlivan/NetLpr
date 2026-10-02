using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services;
using NetLpr.Core.Services.Messaging;
using NetLpr.Core.Values.Request;

namespace NetLpr.Persistence.Sqlite.Repository
{
    public class TrackedInfoRepository : ITrackedInfoRepository
    {
        private static SemaphoreSlim _lock = new(1);
        private readonly SqliteDbContext _context;
        private readonly ILogger _logger;
        private readonly IMessageBus _messageBus;
        private readonly string _id = Guid.NewGuid().ToString();
        public TrackedInfoRepository(ILogger logger, SqliteDbContext context, IMessageBus messageBus)
        {
            _logger = logger;
            _context = context;
            _messageBus = messageBus;
            _messageBus.Subscribe<TrackedInfoCompletedRequest>(_id,async (info) =>
            {
                if(info.TrackedInfo != null)
                {
                    await AddAsync(info.TrackedInfo);
                    
                }
                
            });
        }
        ~TrackedInfoRepository()
        {
            _messageBus.Unsubscribe<TrackedInfo>(_id);
        }
        public async Task AddAsync(TrackedInfo model)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                await _context.TrackedInfos.AddAsync(new Models.TrackedInfoDbModel()
                {
                    SourceId = model.SourceId,
                    TrackedId = model.TrackedId,
                    DateTime = model.DateTime,
                    ImagePath = model.ImagePath,
                    TrackingValues = model.TrackingValues,
                    PlateRectangle = model.PlateRectangle,
                });
                await _context.SaveChangesAsync();
           
            }
            catch (Exception e)
            {
                _logger.LogError(e, "PersistenceError");
            }
            finally
            {
                _lock.Release();
            }

        }

        public async Task DeleteAsync(string id)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var dbModel = await _context.TrackedInfos.FindAsync(id);
                if (dbModel != null)
                {
                    _context.TrackedInfos.Remove(dbModel);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "PersistenceError");
            }
            finally
            {
                _lock.Release();
            }
        }

        public List<TrackedInfo> GetAll()
        {
            var dbModels = _context.TrackedInfos
                .ToList();

            return dbModels.Select(x =>
            {
                var model = new TrackedInfo(x.SourceId, x.TrackedId);
                foreach (var item in x.TrackingValues)
                {
                    model.TrackingValues.Add(item.Key, item.Value);
                }
                model.PlateRectangle = x.PlateRectangle;
                model.ImagePath = x.ImagePath;
                model.DateTime = x.DateTime;
                return model;
            }).ToList();
        }
    }
}
