namespace EmergencyServicesBot
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using Microsoft.Bing.Speech;
    using Newtonsoft.Json;
    using Microsoft.Bot.Connector;
    using NAudio;
    using NAudio.Wave;
    using AudioHandler;
    using System.Text;
    

    public class MicrosoftCognitiveSpeechService
    {
        private readonly string subscriptionKey;
        private readonly string speechRecognitionUri;

        public MicrosoftCognitiveSpeechService()
        {
            this.DefaultLocale = "en-US";
            this.subscriptionKey = WebConfigurationManager.AppSettings["MicrosoftSpeechApiKey"];
            this.speechRecognitionUri = Uri.UnescapeDataString(WebConfigurationManager.AppSettings["MicrosoftSpeechRecognitionUri"]);
        }

        public string DefaultLocale { get; set; }

        /// <summary>
        /// Gets text from an audio stream.
        /// </summary>
        /// <param name="audiostream"></param>
        /// <returns>Transcribed text. </returns>
        public async Task<string> GetTextFromAudioAsync(Stream audiostream, StringBuilder messageLog)
        {
            string debugMsg = string.Empty;
            var requestUri = this.speechRecognitionUri + Guid.NewGuid();
            messageLog.Append("\n\n **Running GetTextFromAudioAsync method** \n\n Got the URI " + requestUri);
            using (var client = new HttpClient())
            {
                var token = Authentication.Instance.GetAccessToken();                

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                
                try
                {
                    using (var binaryContent = new ByteArrayContent(StreamToBytes(audiostream)))
                    {
                       
                        binaryContent.Headers.TryAddWithoutValidation("content-type", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                        binaryContent.Headers.TryAddWithoutValidation("Transfer-Encoding", "chunked");

                        messageLog.Append("\n\n Fetching Response ");
                        var response = await client.PostAsync(requestUri, binaryContent);

                        messageLog.Append("\n\n Received Response Code:" + response.ToString());
                        messageLog.Append("\n\n Getting response string ");
                        var responseString = await response.Content.ReadAsStringAsync();

                        messageLog.Append("\n\n Deserializing data: " + responseString + " ");
                        dynamic data = JsonConvert.DeserializeObject(responseString);
                        messageLog.Append("\n\n Deserializing data done ");

                        if (data != null)
                        {
                            messageLog.Append("\n\n data:" + responseString) ;                           
                            return data.header.name;
                        }
                        else
                        {
                            messageLog.Append("\n\n data is null:" + responseString);                              
                            return string.Empty;
                        }                       
                    }
                }
                catch (Exception exp)
                {   
                    return exp.Message + "\n\n " + messageLog.ToString();
                }
            }
        }



        /// <summary>
        /// Converts Stream into byte[].
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <returns>Output byte[]</returns>
        private static byte[] StreamToBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public async Task<Stream> GetAudioStream(ConnectorClient connector, Activity activity, IncomingVoiceMessage audioAttachment)
        {
            using (var httpClient = new HttpClient())
            {
                // The Skype attachment URLs are secured by JwtToken,
                // you should set the JwtToken of your bot as the authorization header for the GET request your bot initiates to fetch the image.
                // https://github.com/Microsoft/BotBuilder/issues/662

                var uri = audioAttachment.M4AFileUrl;
                //await DisplayMessage(connector, activity,"URI "+uri.ToString()+"  Host:"+uri.Host+" Scheme: "+uri.Scheme);
                if ((uri.Host.EndsWith("skype.com") || uri.Host.EndsWith("smba.trafficmanager.net")) && uri.Scheme == "https")
                {
                    //await DisplayMessage(connector, activity, "Fetching Token");
                    var token = await GetTokenAsync(connector);
                    //await DisplayMessage(connector, activity, "Token: " + (token == null).ToString());
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    //await DisplayMessage(connector, activity, "Authorization done");
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/wav"));//application/octet-stream"));
                    // await DisplayMessage(connector, activity, "Accept add done");
                }
                return await httpClient.GetStreamAsync(uri);
            }
        }

        private async Task DisplayMessage(ConnectorClient connector, Activity activity, string msg)
        {
            Activity reply = activity.CreateReply($"{msg}");
            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private static async Task<string> GetTokenAsync(ConnectorClient connector)
        {
            var credentials = connector.Credentials as MicrosoftAppCredentials;
            if (credentials != null)
            {
                return await credentials.GetTokenAsync();
            }

            return null;
        }

       
    }
}