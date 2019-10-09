// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        byte[] SlurpStream(Stream stream)
        {
            var listOfByteArrs = new List<byte[]>();
            int totalCount = 0;
            bool theresMore = false;
            using (var reader = new BinaryReader(stream))
            {
                do
                {
                    var ba = reader.ReadBytes(10000);
                    totalCount += ba.Length;
                    listOfByteArrs.Add(ba);
                    theresMore = ba.Length > 0;
                } while (theresMore);
            }

            var output = new byte[totalCount];
            using (var ostream = new MemoryStream(output))
                foreach (var bytes in listOfByteArrs)
                    ostream.Write(bytes, 0, bytes.Length);

            return output;
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
                    //using (var reader = new BinaryReader(stream))
                    {
                        // TODO - find the right way to read the whole stream
                        var t = SlurpStream(stream); //  reader.ReadBytes((int)stream.Length);
                        dynamic atch = new JObject();
                        atch.contentType = a.ContentType;
                        atch.contentUrl = Convert.ToBase64String(t);
                        atch.thumbnailUrl = a.ThumbnailUrl;
                        attachments.Add(atch);
                    }
                }
                channelData.originalActivity.attachments =  attachments;
            }

            if (act.Text.StartsWith("attach" ))
            {
                var urls = act.Text.Split(" ");
                var attachments = new List<Attachment>();
                foreach (var url in urls.Skip(1))
                {
                    byte[] rawBytes;
                    using (WebClient webClient = new WebClient())
                    {
                        rawBytes = webClient.DownloadData(url);
                    }

                    var dataUri = "data:text/plain;base64," + Convert.ToBase64String(rawBytes);

                    var attachment = new Attachment
                    {
                        Name = "hello.png",
                        ContentType = "image/png",
                        ContentUrl = dataUri
                    };
                    attachments.Add(attachment);
                }
                resp.Attachments = attachments;
            }


            channelData.originalActivity.text = act.Text;
            await turnContext.SendActivityAsync(resp, cancellationToken);
        }
    }
}
