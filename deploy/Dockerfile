# Imagem de produção da Innovera: API .NET + frontend React servido pela própria API.
#   docker build -f deploy/Dockerfile -t innovera .      (a partir da pasta do repositório)

# 1) Frontend (React + Vite)
FROM node:22-alpine AS frontend
WORKDIR /src
COPY src/Web/ClientApp/package.json src/Web/ClientApp/package-lock.json ./
RUN npm ci
COPY src/Web/ClientApp/ ./
RUN npm run build

# 2) Backend (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /src
COPY . .
RUN dotnet publish src/Web/Web.csproj -c Release -o /app \
    -p:SemFrontend=true -p:OpenApiGenerateDocuments=false -p:UseAppHost=false

# 3) Imagem final, só com o runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend /app ./
COPY --from=frontend /src/dist ./wwwroot/

# Anexos e chaves de cifra ficam num volume (/data) para sobreviverem às atualizações.
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    Armazenamento__Pasta=/data/ficheiros \
    DataProtection__Pasta=/data/chaves
# libgssapi-krb5-2: o Npgsql procura-a ao ligar à base de dados (sem ela fica um erro nos registos).
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /data/ficheiros /data/chaves && chown -R app:app /data
USER app
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "Innovera.Web.dll"]
