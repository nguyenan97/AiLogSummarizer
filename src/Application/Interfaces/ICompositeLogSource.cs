using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Shared;

namespace Application.Interfaces
{
    public interface ICompositeLogSource
    {
        Task<List<TraceLog>> GetLogsAsync(GetLogModel model);
    }
}
