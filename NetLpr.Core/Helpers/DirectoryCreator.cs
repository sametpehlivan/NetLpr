using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Helpers
{
    public static class DirectoryCreator
    {
        private static object _lock = new object();
        public static void Create(string directory)
        {
            if (!Directory.Exists(directory))
            {
                lock (_lock)
                {
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }
            }
        }
    }
}
