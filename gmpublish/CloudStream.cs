using SteamKit2;
using SteamKit2.Unified.Internal;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GMPublish
{
    public static class CloudStream
    {
        public static async Task DeleteFile(string cloudFileName, uint appID, SteamClient steamClient)
        {
            var steamUnifiedMessages = steamClient.GetHandler<SteamUnifiedMessages>();
            var cloudService = steamUnifiedMessages.CreateService<ICloud>();

            var request = new CCloud_Delete_Request
            {
                appid = appID,
                filename = cloudFileName,
            };
            await cloudService.SendMessage(cloud => cloud.Delete(request));
        }

        public static async Task<bool> UploadFile(string cloudFileName, uint appID, SteamClient steamClient)
        {
            return await UploadFile(cloudFileName, appID, steamClient, cloudFileName);
        }

        public static async Task<bool> UploadFile(string cloudFileName, uint appID, SteamClient steamClient, string filePath)
        {
            var fileStream = File.OpenRead(filePath);
            byte[] hash;
            using (var sha = new SHA1Managed())
            {
                hash = sha.ComputeHash(fileStream);
            }
            return await UploadStream(cloudFileName, appID, hash, fileStream.Length, steamClient, fileStream);
        }

        public static async Task<bool> UploadStream(string fileName, uint appID, byte[] SHAHash, long fileSize, SteamClient steamClient, Stream stream)
        {
            var steamUnifiedMessages = steamClient.GetHandler<SteamUnifiedMessages>();
            var cloudService = steamUnifiedMessages.CreateService<ICloud>();

            var uploadRequest = new CCloud_ClientBeginFileUpload_Request
            {
                appid = appID,
                file_size = (uint)fileSize,
                raw_file_size = (uint)fileSize,
                file_sha = SHAHash,
                time_stamp = DateUtils.DateTimeToUnixTime(DateTime.Now),
                filename = fileName,
                platforms_to_sync = 4294967295,
                cell_id = steamClient.CellID.Value,
                can_encrypt = true,
                is_shared_file = false,
            };

            var callback = await cloudService.SendMessage(api => api.ClientBeginFileUpload(uploadRequest));
            var commitRequest = new CCloud_ClientCommitFileUpload_Request { appid = appID, file_sha = SHAHash, transfer_succeeded = true, filename = fileName };

            if (callback.Result != EResult.OK)
            {
                commitRequest.transfer_succeeded = false;
                var failedCallback = await cloudService.SendMessage(api => api.ClientCommitFileUpload(commitRequest));
                return false;
            }

            var response = callback.GetDeserializedResponse<CCloud_ClientBeginFileUpload_Response>();
            foreach (var block in response.block_requests)
            {
                Console.WriteLine("{0} -> {1} to {2} {3}", block.block_offset, block.block_length, block.url_host, block.url_path);
                using (var webClient = new WebClient())
                {
                    string url = (block.use_https ? "https" : "http") + "://" + block.url_host + block.url_path;

                    byte[] slice = new byte[block.block_length];
                    stream.Position = (long)block.block_offset;
                    await stream.ReadAsync(slice, 0, (int)block.block_length);

                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "PUT";

                    foreach (var header in block.request_headers)
                    {
                        switch (header.name.ToLower())
                        {
                            case "host": break;
                            case "content-type":
                                request.ContentType = header.value;
                                break;
                            case "content-length":
                                request.ContentLength = long.Parse(header.value);
                                break;
                            default:
                                request.Headers.Add(header.name, header.value);
                                break;
                        }
                    }

                    var webStream = await request.GetRequestStreamAsync();
                    webStream.Write(slice, 0, slice.Length);

                    var webResponse = (HttpWebResponse)(await request.GetResponseAsync());
                    var transferReport = new CCloud_ExternalStorageTransferReport_Notification
                    {
                        bytes_actual = block.block_length,
                        bytes_expected = block.block_length,
                        cellid = steamClient.CellID.Value,
                        host = block.url_host,
                        path = block.url_path,
                        duration_ms = 3000,
                        http_status_code = (uint)webResponse.StatusCode,
                        is_upload = true,
                        success = true,
                    };

                    cloudService.SendMessage(api => api.ExternalStorageTransferReport(transferReport));
                }
            }

            var commitJob = cloudService.SendMessage(api => api.ClientCommitFileUpload(commitRequest));
            commitJob.Timeout = TimeSpan.FromMinutes(2);
            var callbackCommit = await commitJob;
            if (callbackCommit.Result != EResult.OK) return false;
            var responseCommit = callback.GetDeserializedResponse<CCloud_ClientCommitFileUpload_Response>();
            if (responseCommit == null) return false;
            return true;//?????responseCommit.file_committed;
        }
    }
}
