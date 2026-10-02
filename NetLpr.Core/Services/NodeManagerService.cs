using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Exceptions;
using NetLpr.Core.Extensions;
using NetLpr.Core.Localization;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Services.Messaging;
using NetLpr.Core.Services.Node;
using NetLpr.Core.Services.Persistence;
using NetLpr.Core.Values.Request;
using NetLpr.Core.Values.Response;

namespace NetLpr.Core.Services
{
    public class NodeManagerService
    {
        private string _id = Guid.NewGuid().ToString();
        private Dictionary<string, RtspNode> _nodes = new();
        private ILogger _logger;
        private INodeFactory _nodeCreatorFactory;
        private IMessageBus _messageBus;
        private ILocalizationService _localizationService;
        private IRtspSourcePersistenceService _rtspSourcePersistenceService;
        public NodeManagerService(
            ILogger logger,
            ILocalizationService localizationService,
            INodeFactory nodeCreatorFactory,
            IMessageBus messageBus,
            IRtspSourcePersistenceService rtspSourcePersistenceService) {
            _logger = logger;
            _nodeCreatorFactory = nodeCreatorFactory;
            _messageBus = messageBus;
            _localizationService = localizationService;
            _rtspSourcePersistenceService = rtspSourcePersistenceService;
             
            SubscribeCreateNodeRequest();
            SubscribeUpdateNodeRequest();
            SubscribeDeleteNodeRequest();
            LoadAll();
        }
        public RtspNode? GetRtspNode(string id)
        {
            _nodes.TryGetValue(id, out var rtspNode);
            return rtspNode;
        }
        public List<RtspSourceInfo> GetAllRtspSourceInfos()
        {
            return _nodes.Values.Select(x => x.GetRtspSourceInfo()).ToList();
        }
        private void LoadAll()
        {
            _rtspSourcePersistenceService.GetAll().ForEach(x =>
            {
                try
                {
                    var createNodeResponse = _nodeCreatorFactory.CreateNode(new CreateNodeRequest() { RtspSourceInfo = x});
                    if (createNodeResponse.IsSuccess() && createNodeResponse.RtspNode != null)
                    {
                        _nodes.Add(createNodeResponse.RtspNode.GetRtspSourceInfo().Id, createNodeResponse.RtspNode);
                    }
                }
                catch (LocalizationException ex)
                {
                    _logger.LogError($"{x.RtspConnectionInfo.IpAddress}:{x.RtspConnectionInfo.Port} rtsp cannot started!");
                    ex.MessageKeys.ForEach(x => _logger.LogError(_localizationService.GetLocalizeMessage(x.Message, x.Args)));
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, $"{x.RtspConnectionInfo.IpAddress}:{x.RtspConnectionInfo.Port} rtsp cannot started!");
                }
            });
        }
        private void SubscribeCreateNodeRequest()
        {
            _messageBus.Subscribe<CreateNodeRequest>(_id, req =>
            {
                CreateNodeResponse createNodeResponse;
                try
                {
                    req.RtspSourceInfo.Id = string.IsNullOrWhiteSpace(req.RtspSourceInfo.Id) ? Guid.NewGuid().ToString() : req.RtspSourceInfo.Id;
                    createNodeResponse = _nodeCreatorFactory.CreateNode(req);
                    if (createNodeResponse.IsSuccess() && createNodeResponse.RtspNode != null)
                    {
                        _nodes.Add(createNodeResponse.RtspNode.GetRtspSourceInfo().Id, createNodeResponse.RtspNode);
                    }
                }
                catch (LocalizationException e)
                {
                    createNodeResponse = new CreateNodeResponse(req);
                    e.MessageKeys.ForEach(x =>
                    {
                        createNodeResponse.AddFailMessage(_localizationService.GetLocalizeMessage(x.Message, x.Args));
                    });
                }
                catch (Exception ex)
                {
                    createNodeResponse = new CreateNodeResponse(req);
                    createNodeResponse.AddFailMessage(_localizationService.GetLocalizeMessageWithDefaultMessage("unexpectedError", ex.Message, ex.Message));
                }
                _messageBus.Publish<CreateNodeResponse>(createNodeResponse);
            });
        }
        private void SubscribeUpdateNodeRequest()
        {
            _messageBus.Subscribe<UpdateNodeRequest>(_id, req =>
            {
                UpdateNodeResponse updateNodeResponse;
                try
                {
                    if (_nodes.TryGetValue(req.RtspSourceInfo.Id, out var rtspNode))
                    {
                        rtspNode.UpdateRtspSourceInfo(req.RtspSourceInfo);
                        updateNodeResponse = new UpdateNodeResponse(req) { 
                            RtspSourceInfo = req.RtspSourceInfo
                        };
                    }
                    else
                        throw new LocalizationException(new LocalizationMessage("request.nodeNotFound"));

                }
                catch (LocalizationException e)
                {
                    updateNodeResponse = new UpdateNodeResponse(req);
                    e.MessageKeys.ForEach(x =>
                    {
                        updateNodeResponse.AddFailMessage(_localizationService.GetLocalizeMessage(x.Message, x.Args));
                    });
                }
                catch (Exception ex)
                {
                    updateNodeResponse = new UpdateNodeResponse(req);
                    updateNodeResponse.AddFailMessage(_localizationService.GetLocalizeMessageWithDefaultMessage("unexpectedError", ex.Message, ex.Message));
                }
                _messageBus.Publish(updateNodeResponse);
            });
        }
        private void SubscribeDeleteNodeRequest()
        {
            _messageBus.Subscribe<DeleteNodeRequest>(_id, req =>
            {
                DeleteNodeResponse updateNodeResponse;
                try
                {
                    if (_nodes.TryGetValue(req.SourceId, out var rtspNode))
                    {
                        rtspNode.Dispose();
                        _nodes.Remove(req.SourceId);
                        updateNodeResponse = new DeleteNodeResponse(req);
                    }
                    else
                        throw new LocalizationException(new LocalizationMessage("request.nodeNotFound"));

                }
                catch (LocalizationException e)
                {
                    updateNodeResponse = new DeleteNodeResponse(req);
                    e.MessageKeys.ForEach(x =>
                    {
                        updateNodeResponse.AddFailMessage(_localizationService.GetLocalizeMessage(x.Message, x.Args));
                    });
                }
                catch (Exception ex)
                {
                    updateNodeResponse = new DeleteNodeResponse(req);
                    updateNodeResponse.AddFailMessage(_localizationService.GetLocalizeMessageWithDefaultMessage("unexpectedError", ex.Message, ex.Message));
                }
                _messageBus.Publish(updateNodeResponse);
            });
        }
    }
}
