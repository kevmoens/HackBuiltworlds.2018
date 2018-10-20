using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartHome.Service
{
    public static class Sessions
    {
        public static System.Collections.Concurrent.ConcurrentDictionary<string, Guid> Clients = new System.Collections.Concurrent.ConcurrentDictionary<string, Guid>();

        public static List<Shared.BulbAddedDto> Notes { get; set; }
    }
}