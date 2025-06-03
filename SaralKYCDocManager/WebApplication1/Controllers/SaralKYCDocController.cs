using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SaralKYCDocController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SaralKYCDocController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        [Route("UploadFile")]
        public IActionResult UploadFile([FromBody] FileItem file)
        {
            if (file == null)
                return BadRequest("No files provided for zipping.");
            string folderPath = Path.Combine("KYCDocFiles/" + file.FileName + "/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var fileName = file.FileName;
            fileName = "KYCDocFiles/" + file.FileName + ".zip";
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }
            System.IO.File.WriteAllBytes(fileName, file.Content);
            ZipFile.ExtractToDirectory(fileName, folderPath, Encoding.UTF8, true);
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }
            return Ok();
        }

        [HttpGet]
        [Route("DownloadFile")]
        public FileItem DownloadFile(string fileName)
        {
            byte[] result = Array.Empty<byte>();
            string folderPath = Path.Combine("KYCDocFiles/");
            folderPath = folderPath + fileName;

            if (System.IO.Directory.Exists(folderPath))
            {
                var fn = "KYCDocFiles/" + fileName + ".zip";
                if (System.IO.File.Exists(fn))
                {
                    System.IO.File.Delete(fn);
                }
                ZipFile.CreateFromDirectory(folderPath, fn);
                result = System.IO.File.ReadAllBytes(fn);
                System.IO.File.Delete(fn);
            }
            
            return new FileItem { Content = result, FileName = fileName };
        }

        [HttpPost]
        [Route("UploadCashFlowFile")]
        public IActionResult UploadCashFlowFile([FromBody] FileItem file)
        {
            if (file == null)
                return BadRequest("No files provided for zipping.");
            string folderPath = Path.Combine("CashFlowFiles/" + file.FileName + "/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var fileName = file.FileName;
            fileName = "CashFlowFiles/" + file.FileName + ".zip";
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }
            System.IO.File.WriteAllBytes(fileName, file.Content);
            ZipFile.ExtractToDirectory(fileName, folderPath, Encoding.UTF8, true);
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }
            return Ok();
        }

        [HttpGet]
        [Route("DownloadCashFlowFile")]
        public FileItem DownloadCashFlowFile(string fileName)
        {
            byte[] result = Array.Empty<byte>();
            string folderPath = Path.Combine("CashFlowFiles/");
            folderPath = folderPath + fileName;

            if (System.IO.Directory.Exists(folderPath))
            {
                var fn = "CashFlowFiles/" + fileName + ".zip";
                if (System.IO.File.Exists(fn))
                {
                    System.IO.File.Delete(fn);
                }
                ZipFile.CreateFromDirectory(folderPath, fn);
                result = System.IO.File.ReadAllBytes(fn);
                System.IO.File.Delete(fn);
            }

            return new FileItem { Content = result, FileName = fileName };
        }
    }

    public class FileItem
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
    }

    public class ZipInfo
    {
        List<FileItem> Files { get; set; }
        public string ZipName { get; set; }
    }
}
