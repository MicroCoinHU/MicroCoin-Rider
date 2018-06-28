# MicroCoin Rider

Source code of the https://rider.microcoin.hu project

MicroCoin Rider is an API server for the MicroCoin ecosystem.
It acts as the interface between MicroCoin network and applications that want to access the MicroCoin network.
It allows you to submit transactions to the network, check the status of accounts, subscribe to transactions, etc.
Rider provides a RESTful API to allow client applications to interact with the MicroCoin network.
You can communicate with Rider using cURL or just your web browser. However, if you’re building a client application, you’ll likely want to use a MicroCoin SDK in the language of your client.

# Before you begin
Before you start developing useful to download the MicroCoin wallet. You can download the latest version from
the official [MicroCoin website](https://microcoin.hu)

# Building

1. Download MicroCoin wallett, or build a daemon and run it
2. Clone this repository and init submodules
3. Run `dotnet build`
4. Run `dotnet publish`
5. Run the server using the `dotnet run` command
