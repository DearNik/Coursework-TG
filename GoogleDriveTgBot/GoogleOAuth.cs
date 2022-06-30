using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Google.Apis.Drive.v3.Data;

namespace GoogleDriveTgBot
{
    public class GoogleOAuth
    {
        static string[] Scopes = { "https://www.googleapis.com/auth/drive" };
        static string ApplicationName = "GoogleDriveExp";
        public void GoogleDriveOAuth(out IList<Google.Apis.Drive.v3.Data.File> files)
        {
            files = null;
            try
            {


                FilesResource.ListRequest listRequest = GetService().Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name)";

                // List files.
                files = listRequest.Execute()
                    .Files;

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public DriveService GetService()
        {
            UserCredential credential;

            using (var stream =
                   new FileStream("credentials.json", FileMode.Open, FileAccess.Read)) 
            {
                string tokenPath = "D:\\Институт\\Прога\\GoogleDriveTgBot\\token.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user,permissions",
                    CancellationToken.None,
                    new FileDataStore(tokenPath, true)).Result;
            }
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Drive Exp",
            });
            return service;
        }
        public void DeleteFile(string fileId)

        {
            var service = GetService();
            try
            {
                service.Files.Delete(fileId).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка удаления : " + e.Message);
            }
        }
        public void OpenFile(string fileId)

        {
            var service = GetService();
            try
            {
                service.Files.Delete(fileId).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка удаления : " + e.Message);
            }
        }
        public Permission InsertPermissionAnyone(DriveService service,String fileId)
        {
            Permission permission = new Permission();
            permission.Role = "reader";
            permission.Type = "anyone";

            try
            {
                return service.Permissions.Create(permission, fileId ).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
            return null;
        }
        private static void OverwriteDriveFileNames(DriveService driveService)
        {
            string fileId = "myId";
            Google.Apis.Drive.v3.Data.File file = driveService.Files.Get(fileId).Execute();
            file.Id = null;
            FilesResource.UpdateRequest request = driveService.Files.Update(file, fileId);
            request.Execute();
        }
    }
}
