# EtumrepMMO.Server

Server application for remote, authorized bot clients to process PLA MO/MMO/MultiSpawner data on. Client supplies the required data, server calculates and sends back the result.
Leverages EtumrepMMO.Lib, PLA-SeedFinder, and PKHeX.Core to find the group seed.

Requirements:
- [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0).
- IDE that supports C#12 (such as Visual Studio CE 2022).
- x64 OS.

Usage:
- Compile the EtumrepMMO.Server.WinForms project using the x64 build configuration.
- Configure settings as desired (clients have to be whitelisted).
- Configure your router and/or firewall to allow connections to your specified port (may also need to set up port forwarding).
- Use either a SysBot.NET fork that establishes a client/server connection or write your own.
- Click 'Start' to begin listening for connections.

Credits:
- [@kwsch](https://github.com/kwsch) for EtumrepMMO and PKHeX.Core libraries.
- [@Mysticial](https://github.com/Mysticial) for [PLA-SeedFinder](https://github.com/PokemonAutomation/Experimental) (uses BSD license).
