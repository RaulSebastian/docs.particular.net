﻿namespace Core9.Headers
{
    using System.Threading.Tasks;
    using NServiceBus;

    #region header-outgoing-handler

    public class WriteHandler :
        IHandleMessages<MyMessage>
    {
        public async Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            var sendOptions = new SendOptions();
            sendOptions.SetHeader("MyCustomHeader", "My custom value");
            await context.Send(new SomeOtherMessage(), sendOptions)
                .ConfigureAwait(false);

            var replyOptions = new ReplyOptions();
            replyOptions.SetHeader("MyCustomHeader", "My custom value");
            await context.Reply(new SomeOtherMessage(), replyOptions)
                .ConfigureAwait(false);

            var publishOptions = new PublishOptions();
            publishOptions.SetHeader("MyCustomHeader", "My custom value");
            await context.Publish(new SomeOtherMessage(), publishOptions)
                .ConfigureAwait(false);
        }
    }

    #endregion


}