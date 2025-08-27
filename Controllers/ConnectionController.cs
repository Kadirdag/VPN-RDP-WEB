using Microsoft.AspNetCore.Mvc;
using VPN_RDP_Manager_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace VPN_RDP_Manager_Web.Controllers
{
    public class ConnectionController : Controller
    {
        private readonly VPNContext _context;

        public ConnectionController(VPNContext context)
        {
            _context = context;
        }

        // Index
        public IActionResult Index()
        {
            var connections = _context.CONNECTIONS.ToList();
            return View(connections);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Connection connection)
        {
            if (ModelState.IsValid)
            {
                connection.KAYIT_ANI = DateTime.Now;
                _context.CONNECTIONS.Add(connection);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(connection);
        }

        // GET: Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var connection = _context.CONNECTIONS.FirstOrDefault(c => c.SYS_NO == id);
            if (connection == null)
                return NotFound();

            return View(connection);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Connection connection)
        {
            if (id != connection.SYS_NO)
                return BadRequest();

            if (ModelState.IsValid)
            {
                // ExecuteUpdate kullanarak trigger ile uyumlu şekilde güncelle
                var rowsAffected = _context.CONNECTIONS
                    .Where(c => c.SYS_NO == id)
                    .ExecuteUpdate(s => s
                        .SetProperty(c => c.KURUM, connection.KURUM)
                        .SetProperty(c => c.TIP, connection.TIP)
                        .SetProperty(c => c.IP, connection.IP)
                        .SetProperty(c => c.PORT, connection.PORT)
                        .SetProperty(c => c.KULLANICI, connection.KULLANICI)
                        .SetProperty(c => c.SIFRE, connection.SIFRE)
                        .SetProperty(c => c.NOTLAR, connection.NOTLAR)
                    );

                if (rowsAffected == 0)
                    return NotFound();

                return RedirectToAction(nameof(Index));
            }
            return View(connection);
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // ExecuteDelete ile trigger ile uyumlu silme
            var rowsAffected = _context.CONNECTIONS
                .Where(c => c.SYS_NO == id)
                .ExecuteDelete();

            if (rowsAffected == 0)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Filter(string kurum)
        {
            var data = _context.CONNECTIONS
                               .Where(x => x.KURUM == kurum)
                               .ToList();

            return PartialView("_ConnectionList", data);
        }
    }
}
