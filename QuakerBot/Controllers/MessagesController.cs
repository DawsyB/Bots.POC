namespace EmergencyServicesBot
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Bot.Connector;
    using System.Linq;
    using NAudio.Wave;
    using AudioHandler;
    using Newtonsoft.Json;
    using System.Text;
    using System.Xml;


    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            StringBuilder messageLog = new StringBuilder();
            if (activity.Type == ActivityTypes.Message)
            {
                System.Diagnostics.Trace.TraceInformation(JsonConvert.SerializeObject(activity));
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));                
                //await DisplayMessage(connector, activity, $"this is what i have got Content:{activity.Attachments[0].Content}\n ContentType:{activity.Attachments[0].ContentType}\n ContentUrl:{activity.Attachments[0].ContentUrl}\n Name:{activity.Attachments[0].Name}\n Properties:{activity.Attachments[0].Properties}" );
                IncomingVoiceMessage voice = null;

                try
                {
                    voice = new IncomingVoiceMessage(activity);                   
                }
                catch
                {
                    //message = "Send me a voice message instead!"; // no voice file found
                    await DisplayMessage(connector, activity, "No voice file found");
                }
                try
                {
                    if (voice.ContentType != null)
                    {

                        await DisplayMessage(connector, activity, "I have received a file \n\n Content Type: " + voice.ContentType + "\n\n Name: "+ voice.FileName);
                        //Download original voice message
                        MicrosoftCognitiveSpeechService service = new MicrosoftCognitiveSpeechService();
                        var stream = await service.GetAudioStream(connector, activity, voice);
                        var wavStream = await voice.SaveFile(stream, messageLog);                        
                        if (wavStream.Length >0)
                        {
                            await DisplayMessage(connector, activity, "reading contents of the wave file");
                           
                            var text = await service.GetTextFromAudioAsync(wavStream, messageLog);
                            
                            await DisplayMessage(connector, activity, "Done length of text is: "+text.Length);
                            if (text.Length > 0)
                            {
                               
                                
                                Activity reply = activity.CreateReply($"Did you say: {text}?");
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                            else
                            {
                                await DisplayMessage(connector, activity, "Failed if");
                                Activity reply = activity.CreateReply($"Sorry.. couldn't undertsand your intent! Please try again..");
                                await connector.Conversations.ReplyToActivityAsync(reply);
                            }
                        }
                        else
                        {
                            await DisplayMessage(connector, activity, "Empty if");
                            Activity reply = activity.CreateReply($"wavStream is empty");
                            await connector.Conversations.ReplyToActivityAsync(reply);
                        }
                    }
                    else
                    {
                        
                        // calculate something for us to return
                        int length = (activity.Text ?? string.Empty).Length;
                        // return our reply to the user
                        Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters. You can also send me a .wav file and I will tell you it says ;)");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }
                catch (Exception ex)
                {
                    //var innerMessage = (ex.InnerException != null) ? ex.InnerException.ToString() : string.Empty;
                    //var stacktrace = (ex.StackTrace != null) ? ex.StackTrace.ToString() : string.Empty;
                    //await DisplayMessage(connector, activity, "Caught Exception: " + ex.Message.Trim().Trim('.')+" \n\n MessageTrace: "+ messageLog.ToString());
                    await DisplayMessage(connector, activity, "Caught Exception: " + ex.Message );
                    await DisplayMessage(connector, activity, "MessageLength: " + messageLog.Length);
                    await DisplayMessage(connector, activity, "MessageTrace: " + messageLog.ToString());

                }


                /* if (audioAttachment != null)
                 {
                     try
                     {
                         MicrosoftCognitiveSpeechService service = new MicrosoftCognitiveSpeechService();
                         await DisplayMessage(connector, activity, "Analysing your voice request..");
                         var stream = await service.GetAudioStream(connector,activity, audioAttachment);
                         await DisplayMessage(connector, activity, "I'm still working on it...");
                         await DisplayMessage(connector, activity, "I'm still working on it..." +(stream == null));
                         var text = await service.GetTextFromAudioAsync(stream);
                         if (text.Length > 0)
                         {
                             Activity reply = activity.CreateReply($"Did you say: {text}?");
                             await connector.Conversations.ReplyToActivityAsync(reply);
                         }
                         else
                         {
                             Activity reply = activity.CreateReply($"Sorry.. couldn't undertsand your intent! Please try again..");
                             await connector.Conversations.ReplyToActivityAsync(reply);
                         }
                     }
                     catch (Exception ex)
                     {
                         Activity reply = activity.CreateReply($"Exception found {ex.Message}");
                         await connector.Conversations.ReplyToActivityAsync(reply);
                     }
                 }*/


            }
            else
            {
                this.HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }



        private async Task DisplayMessage(ConnectorClient connector, Activity activity, string msg)
        {
            Activity reply = activity.CreateReply($"\n\n{msg}");
            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}