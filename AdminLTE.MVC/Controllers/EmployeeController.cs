﻿using AdminLTE.Models;
using AdminLTE.Models.ViewModels;
using AdminLTE.MVC.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdminLTE.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _wb;
        public EmployeeController(ApplicationDbContext db, IWebHostEnvironment wb)
        {
            _db = db;
            _wb = wb;
        }

        public IActionResult Index()
        {
            EmployeesLocalCommunitiesVM vm = new EmployeesLocalCommunitiesVM()
            {
                Employees = _db.Employees.Include(x => x.LocalCommunity),
                LocalCommunities = _db.LocalCommunities.Select(x => new SelectListItem()
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                }),
                LocalCommunity = new LocalCommunity()
            };
            return View(vm);
        }
        [HttpPost]
        public IActionResult Index(EmployeesLocalCommunitiesVM vm)
        {
            EmployeesLocalCommunitiesVM _vm = new EmployeesLocalCommunitiesVM
            {
                LocalCommunities = _db.LocalCommunities.Select(x => new SelectListItem()
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                }),
                LocalCommunity = new LocalCommunity()
            };
            if (vm.LocalCommunity.Id == 0)
                _vm.Employees = _db.Employees.Include(x => x.LocalCommunity);
            else
                _vm.Employees = _db.Employees.Where(x => x.LocalCommunityId == vm.LocalCommunity.Id).Include(x => x.LocalCommunity);

            return View(_vm);
        }
        public IActionResult Upsert(int? id)
        {
            EmployeeLCVM lCVM = new EmployeeLCVM()
            {
                Employee = new Employee(),
                LocalCommunities = _db.LocalCommunities.Select(x => new SelectListItem()
                {
                    Text = x.Title,
                    Value = x.Id.ToString()
                })
            };

            return View(lCVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(EmployeeLCVM emp)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _wb.WebRootPath;
                string upload = webRootPath + ENV.ImagePath;

                string fileName = "";
                string extensions = "";
                if (files.Count() == 0)
                {
                    fileName = ENV.NoImageName;
                }
                else
                {
                    fileName = Guid.NewGuid().ToString();
                    extensions = Path.GetExtension(files[0].FileName);
                }

                if (emp.Employee.Id == 0)
                {
                    if (files.Count() != 0)
                    {
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extensions), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                    }

                    emp.Employee.PhotoPath = fileName + extensions;
                    _db.Employees.Add(emp.Employee);
                }
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Upsert));
        }
        public IActionResult Cancel()
        {
            return RedirectToAction(nameof(Upsert));
        }
    }
}
