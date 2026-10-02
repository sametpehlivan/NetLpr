using NetLpr.Core.Models;

namespace NetLpr.Core.Validators.Sources

{
    public static class RtspSourceInfoValidator
    {
        public static List<string> Validate(RtspSourceInfo rtspSourceInfo)
        {
            List<string> errors = new();
            if(rtspSourceInfo.Id.Equals(Guid.Empty))
                errors.Add("rtspSource.error.identificationNumberCannotBeEmpty");
            if (rtspSourceInfo.RtspConnectionInfo == null )
                errors.Add("rtspSource.error.connectionInfoMissing");
            else
                errors.AddRange(rtspSourceInfo.RtspConnectionInfo.Validate());
            errors.AddRange(rtspSourceInfo.StreamAnalysesInfo.Validate());
            return errors;
        }
    }
}
