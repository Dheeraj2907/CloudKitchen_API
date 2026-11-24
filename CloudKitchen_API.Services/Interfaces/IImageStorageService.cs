using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Interfaces
{
    public interface IImageStorageService
    {
        /// <summary>
        /// Saves the uploaded image and returns its public URL.
        /// <paramref name="folder"/> is a logical bucket, e.g. "menu" or "category".
        /// </summary>
        Task<string> SaveAsync(Stream fileStream, string originalFileName, string contentType, string folder);

        /// <summary>Deletes a previously saved image by its public URL. No-op if missing.</summary>
        Task DeleteAsync(string publicUrl);
    }
}
