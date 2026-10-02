using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Models;
using static System.Net.Mime.MediaTypeNames;

namespace NetLpr.Core.Localization
{
    public interface IImageFactory
    {
        public BaseImage Create(IDecodedFrame frame);
    }
}
