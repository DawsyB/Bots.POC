using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Multipurpose.Forms;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot.Multipurpose.Dialogs
{
	[Serializable]
	public class RootDialog : IDialog<object>
	{
		public Task StartAsync(IDialogContext context)
		{
			context.Wait(MessageReceivedAsync);

			return Task.CompletedTask;
		}

		private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
		{
			var message = await result;

			if (message.Value != null)
			{
				// PENDING ITEMS:
					// Validation on adaptive required is requied true property
					// all validations are handled below which should not be the case!
				// Got an Action Submit
				dynamic value = message.Value;
				string submitType = value.Type.ToString();
				switch (submitType)
				{
					case "Send Feedback":
						FeedbackForm query;
						try
						{
							query = FeedbackForm.Parse(value);

							// Trigger validation using Data Annotations attributes from the HotelsQuery model
							List<ValidationResult> results = new List<ValidationResult>();
							bool valid = Validator.TryValidateObject(query, new ValidationContext(query, null, null), results, true);
							if (!valid)
							{
								// Some field in the Feedback Form Query are not valid
								var errors = string.Join("\n", results.Select(o => " - " + o.ErrorMessage));
								await context.PostAsync("Please complete all the search parameters:\n" + errors);
								return;
							}
						}
						catch (InvalidCastException ex)
						{
							// Feedback Form Query could not be parsed
							await context.PostAsync("Please complete all the search parameters");
							return;
						}

						// Proceed with hotels search
						await context.Forward(new FeedbackDialog(), this.ResumeAfterDialog, message, CancellationToken.None);

						return;

						// More cases will go here if the more adaptive card forms are used with Action Submit
				}
			}
			else
			{
				// Send the Message to LuisDialog to handle
				await context.Forward(new RootLuisDialog(), this.ResumeAfterDialog, message, CancellationToken.None);

			}


		}

		private async Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
		{
			context.Wait(this.MessageReceivedAsync);
		}
	}

}