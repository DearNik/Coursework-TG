/*using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;

namespace GoogleDriveTgBot
{
    public class GoogleDriveCommands
    {
        public class GoogleDriveFilesRepository
        {
            IList<Google.Apis.Drive.v3.Data.File> files = null;
            GoogleOAuth newgd = new GoogleOAuth();
            newgd.GoogleDriveOAuth(out files);
            //Delete file from the Google drive
            public static void DeleteFile(GoogleDriveFiles files)
            {
                DriveService service = GetService();
                try
                {
                    // Initial validation.
                    if (service == null)
                        throw new ArgumentNullException("service");

                    if (files == null)
                        throw new ArgumentNullException(files.Id);

                    // Make the request.
                    service.Files.Delete(files.Id).Execute();
                }
                catch (Exception ex)
                {
                    throw new Exception("Request Files.Delete failed.", ex);
                }
            }
        }
    }
}
*/