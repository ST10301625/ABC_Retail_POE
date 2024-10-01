using Azure;
using Azure.Data.Tables;
using Cloud_Storage.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Cloud_Storage.Controllers
{
    public class AccountController : Controller
    {
        private readonly TableClient _tableClient;

        public AccountController()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=st10301625;AccountKey=316EDPhrzRysRzRYGE1E2tyEMW0JEEu4QMzqdsmOGQA5+GoCSkWWox0i44gkS/lSoDEEukt9Urm8+ASt1rbOcA==;EndpointSuffix=core.windows.net";
            string tableName = "sign";

            // Initialize the TableClient for the "sign" table
            _tableClient = new TableClient(connectionString, tableName);

            // Ensure the table exists, otherwise create it
            _tableClient.CreateIfNotExists();
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if passwords match
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View(model);
                }

                // Create a new UserEntity
                var userEntity = new UserEntity(model.Username, model.Email, model.Password);

                // Insert the user into Table Storage
                _tableClient.AddEntity(userEntity);

                // Redirect to a success page (e.g., login page or confirmation)
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Loginn()
        {
            return View();
        }

    }
}
