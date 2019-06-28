using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Examen.Services;
using Examen.Models;
using Examen.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Examen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUsersService _userService;

        public UsersController(IUsersService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// GETS THE USER WHO IS CURRENTLY LOGGED IN
        /// </summary>
        /// <returns>The user who is currently logged in</returns>
        private User GetLoggedInUser()
        {
            return _userService.GetLoggedInUser(HttpContext);
        }


        /// <summary>
        /// AUTHENTICATES/LOGS IN AN EXISTING USER
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     {
        ///         "Username" : "name1",
        ///         "Password" : "123456"
        ///     }
        /// </remarks>
        /// <param name="login.Username"> username</param>
        /// <param name="login.Password"> password</param>
        /// <returns> The logged in user or null if the authentication failed (invalid username/password)</returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Authenticate([FromBody]LoginPostModel login)
        {
            var user = _userService.Authenticate(login.Username, login.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }


        /// <summary>
        /// REGISTER/POST : CREATES A NEW USER
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST/User
        /// {
        ///     "FirstName": "admin1",
        ///     "LastName": "admin1",
        ///     "Username": "admin1",
        ///     "Email": "admin@a.a",
        ///     "Password": "123456789"
        /// }
        /// </remarks>
        /// <param name="registerModel">a registerModel of the user to create</param>
        /// <returns>The newly created user</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]RegisterPostModel registerModel)
        {
            var user = _userService.Register(registerModel);
            return Ok(user);
        }


        /// <summary>
        /// GETS ALL USERS
        /// </summary>
        /// <returns>The list of Users</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult GetAll()
        {
            int forbidden;

            var users = _userService.GetAll(GetLoggedInUser(), out forbidden);

            if (forbidden == 1)
            {
                return Forbid();
            }
            else
            {
                return Ok(users);
            }
        }

        /// <summary>
        /// GETS USER BY ID
        /// </summary>
        /// <param name="userId">Given id</param>
        /// <returns>A User object</returns>
        [HttpGet("{userId}")]
        [Authorize(Roles = "Admin,Moderator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUser(int userId)
        {
            int notFoundOrForbidden;

            var existing = this._userService.GetUserById(userId, GetLoggedInUser(), out notFoundOrForbidden);

            if (existing == null)
            {
                if (notFoundOrForbidden == 1)
                {
                    return NotFound();
                }
                if (notFoundOrForbidden == 2)
                {
                    return Forbid();
                }
            }

            return Ok(existing);
        }


        /// <summary>
        /// DELETE: DELETES USER
        /// </summary>
        /// <param name="id">User id to delete</param>
        /// <param name="loggedInUser">The logged in user who does the delete action</param>
        /// <returns>The deleted user or null if there is no user with the given id</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        public IActionResult Delete(int id)
        {
            int notFoundOrForbidden;
            var result = _userService.Delete(id, GetLoggedInUser(), out notFoundOrForbidden);
            if (result == null)
            {
                if (notFoundOrForbidden == 1)
                {
                    return NotFound();
                }
                if (notFoundOrForbidden == 2)
                {
                    return Forbid();
                }
            }
            result.Password = null;

            return Ok(result);
        }

        /// <summary>
        ///  PUT: UPSERTS USER (Update/Insert User)
        /// </summary>
        /// <param name="id">the user id to upsert</param>
        /// <param name="user">User to upsert</param>
        /// <param name="loggedInUser">The logged in user who does the upsert action</param>
        /// <returns>The upsert user</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Moderator,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Put(int id, [FromBody] User user)
        {

            User loggedInUser = _userService.GetLoggedInUser(HttpContext);

            var result = _userService.Upsert(id, user, loggedInUser);
            if (result == null)
            {
                return Forbid();
            }
            result.Password = null;

            return Ok(user);
        }

    }
}