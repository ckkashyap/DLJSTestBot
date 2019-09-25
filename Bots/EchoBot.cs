// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {
        private async Task SendAttachmentAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity as Activity;

            byte[] rawBytes;
            using (WebClient webClient = new WebClient())
            {
                //rawBytes = webClient.DownloadData("https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE3yfaN");
                rawBytes = webClient.DownloadData("https://image.flaticon.com/sprites/new_packs/993881-stock-market.png");
            }

            //var dataUri = "data:image/jpeg;base64," + Convert.ToBase64String(rawBytes);
            var dataUri = "data:text/plain;base64," + Convert.ToBase64String(rawBytes);

            var attachment = new Attachment
            {
                Name = "hello.png", 
                ContentType = "image/png",
                ContentUrl = dataUri
            };

            Activity reply;
            // send description
            // var clone = JsonConvert.DeserializeObject<Attachment>(JsonConvert.SerializeObject(attachment));
            // clone.ContentUrl = clone.ContentUrl;
            // clone.ThumbnailUrl = clone.ThumbnailUrl;
            // if (clone.Content != null)
            //     clone.Content = JsonConvert.SerializeObject(attachment.Content)?.Trim('"');

            // var reply = activity.CreateReply(JsonConvert.SerializeObject(clone));
            // reply.Type = ActivityTypes.Message;
            // //await activityContext.SendResponse(reply).ConfigureAwait(false);
            // await turnContext.SendActivityAsync(reply).ConfigureAwait(false);

            // send attachment
            //reply = activity.CreateReply();
            reply = MessageFactory.Text($"{turnContext.Activity.Text}");
            reply.Type = ActivityTypes.Message;
            reply.Attachments = new List<Attachment> { attachment };
            try
            {
                //await activityContext.SendResponse(reply).ConfigureAwait(false);
                await turnContext.SendActivityAsync(reply, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                reply = activity.CreateReply($"Failed: {e.Message}");
                reply.Type = ActivityTypes.Message;
                await turnContext.SendActivityAsync(reply).ConfigureAwait(false);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //await turnContext.SendActivityAsync(MessageFactory.Text($"Echo1: {turnContext.Activity.Text}"), cancellationToken);
            //await turnContext.SendActivityAsync(MessageFactory.Text($"{turnContext.Activity.Text}"), cancellationToken);

            dynamic cd = new JObject();
            cd.originalActivity = new JObject();
            IMessageActivity act = turnContext.Activity;

            if (act.Type == "message" && act.Text != null && act.Text.Contains("atch"))
            {
                await SendAttachmentAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }
            else
            {


                cd.originalActivity.id = act.Id;
                //cd.originalActivity.text = act.Text;
                var resp = MessageFactory.Text($"{turnContext.Activity.Text}");
                resp.ChannelData = cd;



                if (turnContext.Activity.Attachments != null)
                {
                    foreach (var a in turnContext.Activity.Attachments)
                    {
                        var stream = a.Content as Stream;
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            var t = reader.ReadToEnd();
                        }
                    }
                }

                //resp.ChannelData = turnContext.Activity;
                await turnContext.SendActivityAsync(resp, cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                     await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome"), cancellationToken);
                }
            }
        }
    }
}
