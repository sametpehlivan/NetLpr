using NetLpr.Core.Datas;

namespace NetLpr.Core.Services.Inference.Pipelines
{
    public interface IInferencePipeline : IDisposable
    {
        public Task ProcessAsync(InferencedInput input);

    }
}
