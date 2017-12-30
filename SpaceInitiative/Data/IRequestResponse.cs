using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceInitiative.Data
{
    public interface IRequestResponse
    {
        HttpRequest PageRequest { get; }
        HttpResponse PageResponse { get; }
    }
}
