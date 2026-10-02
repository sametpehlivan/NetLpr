using NetLpr.Core.Values.Request;

namespace NetLpr.Core.Values.Response
{
    public abstract class BaseResponse<T>  where T : BaseRequest
    {


        private DateTime _createdTime = DateTime.UtcNow;
        public T Request { get; private set; }
        public List<string> FailureMessages { get; private set; }  = new List<string>();
        public BaseResponse(T request)
        {
            Request = request;
        }

        public string GetRequestId() => Request.GetRequestId();
        public bool IsSuccess() => FailureMessages.Count == 0;
        public void AddFailMessage(string message) => FailureMessages.Add(message);
        public DateTime GetCreatedDateTime() => _createdTime;

        

    }
    
}
