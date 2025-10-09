using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Utils
{
    public static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
