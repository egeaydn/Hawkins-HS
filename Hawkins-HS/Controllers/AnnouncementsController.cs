using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Hawkins_HS.Data;
using Hawkins_HS.Models;
using Hawkins_HS.ViewModels;
using Hawkins_HS.Services;

namespace Hawkins_HS.Controllers;

[Authorize]
public class AnnouncementsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<AnnouncementsController> _logger;
    private readonly INotificationService _notificationService;

    public AnnouncementsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ILogger<AnnouncementsController> logger,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
    }

    // GET: Announcements
    public async Task<IActionResult> Index()
    {
        var announcements = await _context.Announcements
            .Include(a => a.Creator)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        // Filter by user role
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user!);

        if (roles.Contains(RoleConstants.Student))
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user!.Id);

            announcements = announcements.Where(a =>
                a.Audience == "All" ||
                a.Audience == "Students" ||
                (student != null && a.Audience == $"Class:{student.ClassName}")
            ).ToList();
        }
        else if (roles.Contains(RoleConstants.Teacher))
        {
            announcements = announcements.Where(a =>
                a.Audience == "All" ||
                a.Audience == "Teachers"
            ).ToList();
        }

        var viewModels = _mapper.Map<List<AnnouncementViewModel>>(announcements);
        return View(viewModels);
    }

    // GET: Announcements/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var announcement = await _context.Announcements
            .Include(a => a.Creator)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (announcement == null) return NotFound();

        var viewModel = _mapper.Map<AnnouncementViewModel>(announcement);
        return View(viewModel);
    }

    // GET: Announcements/Create
    [Authorize(Roles = "Admin,Teacher")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Announcements/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(AnnouncementViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var announcement = _mapper.Map<Announcement>(viewModel);
            announcement.CreatorId = user!.Id;
            announcement.CreatedAt = DateTime.UtcNow;
            
            _context.Add(announcement);
            await _context.SaveChangesAsync();
            
            // Bildirim gönder
            await _notificationService.NotifyAnnouncementCreatedAsync(announcement);
            
            _logger.LogInformation("Announcement created: {Title} by {User}", announcement.Title, user.UserName);
            TempData["Success"] = "Duyuru başarıyla oluşturuldu.";
            
            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    // GET: Announcements/Edit/5
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null) return NotFound();

        var viewModel = _mapper.Map<AnnouncementViewModel>(announcement);
        return View(viewModel);
    }

    // POST: Announcements/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id, AnnouncementViewModel viewModel)
    {
        if (id != viewModel.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null) return NotFound();

                announcement.Title = viewModel.Title;
                announcement.Body = viewModel.Body;
                announcement.Audience = viewModel.Audience;
                announcement.IsImportant = viewModel.IsImportant;
                
                _context.Update(announcement);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Announcement updated: {Title}", announcement.Title);
                TempData["Success"] = "Duyuru başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnnouncementExists(viewModel.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    // GET: Announcements/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var announcement = await _context.Announcements
            .Include(a => a.Creator)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (announcement == null) return NotFound();

        return View(announcement);
    }

    // POST: Announcements/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement != null)
        {
            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Announcement deleted: {Title}", announcement.Title);
            TempData["Success"] = "Duyuru başarıyla silindi.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool AnnouncementExists(int id)
    {
        return _context.Announcements.Any(e => e.Id == id);
    }
}
