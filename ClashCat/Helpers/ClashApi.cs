﻿using ClashCat.Models.Clash.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClashCat.Helpers
{
    internal class ClashApi
    {
        public ClashApi(Uri baseAddress)
        {
            Client = new HttpClient();
            Client.BaseAddress = baseAddress;
        }

        public HttpClient Client { get; }

        public Task<ClashBasicConfig?> GetConfigAsync()
        {
            return Client.GetFromJsonAsync<ClashBasicConfig>("/configs", default);
        }


    }
}
