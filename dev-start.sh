# scripts/dev-start.sh
#!/bin/bash
echo "🚀 Starting Idle RPG Development Environment..."
docker-compose up -d postgres redis pgadmin
echo "✅ Database and cache services started"
echo "🌐 pgAdmin: http://localhost:8082 (admin@idlerpg.com / admin123)"
echo "🔧 Ready for development!"

# scripts/dev-stop.sh
#!/bin/bash
echo "🛑 Stopping Idle RPG Development Environment..."
docker-compose down
echo "✅ All services stopped"

# scripts/build-deploy.sh
#!/bin/bash
echo "🏗️ Building and deploying Idle RPG Server..."
docker-compose build api
docker-compose up -d
echo "✅ Server deployed and running"
echo "🌐 API: http://localhost:8080/swagger"