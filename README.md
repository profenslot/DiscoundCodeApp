# Strong Redis with Concurrent Requests (Server_Redis)
Redis is used to check for uniqueness, and data is stored in the database. All requests are processed concurrently, meaning the application relies on Redis to handle high-volume traffic.

# Sequential Queue Processing (Server)
All incoming requests are queued, and Redis is used to check uniqueness sequentially. After passing uniqueness checks, data is stored in the database.

# Background Generation of Unique Codes (Server_PreGenerate)
Unique codes are pre-generated in a background service and stored in Redis and in a database. When a request comes in, it pulls the pre-generated codes from Redis.