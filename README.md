# MGC_CasinoGames

A multi-project solution containing a **shared library**, a **server**, and a **client**.  
This repository is structured as a **monorepo** to keep related components together.

---

## Project Structure

root/<br>
├─ CardGamesLibrary/ # Shared code used by server and client<br>
│ └─ CardGamesLibrary.csproj<br>
│<br>
├─ CasinoServer/ # Server authority application<br>
│ └─ CasinoServer.csproj<br>
│<br>
├─ CasinoPlayerClient/ # Client interface application<br>
│ └─ CasinoPlayerClient.csproj<br>
│<br>
└─ README.md

---

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (`.NET 8.0`)

### Clone the Repository
```sh
git clone https://github.com/MapleGlazedCrow/MGC_CasinoGames.git
cd MGC_CasinoGames
```

## Build Instructions
### Library
```
cd CardGamesLibrary
dotnet build -c Release
cd ..
dotnet nuget add source "$(pwd)/CardGamesLibrary/bin/Release" -n LocalPackages
```
### Server
```
cd server
dotnet build -c Release
```

```
cd .\bin\Release\net8.0\
.\CasinoServer.exe
```
### Client
```
cd client
dotnet build -c Release
```

```
cd .\bin\Release\net8.0\
.\CasinoPlayerClient.exe
```

## Development

- The library contains general-purpose code shared between server and client.
- Both server and client reference the library as a project dependency.

## License
This project is licensed under the MIT License.

---
