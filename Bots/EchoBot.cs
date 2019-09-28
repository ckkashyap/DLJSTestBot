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

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            IMessageActivity activity = turnContext.Activity;
            var testType = activity?.ChannelData?.testType?.Value as string;

            if (!string.IsNullOrEmpty(testType) && testType == "streaming")
            {
                await OnStreamingMessageActivityAsync(turnContext, cancellationToken);
            }
            else
            {
                await OnNonStreamingMessageActivityAsync(turnContext, cancellationToken);
            }
        }

        async Task OnNonStreamingMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            dynamic channelData = new JObject();
            channelData.originalActivity = new JObject();
            channelData.originalActivity.id = activity.Id;

            if (activity.Attachments != null)
            {
                var attachments = new JArray();
                foreach (var attachment in activity.Attachments)
                {
                    dynamic atch = new JObject();
                    atch.contentType = attachment.ContentType;
                    atch.contentUrl = attachment.ContentUrl;
                    atch.thumbnailUrl = attachment.ThumbnailUrl;
                    attachments.Add(atch);
                }
                channelData.originalActivity.attachments = attachments;
            }

            var resp = MessageFactory.Text("OK");
            resp.ChannelData = channelData;

            if (string.IsNullOrEmpty(activity.Text))
            {
                await turnContext.SendActivityAsync(resp, cancellationToken);
                return;
            }

            channelData.originalActivity.text = activity.Text;
            resp.Text = activity.Text;
            await turnContext.SendActivityAsync(resp, cancellationToken);
        }

        async Task OnStreamingMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            dynamic channelData = new JObject();
            channelData.originalActivity = new JObject();
            IMessageActivity act = turnContext.Activity;

            channelData.originalActivity.id = act.Id;
            var resp = MessageFactory.Text("OK");
            resp.ChannelData = channelData;

            if (turnContext.Activity.Attachments != null)
            {
                var attachments = new JArray();
                foreach (var a in turnContext.Activity.Attachments)
                {
                    var stream = a.Content as Stream;
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var t = reader.ReadToEnd();
                        dynamic atch = new JObject();
                        atch.contentType = a.ContentType;
                        atch.contentUrl = "ABCD";
                        atch.thumbnailUrl = a.ThumbnailUrl;
                        attachments.Add(atch);
                    }
                }
                channelData.originalActivity.attachments =  attachments;
            }

            channelData.originalActivity.text = act.Text;
            //resp.ChannelData = turnContext.Activity;
            await turnContext.SendActivityAsync(resp, cancellationToken);
        }
    }
}
