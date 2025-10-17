using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using ChunkedUploadWebApi.Service;
using ChunkedUploadWebApi.Data;
using ChunkedUploadWebApi.Model;
using Swashbuckle.Swagger.Annotations;

using Models;
using System.Security.Cryptography;
using ViewGeneratorBase;

namespace ChunkedUploadWebApi.Controllers
{
    /// <summary>
    /// File Management Controller
    /// </summary>
    /// <remarks>Manages upload sessions</remarks>
    [Route("api/file")]
    public class FileController : Controller
    {
        private static UploadService uploadService = new UploadService(new LocalFileSystemRepository());

        public FileController()
        {

        }

        /// <summary>
        /// Create an upload session
        /// </summary>
        /// <remarks>creates a new upload session</remarks>
        /// <param name="userId">User ID</param>
        /// <param name="sessionParams">Session creation params</param>
        [HttpPost("create")]
        [Produces("application/json")]
        [SwaggerResponse(201, Type = typeof(SessionCreationStatusResponse))]
        [SwaggerResponse(500)]
        public SessionCreationStatusResponse StartSession(
                         [FromBody] CreateSessionParams sessionParams)
        {
            long userId = 0;
            Session session = uploadService.createSession(userId, sessionParams.FileName,
                                                          sessionParams.ChunkSize,
                                                          sessionParams.TotalSize);
            // Build the final filename with optional GUID prefix and directory
            string finalFileName = sessionParams.FileName;

            // Add GUID prefix if requested
            if (sessionParams.dontChangeFileNameWithGUID)
            {
                finalFileName = $"{Guid.NewGuid()}-{finalFileName}";
            }

            // Add directory path if specified
            string relativePath = finalFileName;
            if (!string.IsNullOrEmpty(sessionParams.dir))
            {
                relativePath = Path.Combine(sessionParams.dir.Trim('/').Trim('\\'), finalFileName).Replace("\\", "/");
            }

            // Build full path to file
            string filename = Path.Combine(JsonBase64File.UploadFolderPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            
            if (!System.IO.File.Exists(filename))
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filename));
                using (FileStream fs = System.IO.File.Create(filename))
                {
                    //file.CopyTo(fs);
                    fs.Flush();
                }
            }
            else
            {
                string checksum = "";
                using (var md5 = MD5.Create())
                {
                    using (var stream = System.IO.File.OpenRead(filename))
                    {
                        checksum = BitConverter.ToString(md5.ComputeHash(stream));
                    }
                }
                if (checksum == sessionParams.checkSum)
                    return new SessionCreationStatusResponse()
                    {
                        SessionId = relativePath,
                        FileName = relativePath,
                        done = true,
                        saveBefor = true
                    };
                if (sessionParams.forceWrite) {
                    System.IO.File.Delete(filename);
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        //file.CopyTo(fs);
                        fs.Flush();
                        fs.Close();
                    }
                }
                else
                {
                    return new SessionCreationStatusResponse()
                    {
                        done = false,
                        saveBefor = true,
                        text="file with this path with different checksum exists"
                    };
                }
            }
            return new SessionCreationStatusResponse()
            {
                SessionId = relativePath,
                FileName = relativePath,
                done=true
            };
            //return SessionCreationStatusResponse.fromSession(session);
        }

        /// <summary>
        /// Uploads a file chunk
        /// </summary>
        /// <remarks>uploads a file chunk</remarks>
        /// <param name="userId">User ID</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="chunkNumber">Chunk number (starts from 1)</param>
        /// <param name="inputFile">File chunk content</param>
        [HttpPut("upload")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        [SwaggerResponse(200, Description = "Block upload successfully")]
        [SwaggerResponse(202, Description = "Server busy during that particular upload. Try again")]
        [SwaggerResponse(410, Description = "Session timeout")]
        [SwaggerResponse(500, Description = "Internal server error")]
        public async Task<ActionResult<UploadResult>> UploadFileChunk(

                                        [FromForm, Required] UploadFileChunk data
            //                            [FromForm] IFormFile inputFile
            )
        {
            long userId = 0;
            
            if (String.IsNullOrWhiteSpace(data.FileName))
                return badRequest("Session ID is missing");

            //if (chunkNumber < 1)
            //    return badRequest("Invalid chunk number");

            // due to a bug, inputFile comes null from Mvc
            // however, I want to test the code and have to pass it to the UploadFileChunk function...
            IFormFile file = (data.File ?? Request.Form.Files.First());
            
            //uploadService.persistBlock(sessionId, userId, chunkNumber, ToByteArray(file.OpenReadStream()));

            // Build full path to file using proper path combination
            string filename = Path.Combine(JsonBase64File.UploadFolderPath, data.FileName.Replace("/", Path.DirectorySeparatorChar.ToString()));

            using (FileStream fs = System.IO.File.Open(filename, FileMode.Append))
            {
                //fs.WriteAsync(file.)
                file.CopyTo(fs);
                fs.Flush();
            }

            return new UploadResult()
            {
                Uploaded = true
            };
        }

        /// <summary>
        /// Gets the status of a single upload
        /// </summary>
        /// <remarks>gets the status of a single upload</remarks>
        /// <param name="sessionId">Session ID</param>
        [HttpGet("upload/{sessionId}")]
        [Produces("application/json")]
        [SwaggerResponse(404, Description = "Session not found")]
        [SwaggerResponse(500, Description = "Internal server error")]
        [SwaggerResponse(200, type: typeof(UploadStatusResponse))]
        public UploadStatusResponse GetUploadStatus([FromRoute, Required] string sessionId)
        {
            return UploadStatusResponse.fromSession(uploadService.getSession(sessionId));
        }

        /// <summary>
        /// Gets the status of all uploads
        /// </summary>
        /// <remarks>gets the status of all uploads</remarks>
        [HttpGet("uploads")]
        [Produces("application/json")]
        [SwaggerResponse(404, Description = "Session not found")]
        [SwaggerResponse(200, type:typeof(List<UploadStatusResponse>))]
        public List<UploadStatusResponse> GetAllUploadStatus()
        {
            return UploadStatusResponse.fromSessionList(uploadService.getAllSessions());
        }

        /// <summary>
        /// Downloads a previously uploaded file
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <remarks>downloads a previously uploaded file</remarks>
        [HttpGet("download/{sessionId}")]
        [Produces("multipart/form-data")]
        [SwaggerResponse(200, Description = "OK")]
        [SwaggerResponse(404, Description = "Session not found")]
        [SwaggerResponse(500, Description = "Internal server error")]
        public void DownloadFile([FromRoute, Required] string sessionId)
        {
            Session session = uploadService.getSession(sessionId);

            var response = targetResponse ?? Response;

            response.ContentType = "application/octet-stream";
            response.ContentLength = session.FileInfo.FileSize;
            response.Headers["Content-Disposition"] = "attachment; fileName=" + session.FileInfo.FileName;

            uploadService.WriteToStream(targetOutputStream ?? Response.Body, session);
        }

        private byte[] ToByteArray(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private JsonResult badRequest(string message) {
            var result = new JsonResult("{'message': '" + message + "' }");
            result.StatusCode = 400;
            return result;
        }

        Stream targetOutputStream = null;
        // intended for integration tests only
        [ApiExplorerSettings(IgnoreApi=true)]
        public void SetOuputStream(Stream replacementStream) {
            this.targetOutputStream = replacementStream;
        }

        HttpResponse targetResponse = null;
        [ApiExplorerSettings(IgnoreApi=true)]
        public void SetTargetResponse(HttpResponse replacementResponse) {
            this.targetResponse = replacementResponse;
        }

        /// <summary>
        /// Browse files and directories
        /// </summary>
        /// <param name="path">Path to browse (optional)</param>
        /// <param name="includeHidden">Whether to include hidden files</param>
        [HttpGet("browse")]
        [Produces("application/json")]
        [SwaggerResponse(200, Type = typeof(FileBrowserResponse))]
        [SwaggerResponse(400)]
        [SwaggerResponse(500)]
        public ActionResult<FileBrowserResponse> BrowseFiles(
            [FromQuery] string path = null,
            [FromQuery] bool includeHidden = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string search = null)
        {
            try
            {
                // Use the upload folder as root if no path specified
                string basePath = JsonBase64File.UploadFolderPath;
                string currentPath = string.IsNullOrEmpty(path) ? basePath : Path.Combine(basePath, path.TrimStart('/').TrimStart('\\'));
        
                // Ensure the path is within the upload folder for security
                if (!currentPath.StartsWith(basePath))
                {
                    return BadRequest(new FileBrowserResponse
                    {
                        done = false,
                        text = "Access denied: Path outside allowed directory"
                    });
                }
        
                // Check if directory exists
                if (!Directory.Exists(currentPath))
                {
                    return NotFound(new FileBrowserResponse
                    {
                        done = false,
                        text = "Directory not found"
                    });
                }
        
                var response = new FileBrowserResponse
                {
                    done = true,
                    CurrentPath = path ?? "",
                    Items = new List<Models.FileBrowserItem>()
                };
        
                // Set parent path
                if (!string.IsNullOrEmpty(path))
                {
                    string parentPath = Path.GetDirectoryName(path.TrimEnd('/').TrimEnd('\\'));
                    response.ParentPath = string.IsNullOrEmpty(parentPath) ? "" : parentPath;
                }
        
                // Get directories
                var directories = Directory.GetDirectories(currentPath);
                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (!includeHidden && dirInfo.Name.StartsWith("."))
                        continue;
        
                    response.Items.Add(new Models.FileBrowserItem
                    {
                        Name = dirInfo.Name,
                        Path = Path.Combine(path ?? "", dirInfo.Name).Replace("\\", "/"),
                        IsDirectory = true,
                        Size = 0,
                        LastModified = dirInfo.LastWriteTime,
                        Extension = ""
                    });
                }
        
                // Get files
                var files = Directory.GetFiles(currentPath);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (!includeHidden && fileInfo.Name.StartsWith("."))
                        continue;
        
                    response.Items.Add(new Models.FileBrowserItem
                    {
                        Name = fileInfo.Name,
                        Path = Path.Combine(path ?? "", fileInfo.Name).Replace("\\", "/"),
                        IsDirectory = false,
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        Extension = fileInfo.Extension.TrimStart('.')
                    });
                }

                // Filter by search term if provided
                var allItems = response.Items
                    .Where(x => string.IsNullOrWhiteSpace(search) ||
                        (x.Name != null && x.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderByDescending(x => x.IsDirectory)
                    .ThenBy(x => x.Name)
                    .ToList();
        
                response.TotalCount = allItems.Count;
        
                // Pagination
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 50;
                response.Items = allItems
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
        
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new FileBrowserResponse
                {
                    done = false,
                    text = $"Error browsing files: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get file information
        /// </summary>
        /// <param name="path">File path</param>
        [HttpGet("info")]
        [Produces("application/json")]
        [SwaggerResponse(200, Type = typeof(FileBrowserItem))]
        [SwaggerResponse(404)]
        [SwaggerResponse(500)]
        public ActionResult<FileBrowserItem> GetFileInfo([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return BadRequest("Path is required");
                }

                string basePath = JsonBase64File.UploadFolderPath;
                string fullPath = Path.Combine(basePath, path.TrimStart('/').TrimStart('\\'));

                // Ensure the path is within the upload folder for security
                if (!fullPath.StartsWith(basePath))
                {
                    return BadRequest("Access denied: Path outside allowed directory");
                }

                if (Directory.Exists(fullPath))
                {
                    var dirInfo = new DirectoryInfo(fullPath);
                    return new Models.FileBrowserItem
                    {
                        Name = dirInfo.Name,
                        Path = path,
                        IsDirectory = true,
                        Size = 0,
                        LastModified = dirInfo.LastWriteTime,
                        Extension = ""
                    };
                }
                else if (System.IO.File.Exists(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    return new Models.FileBrowserItem
                    {
                        Name = fileInfo.Name,
                        Path = path,
                        IsDirectory = false,
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        Extension = fileInfo.Extension.TrimStart('.')
                    };
                }
                else
                {
                    return NotFound("File or directory not found");
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error getting file info: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new directory
        /// </summary>
        /// <param name="path">Parent directory path</param>
        /// <param name="name">Directory name</param>
        [HttpPost("createdir")]
        [Produces("application/json")]
        [SwaggerResponse(200, Type = typeof(FileBrowserResponse))]
        [SwaggerResponse(400)]
        [SwaggerResponse(500)]
        public ActionResult<FileBrowserResponse> CreateDirectory([FromQuery] string path, [FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new FileBrowserResponse
                    {
                        done = false,
                        text = "Directory name is required"
                    });
                }

                // Validate directory name (no special characters that could be dangerous)
                if (name.Contains("..") || name.Contains("/") || name.Contains("\\") ||
                    name.Contains("<") || name.Contains(">") || name.Contains(":") ||
                    name.Contains("\"") || name.Contains("|") || name.Contains("?") || name.Contains("*"))
                {
                    return BadRequest(new FileBrowserResponse
                    {
                        done = false,
                        text = "Directory name contains invalid characters"
                    });
                }

                string basePath = JsonBase64File.UploadFolderPath;
                string parentPath = string.IsNullOrEmpty(path) ? basePath : Path.Combine(basePath, path.TrimStart('/').TrimStart('\\'));

                // Ensure the parent path is within the upload folder for security
                if (!parentPath.StartsWith(basePath))
                {
                    return BadRequest(new FileBrowserResponse
                    {
                        done = false,
                        text = "Access denied: Path outside allowed directory"
                    });
                }

                // Create the full path for the new directory
                string newDirPath = Path.Combine(parentPath, name);

                // Ensure the new directory path is still within the upload folder
                if (!newDirPath.StartsWith(basePath))
                {
                    return BadRequest(new FileBrowserResponse
                    {
                        done = false,
                        text = "Access denied: New directory path outside allowed directory"
                    });
                }

                // Check if directory already exists
                if (Directory.Exists(newDirPath))
                {
                    return BadRequest(new FileBrowserResponse
                    {
                        done = false,
                        text = "Directory already exists"
                    });
                }

                // Create the directory
                Directory.CreateDirectory(newDirPath);

                // Return updated file list
                return BrowseFiles(path, false);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new FileBrowserResponse
                {
                    done = false,
                    text = $"Error creating directory: {ex.Message}"
                });
            }
        }
    }
}
