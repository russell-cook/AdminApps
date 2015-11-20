using AdminApps.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AdminApps.Controllers
{
    public class DeptsController : ApiController
    {
        NebsContext _nebsDb;

        public DeptsController()
        {
            _nebsDb = new NebsContext();
        }

        public IHttpActionResult Get()
        {
            try
            {
                var depts = _nebsDb.NebsDepts;

                return Ok(depts.ToList());
            }
            catch
            {
                return InternalServerError();
            }
        }

        public IHttpActionResult Get(int id)
        {
            try
            {
                var depts = _nebsDb.NebsDepts.Where(d => d.NebsBudgetPeriodID == (decimal)id);

                return Ok(depts.ToList());
            }
            catch
            {
                return InternalServerError();
            }
        }

    }
}
