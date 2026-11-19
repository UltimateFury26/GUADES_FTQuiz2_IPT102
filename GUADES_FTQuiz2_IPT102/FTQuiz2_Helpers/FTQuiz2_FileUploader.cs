using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Helpers
{
    /// <summary>Outcome of FTQuiz2_FileUploader.SaveImage: either the saved path or a message for the Admin.</summary>
    public sealed class FTQuiz2_UploadResult
    {
        public bool Success { get; private set; }

        /// <summary>Path relative to the site root, e.g. "FTQuiz2_Uploads/3f2a....jpg". Empty on failure.</summary>
        public string RelativePath { get; private set; }

        /// <summary>Friendly explanation when Success is false.</summary>
        public string Error { get; private set; }

        public static FTQuiz2_UploadResult Ok(string relativePath)
        {
            return new FTQuiz2_UploadResult { Success = true, RelativePath = relativePath, Error = string.Empty };
        }

        public static FTQuiz2_UploadResult Fail(string error)
        {
            return new FTQuiz2_UploadResult { Success = false, RelativePath = string.Empty, Error = error };
        }
    }

    /// <summary>
    /// Port of source Helpers/FileUploader. Accepts a file only when its extension, its content type
    /// AND its first bytes all say JPG, PNG or GIF, and it is at most 5 MB. It is saved under a
    /// generated name, never the original one. Seeded images in FTQuiz2_Media are never deleted here.
    /// Folder and size limit come from FTQuiz2_AppSettings (FTQuiz2_Uploads, 5 MB).
    /// </summary>
    public static class FTQuiz2_FileUploader
    {
        private const string KindJpg = "jpg";
        private const string KindPng = "png";
        private const string KindGif = "gif";

        private const string NoFileMessage = "Choose an image to upload.";
        private const string TooLargeMessage = "That image is larger than 5 MB.";
        private const string WrongTypeMessage = "Use a JPG, PNG or GIF image.";
        private const string NotAnImageMessage = "That file doesn't look like a real JPG, PNG or GIF image.";
        private const string SaveFailedMessage = "The image couldn't be saved on the server. Try again.";

        /// <summary>Checks and saves one uploaded image. Never throws for a bad file; read Success and Error.</summary>
        public static FTQuiz2_UploadResult SaveImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return FTQuiz2_UploadResult.Fail(NoFileMessage);
            }

            if (file.ContentLength > FTQuiz2_AppSettings.MaxUploadBytes)
            {
                return FTQuiz2_UploadResult.Fail(TooLargeMessage);
            }

            string kind = KindForExtension(Path.GetExtension(file.FileName ?? string.Empty));

            if (kind == null || !ContentTypeMatches(kind, file.ContentType))
            {
                return FTQuiz2_UploadResult.Fail(WrongTypeMessage);
            }

            if (!SignatureMatches(kind, file.InputStream))
            {
                return FTQuiz2_UploadResult.Fail(NotAnImageMessage);
            }

            try
            {
                string uploadFolder = FTQuiz2_AppSettings.UploadFolder;
                string folder = HostingEnvironment.MapPath("~/" + uploadFolder);
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString("N") + "." + kind;
                file.SaveAs(Path.Combine(folder, fileName));

                return FTQuiz2_UploadResult.Ok(uploadFolder + "/" + fileName);
            }
            catch (IOException)
            {
                return FTQuiz2_UploadResult.Fail(SaveFailedMessage);
            }
            catch (UnauthorizedAccessException)
            {
                return FTQuiz2_UploadResult.Fail(SaveFailedMessage);
            }
            catch (HttpException)
            {
                return FTQuiz2_UploadResult.Fail(SaveFailedMessage);
            }
        }

        /// <summary>
        /// Deletes a file previously saved by SaveImage. Does nothing for an empty path, for anything
        /// outside the uploads folder (so the seeded FTQuiz2_Media images are safe) or for a file that is gone.
        /// </summary>
        public static void DeleteIfUploaded(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            string uploadFolder = FTQuiz2_AppSettings.UploadFolder;
            string clean = relativePath.Trim().Replace('\\', '/');
            string prefix = uploadFolder + "/";

            if (!clean.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string fileName = clean.Substring(prefix.Length);

            if (fileName.Length == 0 || fileName.Contains("..") || fileName != Path.GetFileName(fileName))
            {
                return;
            }

            try
            {
                string physicalPath = Path.Combine(HostingEnvironment.MapPath("~/" + uploadFolder), fileName);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
            catch (IOException)
            {
                // A file that cannot be removed right now is left behind; it is never shown anywhere.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        /// <summary>
        /// Deletes an uploaded file only when NO business record still points at it (business profile
        /// hero/about image, owner photos, product images). Call it AFTER the database change succeeded.
        /// If the database cannot be checked the file is kept: a leftover file is harmless, a deleted
        /// image that is still shown is not. Seeded images in FTQuiz2_Media are never touched.
        /// </summary>
        public static void DeleteIfUnreferenced(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) { return; }

            try
            {
                if (IsReferenced(relativePath)) { return; }
            }
            catch (Exception)
            {
                return;
            }

            DeleteIfUploaded(relativePath);
        }

        private static bool IsReferenced(string relativePath)
        {
            string target = NormalizePath(relativePath);

            using (var db = new FTQuiz2_Db())
            {
                var business = new FTQuiz2_BusinessRepository(db);

                FTQuiz2_BusinessProfile profile = business.GetBusinessProfile();
                if (profile != null
                    && (NormalizePath(profile.HeroImagePath) == target || NormalizePath(profile.AboutImagePath) == target))
                {
                    return true;
                }

                foreach (FTQuiz2_OwnerInfo owner in business.GetOwners())
                {
                    if (NormalizePath(owner.PhotoPath) == target) { return true; }
                }

                foreach (FTQuiz2_Product product in new FTQuiz2_ProductRepository(db).GetProducts())
                {
                    if (NormalizePath(product.ImagePath) == target) { return true; }
                }
            }

            return false;
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Trim().Replace('\\', '/').TrimStart('~', '/').ToLowerInvariant();
        }

        /// <summary>Maps an extension to "jpg", "png" or "gif", or null when it is not an allowed image type.</summary>
        private static string KindForExtension(string extension)
        {
            switch ((extension ?? string.Empty).ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return KindJpg;

                case ".png":
                    return KindPng;

                case ".gif":
                    return KindGif;

                default:
                    return null;
            }
        }

        private static bool ContentTypeMatches(string kind, string contentType)
        {
            string type = (contentType ?? string.Empty).Trim().ToLowerInvariant();

            switch (kind)
            {
                case KindJpg:
                    return type == "image/jpeg" || type == "image/pjpeg";

                case KindPng:
                    return type == "image/png" || type == "image/x-png";

                default:
                    return type == "image/gif";
            }
        }

        /// <summary>Compares the first bytes of the file with the signature of the claimed image type.</summary>
        private static bool SignatureMatches(string kind, Stream stream)
        {
            byte[] header = new byte[8];

            stream.Position = 0;
            int read = stream.Read(header, 0, header.Length);
            stream.Position = 0;

            switch (kind)
            {
                case KindJpg:
                    return read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

                case KindPng:
                    return read >= 8
                        && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                        && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;

                default:
                    return read >= 6
                        && header[0] == 'G' && header[1] == 'I' && header[2] == 'F' && header[3] == '8'
                        && (header[4] == '7' || header[4] == '9') && header[5] == 'a';
            }
        }
    }
}
