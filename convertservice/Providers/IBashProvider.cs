using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace convertservice.Providers
{
    public interface IBashProvider
    {
        Task ConvertAsync(string outDir, string documentPath);
    }
}
