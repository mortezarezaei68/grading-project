# ADR: Reducing Single Point of Failure in API Gateway

## Context

An API Gateway is used to handle all incoming requests to microservices, routing them to appropriate backend services. However, relying on a single API Gateway instance can create a Single Point of Failure (SPOF), which negatively impacts the system’s availability and resilience. It is crucial to design the API Gateway in a way that avoids this risk, ensuring that it can handle failures gracefully and provide high availability.

### Key Requirements:
- High Availability (HA) for the API Gateway
- Horizontal scaling for increased traffic capacity
- Automated failover and fault tolerance mechanisms
- Load balancing to evenly distribute traffic
- Disaster recovery mechanisms in case of failure

## Decision

To mitigate the risk of SPOF, the following strategies will be adopted:

### 1. **Multiple API Gateway Instances (Redundancy)**
- **Deploy in multiple availability zones or regions**: API Gateway instances will be deployed across multiple availability zones or even regions, ensuring that if one instance or region becomes unavailable, others can continue handling the traffic without downtime.
- **Load balancing**: A load balancer will be introduced to route traffic across multiple API Gateway instances. This ensures even distribution of requests and prevents overloading any single instance.

### 2. **Auto-Scaling**
- **Horizontal scaling**: Auto-scaling groups will be configured for the API Gateway to automatically scale up or down based on traffic load. This ensures that the system can handle high traffic while minimizing cost during low-traffic periods.
- **Elastic Infrastructure**: Cloud-native services (such as AWS Auto Scaling, Azure Scale Sets) will be used to provide elasticity and ensure that the number of instances adjusts dynamically based on real-time demand.

### 3. **Geographical Redundancy**
- **Global deployment**: The API Gateway will be deployed in multiple geographic regions to ensure that if one region goes down due to an outage, another region can take over the traffic.
- **Global Load Balancing**: Services such as AWS Global Accelerator, Azure Traffic Manager, or Google Cloud Load Balancer will be used to manage the routing of traffic between regions for improved performance and availability.

### 4. **Stateless API Gateway**
- **Stateless Design**: The API Gateway will be stateless, meaning it will not store session or request-specific data locally. This ensures that any instance of the API Gateway can handle requests independently, without relying on any specific instance's state.
- **Session management**: All session information, such as user tokens, will be handled by external, highly available systems like Redis, Memcached, or a distributed session management service.

### 5. **Fault Tolerance and Circuit Breaker**
- **Circuit Breaker Pattern**: The API Gateway will implement a circuit breaker pattern to manage failures in downstream services. This prevents cascading failures and allows the system to gracefully handle failures by temporarily "breaking" the circuit when a service is down.
- **Graceful Degradation**: In case of downstream service failures, the API Gateway will return minimal responses or fallback options to avoid disrupting the user experience.

### 6. **Monitoring and Health Checks**
- **Automated Health Checks**: Health checks will be configured to monitor the status of the API Gateway instances. If any instance fails, traffic will be routed to a healthy instance automatically.
- **Real-time Monitoring**: Continuous monitoring will be set up to track the health, performance, and traffic load of API Gateway instances. This will help in identifying issues early and responding proactively.

### 7. **Disaster Recovery**
- **Backup Configurations**: Backup configurations and policies will be implemented for the API Gateway, ensuring that in case of a major failure, the gateway can be restored to a previous stable state quickly.
- **Failover and Redundancy**: API Gateway failover policies will be defined to automatically switch to backup instances in the event of an instance or region failure.

## Consequences

### Positive
- **Increased Availability**: By deploying multiple instances across different regions or availability zones, the risk of downtime due to a single failure is minimized.
- **Scalability**: The system can handle variable traffic loads, scaling up or down automatically based on demand.
- **Fault Tolerance**: Failures in one part of the system do not impact the entire architecture, thanks to distributed deployment and the use of circuit breakers.
- **Seamless Failover**: In case of failure, automatic failover ensures that the system remains functional without significant impact on users.

### Negative
- **Increased Complexity**: Managing multiple API Gateway instances, ensuring consistent configuration, and handling failover mechanisms add complexity to the system.
- **Cost Overhead**: Redundant deployments across multiple availability zones or regions will increase operational costs.
- **Monitoring and Maintenance**: Setting up and maintaining robust monitoring and health check systems requires additional resources.

## Steps

1. **Deploy API Gateway in Multiple Availability Zones**: Configure API Gateway instances in different availability zones for fault tolerance.
2. **Implement Auto-Scaling**: Configure auto-scaling policies for the API Gateway instances based on traffic load.
3. **Set Up Load Balancing**: Set up a load balancer to distribute traffic between the API Gateway instances.
4. **Introduce Health Checks**: Implement automated health checks for real-time monitoring of API Gateway instances.
5. **Enable Circuit Breaker Pattern**: Implement a circuit breaker to gracefully handle service failures.
6. **Set Up Disaster Recovery Plans**: Define disaster recovery and backup strategies for the API Gateway.
7. **Monitor and Optimize**: Continuously monitor the system, analyze traffic patterns, and optimize configurations.

