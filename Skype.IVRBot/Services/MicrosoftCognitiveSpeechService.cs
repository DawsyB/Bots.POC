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
using System.Text;

namespace Skype.IVRBot
{
    public class MicrosoftCognitiveSpeechService
    {
        private readonly string subscriptionKey;
        private readonly string speechRecognitionUri;
        public string DefaultLocale { get; set; }
        public MicrosoftCognitiveSpeechService()
        {
            this.DefaultLocale = "en-US";
            this.subscriptionKey = WebConfigurationManager.AppSettings["MicrosoftSpeechApiKey"];
            this.speechRecognitionUri = Uri.UnescapeDataString(WebConfigurationManager.AppSettings["MicrosoftSpeechRecognitionUri"]);
        }


        /// <summary>
        /// Gets text from an audio stream.
        /// </summary>
        /// <param name="audiostream"></param>
        /// <returns>Transcribed text. </returns>
        public async Task<string> GetTextFromAudioAsync(Stream audiostream)
        {
            string debugMsg = string.Empty;
            var requestUri = this.speechRecognitionUri + Guid.NewGuid();
            
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

                       
                        var response = await client.PostAsync(requestUri, binaryContent);

                       
                        
                        var responseString = await response.Content.ReadAsStringAsync();

                        
                        dynamic data = JsonConvert.DeserializeObject(responseString);
                        

                        if (data != null)
                        {
                            
                            return data.header.name;
                        }
                        else
                        {
                            
                            return string.Empty;
                        }
                    }
                }
                catch (Exception exp)
                {
                    return exp.Message;
                }
            }
        }

        /// <summary>
        /// Gets the response from Bing Speech
        /// </summary>
        /// <param name="audiostream"></param>
        /// <returns></returns>
        public async Task<dynamic> GetResponseFromAudioAsync(Stream audiostream)
        {
            string debugMsg = string.Empty;
            var requestUri = this.speechRecognitionUri + Guid.NewGuid();

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


                        var response = await client.PostAsync(requestUri, binaryContent);



                        var responseString = await response.Content.ReadAsStringAsync();


                        dynamic data = JsonConvert.DeserializeObject(responseString);

                        return data;
                       
                    }
                }
                catch (Exception exp)
                {
                    return exp.Message;
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
    }
}