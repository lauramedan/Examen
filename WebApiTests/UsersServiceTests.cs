using Examen.Models;
using Examen.Services;
using Examen.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class UsersServiceTests
    {
        private IOptions<AppSettings> config;

        [SetUp]
        public void Setup()
        {
            config = Options.Create(new AppSettings
            {
                Secret = "qwruiolmkjdertykhgi"
            });

        }

        [Test]
        public void ValidRegisterShouldCreateNewUser()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ValidRegisterShouldCreateNewUser))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                var added = new Examen.ViewModels.RegisterPostModel
                {
                    Email = "x@y.z",
                    FirstName = "abc",
                    LastName = "def",
                    Password = "1234567",
                    Username = "abc"

                };

                var result = userService.Register(added);

                Assert.IsNotNull(result);
                Assert.AreEqual(added.Username, result.Username);
            }

        }

        [Test]
        public void InvalidRegisterShouldNotCerateNewUserWithTheSameUsername()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(InvalidRegisterShouldNotCerateNewUserWithTheSameUsername))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                var added1 = new Examen.ViewModels.RegisterPostModel
                {
                    Email = "aaa@bbb.ccc",
                    FirstName = "aaa",
                    LastName = "bbb",
                    Password = "123456789",
                    Username = "xxx"

                };
                // dam acelasi username
                var added2 = new Examen.ViewModels.RegisterPostModel
                {
                    Email = "xxx@yyy.zzz",
                    FirstName = "xxx",
                    LastName = "yyy",
                    Password = "12345678910",
                    Username = "xxx"

                };

                userService.Register(added1);
                var result = userService.Register(added2);

                Assert.IsNull(result);
            }

        }

        [Test]
        public void ValidAuthenticationShouldAuthenticateValidUser()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ValidAuthenticationShouldAuthenticateValidUser))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                var addedUser = new Examen.ViewModels.RegisterPostModel
                {
                    Email = "d@e.f",
                    FirstName = "ddd",
                    LastName = "eee",
                    Password = "111222333",
                    Username = "ddd"

                };

                var addResult = userService.Register(addedUser);

                Assert.IsNotNull(addResult);
                Assert.AreEqual(addedUser.Username, addResult.Username);

                var authenticate = new Examen.ViewModels.UserGetModel
                {
                    Email = "d@e.f",
                    Username = "ddd"
                };

                var result = userService.Authenticate(addedUser.Username, addedUser.Password);

                Assert.IsNotNull(result); //metoda Authenticate nu a returnat null

                Assert.AreEqual(authenticate.Username, result.Username); // Mai mult, UserGetModel returnat de metoda Authenticate e identic cu obiectul creat initial (e acelasi)
            }

        }

        [Test]
        public void InvalidAuthenticationShouldNotAuthenticateUserWithInvalidPassword()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(InvalidAuthenticationShouldNotAuthenticateUserWithInvalidPassword))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                var addedUser = new Examen.ViewModels.RegisterPostModel
                {
                    Email = "g@h.i",
                    FirstName = "ggg",
                    LastName = "hhh",
                    Password = "12345678",
                    Username = "ggg",
                    UserRole = UserRole.Regular

                };

                var addResult = userService.Register(addedUser);

                Assert.IsNotNull(addResult);
                Assert.AreEqual(addedUser.Username, addResult.Username);


                //dam o parola gresita
                var result = userService.Authenticate(addedUser.Username, "11111111");

                Assert.IsNull(result);
            }

        }


        ///////////////////
        ///

        [Test]
        public void AnAdminShouldBeAbleToInsertAnyTypeOfUser()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnAdminShouldBeAbleToInsertAnyTypeOfUser))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User managerUserToBeAdded = new User();
                managerUserToBeAdded.UserRole = UserRole.Moderator;
                managerUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

            }
        }

        [Test]
        public void AnAdminShouldBeAbleToUpdateAnyTypeOfUser()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnAdminShouldBeAbleToUpdateAnyTypeOfUser))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // il fac pe regular user moderator
                regularUserToBeAdded.UserRole = UserRole.Moderator;

                id = 1; // dam id-ul corect
                rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                //il fac pe moderator admin
                moderatorUserToBeAdded.UserRole = UserRole.Admin;

                id = 2; // dam id-ul corect
                mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                //il fac pe admin regular
                adminUserToBeAdded.UserRole = UserRole.Regular;

                id = 3; // dam id-ul corect
                aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // verific ca schimbarile au avut loc
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(mUser.UserRole, UserRole.Admin);
                Assert.AreEqual(aUser.UserRole, UserRole.Regular);

            }
        }

        [Test]
        public void AnAdminShouldBeAbleToDeleteAnyTypeOfUser()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnAdminShouldBeAbleToDeleteAnyTypeOfUser))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User managerUserToBeAdded = new User();
                managerUserToBeAdded.UserRole = UserRole.Moderator;
                managerUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                int notFoundOrForbidden;

                id = 1; // dam id-ul corect
                rUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 0);

                id = 2; // dam id-ul corect
                mUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 0);

                id = 3; // dam id-ul corect
                aUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 0);

                int numberOfCurrentUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                Assert.AreEqual(numberOfCurrentUsers, 0);

            }
        }

        [Test]
        public void ANewModeratorShouldBeAbleToInsertOnlyRegularUsers()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ANewModeratorShouldBeAbleToInsertOnlyRegularUsers))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Moderator;
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-5); // ne asiguram ca moderatorul are doar 5 luni vechime (azi - 5 luni)

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User managerUserToBeAdded = new User();
                managerUserToBeAdded.UserRole = UserRole.Moderator;
                managerUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                loggedInUser.UserRole = UserRole.Admin; // ne logam ca admin ca sa vedem toti userii inserati
                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                Assert.AreEqual(numberOfAddedUsers, 1); //a fost inserat doar un user din cei 3
                Assert.AreEqual(rUser.UserRole, UserRole.Regular); // acel user inserat e regular
                Assert.IsNull(mUser); // celelalte inserturi au esuat (moderator)
                Assert.IsNull(aUser); // celelalte inserturi au esuat (admin)

            }
        }

        [Test]
        public void AnOldModeratorShouldBeAbleToInsertBothRegularUsersAndModerators()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnOldModeratorShouldBeAbleToInsertBothRegularUsersAndModerators))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Moderator;
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-7); // ne asigura ca moderatorul are 7 luni vechime (>6)

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User managerUserToBeAdded = new User();
                managerUserToBeAdded.UserRole = UserRole.Moderator;
                managerUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                loggedInUser.UserRole = UserRole.Admin; // ne logam ca admin ca sa vedem toti userii inserati
                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                Assert.AreEqual(numberOfAddedUsers, 2);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular); // regular a fost adaugat
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator); //moderator a fost adaugat
                Assert.IsNull(aUser); //adminul nu a fost adaugat

            }
        }

        [Test]
        public void ANewModeratorShouldBeAbleToUpdateOnlyRegularUsers()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ANewModeratorShouldBeAbleToUpdateOnlyRegularUsers))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User managerUserToBeAdded = new User();
                managerUserToBeAdded.Username = "Ghitza";
                managerUserToBeAdded.UserRole = UserRole.Moderator;
                managerUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Ghitza";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);
                Assert.AreEqual(rUser.Username, "Ghitza");
                Assert.AreEqual(mUser.Username, "Ghitza");
                Assert.AreEqual(aUser.Username, "Ghitza");

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu-zis
                loggedInUser.UserRole = UserRole.Moderator; // Moderator
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-5); // Mai putin de 6 luni

                // Modifica user-name-ul utilzatorului Regular
                regularUserToBeAdded.Username = "Mishu";

                id = 1; // dam id-ul corect
                rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                // Incearca sa modifice user-name-ul utilzatorului Moderator
                managerUserToBeAdded.Username = "Mishu";

                id = 2; // dam id-ul corect
                mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                // Incearca sa modifice user-name-ul utilzatorului Admin
                adminUserToBeAdded.Username = "Mishu";

                id = 3; // dam id-ul corect
                aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                // Ar trebui ca update-ul utilizatorului Regular sa fi reusit,
                // iar cele ale utilizatorilor Moderator si Admin sa fi esuat
                Assert.AreEqual(rUser.Username, "Mishu");
                Assert.IsNull(mUser); //cand nu poate face upsert, returneaza null
                Assert.IsNull(aUser);

            }
        }

        [Test]
        public void AnOldModeratorShouldBeAbleToUpdateBothRegularUsersAndModerators()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnOldModeratorShouldBeAbleToUpdateBothRegularUsersAndModerators))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User managerUserToBeAdded = new User();
                managerUserToBeAdded.Username = "Ghitza";
                managerUserToBeAdded.UserRole = UserRole.Moderator;
                managerUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Ghitza";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);
                Assert.AreEqual(rUser.Username, "Ghitza");
                Assert.AreEqual(mUser.Username, "Ghitza");
                Assert.AreEqual(aUser.Username, "Ghitza");

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu-zis
                loggedInUser.UserRole = UserRole.Moderator; // Moderator
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-7); // Mai mult de 6 luni

                // Modifica user-name-ul utilzatorului Regular
                regularUserToBeAdded.Username = "Mishu";

                id = 1; // dam id-ul corect
                rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                // Modifica user-name-ul utilzatorului Moderator
                managerUserToBeAdded.Username = "Mishu";

                id = 2; // dam id-ul corect
                mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                // Incearca sa modifice user-name-ul utilzatorului Admin
                adminUserToBeAdded.Username = "Mishu";

                id = 3; // dam id-ul corect
                aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                // Ar trebui ca update-ul utilizatorilor Regular si Moderator sa fi reusit,
                // iar cel al utilizatorului Admin sa fi esuat
                Assert.AreEqual(rUser.Username, "Mishu");
                Assert.AreEqual(mUser.Username, "Mishu");
                Assert.IsNull(aUser); //metoda upsert returneaza null daca upsertul nu a avut succes

            }
        }

        [Test]
        public void ANewModeratorShouldBeAbleToDeleteOnlyRegularUsers()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ANewModeratorShouldBeAbleToDeleteOnlyRegularUsers))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User managerUserToBeAdded = new User();
                managerUserToBeAdded.UserRole = UserRole.Moderator;
                managerUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu-zis
                loggedInUser.UserRole = UserRole.Moderator; // Moderator
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-5); // Mai putin de 6 luni

                int notFoundOrForbidden;

                id = 1; // dam id-ul corect
                rUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 0); // stergerea ar trebui sa functioneze

                id = 2; // dam id-ul corect
                mUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 2); // stergerea ar trebui sa esueze cu Forbidden (2)

                id = 3; // dam id-ul corect
                aUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 2); // stergerea ar trebui sa esueze cu Forbidden (2)

                loggedInUser.UserRole = UserRole.Admin; // Revenim la Admin, ca sa putem numara toti utilizatorii ramasi
                int numberOfCurrentUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Ar trebui ca stergerea utilizatorului Regular sa fi reusit,
                // iar cele ale utilizatorilor Moderator si Admin sa fi esuat: 3 - 1 = 2
                Assert.AreEqual(numberOfCurrentUsers, 2);

            }
        }

        [Test]
        public void AnOldModeratorShouldBeAbleToDeleteBothRegularUsersAndModerators()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnOldModeratorShouldBeAbleToDeleteBothRegularUsersAndModerators))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User managerUserToBeAdded = new User();
                managerUserToBeAdded.UserRole = UserRole.Moderator;
                managerUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, managerUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu-zis
                loggedInUser.UserRole = UserRole.Moderator; // Moderator
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-7); // Mai mult de 6 luni

                int notFoundOrForbidden;

                id = 1; // dam id-ul corect
                rUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 0); // stergerea ar trebui sa functioneze

                id = 2; // dam id-ul corect
                mUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 0); // stergerea ar trebui sa functioneze

                id = 3; // dam id-ul corect
                aUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 2); // stergerea ar trebui sa esueze

                loggedInUser.UserRole = UserRole.Admin; // Revenim la Admin, ca sa putem numara toti utilizatorii ramasi
                int numberOfCurrentUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Ar trebui ca stergerea utilizatorilor Regular si Moderator sa fi reusit,
                // iar cea a utilizatorului Admin sa fi esuat: 3 - 2 = 1
                Assert.AreEqual(numberOfCurrentUsers, 1);

            }
        }

        [Test]
        public void ARegularUserShouldNotBeAbleToCreateAnyKindOfUsers()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ARegularUserShouldNotBeAbleToCreateAnyKindOfUsers))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Regular;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Ghitza";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Ghitza";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                Assert.IsNull(rUser);
                Assert.IsNull(mUser);
                Assert.IsNull(aUser);

                loggedInUser.UserRole = UserRole.Admin;

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                Assert.AreEqual(numberOfAddedUsers, 0);
            }
        }

        [Test]
        public void ARegularUserShouldNotBeAbleToUpdateAnyKindOfUsers()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ARegularUserShouldNotBeAbleToUpdateAnyKindOfUsers))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Ghitza";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Ghitza";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);
                Assert.AreEqual(rUser.Username, "Ghitza");
                Assert.AreEqual(mUser.Username, "Ghitza");
                Assert.AreEqual(aUser.Username, "Ghitza");

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu-zis
                loggedInUser.UserRole = UserRole.Regular; // Regular

                // Incearca sa modifice user-name-ul utilzatorului Regular
                regularUserToBeAdded.Username = "Mishu";

                id = 1; // dam id-ul corect
                rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                // Incearca sa modifice user-name-ul utilzatorului Moderator
                moderatorUserToBeAdded.Username = "Mishu";

                id = 2; // dam id-ul corect
                mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                // Incearca sa modifice user-name-ul utilzatorului Admin
                adminUserToBeAdded.Username = "Mishu";

                id = 3; // dam id-ul corect
                aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                // Ar trebui ca toate update-urile utilizatorilor  fi esuat
                Assert.IsNull(rUser);
                Assert.IsNull(mUser);
                Assert.IsNull(aUser);
            }
        }

        [Test]
        public void ARegularUserShouldNotBeAbleToDeleteAnyKindOfUsers()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ARegularUserShouldNotBeAbleToDeleteAnyKindOfUsers))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Ghitza";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Ghitza";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu-zis
                loggedInUser.UserRole = UserRole.Regular; // Regular

                int notFoundOrForbidden;

                id = 1; // dam id-ul corect
                rUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 2); // stergerea ar trebui sa esueze

                id = 2; // dam id-ul corect
                mUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 2); // stergerea ar trebui sa esueze

                id = 3; // dam id-ul corect
                aUser = userService.Delete(id, loggedInUser, out notFoundOrForbidden);
                Assert.AreEqual(notFoundOrForbidden, 2); // stergerea ar trebui sa esueze

                loggedInUser.UserRole = UserRole.Admin; // Revenim la Admin, ca sa putem numara toti utilizatorii ramasi
                int numberOfCurrentUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Ar trebui ca toate stergerile utilizatorilor sa fi esuat: 3 - 0 = 3
                Assert.AreEqual(numberOfCurrentUsers, 3);
            }
        }

        [Test]
        public void ARegularUserShouldNotBeAbleToGetAnyKindOfUsers()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ARegularUserShouldNotBeAbleToGetAnyKindOfUsers))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Ghitza";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Ghitza";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu - zis
                loggedInUser.UserRole = UserRole.Regular; // Regular

                Assert.IsNull(userService.GetAll(loggedInUser, out forbidden)); //cu count = 0 nu merge, pt ca returneaza null...
                Assert.AreEqual(forbidden, 1); // nu numai ca getAll nu a returnat userii, dar a setat si parametrul de iesire pe "Forbidden" (1)
            }
        }

        [Test]
        public void ARegularUserShouldNotBeAbleToGetAnyKindOfUserById()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ARegularUserShouldNotBeAbleToGetAnyKindOfUserById))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Marioara";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Fanica";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu-zis
                loggedInUser.UserRole = UserRole.Regular; // Regular

                int notFoundOrForbidden;

                id = 1; // dam id-ul corect
                rUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNull(rUser); // returneaza null
                Assert.AreEqual(notFoundOrForbidden, 2); //si Forbidden

                id = 2; // dam id-ul corect
                mUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNull(mUser);
                Assert.AreEqual(notFoundOrForbidden, 2);

                id = 3; // dam id-ul corect
                aUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNull(aUser);
                Assert.AreEqual(notFoundOrForbidden, 2);
            }
        }

        [Test]
        public void ANewModeratorShouldBeAbleToGetOnlyRegularUsers()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ANewModeratorShouldBeAbleToGetOnlyRegularUsers))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Ghitza";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Ghitza";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu - zis
                loggedInUser.UserRole = UserRole.Moderator; // Moderator
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-5); // Mai putin de 6 luni

                int numberOfCurrentUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Ar trebui sa "vada" doar unul din cei 3 useri
                Assert.AreEqual(numberOfCurrentUsers, 1);
            }
        }

        [Test]
        public void AnOldModeratorShouldBeAbleToGetBothRegularUsersAndModeratorsButNoAdmins()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnOldModeratorShouldBeAbleToGetBothRegularUsersAndModeratorsButNoAdmins))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Ghitza";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Ghitza";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu - zis
                loggedInUser.UserRole = UserRole.Moderator; // Moderator
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-7); // Mai mult de 6 luni

                int numberOfCurrentUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Ar trebui sa "vada" doar 2 din cei 3 useri
                Assert.AreEqual(numberOfCurrentUsers, 2);
            }
        }

        [Test]
        public void AnAdminShouldBeAbleToGetAnyKindOfUsers()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnAdminShouldBeAbleToGetAnyKindOfUsers))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Ghitza";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Ghitza";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                // Verificam faptul ca utilizatorii au fost creati conform planului
                // si, implicit, ca un Admin poate face GetAll pe toti

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);
            }
        }

        [Test]
        public void ANewModeratorShouldBeAbleToGetOnlyReglarUsersById()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(ANewModeratorShouldBeAbleToGetOnlyReglarUsersById))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Marioara";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Fanica";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu-zis
                loggedInUser.UserRole = UserRole.Moderator; // Moderator
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-5); // Mai putin de 6 luni

                int notFoundOrForbidden;

                id = 1; // dam id-ul corect
                rUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNotNull(rUser); // returneaza user-ul
                Assert.AreEqual(notFoundOrForbidden, 0); // si nu e Forbidden

                id = 2; // dam id-ul corect
                mUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNull(mUser); // returneaza null in loc de user
                Assert.AreEqual(notFoundOrForbidden, 2); // si Forbidden

                id = 3; // dam id-ul corect
                aUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNull(aUser); // returneaza null in loc de user
                Assert.AreEqual(notFoundOrForbidden, 2); // si Forbidden
            }
        }

        [Test]
        public void AnOldModeratorShouldBeAbleToGetBothReglarUsersAndModeratorsById()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnOldModeratorShouldBeAbleToGetBothReglarUsersAndModeratorsById))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Marioara";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Fanica";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                // Acum schimbam rolul utilizatorului logat, pentru a efectua testul propriu-zis
                loggedInUser.UserRole = UserRole.Moderator; // Moderator
                loggedInUser.UserRoleStartDate = DateTime.Now.AddMonths(-7); // Mai mult de 6 luni

                int notFoundOrForbidden;

                id = 1; // dam id-ul corect
                rUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNotNull(rUser); // returneaza user-ul Regular
                Assert.AreEqual(notFoundOrForbidden, 0); // si nu e Forbidden

                id = 2; // dam id-ul corect
                mUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNotNull(mUser); // returneaza user-ul Moderator
                Assert.AreEqual(notFoundOrForbidden, 0); // si nu e Forbidden

                id = 3; // dam id-ul corect
                aUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNull(aUser); // returneaza null in loc de userul Admin
                Assert.AreEqual(notFoundOrForbidden, 2); // si e Forbidden
            }
        }

        [Test]
        public void AnAdminShouldBeAbleToGetAnyKindOfUsersById()
        {
            var options = new DbContextOptionsBuilder<ExamenDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(AnAdminShouldBeAbleToGetAnyKindOfUsersById))
                .Options;

            using (var context = new ExamenDbContext(options))
            {
                var userService = new UsersService(context, config);

                // Ca sa-i poata introduce pe toti, trebuie sa aiba (provizoriu) rolul de Admin
                User loggedInUser = new User();
                loggedInUser.UserRole = UserRole.Admin;

                User regularUserToBeAdded = new User();
                regularUserToBeAdded.Username = "Ghitza";
                regularUserToBeAdded.UserRole = UserRole.Regular;
                regularUserToBeAdded.Password = "123456789";

                int id = 53; // dam un id inexistent
                User rUser = userService.Upsert(id, regularUserToBeAdded, loggedInUser);

                User moderatorUserToBeAdded = new User();
                moderatorUserToBeAdded.Username = "Marioara";
                moderatorUserToBeAdded.UserRole = UserRole.Moderator;
                moderatorUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User mUser = userService.Upsert(id, moderatorUserToBeAdded, loggedInUser);

                User adminUserToBeAdded = new User();
                adminUserToBeAdded.Username = "Fanica";
                adminUserToBeAdded.UserRole = UserRole.Admin;
                adminUserToBeAdded.Password = "123456789";

                id = 53; // dam un id inexistent
                User aUser = userService.Upsert(id, adminUserToBeAdded, loggedInUser);

                int forbidden;
                int numberOfAddedUsers = userService.GetAll(loggedInUser, out forbidden).Count();

                // Verificam faptul ca utilizatorii au fost creati conform planului
                Assert.AreEqual(numberOfAddedUsers, 3);
                Assert.AreEqual(rUser.UserRole, UserRole.Regular);
                Assert.AreEqual(mUser.UserRole, UserRole.Moderator);
                Assert.AreEqual(aUser.UserRole, UserRole.Admin);

                int notFoundOrForbidden;

                id = 1; // dam id-ul corect
                rUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNotNull(rUser); // returneaza user-ul Regular
                Assert.AreEqual(notFoundOrForbidden, 0); // si nu e Forbidden

                id = 2; // dam id-ul corect
                mUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNotNull(mUser); // returneaza user-ul Moderator
                Assert.AreEqual(notFoundOrForbidden, 0); // si nu e Forbidden

                id = 3; // dam id-ul corect
                aUser = userService.GetUserById(id, loggedInUser, out notFoundOrForbidden);
                Assert.IsNotNull(aUser); // returneaza user-ul Admin
                Assert.AreEqual(notFoundOrForbidden, 0); // si nu e Forbidden
            }
        }


    }
}