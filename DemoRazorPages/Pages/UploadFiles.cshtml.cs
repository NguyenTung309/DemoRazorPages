using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DemoRazorPages.Pages

{
    public class UploadFilesModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public UploadFilesModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [Required(ErrorMessage = "Please choose at least one file.")]
        [DataType(DataType.Upload)]
        //[FileExtensions(Extensions = "png,jpg,jpeg,gif")]
        [Display(Name = "Choose file(s) to upload")]
        [BindProperty]
        public IFormFile[] FileUploads { get; set; }

        public string Message { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            //if (FileUploads != null)
            //{
            //    foreach (var FileUpload in FileUploads)
            //    {
            //        var file = Path.Combine(_environment.ContentRootPath,
            //            "Images", FileUpload.FileName);
            //        using (var fileStream = new FileStream(file, FileMode.Create))
            //        {
            //            await FileUpload.CopyToAsync(fileStream);
            //        }
            //    }
            //}
            if (FileUploads == null || FileUploads.Length == 0)
            {
                Message = "No files selected.";
                return Page();
            }

            try
            {
                // Save to wwwroot/Images so files are web-accessible.
                // Nếu bạn muốn lưu ở folder khác, thay _environment.WebRootPath bằng _environment.ContentRootPath
                var imagesFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "Images");

                // Tạo folder nếu chưa có
                Directory.CreateDirectory(imagesFolder);

                foreach (var fileUpload in FileUploads)
                {
                    if (fileUpload == null || fileUpload.Length == 0)
                        continue;

                    // Lấy tên file an toàn (tránh path traversal)
                    var originalFileName = Path.GetFileName(fileUpload.FileName);

                    // Option: giới hạn extension (basic check)
                    var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
                    var allowed = new[] { ".png", ".jpg", ".jpeg", ".gif" };
                    if (Array.IndexOf(allowed, ext) < 0)
                    {
                        // bỏ qua file không hợp lệ hoặc bạn có thể thêm vào danh sách lỗi
                        continue;
                    }

                    // Sinh tên file mới để tránh trùng lặp
                    var newFileName = $"{Guid.NewGuid()}{ext}";

                    var fullPath = Path.Combine(imagesFolder, newFileName);

                    // Ghi file
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(fileStream);
                    }
                }

                Message = "Upload completed successfully.";
            }
            catch (Exception ex)
            {
                // Log (nên dùng ILogger trong thực tế); ở đây trả về message cho dev
                Message = "Error during upload: " + ex.Message;
            }

            // Trả Page để hiển thị Message (bạn có thể redirect để tránh re-post)
            return Page();
        }
    }
}

