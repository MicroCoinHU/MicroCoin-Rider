# MicroCoin Rider

Source code of the https://rider.microcoin.hu project

MicroCoin Rider is an API server for the MicroCoin ecosystem.
It acts as the interface between MicroCoin network and applications that want to access the MicroCoin network.
It allows you to submit transactions to the network, check the status of accounts, subscribe to transactions, etc.
Rider provides a RESTful API to allow client applications to interact with the MicroCoin network.
You can communicate with Rider using cURL or just your web browser. However, if you’re building a client application, you’ll likely want to use a MicroCoin SDK in the language of your client.

# Before you begin
Before you start developing useful to download the MicroCoin wallet. You can download the latest version from here [MicroCoin Wallet](https://github.com/MicroCoinHU/MicroCoinWallet)

# Building

1. Download MicroCoin wallett, or build a daemon and run it
2. Clone this repository and init submodules
3. Run `dotnet build`
4. Run `dotnet publish`
5. Run the server using the `dotnet run` command

# Run your node on Ubuntu

Download and run [microcoind](https://github.com/MicroCoinHU/microcoind)

**Install dotnetcore**
```bash
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1
```

**Publish the project**
```bash
dotnet publish --configuration Release
```

**Copy the files to the server (eg. to /var/microcoinapi)**

**Create a service**
```bash
sudo nano /etc/systemd/system/microcoin.service
```
Paste this:
```ini
[Unit]
Description=MicroCoin API

[Service]
WorkingDirectory=/var/microcoinapi
ExecStart=/usr/bin/dotnet /var/microcoinapi/MicroCoinApi.dll
Restart=always
RestartSec=10  # Restart service after 10 seconds if dotnet service crashes
SyslogIdentifier=microcoin-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

**Enable the service:**
```bash
sudo systemctl enable microcoin.service
```

**Start the service:**
```bash
sudo systemctl start microcoin.service
```

To check your node, point your browser to: http://yourdomain.com:5000

**Install nginx**
```bash
sudo apt-get install nginx
sudo /etc/init.d/nginx start
```

**Configure nginx**
```nginx
server {
    listen        80;
    server_name   yourdomain.com *.yourdomain.com;
    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```
**Configure the firewall**
```bash
sudo apt-get install ufw
sudo ufw enable

sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 4004/tcp

# on testnet: sudo ufw allow 4104/tcp
```
