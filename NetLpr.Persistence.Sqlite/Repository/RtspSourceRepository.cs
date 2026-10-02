using NetLpr.Persistence.Sqlite.Models;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Messaging;
using NetLpr.Core.Services.Persistence;
using NetLpr.Core.Values.Response;
using Microsoft.EntityFrameworkCore;

namespace NetLpr.Persistence.Sqlite
{
    public class RtspSourceRepository : IRtspSourcePersistenceService
    {
        private static SemaphoreSlim _lock = new(1);
        private readonly SqliteDbContext _context;
        private readonly ILogger _logger;
        private readonly IMessageBus _messageBus;
        private readonly string _id = Guid.NewGuid().ToString();
        public RtspSourceRepository(ILogger logger, SqliteDbContext context, IMessageBus messageBus)
        {
            _logger = logger;
            _context = context;
            _messageBus = messageBus;
            _messageBus.Subscribe<CreateNodeResponse>(_id, async (response) =>
            {
                if (response.IsSuccess() && response.RtspNode != null)
                {
                    var rtspInfo = response.RtspNode.GetRtspSourceInfo();
                    var entity = await GetByIdAsync(rtspInfo.Id);
                    if (entity == null)
                    {
                        await AddAsync(rtspInfo);
                    }
                }
            });
            _messageBus.Subscribe<UpdateNodeResponse>(_id, async (response) =>
            {
                if (response.IsSuccess() && response.RtspSourceInfo != null)
                {
                    await UpdateAsync(response.RtspSourceInfo);
                }
            });
            _messageBus.Subscribe<DeleteNodeResponse>(_id, async (response) =>
            {
                if (response.IsSuccess())
                {
                    await DeleteAsync(response.Request.SourceId);
                }
            });
        }
        ~RtspSourceRepository()
        {
            _messageBus.Unsubscribe<CreateNodeResponse>(_id);
            _messageBus.Unsubscribe<UpdateNodeResponse>(_id);
            _messageBus.Unsubscribe<DeleteNodeResponse>(_id);
        }
        private RtspSourceInfoDbModel FromModel(RtspSourceInfo model, RtspSourceInfoDbModel? entity = null, SqliteDbContext? context = null)
        {
            entity = entity ?? new RtspSourceInfoDbModel();
            entity.Id = model.Id;
            entity.IpAddress = model.RtspConnectionInfo.IpAddress;
            entity.Port = model.RtspConnectionInfo.Port;
            entity.Username = model.RtspConnectionInfo.Username;
            entity.Password = model.RtspConnectionInfo.Password;
            entity.PathAndQuery = model.RtspConnectionInfo.PathAndQuery;
            entity.TransportType = model.RtspConnectionInfo.TransportType.ToString().ToLowerInvariant();
            entity.RoiX = model.StreamAnalysesInfo.AnalysesRoi.X;
            entity.RoiY = model.StreamAnalysesInfo.AnalysesRoi.Y;
            entity.RoiHeight = model.StreamAnalysesInfo.AnalysesRoi.Height;
            entity.RoiWidth = model.StreamAnalysesInfo.AnalysesRoi.Width;
            entity.AnalysesType = model.StreamAnalysesInfo.AnalysesType.ToString().ToLowerInvariant();
            entity.IsAnalysesOpen = model.StreamAnalysesInfo.IsAnalysesOpen;
            if (context != null)
            {
                context.AreaPoints.RemoveRange(entity.AnalysesArea);
                entity.AnalysesArea.Clear();

            }
            model.StreamAnalysesInfo.AnalysesPoints.ForEach(x => entity.AnalysesArea.Add(new() { SourceId = model.Id, X = x.X, Y = x.Y }));
            return entity;
        }

        public async Task AddAsync(RtspSourceInfo model)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var dbModel = FromModel(model);
                await _context.RtspSources.AddAsync(dbModel);
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
        public List<RtspSourceInfo> GetAll()
        {
            var dbModels = _context.RtspSources
                .Include(x => x.AnalysesArea)
                .ToList();

            return dbModels.Select(x => x.ToModel()).ToList();
        }
        public async Task<RtspSourceInfo?> GetByIdAsync(string id)
        {
            var dbModel = await _context.RtspSources
                .Include(x => x.AnalysesArea)
                .FirstOrDefaultAsync(x => x.Id == id);

            return dbModel?.ToModel();
        }
        public async Task UpdateAsync(RtspSourceInfo model)
        {


            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                var existingDbModel = await _context.RtspSources
              .Include(x => x.AnalysesArea)
              .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (existingDbModel == null)
                {
                    return;
                }
                FromModel(model, existingDbModel, _context);
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
                var dbModel = await _context.RtspSources.FindAsync(id);
                if (dbModel != null)
                {
                    _context.RtspSources.Remove(dbModel);
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

    }
}
