using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCoinApi
{
    public class MicroCoinHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("Transaction", message);
        }
    }
}
