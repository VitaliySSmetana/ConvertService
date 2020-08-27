using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Logging;

namespace convertservice.Providers
{
    public class BashProvider : IBashProvider
    {
        private readonly ILogger<BashProvider> _logger;
        private readonly string _convertCommand = "--convert-to {0} --outdir {1} \"{2}\"";

        public BashProvider(ILogger<BashProvider> logger)
        {
            _logger = logger;
        }

        public async Task ConvertAsync(string outDir, string documentPath)
        {
            var procStartInfo = new ProcessStartInfo{
                FileName = "soffice",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = string.Format(_convertCommand, "pdf", outDir, documentPath)
            };

            _logger.LogInformation($"Log from {nameof(BashProvider)}: {procStartInfo.FileName} {procStartInfo.Arguments}");

            using var proc = new Process { StartInfo = procStartInfo };
            
            proc.Start();
            proc.WaitForExit();
        }
    }
}
