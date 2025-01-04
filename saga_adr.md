# ADR: Choreography, Orchestrator, and Routing Slips in Microservices

## Context

To further improve the scalability, resilience, and flexibility of our Order Management System (OMS) microservices, we are considering adopting a combination of choreography, orchestration, and routing slips for workflow management and distributed transaction handling. This approach will enable microservices to communicate in a loosely coupled fashion while providing mechanisms to ensure transactional consistency across multiple services.

Choreography will be leveraged where services can react to events independently without the need for a central controller. In contrast, orchestration will be used when more control and coordination are required for complex workflows involving multiple steps or services. Additionally, routing slips will help manage dynamic workflows, enabling better handling of multi-step processes without rigidly defined service dependencies.

## Decision

We will use a hybrid approach that combines choreography, orchestration, and routing slips for distributed workflow management in our microservices architecture:

### Choreography for Simple Workflows
For scenarios where services can independently react to events without requiring coordination (e.g., `OrderCreated -> PaymentProcessed -> InventoryUpdated`), choreography will be used. Each microservice will subscribe to relevant events and perform its tasks autonomously.

### Orchestration for Complex Workflows
For more complex, long-running workflows that require explicit coordination between services (e.g., `OrderCancellation -> PaymentRefund -> NotificationDispatch`), an orchestrator will be introduced. The orchestrator will take responsibility for managing the order of execution and handling failures across services.

### Routing Slips for Dynamic Workflows
In cases where the workflow steps can vary or are determined at runtime, routing slips will be employed. Routing slips allow services to define a sequence of steps dynamically and track the progress of a message as it moves through multiple services. This is especially useful for tasks like order fulfillment, where the steps may change based on the order details (e.g., different shipping providers, packaging requirements).

## How This Applies to OMS Microservices

- **Order Processing Service**: Will use choreography to publish an event (e.g., `OrderCreated`), allowing services like Payment, Inventory, and Fulfillment to react asynchronously. Each service will handle its part of the process without requiring centralized control.

- **Order Fulfillment Service**: Will use routing slips to dynamically determine the sequence of fulfillment tasks (e.g., packaging, shipping, delivery confirmation). The routing slip will track the progress of the order across multiple services.

- **Order Cancellation Service**: Will use orchestration to coordinate the order cancellation process. The orchestrator will ensure that the cancellation triggers a refund, inventory update, and notification to the customer. It will manage retries and compensating actions if any step fails.

- **Order Payment Service**: Can benefit from orchestration when handling complex refund workflows or compensating actions in the event of a payment failure.

- **Order Notification Service**: Will use choreography to listen for events (e.g., `OrderShipped`, `OrderDelivered`) and send notifications to customers without requiring orchestration.

## Reasoning

- **Choreography** allows for greater service independence and decoupling. Each service can evolve independently as long as it continues to publish or consume the expected events.

- **Orchestration** is essential for managing complex workflows where coordination is necessary. It provides a clear view of the workflow and ensures transactional consistency across services.

- **Routing Slips** offer flexibility in handling dynamic and variable workflows. They allow microservices to define workflows on the fly, accommodating different scenarios without rigidly defined processes.

## Consequences

### Positive

- **Increased Flexibility**: Using choreography, orchestration, and routing slips allows us to handle both simple and complex workflows efficiently.

- **Decoupling**: Microservices remain loosely coupled, especially in choreographed workflows, reducing the impact of changes on other services.

- **Dynamic Workflows**: Routing slips provide the ability to handle multi-step processes where the steps may change based on business logic or runtime conditions.

- **Transactional Integrity**: Orchestration ensures that complex workflows are managed in a controlled manner, with retries and compensating actions in case of failures.

### Negative

- **Increased Complexity**: Introducing multiple workflow management patterns (choreography, orchestration, routing slips) adds complexity to the system architecture. Developers will need to carefully choose the right pattern for each use case.

- **Orchestrator as a Bottleneck**: In highly orchestrated workflows, the orchestrator may become a single point of failure or bottleneck. Careful attention is needed to ensure scalability and fault tolerance.

- **Monitoring Overhead**: Monitoring and tracking the progress of routing slips and orchestrated workflows will require additional tooling and logging to ensure visibility into the system’s state.

## Steps

1. Implement **MassTransit** for event-driven communication and orchestration.
2. Use **choreography** for simple, autonomous workflows (e.g., order creation, notifications).
3. Set up **routing slips** for dynamic workflows within the Order Fulfillment Service.
4. Introduce an **orchestrator** to manage complex, multi-step workflows (e.g., order cancellation, refunds).
5. Implement monitoring tools to track the progress of routing slips and orchestrated workflows.
