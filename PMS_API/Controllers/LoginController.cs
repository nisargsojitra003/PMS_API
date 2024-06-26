using Microsoft.AspNetCore.Mvc;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API.Controllers
{
    [ApiController]
    [Route("login")]
    public class LoginController : ControllerBase
    {
        private readonly ILogin _LoginService;
        private readonly IJwt _JwtService;
        public LoginController(ILogin login, IJwt jwtService)
        {
            _LoginService = login;
            _JwtService = jwtService;
        }

        #region Login Method
        /// <summary>
        /// Check user is validate
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns>indicating success or failure.</returns>
        [HttpPost("login", Name = "UserLogin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login([FromForm] Login userInfo)
        {
            if (await _LoginService.CheckEmailInDb(userInfo.Email))
            {
                return NotFound();
            }

            AspNetUser user = await _LoginService.LoginUser(userInfo);

            if (user != null)
            {
                string jwtToken = _JwtService.GenerateToken(user);
                //Response.Cookies.Append("jwt", jwtToken);

                string actionName = "";
                string controllerName = "";
                switch (user.Role)
                {
                    case "Admin":
                        actionName = "index";
                        controllerName = "home";
                        break;
                    case "User":
                        actionName = "index";
                        controllerName = "home";
                        break;
                }

                var response = new
                {
                    Action = actionName,
                    Controller = controllerName,
                    JwtToken = jwtToken,
                    role = user.Role,
                    userId = user.Id
                };

                return Ok(response);
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion

        #region CreateAccount Post Method
        /// <summary>
        /// Create account method
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns>indicating success or failure</returns>
        [HttpPost("createaccount", Name = "RegisterUser")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> CreateAccount([FromForm] Login userInfo)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            if (await _LoginService.CheckEmailInDb(userInfo.Email))
            {
                await _LoginService.CreateNewUser(userInfo);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion

        #region Logout
        /// <summary>
        /// Logout method
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout", Name = "Logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            if (Request.Cookies["jwt"] != null)
            {
                Response.Cookies.Delete("jwt");
                return Ok(new { message = "Logout successful" });
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion
    }
}