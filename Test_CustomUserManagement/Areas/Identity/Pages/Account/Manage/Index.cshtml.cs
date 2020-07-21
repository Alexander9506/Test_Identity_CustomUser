﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Test_CustomUserManagement.Areas.Identity.Data;
using Test_CustomUserManagement.Models;
using Test_CustomUserManagement.Models.Repositories;

namespace Test_CustomUserManagement.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IFileRepository _fileRepository;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IFileRepository fileRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _configuration = configuration;
            _fileRepository = fileRepository;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name ="Last Name")]
            public string LastName { get; set; }
            [Display(Name ="Username")]
            public string Username { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name ="Profile Picture")]
            public String ProfilePicturePath { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            var firstName = user.FirstName;
            var lastName = user.LastName;
            var profilePicturePath = user.ProfilePicturePath;

            Username = userName;

            Input = new InputModel
            {
                Username = userName,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                ProfilePicturePath = profilePicturePath
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var firstName = user.FirstName;
            if(Input.FirstName != firstName)
            {
                user.FirstName = Input.FirstName;
                await _userManager.UpdateAsync(user);
            }

            var lastName = user.LastName;
            if(Input.LastName != lastName)
            {
                user.LastName = Input.LastName;
                await _userManager.UpdateAsync(user);
            }

            if(Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                FileContainer fileContainer = await SaveProfilePictureAsync(file);
                if(fileContainer != null)
                {
                    user.ProfilePicturePath = fileContainer.FilePathFull;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    StatusMessage = "Unexpected error when trying to set Profile Picture.";
                    return RedirectToPage();
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        private async Task<FileContainer> SaveProfilePictureAsync(IFormFile file)
        {
            String basePath = Path.Combine(_environment.WebRootPath, _configuration.GetValue<String>("ProfileImagePath"));
            String[] allowedFileExtensions = _configuration.GetSection("AllowedFileExtensions").Get<String[]>();

            IFormFile formFile = file;

            //New random Filename
            string originalFileExtension = Path.GetExtension(formFile.FileName).Replace(".", string.Empty);
            string newFileName = Path.GetRandomFileName();
            if (allowedFileExtensions.Contains(originalFileExtension.ToLower()))
            {
                if (newFileName.Contains("."))
                {
                    newFileName = newFileName.Substring(0, newFileName.IndexOf(".") - 1);
                    newFileName += "." + originalFileExtension;
                }
            }

            //Create and Save FileContainer
            FileContainer container = FileContainerFactory.CreateFileContainer(formFile, basePath, newFileName);

            if (container != null)
            {
                try
                {
                    using (var stream = System.IO.File.Create(container.FilePathFull))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    await _fileRepository.SaveFileContainer(container);
                    //TODO: delete old ProfilePicture
                    return container;
                }
                catch (Exception e)
                {
                    //TODO: Logging
                }
            }
            return null;
        }
    }
}
