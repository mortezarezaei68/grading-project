# ADR: Inventory Processing Bottleneck

## Status
Accepted

## Context
The current inventory system processes each order request one-by-one in a serialized manner, which can lead to a bottleneck when handling a high volume of simultaneous requests for the same product. When multiple users place an order for a product with limited stock, the inventory service processes requests sequentially. This can cause delays, especially during high traffic, and results in a suboptimal user experience with potential overselling or order cancellations.

## Decision
To mitigate the bottleneck and improve concurrency handling, the following strategies are proposed:

1. **Optimistic Concurrency Control (OCC):**
    - Multiple requests are allowed to proceed concurrently without locking the inventory resource. Conflicts are checked before committing changes, allowing the first request to reserve inventory and rejecting others if conflicts arise.

2. **Distributed Locking:**
    - Use a distributed lock (e.g., Redis or etcd) to lock access to the inventory for short periods while processing requests, ensuring only one process modifies the inventory at a time. Locks are held for a short duration to minimize contention.

3. **Inventory Reservation with Eventual Consistency:**
    - Allow multiple requests to soft-reserve inventory at the same time, with only one reservation being finalized based on payment confirmation. A background reconciliation process ensures consistency.

4. **Batch Processing:**
    - Collect and batch requests together over a short period, then process them as a group. This reduces the number of round trips to the database and allows for bulk inventory updates.

5. **CQRS (Command Query Responsibility Segregation):**
    - Separate read and write operations, allowing for independent optimization of query and update paths. Use event sourcing to ensure consistency in inventory updates.

6. **Sharding Inventory by Product ID:**
    - Partition the inventory data by product ID across multiple shards or databases. Each shard processes its inventory independently, reducing contention and improving scalability.

## Consequences
By adopting these strategies, the system will be able to handle concurrent requests more efficiently, reducing delays, preventing overselling, and ensuring a better user experience. This will also improve scalability as the system grows, allowing it to handle high traffic scenarios without becoming a bottleneck.
