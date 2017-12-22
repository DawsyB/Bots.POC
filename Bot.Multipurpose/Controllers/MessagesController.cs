using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Bot.Multipurpose.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Bot.Multipurpose
{
	[BotAuthentication]
	public class MessagesController : ApiController
	{
		private readonly ILifetimeScope scope;

		//public MessagesController(ILifetimeScope scope)
		//{
		//	SetField.NotNull(out this.scope, nameof(scope), scope);
		//}

		/// <summary>
		/// POST: api/Messages
		/// Receive a message from a user and reply to it
		/// </summary>
		public async Task<HttpResponseMessage> Post([FromBody]Activity activity, CancellationToken token)
		{
			if (activity.Type == ActivityTypes.Message)
			{
				

				//await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
				await Conversation.SendAsync(activity, () => new RootDialog());

				// Send is typing reply as soon as response is received.
				var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
				var isTypingReply = activity.CreateReply();
				isTypingReply.Type = ActivityTypes.Typing;
				await connector.Conversations.ReplyToActivityAsync(isTypingReply);

				// Then actually process the message.
				using (var scope = DialogModule.BeginLifetimeScope(this.scope, activity))
				{
					var postToBot = scope.Resolve<IPostToBot>();
					await postToBot.PostAsync(activity, token);
				}

			}
			else if (activity.Type == ActivityTypes.ConversationUpdate)
			{
				var client = new ConnectorClient(new Uri(activity.ServiceUrl), new MicrosoftAppCredentials());
				// Show a message when conversation is started. This needs to be handled differently depending on channel.
				bool showWelcomeMessage = false;

				// For web chat channel, only show for the conversation update which corresponds to the bot being added to the conversation, i.e. the 
				// first update which happens.
				if (activity.ChannelId == constants.ChannelNames.Web || activity.ChannelId == constants.ChannelNames.DirectLine)
				{
					if (activity.MembersAdded != null && activity.MembersAdded.Any())
					{
						foreach (var newMember in activity.MembersAdded)
						{
							// We have added the bot itself.
							if (newMember.Id == activity.Recipient.Id)
							{
								showWelcomeMessage = true;
							}
						}
					}
				}
				else
				{
					if (activity.MembersAdded != null && activity.MembersAdded.Any())
					{
						foreach (var newMember in activity.MembersAdded)
						{
							if (newMember.Id != activity.Recipient.Id)
							{
								showWelcomeMessage = true;
							}
						}
					}
				}

				if (showWelcomeMessage)
				{
					var reply = activity.CreateReply();
					reply.Text = constants.Responses.InitialGreeting1;
					await client.Conversations.ReplyToActivityAsync(reply);



					//Intro Attachment
					reply = activity.CreateReply();
					Attachment attachment = utlities.GetAttachment(constants.Cards.IntroCard);
					if (attachment != null)
					{
						reply.Attachments.Add(attachment);
						reply.Type = ActivityTypes.Message;
						await client.Conversations.ReplyToActivityAsync(reply);
					}
				}
			}
			else
			{
				await HandleSystemMessage(activity);
			}
			var response = Request.CreateResponse(HttpStatusCode.OK);
			return response;
		}

		private async Task<Activity> HandleSystemMessage(Activity message)
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

				//var msg = message.CreateReply("Hi, I'm multipurpose bot. What can I do for you today?");
				//ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
				//await connector.Conversations.SendToConversationAsync(msg);


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