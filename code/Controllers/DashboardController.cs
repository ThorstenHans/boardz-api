using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardZ.API.Extensions;
using BoardZ.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoardZ.API.Controllers
{
    [Route("/api/[controller]")]
    public class DashboardController: Controller
    {
        public DashboardController(DashboardService dashboardService)
        {
            DashboardService = dashboardService;
        }

        protected DashboardService DashboardService { get; }

        [HttpGet]
        [Route("stats")]
        public IActionResult Stats()
        {
            var stats = DashboardService.GetStats(User.GetSubjectOrThrow());
            return Ok(stats);
        }
    }
}
