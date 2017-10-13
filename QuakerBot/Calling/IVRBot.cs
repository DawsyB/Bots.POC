namespace EmergencyServicesBot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Calling;
    using Microsoft.Bot.Builder.Calling.Events;
    using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
    using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
    using System.Text;

    public class IVRBot : IDisposable, ICallingBot
    {
        // DTMF keys required for each of option, will be used for parsing results of recognize
        private const string Support = "1";
        private StringBuilder ConversationLogs;
        private string botName = "Quaker Parrot";
        private string botUser = "You";
        private int counter;

        // Response messages depending on user selection
        private string WelcomeMessage = "Hi, I'm Quaker Parrot. I have the ability to imitate human voice. Please try saying something?";
        private const string MainMenuPromptMessage = "If you have a life threatening medical emergency please contact the emergency services or go to your nearest hospital. For non-life threatening situations please press 1.";
        private const string NoConsultantsMessage = "Whilst we wait to connect you, please leave your name and a description of your problem. You can press the hash key when finished. We will call you as soon as possible.";
        private const string EndingMessage = "Thank you for leaving the message, goodbye";
        private const string OptionMenuNotSupportedMessage = "The option you entered is not supported. Please try again.";

        private readonly Dictionary<string, CallState> callStateMap = new Dictionary<string, CallState>();

        private readonly MicrosoftCognitiveSpeechService speechService = new MicrosoftCognitiveSpeechService();

        public IVRBot(ICallingBotService callingBotService)
        {
            if (callingBotService == null)
            {
                throw new ArgumentNullException(nameof(callingBotService));
            }

            this.CallingBotService = callingBotService;

            this.CallingBotService.OnIncomingCallReceived += this.OnIncomingCallReceived;
            this.CallingBotService.OnPlayPromptCompleted += this.OnPlayPromptCompleted;
            this.CallingBotService.OnRecordCompleted += this.OnRecordCompleted;
            this.CallingBotService.OnRecognizeCompleted += this.OnRecognizeCompleted;
            this.CallingBotService.OnHangupCompleted += OnHangupCompleted;
        }

        public ICallingBotService CallingBotService { get; }

        public void Dispose()
        {
            if (this.CallingBotService != null)
            {
                this.CallingBotService.OnIncomingCallReceived -= this.OnIncomingCallReceived;
                this.CallingBotService.OnPlayPromptCompleted -= this.OnPlayPromptCompleted;
                this.CallingBotService.OnRecordCompleted -= this.OnRecordCompleted;
                this.CallingBotService.OnRecognizeCompleted -= this.OnRecognizeCompleted;
                this.CallingBotService.OnHangupCompleted -= OnHangupCompleted;
            }
        }

        

        private static void SetupInitialMenu(Workflow workflow)
        {
            workflow.Actions = new List<ActionBase> { GetInitialMenu() };
        }

        private static void SetupInitialMenuWithErrorMessage(Workflow workflow)
        {
            workflow.Actions = new List<ActionBase>
            {
                GetPromptForText(OptionMenuNotSupportedMessage),
                GetInitialMenu()
            };
        }

        private static ActionBase GetInitialMenu()
        {
            return CreateIvrOptions(MainMenuPromptMessage, 1, false);
        }

        private static void ProcessMainMenuSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                if (outcome.RecognizeOutcome.FailureReason == RecognitionCompletionReason.IncorrectDtmf.ToString())
                {
                    SetupInitialMenuWithErrorMessage(outcome.ResultingWorkflow);
                }
                else
                {
                    SetupInitialMenu(outcome.ResultingWorkflow);
                }

                return;
            }

            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case Support:
                    callStateForClient.ChosenMenuOption = Support;
                    SetupRecording(outcome.ResultingWorkflow);
                    break;
                default:
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }

        private static Recognize CreateIvrOptions(string textToBeRead, int numberOfOptions, bool includeBack)
        {

            var id = Guid.NewGuid().ToString();
            var choices = new List<RecognitionOption>();
            choices.Add(new RecognitionOption
            {
                Name = "Yes",
                SpeechVariation = new List<string>() { "Yes", "Okay","yeah" }
            });
            choices.Add(new RecognitionOption
            {
                Name = "No",
                SpeechVariation = new List<string>() { "No", "None","Nope","get me out of here" }
            });


            var recognize = new Recognize
            {
                OperationId = Guid.NewGuid().ToString(),
                PlayPrompt = GetPromptForText(textToBeRead),
                BargeInAllowed = true,
                Choices = choices
            };

            return recognize;
        }

        private static void SetupRecording(Workflow workflow)
        {
            var id = Guid.NewGuid().ToString();

            var prompt = GetPromptForText(NoConsultantsMessage);
            var record = new Record
            {
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 60,
                InitialSilenceTimeoutInSeconds = 5,
                MaxSilenceTimeoutInSeconds = 4,
                PlayBeep = true,
                RecordingFormat = RecordingFormat.Wav,
                StopTones = new List<char> { '#' }
            };
            workflow.Actions = new List<ActionBase> { record };
        }

       

       
        

       

        

        #region Call Events
        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            this.callStateMap[incomingCallEvent.IncomingCall.Id] = new CallState(incomingCallEvent.IncomingCall.Participants);
            ConversationLogs = new StringBuilder();
            counter = 1;
            AddToTranscript(botName, WelcomeMessage);
            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    new Answer { OperationId = Guid.NewGuid().ToString() },
                    //GetPromptForText(WelcomeMessage)
                    GetRecordForText(WelcomeMessage)
                };

            return Task.FromResult(true);
        }

       
        private Task OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            var callState = this.callStateMap[playPromptOutcomeEvent.ConversationResult.Id];
            SetupInitialMenu(playPromptOutcomeEvent.ResultingWorkflow);

            return Task.FromResult(true);
        }

        private async Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            //recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
            //    {
            //        GetPromptForText(EndingMessage),
            //        new Hangup { OperationId = Guid.NewGuid().ToString() }
            //    };

            // Convert the audio to text
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {

                var record = await recordOutcomeEvent.RecordedContent;
                string text = await this.GetTextFromAudioAsync(record);

                var callState = this.callStateMap[recordOutcomeEvent.ConversationResult.Id];
                AddToTranscript(botUser, text);

                if (counter == 1)
                {
                    text = $"ok. I think you said *'{text}'*. Please say something else and I can repeat after you?";
                    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetRecordForText(text)
                    };
                }
                if (counter == 2)
                {
                    text = $"ok. This time you said *'{text}'*. Lets try something different! Say 'Yes' if you want to know the current weather condition, otherwise say 'No'";
                    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        CreateIvrOptions(text, 2, false)
                    };
                }
               
                counter++;
                AddToTranscript(botName, text);

            }
            else
            {

            }

            // recordOutcomeEvent.ResultingWorkflow.Links = null;
            //this.callStateMap.Remove(recordOutcomeEvent.ConversationResult.Id);
        }

        private Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {  
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);            
        }

        private async Task<bool> OnRecognizeCompleted(RecognizeOutcomeEvent recognizeOutcomeEvent)
        {
            var text = string.Empty;
            var callState = this.callStateMap[recognizeOutcomeEvent.ConversationResult.Id];
            bool calledHangUp = false;
            switch (recognizeOutcomeEvent.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case "Yes":
                    AddToTranscript(botUser, "yes");
                    text = "Current weather is sunny. I hope you have a wonderful day. Goodbye!";
                    recognizeOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText(text),
                        new Hangup { OperationId = Guid.NewGuid().ToString() }
                    };                   
                    calledHangUp = true;
                    break;
                case "No":
                    AddToTranscript(botUser, "No");
                    text = "ok. That is all I can do for now. Goodbye!";
                    recognizeOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        GetPromptForText(text),
                        new Hangup { OperationId = Guid.NewGuid().ToString() }
                    };                   
                    calledHangUp = true;
                    break;
                default:                    
                    text = "I didn't understand. Say 'Yes' if you want to know weather conditions, otherwise say 'No'";
                    recognizeOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                    {
                        CreateIvrOptions(text, 2, false)
                    };                   
                    break;
            }
            AddToTranscript(botName, text);
            if (calledHangUp)
                await  this.SendTranscriptToUser(ConversationLogs.ToString(), callState.Participants);
            //return Task.FromResult(true);
            return true;
        }
        #endregion

        #region Call helpers
        private static PlayPrompt GetPromptForText(string text)
        {
            var prompt = new Prompt { Value = text, Voice = VoiceGender.Male };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        private ActionBase GetRecordForText(string promptText)
        {
            PlayPrompt prompt;
            if (string.IsNullOrEmpty(promptText))
                prompt = null;
            else
                prompt = GetPromptForText(promptText);

            var id = Guid.NewGuid().ToString();
            return new Record()
            {
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 10,
                InitialSilenceTimeoutInSeconds = 5,
                MaxSilenceTimeoutInSeconds = 2,
                PlayBeep = false,
                RecordingFormat = RecordingFormat.Wav,
                StopTones = new List<char> { '#' }
            };
        }

        /// <summary>
        /// Send the message back to the user
        /// </summary>
        /// <param name="text"></param>
        /// <param name="participants"></param>
        /// <returns></returns>
        private async Task SendTranscriptToUser(string text, IEnumerable<Participant> participants)
        {
            var to = participants.Single(x => x.Originator);
            var from = participants.First(x => !x.Originator);

            await AgentListener.Resume(to.Identity, to.DisplayName, from.Identity, from.DisplayName, to.Identity, text);
        }

        /// <summary>
        /// Gets text from an audio stream.
        /// </summary>
        /// <param name="audiostream"></param>
        /// <returns>Transcribed text. </returns>
        private async Task<string> GetTextFromAudioAsync(Stream audiostream)
        {
            var text = await this.speechService.GetTextFromAudioAsync(audiostream,new System.Text.StringBuilder());
            Debug.WriteLine(text);
            return text;
        }
        #endregion

        #region Classes
        private class CallState
        {
            public CallState(IEnumerable<Participant> participants)
            {
                this.Participants = participants;
            }

            public string ChosenMenuOption { get; set; }

            public IEnumerable<Participant> Participants { get; }
        }

        #endregion


        #region Common
        private void AddToTranscript(string Name, string Message)
        {
            if (ConversationLogs == null)
                ConversationLogs = new StringBuilder();
            ConversationLogs.Append($"\n\n **{Name}**: {Message}");
        }

        #endregion
    }
}