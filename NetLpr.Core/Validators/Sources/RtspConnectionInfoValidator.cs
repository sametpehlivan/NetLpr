using NetLpr.Core.Models;

namespace NetLpr.Core.Validators.Sources
{

    internal static class RtspConnectionInfoValidator
    {

        public static List<string> Validate(RtspConnectionInfo info)
        {
            var errors = new List<string>();

            ValidatePort(info.Port, errors);
            ValidateIp(info.IpAddress,errors);
            if (!string.IsNullOrWhiteSpace(info.PathAndQuery) &&
                !info.PathAndQuery.StartsWith("/"))
            {
                info.PathAndQuery = "/" + info.PathAndQuery;
            }
            return errors;
        }

        private static void ValidateIp(string ip, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                errors.Add("connectionInfo.ipAddress.required");
                return;
            }
        }

        private static void ValidatePort(string port, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(port))
            {
                errors.Add("connectionInfo.port.required");
                return;
            }

            if (!int.TryParse(port, out int value))
            {
                errors.Add("connectionInfo.port.invalid");
                return;
            }

            if (value is < 1 or > 65535)
                errors.Add("connectionInfo.port.invalid");
        }

        private static void ValidateCredential(string username, string password, List<string> errors)
        {
            if (!string.IsNullOrWhiteSpace(username) &&
                string.IsNullOrWhiteSpace(password))
            {
                errors.Add("connectionInfo.password.requiredWhenUsernameProvided");
            }
        }

        private static bool IsValidIPv4(string ip)
        {
            var parts = ip.Split('.');

            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (part.Length == 0)
                    return false;

                int value = 0;

                foreach (char c in part)
                {
                    if (c < '0' || c > '9')
                        return false;

                    value = value * 10 + (c - '0');
                }

                if (value > 255)
                    return false;

                if (part.Length > 1 && part[0] == '0')
                    return false;
            }

            return true;
        }
    }


}
