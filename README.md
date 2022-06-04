# EtumrepMMO.Server

Server application for remote, authorized bot clients to process MMO data on. Client supplies the required data, server sends back the result.
Leverages EtumrepMMO.Lib, PLA-SeedFinder, and PKHeX.Core to find the group seed.

Requirements:
- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0).
- IDE that supports C#10.
- x64 OS.

Usage:
- Compile the EtumrepMMO.WinForms project using the x64 build configuration.
- Configure settings as desired (clients have to be whitelisted). Restart the executable in order to save your configuration.
- Configure your router and/or firewall to allow connections to your specified port (may also need to set up port forwarding).
- Click 'Start' to begin listening for connections.

Credits:
- [@kwsch](https://github.com/kwsch) for EtumrepMMO and PKHeX.Core libraries.
- [@Mysticial](https://github.com/Mysticial) for [PLA-SeedFinder](https://github.com/PokemonAutomation/Experimental) (uses BSD license).