FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln ./
COPY TicTacToeAPI/*.csproj ./TicTacToeAPI/
COPY Domain/*.csproj ./Domain/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY Application/*.csproj ./Application/
COPY Shared/*.csproj ./Shared/
COPY TicTacToe.Tests/*.csproj ./TicTacToe.Tests/
COPY TicTacToeAPI/.env .env
RUN dotnet restore

COPY . ./
WORKDIR /app/TicTacToeAPI
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
ENTRYPOINT ["dotnet", "TicTacToeApi.dll"]
