﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Entities;
using ProjectManagementApp.DataAccess;
using ProjectManagementApp.Models;
using System.IO;

namespace ProjectManagementApp.Controllers
{
    public class ProjectController : Controller
    {
        public ProjectController(ProjectManagementDbContext projectManagementDbContext)
        {
            _projectManagementDbContext = projectManagementDbContext;
        }

        [HttpGet("/projects")]
        public IActionResult GetAllProjects()
        {
            var projects = _projectManagementDbContext.Projects
                    .Include(p => p.Employees)
                    .Include(p => p.Tasks)
                    .OrderByDescending(p => p.DateCreated)
                    .ToList();

            return View("Items", projects);
        }

        [HttpGet("/projects/{id}")]
        public IActionResult GetProjectById(int id)
        {
            var project = _projectManagementDbContext.Projects
                .Include(p => p.Employees)
                .Include(p => p.Tasks)
                .Where(p => p.ProjectId == id)
                .FirstOrDefault();
            if (project == null)
                return NotFound();

            ProjectDetailsViewModel projectDetailsViewModel = new ProjectDetailsViewModel()
            {
                ActiveProject = project,
                InProgressTaskCount = project.Tasks.Where(t => t.TaskStatus == TaskStatusOptions.InProgress).Count(),
                CompletedTaskCount = project.Tasks.Where(t => t.TaskStatus == TaskStatusOptions.Completed).Count(),
                CancelledTaskCount = project.Tasks.Where(t => t.TaskStatus == TaskStatusOptions.Cancelled).Count(),

            };
            return View("Details", projectDetailsViewModel);

        }

        [HttpPost("projects/{id}/employees")]
        public IActionResult AddNewEmployee(int id, ProjectDetailsViewModel projectDetailsViewModel)
        {
            var project = _projectManagementDbContext.Projects
                 .Include(p => p.Employees)
                 .Include(p => p.Tasks)
                 .Where(p => p.ProjectId == id)
                 .FirstOrDefault();
            if (ModelState.IsValid)
            {
                project.Employees.Add(projectDetailsViewModel.NewEmployee);
                _projectManagementDbContext.SaveChanges();
                TempData["LastActionMessage"] = $"The employee \"{projectDetailsViewModel.NewEmployee.FullName}\" was added.";
                return RedirectToAction("GetProjectById", "Project", new {id = id});
            }
            else
            {
                return RedirectToAction("GetProjectById", "Project", new { id = id });
            }
        }
        [HttpPost("projects/{id}/employees")]
        public IActionResult AddNewProjectTask(int id, ProjectDetailsViewModel projectDetailsViewModel)
        {
            var project = _projectManagementDbContext.Projects
                 .Include(p => p.Employees)
                 .Include(p => p.Tasks)
                 .Where(p => p.ProjectId == id)
                 .FirstOrDefault();
            if (ModelState.IsValid)
            {
                project.Tasks.Add(projectDetailsViewModel.NewProjectTask);
                _projectManagementDbContext.SaveChanges();
                TempData["LastActionMessage"] = $"The task \"{projectDetailsViewModel.NewProjectTask.Description}\" was added.";
                return RedirectToAction("GetProjectById", "Project", new { id = id });
            }
            else
            {
                return RedirectToAction("GetProjectById", "Project", new { id = id });
            }
        }
        [Authorize()]
        [HttpGet("/projects/add-request")]
        public IActionResult GetAddProjectRequest()
        {
            return View("AddProject", new Project());
        }

        [HttpPost("/projects")]
        public IActionResult AddNewProject(Project project)
        {
            if (ModelState.IsValid)
            {
                _projectManagementDbContext.Projects.Add(project);
                _projectManagementDbContext.SaveChanges();

                TempData["LastActionMessage"] = $"The project \"{project.Name}\" was added.";

                return RedirectToAction("GetAllProjects", "Project");
            }
            else
            {
                return View("AddProject", project);
            }
        }
        [Authorize()]
        [HttpGet("/projects/{id}/edit-request")]
        public IActionResult GetEditRequestById(int id)
        {
            var project = _projectManagementDbContext.Projects.Find(id);
            return View("EditProject", project);
        }

        [HttpPost("/projects/edit-requests")]
        public IActionResult ProcessEditRequest(Project project)
        {
            if (ModelState.IsValid)
            {
                _projectManagementDbContext.Projects.Update(project);
                _projectManagementDbContext.SaveChanges();

                TempData["LastActionMessage"] = $"The project \"{project.Name}\" was updated.";

                return RedirectToAction("GetAllProjects", "Project");
            }
            else
            {
                return View("EditProject", project);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("/projects/{id}/delete-request")]
        public IActionResult GetDeleteRequestById(int id)
        {
            var project = _projectManagementDbContext.Projects.Find(id);
            return View("DeleteConfirmation", project);
        }

        [HttpPost("/projects/{id}/delete-requests")]
        public IActionResult ProcessDeleteRequestById(int id)
        {
            var project = _projectManagementDbContext.Projects.Find(id);

            _projectManagementDbContext.Projects.Remove(project);
            _projectManagementDbContext.SaveChanges();

            TempData["LastActionMessage"] = $"The project \"{project.Name}\" was deleted.";

            return RedirectToAction("GetAllProjects", "Project");
        }

        private ProjectManagementDbContext _projectManagementDbContext;
    }
}
