using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Pipeline;
using NServiceBus.Transport;
using Microsoft.Extensions.DependencyInjection;

class DistributeSubscriptions : IBehavior<IIncomingPhysicalMessageContext, IIncomingPhysicalMessageContext>
{
    HashSet<string> knownPartitionKeys;
    string localPartitionKey;
    ITransportAddressResolver transportAddressResolver;
    QueueAddress logicalAddress;

    public DistributeSubscriptions(string localPartitionKey, HashSet<string> knownPartitionKeys, ITransportAddressResolver transportAddressResolver, QueueAddress logicalAddress)
    {
        this.logicalAddress = logicalAddress;
        this.localPartitionKey = localPartitionKey;
        this.knownPartitionKeys = knownPartitionKeys;
        this.transportAddressResolver = transportAddressResolver;
    }

    public async Task Invoke(IIncomingPhysicalMessageContext context, Func<IIncomingPhysicalMessageContext, Task> next)
    {
        var intent = context.Message.GetMessageIntent();
        var isSubscriptionMessage = intent == MessageIntent.Subscribe || intent == MessageIntent.Unsubscribe;

        if (isSubscriptionMessage)
        {
            var tasks = new List<Task>();

            // Check to see if subscription message was already forwarded to prevent infinite loop
            if (!context.MessageHeaders.ContainsKey(PartitionHeaders.ForwardedSubscription))
            {
                context.Message.Headers[PartitionHeaders.ForwardedSubscription] = string.Empty;

                foreach (var partitionKey in knownPartitionKeys)
                {
                    if (partitionKey == localPartitionKey)
                    {
                        continue;
                    }

                    var queueAddress = new QueueAddress(logicalAddress.BaseAddress, partitionKey, null, null);
                    var destination = transportAddressResolver.ToTransportAddress(queueAddress);
                    tasks.Add(context.ForwardCurrentMessageTo(destination));
                }
            }
            await Task.WhenAll(tasks)
                .ConfigureAwait(false);
        }
        await next(context)
            .ConfigureAwait(false);
    }

    public class Register :
        RegisterStep
    {
        public Register(string localPartitionKey, HashSet<string> knownPartitionKeys, QueueAddress logicalAddress) :
            base("DistributeSubscriptions", typeof(DistributeSubscriptions), "Distributes subscription messages for message driven pubsub using header only.", b => new DistributeSubscriptions(localPartitionKey, knownPartitionKeys, b.GetRequiredService<ITransportAddressResolver>(), logicalAddress))
        {
            InsertBeforeIfExists("ProcessSubscriptionRequests");
        }
    }
}