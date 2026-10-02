using NetLpr.Core;
using NetLpr.Core.Exceptions;
using NetLpr.Core.Extensions;
using NetLpr.Core.Localization;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services;
using NetLpr.Core.Services.Inference.Pipelines;
using NetLpr.Core.Services.Node;
using NetLpr.Core.Services.Sources;
using NetLpr.Core.Services.Syncronizations;
using NetLpr.Core.Services.Tracking;
using NetLpr.Core.Values.Request;
using NetLpr.Core.Values.Response;
using NetLpr.Desktop.Extensions;
using NetLpr.InferenceOV;
using OpenVinoSharp;
using Tmds.DBus.Protocol;

namespace NetLpr.Boot.Node
{
    public class NodeFactory : INodeFactory
    {
        private ILogger _logger;
        private RtspSourceContext _sourceServiceContext;
        private SynchronizationServiceContext _synchronizationServiceContext;
        private TrackingServiceContext _trackingServiceContext;
        private OpenVinoInferencePoolFactory _inferencePoolFactory;
        public ILocalizationService LocalizationService { get; }
        //private OnnxModelFactory _onnxModelFactory;


        public NodeFactory(
            ILogger logger, 
            ILocalizationService localizationService,
            RtspSourceContext sourceServiceContext, 
            SynchronizationServiceContext synchronizationServiceContext,
            TrackingServiceContext trackingServiceContext,
            OpenVinoInferencePoolFactory inferencePoolFactory
            //OnnxModelFactory onnxModelFactory
        ) { 
            _logger = logger;
            LocalizationService = localizationService;
            _sourceServiceContext = sourceServiceContext;
            _synchronizationServiceContext = synchronizationServiceContext;
            _trackingServiceContext = trackingServiceContext;
            _inferencePoolFactory = inferencePoolFactory;
            //_onnxModelFactory = onnxModelFactory;
        }
        public  CreateNodeResponse CreateNode(CreateNodeRequest request)
        {
            try
            {
                request.Validate();
                RtspNode node = CreateNode(request.RtspSourceInfo);
                _ = Task.Run(node.StartAsync);

                return new CreateNodeResponse(request, node);
            }
            catch (LocalizationException e)
            {

                e.MessageKeys.ForEach(m =>
                {
                    var message = LocalizationService.GetLocalizeMessage(m.Message, m.Args);
                    _logger.LogError(message);
                });
                throw;
            }
            catch (Exception e)
            {
                var message = LocalizationService.GetLocalizeMessage("request.nodeNotCreated");
                _logger.LogError(e, message);
                throw new LocalizationException(new LocalizationMessage("request.nodeNotCreated", e.Message));
            }
        }
        private RtspNode CreateNode(RtspSourceInfo rtspSourceInfo)
        {
      
            SyncronizationService? synchronization = new SyncronizationService(_logger, _synchronizationServiceContext);
            IInferencePipeline? pipeline = null ;
            RtspSource? source = null;
            ITrackingService? tracker = null;
            RtspNode? node = null; 

            try
            {
                pipeline = PipelineFactory.CreatePipelineFromOv(_logger, _inferencePoolFactory);
                source = SourceFactory.CreateSource(_logger,rtspSourceInfo, LocalizationService,_sourceServiceContext, _synchronizationServiceContext);
                tracker = TrackerFactory.CreateTrackerService(_logger, _trackingServiceContext);
                 return new RtspNode(
                    _logger,
                    source,
                    synchronization,
                    tracker,
                    pipeline
                );
            }
            catch
            {
                pipeline?.Dispose();
                source?.Dispose();
                synchronization?.Dispose();
                tracker?.Dispose();
                node?.Dispose();
                _logger.LogError($"{nameof(NodeFactory)} not created!");
                throw;
            }
        }
       
    }

 
}
