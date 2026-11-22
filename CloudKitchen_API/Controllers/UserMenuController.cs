    using CloudKitchen_API.Services.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    namespace CloudKitchen_API.Controllers
    {
        [Authorize]
        [Route("api/[controller]")]
        [ApiController]
        public class UserMenuController : ControllerBase
        {
            private readonly IUserMenuService _userMenuService;
            public UserMenuController(IUserMenuService userMenuService)
            {
                _userMenuService = userMenuService;
            }

            [HttpGet("GetMenu")]
            public async Task<IActionResult> GetMenu()
            {
                var items = await _userMenuService.GetAvailableMenuAsync();
                return Ok(items);
            }

            [HttpGet("GetCategories")]
            public async Task<IActionResult> GetCategories()
            {
                var items = await _userMenuService.GetCategoriesAsync();
                return Ok(items);
            }

        }
    }
