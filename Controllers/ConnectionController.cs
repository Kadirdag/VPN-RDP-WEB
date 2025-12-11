using Microsoft.AspNetCore.Mvc;
using VPN_RDP_Manager_Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;


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


        [HttpGet]
        public JsonResult CheckServerStatus(string ip)
        {
            // ADIM 1: Değişkeni EN BAŞTA tanımlıyoruz (Hatanın çözümü bu)
            bool isOnline = false;

            // IP boş gelirse direkt false dönüyoruz
            if (string.IsNullOrEmpty(ip))
            {
                return Json(new { isOnline = false });
            }

            try
            {
                using (var client = new TcpClient())
                {
                    // 3389 (RDP) Portuna bağlanmayı dene
                    var result = client.BeginConnect(ip, 3389, null, null);

                    // 1 Saniye bekle (Timeout)
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                    if (success)
                    {
                        client.EndConnect(result);
                        // Burada isOnline değişkenini kullanıyoruz, o yüzden yukarıda tanımlı olması şart!
                        isOnline = true;
                    }
                }
            }
            catch
            {
                // Hata durumunda
                isOnline = false;
            }

            // ADIM 2: Sonucu gönderiyoruz
            return Json(new { isOnline = isOnline });
        }


        [HttpGet]
        public IActionResult DownloadRdp(int id)
        {
            var item = _context.CONNECTIONS.FirstOrDefault(x => x.SYS_NO == id);

            if (item == null) return NotFound();

            // --- HATA ÇÖZÜMÜ BURADA ---
            // Null gelme ihtimaline karşı önlem aldık.
            int gelenPort = item.PORT.GetValueOrDefault();
            int portToUse = (gelenPort == 0) ? 3389 : gelenPort;
            // --------------------------

            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"full address:s:{item.IP}:{portToUse}");
            sb.AppendLine($"username:s:{item.KULLANICI}");
            sb.AppendLine("screen mode id:i:2");
            sb.AppendLine("session bpp:i:32");
            sb.AppendLine("compression:i:1");
            sb.AppendLine("keyboardhook:i:2");
            sb.AppendLine("displayconnectionbar:i:1");
            sb.AppendLine("disable wallpaper:i:0");
            sb.AppendLine("allow font smoothing:i:1");

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

            string fileName = $"{item.KURUM}_{item.IP}.rdp";

            return File(fileBytes, "application/x-rdp", fileName);
        }




    }
}
