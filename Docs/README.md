[BUILD]
docker build -t TodoApp:v1 -f Dockerfile .

[RUN]
docker run -p 8000:8080 -p 8001:8081 -v C:\github\h4\serverside\DockerTodo\TodoApp\.aspnet\https:/https/ -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_URLS="https://+:8081;http://+:8080" -e ASPNETCORE_Kestrel__Certificates__Default__Password="123Abcd123" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/TodoApp.pfx todoapp:v1