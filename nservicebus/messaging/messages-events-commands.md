---
title: Messages, events and commands
summary: Messages, events, and commands and how to define them.
component: Core
reviewed: 2020-09-16
related:
 - nservicebus/messaging/conventions
 - nservicebus/messaging/unobtrusive-mode
redirects:
 - nservicebus/introducing-ievent-and-icommand
 - nservicebus/messaging/introducing-ievent-and-icommand
 - nservicebus/how-do-i-define-a-message
 - nservicebus/define-a-message
 - nservicebus/messaging/how-do-i-define-a-message
 - nservicebus/definingmessagesas-and-definingeventsas-when-starting-endpoint
 - nservicebus/messaging/definingmessagesas-and-definingeventsas
 - nservicebus/messaging/invalidoperationexception-in-unobtrusive-mode
---

A _message_ is the unit of communication for NServiceBus. There are two types of messages, _commands_ and _events_, that capture more of the intent and help NServiceBus enforce messaging best practices. This enforcement is enabled by default, but can be [disabled](best-practice-enforcement.md).

Command | Event
-- | --
Used to _make a request to perform an action_. | Used to _communicate that an action has been performed_.
Has one logical owner. | Has one logical owner.
Should be _sent to_ the logical owner. | Should be _published by_ the logical owner.
Cannot be _published_. | Cannot be _sent_.
_Cannot_ be subscribed to or unsubscribed from. | _Can_ be subscribed to and unsubscribed from.
_Can_ be sent using the [gateway](/nservicebus/gateway). | _Cannot_ be sent using the [gateway](/nservicebus/gateway).

Note: In a request and response pattern, _reply_ messages are neither a command nor an event.

### Validation

There are checks in place to ensure best practices are followed. Violations of the above guidelines generate the following exceptions:

 * "Pub/Sub is not supported for Commands. They should be sent direct to their logical owner." - this exception is being thrown when attempting to publish a Command or subscribe to/unsubscribe from a Command.
 * "Events can have multiple recipient so they should be published." - this exception will occur when attempting to use 'Send()' to send an event.
 * "Reply is neither supported for Commands nor Events. Commands should be sent to their logical owner using bus.Send and bus. Events should be published." - this exception is thrown when attempting to reply with a Command or an Event.
 * "Cannot configure routing for type {name} because it is not considered a message. Message types have to either implement NServiceBus.IMessage interface or match a defined message convention." - this exception is thrown when configuring destination endpoint for a non-message type.
 * "Cannot configure routing for assembly {name} because it contains no types considered as messages. Message types have to either implement NServiceBus.IMessage interface or match a defined message convention." - this exception is thrown when configuring destination endpoint for an assembly which contains no types considered messages.
 * "Cannot configure routing for namespace {name} because it contains no types considered as messages..." - this exception is thrown when configuring destination endpoint for a namespace which contains no types considered messages.
 * "Cannot configure publisher for type {name} because it is not considered a message. Message types have to either implement NServiceBus.IMessage interface or match a defined message convention." - this exception is thrown when configuring publisher for a type that is not a message.
 * "Cannot configure publisher for type {name} because it is not considered an event. Event types have to either implement NServiceBus.IEvent interface or match a defined event convention." - this exception is thrown when configuring publisher for a type that is not an event.
 * "Cannot configure publisher for type {name} because it is a command." - this exception is thrown when configuring publisher for a command.

## Designing messages

Messages should:

* be simple [POCO](https://en.wikipedia.org/wiki/Plain_old_CLR_object) types
* be as small as possible
* satisfy the [Single Responsibility Principle](https://en.wikipedia.org/wiki/Single_responsibility_principle)

Types used for other purposes (e.g. domain objects, data access objects, or UI binding objects) should not be used as messages.

Note: Prior to NServiceBus version 7.2, messages had to be defined as a `class`. Defining them as a `struct` would result in a runtime exception.

Generic message definitions (e.g. `MyMessage<T>`) are not supported. It is recommended to use dedicated, simple types for each message or to use inheritance to reuse shared message characteristics.

Messages define the data contracts between endpoints. More details are available in the [sharing message contracts documentation](sharing-contracts.md). It may also be beneficial to [use them as interfaces](messages-as-interfaces.md).

## Identifying messages

Endpoints will process any message that can be deserialized into a .NET type but requires message contracts to be identified up front to support:

* [Automatic subscriptions](/nservicebus/messaging/publish-subscribe/controlling-what-is-subscribed.md) for event types
* [Routing based on `namespace` or `assembly`](/nservicebus/messaging/routing.md) for commands

Messages can be defined either by implementing a marker interface or by specifying a custom convention.

### Marker interfaces

The simplest way to identify messages is to use interfaces.

* `NServiceBus.ICommand` for a command.
* `NServiceBus.IEvent` for an event.
* `NServiceBus.IMessage` for any other type of message (e.g. a _reply_ in a request response pattern).

```cs
public class MyCommand : ICommand { }

public class MyEvent : IEvent { }

public class MyMessage : IMessage { }
```

### Conventions

To avoid having message contract assemblies reference the NServiceBus assembly, [custom conventions](/nservicebus/messaging/conventions.md) can be used to identify the types used as contracts for messages, commands, and events. This is known as [unobtrusive mode](unobtrusive-mode.md).
