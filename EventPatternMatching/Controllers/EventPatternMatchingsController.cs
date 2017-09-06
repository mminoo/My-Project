using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventPatternMatching.Models;
using System.IO;

namespace EventPatternMatching.Controllers
{
    public class EventPatternMatchingsController : Controller
    {
        private readonly EventPatternMatchingContext _context;

        public EventPatternMatchingsController(EventPatternMatchingContext context)
        {
            _context = context;
        }

        // GET: EventPatternMatchings
        public async Task<IActionResult> Index()
        {
            return View(await _context.EventPatternMatching.ToListAsync());
        }

       

       

        // POST: EventPatternMatchings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFault(string deviceId, string filename)
        {

            FileStream fileStream = new FileStream(filename, FileMode.Open);
            StreamReader reader = new StreamReader(fileStream);

            var objClass = new ParserEvent();
            objClass.ParseEvents(deviceId, reader);
			return RedirectToAction(nameof(Index));
        }

       
        private bool EventPatternMatchingExists(int id)
        {
            return _context.EventPatternMatching.Any(e => e.ID == id);
        }
    }
}
