using System.IO;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace Billsplitter.Models
{
    public class ImageUploader
    {
        private readonly IConfiguration _config;
        private readonly Account cloudinaryAccount;
        private readonly Cloudinary cloudinary;
        
        public ImageUploader(IConfiguration config)
        {
            _config = config;
            cloudinaryAccount = new Account(
                _config["Cloudinary:CloudName"],
                _config["Cloudinary:ApiKey"],
                _config["Cloudinary:ApiSecret"]);
            cloudinary = new Cloudinary(this.cloudinaryAccount);
        }

        public UploadResult upload(string fileName, Stream openReadStream)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, openReadStream),
            };
            return cloudinary.Upload(uploadParams);
        }
        
    }
}