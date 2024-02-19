#/bin/bash!

docker build -t rinha:latest .

docker-compose down
docker-compose up -d