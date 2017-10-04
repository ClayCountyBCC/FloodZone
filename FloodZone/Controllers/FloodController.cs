using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FloodZone.Models;

namespace FloodZone.Controllers
{
  public class FloodController : ApiController
  {
    public List<FoundAddress> Post(SearchAddress sa)
    {
      return FoundAddress.Find(sa);
    }
  }
}
