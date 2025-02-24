using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Business.Services.CloudServices
{
    public interface ICloudServices
    {
        public Task<List<string>> UploadFile(List<IFormFile> files, string filePath);
        public Task<string> UploadSingleFile(IFormFile files, string filePath);
        public Task DeleteFilesInPathAsync(string path);
    }
}
