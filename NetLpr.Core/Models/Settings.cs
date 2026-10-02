using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Models
{
    public class Settings
    {
        public string ApplicationVersion { get; set; } = "2.0.1";


        public ushort ImagesDeleteDays { get; set; } = 15;
    }
}
