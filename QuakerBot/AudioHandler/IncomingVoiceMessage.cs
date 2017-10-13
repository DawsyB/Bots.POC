using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using System.Text;

namespace EmergencyServicesBot.AudioHandler
{
  
    /// <summary>
    /// Represents an incoming voice message from facebook messenger, 
    /// where the voice data is in an MP4 file (the message contains a link to download it).
    /// </summary>
    public class IncomingVoiceMessage
    {
        #region Properties
        /// <summary>
        /// URL of the M4A file sent by user and stored on Skype servers.
        /// </summary>
        public Uri M4AFileUrl { get; private set; }

        /// <summary>
        /// Local filename of the M4A file after it has been downloaded from Skype.
        /// </summary>
        private string M4ALocalFileName { get; set; }

        /// <summary>
        /// Path to the folder on local disk containing the downloaded voice messages from Facebook.
        /// This is configured in Web.config using the FacebookDownloadedVoiceMessagesFolder key.
        /// The path in the Web.config will be relative to the site's root folder.
        /// </summary>
        public string VoiceMessageFolder { get; private set; }

        /// <summary>
        /// Content-type of the attachment (for debugging - it's not always MP4).
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// FileName of the attachment.
        /// </summary>
        public string FileName { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor that uses an MP4 file link that is 
        /// received in activity.Attachments by bot framework.
        /// </summary>
        /// <param name="MP4FileUrl">URL of the MP4 file sent by user and stored on facebook's servers.</param>
        public IncomingVoiceMessage(string MP4FileUrl)
        {
            if (string.IsNullOrWhiteSpace(MP4FileUrl))
            {
                throw new Exception("The MP4 file URL was empty.");
            }

            this.M4AFileUrl = new Uri(MP4FileUrl);
            this.VoiceMessageFolder = GetVoiceMessagesFolderFromWebConfig();
        }

        public async Task ConvertWavToText()
        {
            var converter = new AudioFileFormatConverter("https://skypeemergencybotstorage.blob.core.windows.net/skypeemergencybotcontainer/msg-643f2899-7d47-4e2a-9bd6-6009cd514f45-18-audioMessage.m4a", "https://skypeemergencybotstorage.blob.core.windows.net/skypeemergencybotcontainer/msg-643f2899-7d47-4e2a-9bd6-6009cd514f45-18-audioMessage.wav");
            var wav = converter.ConvertMP4ToWAV();
        }

        public async Task ConvertToWav()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A shortcut constructor that extracts the URL of the MP4 voice message file
        /// from the Activity object received by the controller in the Bot Framework.
        /// </summary>
        /// <param name="activity">The Activity object that contains an attachment of type video/mp4. If no attachment, throws an exception.</param>
        public IncomingVoiceMessage(IMessageActivity activity)
        {
            if (activity.Attachments.Count > 0)
            {
                var audioAttachment = activity.Attachments?.FirstOrDefault(a => a.ContentType.Equals("video/mp4") || a.ContentType.Contains("audio") || a.ContentType.Contains("video"));
                if (audioAttachment == null)
                {
                    throw new Exception("The message didn't have a voice attachment.");
                }
                else
                {
                    this.M4AFileUrl = new Uri(audioAttachment.ContentUrl);
                    this.VoiceMessageFolder = GetVoiceMessagesFolderFromWebConfig();
                    this.ContentType = audioAttachment.ContentType; //for debugging. Different devices send different content-types, e.g. audio/aac and video/mp4
                    this.FileName = audioAttachment.Name;
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Downloads the MP4 file containing the voice message from Facebook.
        /// </summary>
        /// <returns>The filename (without path) of the MP4 file stored on local disk.</returns>
        public async Task<Stream> SaveFile(Stream voiceMessageStream, StringBuilder messageLog)
        {
            string exceptionMessage = string.Empty;
            var filename = GetRandomFileName();
            filename=filename.Replace(".m4a", ".wav");
            exceptionMessage += "Got filename " + filename;
            Stream ret = new MemoryStream();

            var filenameWithPath = HttpContext.Current.Server.MapPath("/" + VoiceMessageFolder + @"\" + filename);
            exceptionMessage += "\n\t FilenamewithPath: " + filenameWithPath;
            try
            {

                // Parse the connection string and return a reference to the storage account.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureStorageConnectionString"));
                exceptionMessage += "\n\n\t Created StorageAccount: " + storageAccount.FileStorageUri;

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                exceptionMessage += "\n\n\t Created blobClient: ";

                // Retrieve a reference to a container.
                CloudBlobContainer container = blobClient.GetContainerReference("skypeemergencybotcontainer");
                exceptionMessage += "\n\n\t Got container: " + container.Uri;

                // Create the container if it doesn't already exist.
                container.CreateIfNotExists();
                exceptionMessage += "\n\n\t Create cotainer if doesnt exist: ";

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);
                exceptionMessage += "\n\n\t Retrieved reference to blockblob: " + blockBlob.Uri;

                //set public access
                //container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                // Create or overwrite the "myblob" blob with contents from a local file.
                exceptionMessage += "\n\n\t Retrieved content from: " + this.M4AFileUrl.ToString() + " and saving to blockblob";
                blockBlob.UploadFromStream(voiceMessageStream);

                M4ALocalFileName = filename;
                exceptionMessage += "\n\n\t Updated the Local file name. Done from this method";

                
                blockBlob.DownloadToStream(ret);
                exceptionMessage += "\n\n\t Downloading and returning the stream";
                messageLog.Append(exceptionMessage);
            }
            catch (Exception ex)
            {
                messageLog.Append(exceptionMessage);
                throw new Exception(ex.Message + " Inner Exception:"+ex.InnerException+" \n\n"+ exceptionMessage);                
            }
            messageLog.Append("Length of the stream from SaveFile Method is: "+ret.Length);
            return ret;
        }

        /// <summary>
        /// Removes the downloaded MP4 file from the local disk to clean up space.
        /// </summary>
        /// <returns>True if successfully removed, false otherwise.</returns>
        public bool RemoveFromDisk()
        {
            try
            {
                File.Delete(GetLocalPathAndFileName());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the full local path and filename to the downloaded MP4 voice message.
        /// </summary>
        /// <returns>E.g. D:\home\site\wwwroot\abc.mp4</returns>
        public string GetLocalPathAndFileName()
        {
            if (string.IsNullOrWhiteSpace(M4ALocalFileName))
            {
                throw new Exception("The voice message has not been downloaded yet.");
            }

            return VoiceMessageFolder + @"\" + M4ALocalFileName;
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Reads Web.config and returns the path to the folder which will store downloaded messages.
        /// The folder in the config must be relative to the site's root.
        /// </summary>
        /// <returns>Full path to the folder that will be used to store MP4 voice messages.</returns>
        private string GetVoiceMessagesFolderFromWebConfig()
        {
           // return Utils.GetHomeFolder() + WebConfigurationManager.AppSettings["FacebookDownloadedVoiceMessagesFolder"];
            return WebConfigurationManager.AppSettings["FacebookDownloadedVoiceMessagesFolder"];
        }

        /// <summary>
        /// Generates a random filename using a new GUID.
        /// </summary>
        /// <returns>A random file name in the format "msg-GUID.mp4".</returns>
        private string GetRandomFileName()
        {
            return "msg-" + Guid.NewGuid() + "-"+this.FileName;
        }
        #endregion
    }
}