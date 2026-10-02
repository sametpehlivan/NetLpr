using NetLpr.Core.Log;
using NetLpr.Core.Values.Inference;
using OpenVinoSharp;

namespace NetLpr.InferenceOV
{
    public class OpenVinoInferencePoolFactory
    {
        private static object _lock = new object();
        private static OpenVinoSharp.Core _core = new OpenVinoSharp.Core();
        private ILogger _logger;
        public OpenVinoInferencePoolFactory(ILogger logger)
        {
            _logger = logger;
        }
        public  OpenVinoInferenceRequestPool GetModel(ModelConfig modelConfig)
        {
            
            if (!File.Exists(modelConfig.ModelPath))
                throw new ArgumentException($"Model not found {modelConfig.ModelPath}");
            lock (_lock)
            {
                var model = _core.read_model(modelConfig.ModelPath);
                CompiledModel compiledModel = ValidateAndInitiateModel(modelConfig,model);
                return new OpenVinoInferenceRequestPool(_logger, compiledModel);
                  
            }
        }        
        private  CompiledModel ValidateAndInitiateModel(ModelConfig modelConfig, Model model)
        {
           

            var inputTensor = model.inputs()
            .Where(
                x => x.get_any_name()
                .ToLowerInvariant()
                .Equals(
                    modelConfig.ImageTensorName
                )
            )
            .FirstOrDefault();
            if (inputTensor == null) throw new ArgumentException("tensor not found");
            var dim  = modelConfig.GetImageTensorDimensionInt();
            var ps = new PartialShape(new Shape(dim));
            model.reshape(new Dictionary<string, PartialShape>
            {
                [inputTensor.get_any_name()] = ps
            });
            foreach (var item in model.inputs())
            {
                var message = $"{modelConfig.ModelPath} :  {item.get_any_name()} -> {item.get_partial_shape().to_string()}.";
                _logger.LogInformation($"{nameof(OpenVinoInferencePoolFactory)} : {message}");
            }
            foreach (var item in model.outputs())
            {
                var message = $"{modelConfig.ModelPath} :  {item.get_any_name()} -> {item.get_partial_shape().to_string()}.";
                _logger.LogInformation($"{nameof(OpenVinoInferencePoolFactory)} : {message}");
            }
            return _core.compile_model(model, "CPU");
        }
    }
}
