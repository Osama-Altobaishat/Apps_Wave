# Apps_Wave

A. How did you define and enforce overlapping bookings, and why?

Overlapping bookings are defined as any two reservations for the same resource where their time intervals intersect:

NewStart < ExistingEnd and NewEnd > ExistingStart

B. What did you assume about concurrency?

The system assumes low to moderate concurrent booking traffic.
To handle concurrency, the API relies on:

Server-side validation before insert
Database transaction isolation.

C. What would break in your design at scale, and where would the first bottleneck be?

The “overlap check query” becomes expensive perfrmance and memory as data grows (full scans without proper indexing).

What would break:

Race conditions causing occasional double bookings under high concurrency.
Slow response times due to locking and repeated conflict.

D. How would you evolve this into a distributed system?

To evolve the system:

1.System division into microservices
2.Performance scaling (caching)
3.Storing recurring data in
e.g., available slots for each vendor
4.To reduce database load Available slots for each vendor
 and Recent bookings

E. Which tradeoff did you prioritize — simplicity, correctness, or performance — and why?

The system prioritizes correctness over performance and complexity.

Booking systems are integrity and multibal bookings are unacceptable in same time.
It is safer to ensure correctness at the cost of additional checks and simpler architecture.
