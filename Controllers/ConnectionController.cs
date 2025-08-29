using Microsoft.AspNetCore.Mvc;
using VPN_RDP_Manager_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace VPN_RDP_Manager_Web.Controllers
{
    public class ConnectionController : Controller
    {
        private readonly VPNContext _context;
        private readonly IConfiguration _config; // IConfiguration eklendi

        public ConnectionController(VPNContext context, IConfiguration config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // Index
        public IActionResult Index()
        {
            if (_config == null)
                return Content("Config null!");

            string kullaniciKodu = _config["KULLANICI_KODU"];
            if (string.IsNullOrEmpty(kullaniciKodu))
                return Content("KULLANICI_KODU null veya boş!");

            var connections = _context.CONNECTIONS
                                      .Where(c => c.KULLANICI_KODU == kullaniciKodu)
                                      .ToList();
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
                connection.KULLANICI_KODU = _config["KULLANICI_KODU"]; // otomatik set

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
            string kullaniciKodu = _config["KULLANICI_KODU"];

            var connection = _context.CONNECTIONS
                .FirstOrDefault(c => c.SYS_NO == id && c.KULLANICI_KODU == kullaniciKodu);

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
                string kullaniciKodu = _config["KULLANICI_KODU"];

                // ExecuteUpdate ile güncelleme (sadece ilgili kullanıcıya ait kaydı günceller)
                var rowsAffected = _context.CONNECTIONS
                    .Where(c => c.SYS_NO == id && c.KULLANICI_KODU == kullaniciKodu)
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
            string kullaniciKodu = _config["KULLANICI_KODU"];

            // ExecuteDelete ile sadece ilgili kullanıcıya ait kaydı sil
            var rowsAffected = _context.CONNECTIONS
                .Where(c => c.SYS_NO == id && c.KULLANICI_KODU == kullaniciKodu)
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
