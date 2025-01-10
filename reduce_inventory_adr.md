# Architectural Decision Record (ADR) for Inventory Reduction in OMS

## Title: Asynchronous Inventory Reduction with Eventual Consistency in Order Management System

**Date**: January 10, 2025  
**Status**: Proposed

### Context:

The Order Management System (OMS) needs to handle the lifecycle of an order, including order creation, validation, payment processing, and fulfillment, while maintaining consistency across external systems, especially inventory. The system must support high concurrency, scalability, and fault tolerance, as well as integrate with external services like Inventory Service, Payment Service, and Shipping Service.

A key requirement is to reduce inventory after order placement in a scalable, fault-tolerant, and resilient manner without causing bottlenecks or failures when handling multiple concurrent order requests for the same product. The architecture must also ensure that inventory is reserved before payment and fulfillment, and only reduced when the order is completed.

### Problem:

Inventory reduction must occur when an order is placed, but this process should be handled asynchronously to avoid the following issues:

- **Concurrency conflicts**: Multiple users may place orders for the same product simultaneously.
- **Tight coupling**: Synchronous requests to reduce inventory may cause cascading failures if the Inventory Service is down or overloaded.
- **Scalability limitations**: Synchronous inventory updates can limit the system’s ability to handle high concurrency during peak traffic.

The challenge is how to manage the Create Order and Inventory Reduction process in a way that:

- Decouples the services to ensure scalability and fault tolerance.
- Avoids over-selling products in high-concurrency environments.
- Maintains eventual consistency across services while still ensuring that inventory is properly updated when orders are fulfilled.

### Decision:

We will implement an asynchronous inventory reduction process using an event-driven architecture with eventual consistency. This approach decouples the Order Processing and Inventory services while allowing for high concurrency and fault tolerance.

#### Key Components:

- **Order Processing Service**: Handles order creation and validation. Emits an `OrderCreatedEvent` after a successful order.
- **Inventory Service**: Consumes `OrderCreatedEvent` to reserve inventory for the order. It will reduce inventory only after successful payment and fulfillment.
- **Payment Service**: Handles payment processing and emits `PaymentSuccessfulEvent` after payment confirmation.
- **Order Fulfillment Service**: Manages shipment and emits `OrderFulfilledEvent` once the order is shipped.

#### Communication Patterns:

- **Event-Driven Communication**: All communication between services will be done asynchronously using events.
- **Synchronous API Calls**: Where necessary (e.g., between Order Payment and Payment Service), services will use RESTful APIs for real-time communication.

#### Flow:

1. **Order Creation**: The Order Processing Service validates the order and emits an `OrderCreatedEvent` containing the order details.
2. **Inventory Reservation**: The Inventory Service consumes the `OrderCreatedEvent` and reserves the inventory for the order, temporarily marking the stock as unavailable. It emits an `InventoryReservedEvent`.
3. **Payment Processing**: The Payment Service processes payment after receiving the `OrderCreatedEvent` and emits a `PaymentSuccessfulEvent` upon successful payment.
4. **Order Fulfillment**: Once payment is successful, the Order Fulfillment Service ships the order and emits an `OrderFulfilledEvent`.
5. **Final Inventory Reduction**: Upon receiving the `OrderFulfilledEvent`, the Inventory Service reduces the actual inventory, finalizing the stock deduction.

#### Retries & Circuit Breakers:

- In case of service unavailability, retries will be implemented using a message broker like Kafka or RabbitMQ to ensure messages are not lost.
- Circuit breakers will be used to prevent failures from cascading if a service is down.

#### Granularity:

- Each microservice is responsible for its specific domain and communicates with others using well-defined events. The Inventory Service focuses on managing stock, while the Order Processing Service handles order lifecycle events.

### Reasoning:

This decision ensures that the OMS can handle high concurrency, scale independently, and tolerate service failures without causing system-wide issues. By using an event-driven architecture and eventual consistency, the system is more resilient and flexible.

#### Why Event-Driven?

- **Loose Coupling**: By emitting events rather than making direct synchronous calls, services are decoupled, reducing the risk of cascading failures.
- **Resilience**: Asynchronous events allow services to continue functioning even when some services are temporarily unavailable.
- **Scalability**: By decoupling the services, each one can scale independently based on demand.

#### Why Eventual Consistency?

- **Concurrency Handling**: Multiple concurrent requests for the same product can be handled through inventory reservation and eventual stock reduction.
- **Resilience**: Even if a service is unavailable, the system ensures eventual consistency once services are restored.

#### Why Inventory Reservation?

- **Avoid Over-Selling**: By reserving inventory during the order creation process, we prevent over-selling and ensure that stock is available when the order is fulfilled.
- **Flexibility**: Inventory is only reduced after the order is successfully paid for and shipped, reducing the chances of unnecessary stock reduction.

### Consequences:

#### Positive:

- **Scalable and Resilient**: The architecture supports high concurrency and fault tolerance through decoupled services and asynchronous communication.
- **Flexibility**: Each microservice can be developed, deployed, and scaled independently, ensuring maintainability.
- **Concurrency Handling**: By reserving inventory and reducing it after payment and fulfillment, the system can handle multiple concurrent order requests without over-selling.

#### Negative:

- **Operational Complexity**: Managing distributed microservices and asynchronous events adds complexity in terms of monitoring, logging, and debugging.
- **Eventual Consistency**: While eventual consistency ensures resilience, it may cause a delay in reflecting the true state of the inventory, which could impact reporting in real-time systems.

### Alternatives Considered:

- **Synchronous Inventory Reduction**: This approach was rejected due to its tight coupling, scalability limitations, and risk of cascading failures if the Inventory Service is down.
- **Monolithic Design**: A monolithic architecture was considered but rejected for its lack of scalability and flexibility, especially with external system integration.
- **Direct Stock Reduction**: Immediately reducing stock at the time of order creation was rejected due to potential payment failures, resulting in unnecessarily reduced inventory.

### Decision Outcome:

The decision to implement asynchronous inventory reduction using an event-driven architecture ensures the system can meet the requirements for scalability, flexibility, and fault tolerance while handling complex order workflows and multiple concurrent requests.

### Steps:

1. Implement the Order Processing Service and the `OrderCreatedEvent` mechanism.
2. Define the contract and process for Inventory Reservation and `InventoryReservedEvent`.
3. Set up a message broker (Kafka or RabbitMQ) for event-driven communication.
4. Develop retry mechanisms and circuit breakers to ensure system resilience.