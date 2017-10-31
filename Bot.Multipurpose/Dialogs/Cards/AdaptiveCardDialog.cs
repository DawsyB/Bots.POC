using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using AdaptiveCards;

namespace Bot.Multipurpose.Dialogs
{
    [Serializable]
    public class AdaptiveCardDialog : IDialog<object>
    {
        private  readonly  IDictionary<string, string> options = new Dictionary<string, string> {
             { "1", "1. Show Demo Adaptive Card " },
             { "2", "2. Show Demo for Adaptive Card Design with Column" },
             { "3" , "3. Input Adaptive card Design" },
             { "4","4. Exit"}
        };
        public async Task StartAsync(IDialogContext context)
        {

            var welcomeMessage = context.MakeMessage();
            welcomeMessage.Text = "Welcome to bot Adaptive Card Demo";

            await context.PostAsync(welcomeMessage);

            this.DisplayOptionsAsync(context);
        }


        public void DisplayOptionsAsync(IDialogContext context)
        {
            PromptDialog.Choice<string>(
                context,
                this.SelectedOptionAsync,
                this.options.Keys,
                "What Demo / Sample option would you like to see?",
                "Please select Valid option 1 to 4",
                6,
                PromptStyle.PerLine,
                this.options.Values);
        }
        public async Task SelectedOptionAsync(IDialogContext context, IAwaitable<string> argument)
        {
            var message = await argument;
            var replyMessage = context.MakeMessage();
            Attachment attachment = null;
            bool exitDialog = false;
            switch (message)
            {
                case "1":
                    attachment = CreateAdapativecard();
                    replyMessage.Attachments = new List<Attachment> { attachment };
                    break;
                case "2":
                    attachment = CreateAdapativecardWithColumn();
                    replyMessage.Attachments = new List<Attachment> { attachment };
                    break;
                case "3":
                    replyMessage.Attachments = new List<Attachment> { CreateAdapativecardWithColumn(), CreateAdaptiveCardwithEntry() };
                    break;
                case "4":
                    exitDialog = true;
                    break;

            }

            if (!exitDialog)
            {
                await context.SayAsync("This is what it is", speak: "This is what it is");
                replyMessage.Speak = "This is from the message activity";
                replyMessage.InputHint = InputHints.AcceptingInput;
                await context.PostAsync(replyMessage);
                this.DisplayOptionsAsync(context);
            }
            else
            {
                context.Done<object>(null);
            }
        }


        public Attachment CreateAdapativecard()
        {

            AdaptiveCard card = new AdaptiveCard();

            // Specify speech for the card.  
            card.Speak = constants.Ignia.desc;
            // Body content  
            card.Body.Add(new Image()
            {
                Url = constants.Ignia.logourl,
                Size = ImageSize.Small,
                Style = ImageStyle.Person,
                AltText = constants.Ignia.title

            });

            // Add text to the card.  
            card.Body.Add(new TextBlock()
            {
                Text = constants.Ignia.title,
                Size = TextSize.Large,
                Weight = TextWeight.Bolder
            });

            // Add text to the card.  
            card.Body.Add(new TextBlock()
            {
                Text = constants.Ignia.desc
            });

            // Add text to the card.  
            card.Body.Add(new TextBlock()
            {
                Text = constants.Ignia.desc2
            });

            // Create the attachment with adapative card.  
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            return attachment;
        }


        public Attachment CreateAdapativecardWithColumn()
        {
            AdaptiveCard card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
        {  
            // Container  
            new Container()
            {
                Speak = "<s>Hello!</s><s>"+constants.Ignia.desc+"</s>",
                Items = new List<CardElement>()
                {  
                    // first column  
                    new ColumnSet()
                    {
                        Columns = new List<Column>()
                        {
                            new Column()
                            {
                                Size = ColumnSize.Auto,
                                Items = new List<CardElement>()
                                {
                                    new Image()
                                    {
                                        Url = constants.Ignia.logourl,
                                        Size = ImageSize.Small,
                                        Style = ImageStyle.Person
                                    }
                                }
                            },
                            new Column()
                            {
                                Size = "300",

                                Items = new List<CardElement>()
                                {
                                    new TextBlock()
                                    {
                                        Text =  constants.Ignia.title,
                                        Weight = TextWeight.Bolder,
                                        IsSubtle = true
                                    },
                                     new TextBlock()
                                    {
                                        Text =  constants.Ignia.email,
                                        Weight = TextWeight.Lighter,
                                        IsSubtle = true
                                    },
                                      new TextBlock()
                                    {
                                        Text =  constants.Ignia.phone,
                                        Weight = TextWeight.Lighter,
                                        IsSubtle = true
                                    },
                                       new TextBlock()
                                    {
                                        Text =  constants.Ignia.web,
                                        Weight = TextWeight.Lighter,
                                        IsSubtle = true
                                    }

                                }
                            }
                        }

                    },  
                    // second column  
                    new ColumnSet()
                    {
                         Columns = new List<Column>()
                        {
                              new Column()
                            {
                                Size = ColumnSize.Auto,
                                Separation =SeparationStyle.Strong,
                                Items = new List<CardElement>()
                                {
                                    new TextBlock()
                                    {
                                        Text = constants.Ignia.desc2,
                                        Wrap = true
                                    }
                                }
                            }
                        }
                    }
                }
            }
         },

            };
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            return attachment;

        }


        public Attachment CreateAdaptiveCardwithEntry()
        {
            var card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
        {  
                // contact form  
                
                new TextBlock() { Text = "Please Share your detail for contact:" },
                new TextInput()
                {
                    Id = "name",
                    Speak = "<s>Please Enter Your Name</s>",
                    Placeholder = "Please Enter Your Name",
                    IsRequired = true,
                    Style = TextInputStyle.Text
                },
                new TextBlock() { Text = "When your Free" },
                new DateInput()
                {
                    Id = "free",
                    Placeholder ="Your free Date"
                },
                new TextBlock() { Text = "Years of Experence" },
                new NumberInput()
                {
                    Id = "experence",
                    Min = 1,
                    Max = 20,
                },
                new TextBlock() { Text = "Email" },
                new TextInput()
                {
                    Id = "Email",
                    Speak = "<s>Please Enter Your email</s>",
                    Placeholder = "Please Enter Your email",
                    Style = TextInputStyle.Text
                },
                 new TextBlock() { Text = "Phone" },
                new TextInput()
                {
                    Id = "Phone",
                    Speak = "<s>Please Enter Your phone</s>",
                    Placeholder = "Please Enter Your Phone",
                    Style = TextInputStyle.Text
                },
        },
                Actions = new List<ActionBase>()
        {
            new SubmitAction()
            {
                Title = "Contact",
                Speak = "<s>Contact</s>",

            }
        }
            };
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            return attachment;
        }



    }
}