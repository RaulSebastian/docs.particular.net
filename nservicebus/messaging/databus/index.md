---
title: Data Bus
summary: How to handle messages that are too large to be sent by a transport natively
component: Core
reviewed: 2020-12-01
redirects:
 - nservicebus/databus
related:
 - samples/databus/file-share-databus
 - samples/databus/custom-serializer
 - samples/databus/blob-storage-databus
---

Although messaging systems work best with small message sizes, some scenarios require sending binary large objects ([BLOBs](https://en.wikipedia.org/wiki/Binary_large_object)) data along with a message (also known as a [_Claim Check_](https://learn.microsoft.com/en-us/azure/architecture/patterns/claim-check)). For this purpose, NServiceBus has a Data Bus feature to overcome the message size limitations imposed by an underlying transport.

## How it works

Instead of serializing the payload along with the rest of the message, the `Data Bus` approach involves storing the payload in a separate location that both the sending and receiving parties can access, then putting the reference to that location in the message.

If the location is not available upon sending, the send operation will fail. When a message is received and the payload location is not available, the receive operation will fail as well, resulting in the standard NServiceBus retry behavior, possibly resulting in the message being moved to the error queue if the error could not be resolved.

## Transport message size limits

The data bus should be used when it expected that message size may exceed the message size limit for the transport being used.

Note: Not all transports have message size limits and Azure Service Bus has increased its limit over time. The documentation for each transport should be checked for the latest message size limit.

| Transport                  | Maximum size |
| -------------------------- | ------------:|
| Amazon SQS                 | 256KB        |
| Amazon SQS + S3            | 2GB          |
| Azure Storage Queues       | 64KB         |
| Azure Service Bus Standard | 256KB        |
| Azure Service Bus Premium  | 100MB        |
| RabbitMQ                   | No limit     |
| SQL Server                 | No limit     |
| Learning                   | No limit     |
| MSMQ                       | 4MB          |

## Enabling the data bus

See the individual data bus implementations for details on enabling and configuring the data bus.

* [File Share Data Bus](file-share.md)
* [Azure Blob Storage Data Bus](azure-blob-storage.md)

## Cleanup

By default, BLOBs are stored with no set expiration. If messages have a [time to be received](/nservicebus/messaging/discard-old-messages.md) set, the data bus will pass this along to the data bus storage implementation.

NOTE: The value used should be aligned with the [ServiceContol audit retention period](/servicecontrol/how-purge-expired-data.md) if it is required that data bus BLOB keys in messages send to the audit queue can still be fetched.

## Specifying data bus properties

There are two ways to specify the message properties to be sent using the data bus:

 1. Using the `DataBusProperty<T>` type
 1. Message conventions

Note: Data bus properties must be top-level properties on a message class.

### Using `DataBusProperty<T>`

Set the type of the property to be sent over the data bus as `DataBusProperty<byte[]>`:

snippet: MessageWithLargePayload

### Using message conventions

NServiceBus also supports defining data bus properties by convention. This allows data properties to be sent using the data bus without using `DataBusProperty<T>`, thus removing the need for having a dependency on NServiceBus from the message types.

In the configuration of the endpoint include:

snippet: DefineMessageWithLargePayloadUsingConvention

Set the type of the property as `byte[]`:

snippet: MessageWithLargePayloadUsingConvention

partial: serialization

## Data bus attachments cleanup

The various data bus implementations each behave differently with regard to cleanup of physical attachments used to transfer data properties depending on the implementation used.

### Why attachments are not removed by default

Automatically removing these attachments can cause problems in many situations. For example:

* The supported data bus implementations do not participate in distributed transactions. If the message handler throws an exception and the transaction rolls back, the delete operation on the attachment cannot be rolled back. Therefore, when the message is retried, the attachment will no longer be present causing additional problems.
* The message can be deferred so that the file will be processed later. Removing the file after deferring the message, results in a message without the corresponding file.
* Functional requirements might dictate the message to be available for a longer duration.
* If the data bus feature is used when publishing an event to multiple subscribers, neither the publisher nor any specific subscribing endpoint can determine when all subscribers have successfully processed the message allowing the file to be cleaned up.
* If message processing fails, it will be handled by the [recoverability feature](/nservicebus/recoverability/). This message can then be retried some period after that failure. The data bus files need to exist for that message to be re-processed correctly.

## Alternatives

NOTE: A combination of these techniques may be used.

- Use a different transport or different tier (e.g. Azure Service Bus _Premium_ instead of _Standard_).
- Message compression: This works well on text-based payloads like XML and JSON, or any payload (text or binary) that contains repetitive data.
  - See the [message mutator example demonstrating message body compression](/samples/messagemutators/).
- Stream-based properties: The sample showing [handling large stream properties via pipeline](/samples/pipeline/stream-properties/) demonstrates a purely stream-based approach (rather than loading the full payload into memory) using the NServiceBus pipeline.
- A more efficient serializer: For example, a binary serializer. Most serializers can be added with a few lines of code.
   - Several [serializers are maintained by the community](/nservicebus/community/#serializers).
- Attachments: The community maintained [NServiceBus.Attachments](/nservicebus/community/#nservicebus-attachments) package may be used for unbounded binary payloads. Although it has functionality similar to the [DataBus](nservicebus/messaging/databus/), it has the following advantages.
  - Read on demand: Attachments are only retrieved when read by a consumer. Moreover, it processes all data items via an IAsyncEnumerable
  - Reduced memory usage: The package avoids the use of a base64 serializer which results in a significant reduction in memory usage.

## Other considerations

### Monitoring and reliability

The storage location for data bus blobs is critical to the operation of endpoints. As such it should be as reliable as other infrastructure such as the transport or persistence. It should also be monitored for errors and be actively maintained. Since messages cannot be sent or received when the storage location is unavailable, it may be necessary to stop endpoints when maintenance tasks occur.

### Auditing

The data stored in data bus blobs may be considered part of an audit record. In these cases data bus blobs should be archived alongside messages for as long as the audit record is required.
