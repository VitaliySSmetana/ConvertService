using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using convertservice.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace convertservice.Controllers
{
    [Route("convert")]
    public class ConvertController : ControllerBase
    {
        private readonly IBashProvider _bashProvider;
        private readonly ILogger<ConvertController> _logger;

        private readonly string _filesPath;

        public ConvertController(IHostEnvironment hostEnvironment, IBashProvider bashProvider, ILogger<ConvertController> logger)
        {
            _bashProvider = bashProvider;
            _logger = logger;

            _filesPath = Path.Combine(hostEnvironment.ContentRootPath, "uploads");

            if (!Directory.Exists(_filesPath))
            {
                Directory.CreateDirectory(_filesPath);
            }

            _logger.LogInformation($"Log from {nameof(ConvertController)}: content root: {hostEnvironment.ContentRootPath} // file folder: {_filesPath}");
        }

        [HttpPost]
        public async Task<IActionResult> ConvertAsync(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest();
            }

            var filesForDelete = new List<string>();
            var documentName = $"{Guid.NewGuid()}{file.FileName}";
            var pdfName = $"{Path.GetFileNameWithoutExtension(documentName)}.pdf";
            var documentPath = Path.Combine(_filesPath, documentName); 
            var pdfPath = Path.Combine(_filesPath, pdfName);

            try
            {
                byte[] buffer;

                await using (var stream = new FileStream(Path.Combine(_filesPath, documentName), FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    filesForDelete.Add(documentPath);
                }
                
                await _bashProvider.ConvertAsync(_filesPath, documentPath);

                if (!System.IO.File.Exists(pdfPath)) return NotFound();

                filesForDelete.Add(pdfPath);

                await using (var stream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read))
                {
                    var length = (int) stream.Length;
                    buffer = new byte[length];
                    int count;
                    var sum = 0;

                    while ((count = await stream.ReadAsync(buffer, sum, length - sum)) > 0)
                    {
                        sum += count;
                    }
                }

                return File(buffer, "application/pdf", pdfName);
            }
            finally
            {
                foreach (var fileInfo in filesForDelete.Select(filePath => new FileInfo(filePath)).Where(fileInfo => fileInfo.Exists))
                {
                    fileInfo.Delete();
                }
            }
        }
    }
}
