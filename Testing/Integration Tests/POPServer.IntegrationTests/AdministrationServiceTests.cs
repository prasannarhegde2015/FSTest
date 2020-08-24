using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class AdministrationServiceTests : APIClientTestBase
    {
        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        public void CheckUserRoles(UserDTO user, IEnumerable<RoleDTO> expectedRoles)
        {
            if (expectedRoles == null || !expectedRoles.Any())
            {
                Assert.IsTrue(user.Roles == null || user.Roles.Count == 0, "User should have no roles.");
            }
            else
            {
                Assert.IsNotNull(user.Roles, "User roles should not be null.");
                var missingRoles = expectedRoles.Where(er => user.Roles.FirstOrDefault(ur => ur.Id == er.Id) == null).ToList();
                var unexpectedRoles = user.Roles.Where(ur => expectedRoles.FirstOrDefault(er => er.Id == ur.Id) == null).ToList();
                if (missingRoles.Count > 0 || unexpectedRoles.Count > 0)
                {
                    var bob = new StringBuilder();
                    if (missingRoles.Count > 0)
                    {
                        bob.Append("User is missing role(s): " + string.Join(", ", missingRoles.Select(mr => mr.Name)));
                    }
                    if (unexpectedRoles.Count > 0)
                    {
                        bob.Append("User has unexpected role(s): " + string.Join(", ", unexpectedRoles.Select(ur => ur.Name)));
                    }
                    Assert.Fail(bob.ToString());
                }
            }
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void UsersAddGetAllRemove()
        {
            var user = new UserDTO();
            user.Name = "BOGUS\\TestUser";

            AdministrationService.AddUser(user);
            UserDTO addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNotNull(addedUser, "Failed to add user.");
            UserDTO originalUser = addedUser;
            _usersToRemove.Add(addedUser);
            Assert.AreEqual(user.Name, addedUser.Name, "Added user has incorrect name.");
            CheckUserRoles(addedUser, null);

            AdministrationService.RemoveUser(addedUser);
            UserDTO userShouldBeMissing = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNull(userShouldBeMissing, "Failed to remove user.");
            _usersToRemove.Remove(addedUser);


            var testRole = new RoleDTO();
            testRole.Name = "UserTestRole";
            AdministrationService.AddRole(testRole);
            testRole = AdministrationService.GetRoles().FirstOrDefault(r => r.Name == testRole.Name);
            Assert.IsNotNull(testRole, "Failed to add role.");
            _rolesToRemove.Add(testRole);
            var expectedRoles = new List<RoleDTO>() { testRole };
            user.Roles = expectedRoles;
            AdministrationService.AddUser(user);
            addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNotNull(addedUser, "Failed to add user.");
            _usersToRemove.Add(addedUser);
            CheckUserRoles(addedUser, expectedRoles);

            AdministrationService.RemoveUser(addedUser);
            userShouldBeMissing = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNull(userShouldBeMissing, "Failed to remove user.");
            _usersToRemove.Remove(addedUser);


            var testRole2 = new RoleDTO();
            testRole2.Name = "UserTestRole2";
            AdministrationService.AddRole(testRole2);
            testRole2 = AdministrationService.GetRoles().FirstOrDefault(r => r.Name == testRole2.Name);
            Assert.IsNotNull(testRole2, "Failed to add role.");
            _rolesToRemove.Add(testRole2);
            expectedRoles = new List<RoleDTO>() { testRole, testRole2 };
            user.Roles = expectedRoles;
            AdministrationService.AddUser(user);
            addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNotNull(addedUser, "Failed to add user.");
            _usersToRemove.Add(addedUser);
            CheckUserRoles(addedUser, expectedRoles);

            AdministrationService.RemoveUser(addedUser);
            userShouldBeMissing = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNull(userShouldBeMissing, "Failed to remove user.");
            _usersToRemove.Remove(addedUser);
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void UsersAddUpdateGetAllRemove()
        {
            var user = new UserDTO();
            user.Name = "BOGUS\\TestUser";

            AdministrationService.AddUser(user);
            UserDTO addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNotNull(addedUser, "Failed to add user.");
            UserDTO originalUser = addedUser;
            _usersToRemove.Add(addedUser);
            Assert.AreEqual(user.Name, addedUser.Name, "Added user has incorrect name.");
            CheckUserRoles(addedUser, null);

            var testRole = new RoleDTO();
            testRole.Name = "UserTestRole";
            AdministrationService.AddRole(testRole);
            testRole = AdministrationService.GetRoles().FirstOrDefault(r => r.Name == testRole.Name);
            Assert.IsNotNull(testRole, "Failed to add first role.");
            _rolesToRemove.Add(testRole);
            var expectedRoles = new List<RoleDTO>() { testRole };
            addedUser.Roles = expectedRoles;
            AdministrationService.UpdateUser(addedUser);
            addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNotNull(addedUser, "Failed to get updated user after adding first role to user.");
            CheckUserRoles(addedUser, expectedRoles);

            var testRole2 = new RoleDTO();
            testRole2.Name = "UserTestRole2";
            AdministrationService.AddRole(testRole2);
            testRole2 = AdministrationService.GetRoles().FirstOrDefault(r => r.Name == testRole2.Name);
            Assert.IsNotNull(testRole2, "Failed to add second role.");
            _rolesToRemove.Add(testRole2);
            expectedRoles = new List<RoleDTO>() { testRole, testRole2 };
            addedUser.Roles = expectedRoles;
            AdministrationService.UpdateUser(addedUser);
            addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNotNull(addedUser, "Failed to get updated user after adding second role to user.");
            CheckUserRoles(addedUser, expectedRoles);

            var testRole3 = new RoleDTO();
            testRole3.Name = "UserTestRole3";
            AdministrationService.AddRole(testRole3);
            testRole3 = AdministrationService.GetRoles().FirstOrDefault(r => r.Name == testRole3.Name);
            Assert.IsNotNull(testRole3, "Failed to add third role.");
            _rolesToRemove.Add(testRole3);
            expectedRoles = new List<RoleDTO>() { testRole3 };
            addedUser.Roles = expectedRoles;
            AdministrationService.UpdateUser(addedUser);
            addedUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNotNull(addedUser, "Failed to get updated user after adding third role to user and removing all others.");
            CheckUserRoles(addedUser, expectedRoles);

            AdministrationService.RemoveUser(addedUser);
            UserDTO userShouldBeMissing = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == user.Name);
            Assert.IsNull(userShouldBeMissing, "Failed to remove user.");
            _usersToRemove.Remove(_usersToRemove.FirstOrDefault(u => u.Name == user.Name));
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void GetAllUsers()
        {
            UserDTO[] allUsers = AdministrationService.GetUsers();
            Assert.IsNotNull(allUsers, "Returned array of users should not be null.");
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void GetUser()
        {
            UserDTO[] users = AdministrationService.GetUsers();
            UserDTO firstUser = users.FirstOrDefault();
            UserDTO user = AdministrationService.GetUser(firstUser.Id.ToString());
            Assert.AreEqual(firstUser.Id, user.Id);
            Assert.AreEqual(firstUser.Name, user.Name);
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void GetAllPermissions()
        {
            IList<PermissionDTO> permissions = AdministrationService.GetPermissions();
            Assert.IsNotNull(permissions, "Permissions array should not be null.");
            Assert.AreEqual(Enum.GetValues(typeof(PermissionId)).Length - 1, permissions.Count, "Unexpected permission count returned from server.");
            foreach (PermissionDTO permission in permissions)
            {
                Assert.AreNotEqual(PermissionId.Unknown, permission.Id, "The Unknown permission should not actually exist.");
            }
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void GetPermission()
        {
            foreach (PermissionId permission in Enum.GetValues(typeof(PermissionId)))
            {
                if (permission == PermissionId.Unknown)
                {
                    continue;
                }
                long id = (long)permission;
                PermissionDTO dto = AdministrationService.GetPermission(id.ToString());
                Assert.IsNotNull(dto, "Failed to get permission {0}.", permission);
                Assert.AreEqual(permission, dto.Id, "Unexpected value for permission id.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(dto.Name), "Permission name should not be blank/empty.");
            }
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void RolesAddGetAllGetUpdateRemove()
        {
            IList<PermissionDTO> permissions = AdministrationService.GetPermissions();
            var role = new RoleDTO();
            role.Name = "TestRole";
            role.Permissions = permissions.Where(p => p.Id == PermissionId.AddWell || p.Id == PermissionId.RunAnalysis).ToList();
            AdministrationService.AddRole(role);
            IList<RoleDTO> roles = AdministrationService.GetRoles();
            RoleDTO addedRole = roles.FirstOrDefault(r => r.Name.Equals(role.Name));
            Assert.IsNotNull(addedRole, "Failed to find added role.");
            RoleDTO originalRole = addedRole;
            _rolesToRemove.Add(originalRole);
            Assert.IsNotNull(addedRole.Permissions);
            foreach (PermissionDTO permission in role.Permissions)
            {
                Assert.IsNotNull(addedRole.Permissions.FirstOrDefault(p => p.Id == permission.Id), "Permission {0} missing from added role.", permission.Name);
            }
            Assert.AreEqual(role.Permissions.Count, addedRole.Permissions.Count, "Added role has extra permissions.");
            RoleDTO addedRoleGetSingle = AdministrationService.GetRole(addedRole.Id.ToString());
            Assert.IsNotNull(addedRoleGetSingle, "Failed to get added role by id.");
            Assert.IsNotNull(addedRoleGetSingle.Permissions);
            foreach (PermissionDTO permission in role.Permissions)
            {
                Assert.IsNotNull(addedRoleGetSingle.Permissions.FirstOrDefault(p => p.Id == permission.Id), "Permission {0} missing from added role.", permission.Name);
            }
            Assert.AreEqual(role.Permissions.Count, addedRoleGetSingle.Permissions.Count, "Added role has extra permissions.");

            role = addedRole;
            role.Permissions = null;
            AdministrationService.UpdateRole(role);
            addedRole = roles.FirstOrDefault(r => r.Name.Equals(role.Name));
            Assert.IsNotNull(addedRole, "Failed to get updated role.");
            Assert.IsTrue(addedRole.Permissions == null || addedRole.Permissions.Count == 0, "Failed to remove permissions from role.");
            role = addedRole;
            role.Permissions = permissions.Where(p => p.Id == PermissionId.AddWorkover || p.Id == PermissionId.WellTestCreateReadUpdate || p.Id == PermissionId.ScanDevice).ToList();
            addedRole = roles.FirstOrDefault(r => r.Name.Equals(role.Name));
            Assert.IsNotNull(addedRole, "Failed to get updated role.");
            Assert.IsNotNull(addedRole.Permissions);
            foreach (PermissionDTO permission in role.Permissions)
            {
                Assert.IsNotNull(addedRole.Permissions.FirstOrDefault(p => p.Id == permission.Id), "Permission {0} missing from added role.", permission.Name);
            }
            Assert.AreEqual(role.Permissions.Count, addedRole.Permissions.Count, "Updated role has extra permissions.");
            role.Name = "TestRole-NewName";
            AdministrationService.UpdateRole(role);
            roles = AdministrationService.GetRoles();
            addedRole = roles.FirstOrDefault(r => r.Name.Equals(role.Name));
            Assert.IsNotNull(addedRole, "Failed to get updated role.");

            AdministrationService.RemoveRole(addedRole);
            roles = AdministrationService.GetRoles();
            addedRole = roles.FirstOrDefault(r => r.Name.Equals(role.Name));
            Assert.IsNull(addedRole, "Failed to remove added role.");
            _rolesToRemove.Remove(originalRole);
        }

        private static void CheckOneStatus(ServiceConnectionStatus expectedStatus, string name, List<ServiceStatusDTO> serviceStatusList)
        {
            ServiceStatusDTO status = serviceStatusList.FirstOrDefault(s => s.Name.Equals(name));
            Assert.IsNotNull(status, "Failed to get service status for {0}.", name);
            Assert.AreEqual(expectedStatus, status.Status, "Status for service {0} has unexpected value.", name);
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void GetServiceStatus()
        {
            DataConnectionDTO dataConnection = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == s_cvsService);
            if (dataConnection == null)
            {
                dataConnection = GetDefaultCygNetDataConnection();
                DataConnectionService.AddDataConnection(dataConnection);
                dataConnection = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == s_cvsService);
                Assert.IsNotNull(dataConnection, "Failed to add data connection.");
                _dataConnectionsToRemove.Add(dataConnection);
            }
            List<ServiceStatusDTO> serviceStatusList = AdministrationService.GetServiceStatus();
            var expectedStatus = ServiceConnectionStatus.Connected;
            CheckOneStatus(expectedStatus, "WAMI", serviceStatusList);
            CheckOneStatus(expectedStatus, "Catalog Service", serviceStatusList);
            CheckOneStatus(expectedStatus, "Dynacard Library Service", serviceStatusList);
            CheckOneStatus(expectedStatus, "[" + s_domain + "]" + s_site + "." + s_cvsService, serviceStatusList);
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void ADGroupCRUD()
        {
            Domain domain = Domain.GetComputerDomain();
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain.Name);
            GroupPrincipal grp = new GroupPrincipal(ctx);
            PrincipalSearcher searcher = new PrincipalSearcher(grp);
            var results = searcher.FindOne();

            ADGroupDTO groupDto = new ADGroupDTO
            {
                Assets = new List<AssetDTO>(),
                Roles = new List<RoleDTO>(),
                Name = results.Name,
                Id = 0,
                SecurityIdentifier = results.Sid.Value
            };

            Console.WriteLine($"Attempting to add AD group with name {groupDto.Name} and SID {groupDto.SecurityIdentifier}.");

            var existingGroup =
                AdministrationService
                    .GetGroups()
                    .Where(x => x.SecurityIdentifier == groupDto.SecurityIdentifier)
                    .FirstOrDefault();

            if (existingGroup != null)
            {
                Console.WriteLine("Group already exists.  Removing...");
                AdministrationService.RemoveGroup(existingGroup.Id.ToString());
            }

            AdministrationService.AddGroup(groupDto);

            Console.WriteLine("Group added.  Checking that group was added successfully");

            var addedGroup =
                AdministrationService
                    .GetGroups()
                    .Where(x => x.SecurityIdentifier == groupDto.SecurityIdentifier)
                    .FirstOrDefault();

            Assert.IsNotNull(addedGroup);

            Console.WriteLine("Attempting to update the roles for the group");

            var roleToAdd = AdministrationService.GetRoles().First();
            addedGroup.Roles.Add(roleToAdd);
            AdministrationService.UpdateGroup(addedGroup);

            var updatedGroup =
                AdministrationService
                    .GetGroups()
                    .Where(x => x.Id == addedGroup.Id)
                    .First();

            Assert.AreEqual(updatedGroup.Roles.First().Id, roleToAdd.Id);

            Console.WriteLine("Attempting to remove the group");

            AdministrationService.RemoveGroup(addedGroup.Id.ToString());

            var doesGroupExist =
                AdministrationService
                    .GetGroups()
                    .Where(x => x.Id == addedGroup.Id)
                    .Any();

            Assert.IsFalse(doesGroupExist);
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void ExpandActiveDirectoryEntryForContainer()
        {
            var domain = Domain.GetComputerDomain();
            var directoryEntry = domain.GetDirectoryEntry();
            var searcher = new DirectorySearcher(directoryEntry) { SearchScope = SearchScope.OneLevel };
            searcher.PropertiesToLoad.AddRange(new[] { "objectclass", "msDS-Approx-Immed-Subordinates" });
            var results = searcher.FindAll();

            DirectoryEntry entryForTesting = null;

            foreach (SearchResult result in results)
            {
                if (result.Properties["objectclass"].Contains("organizationalUnit")
                    && result.Properties["msDS-Approx-Immed-Subordinates"].Count > 0
                    && int.Parse(result.Properties["msDS-Approx-Immed-Subordinates"][0].ToString()) > 0)
                {
                    entryForTesting = new DirectoryEntry(result.Path);
                    break;
                }
            }

            Assert.IsNotNull(entryForTesting);
            Console.WriteLine($"Testing expansion of {entryForTesting.Path}");

            ActiveDirectoryEntryDTO entryToExpand = new ActiveDirectoryEntryDTO(ActiveDirectoryEntryType.Container)
            {
                Name = entryForTesting.Name,
                Path = entryForTesting.Path
            };

            var expandedEntry = AdministrationService.ExpandActiveDirectoryEntry(entryToExpand);

            Console.WriteLine($"Expanded entry has {expandedEntry.Children.Length} children");

            List<string> expectedDirectoryPaths = new List<string>();

            foreach (DirectoryEntry child in entryForTesting.Children)
            {
                expectedDirectoryPaths.Add(child.Path);
            }

            Console.WriteLine($"Expected count of children is {expectedDirectoryPaths.Count}");
            Assert.AreEqual(expectedDirectoryPaths.Count, expandedEntry.Children.Length);

            Console.WriteLine("Checking that expanded entry children match expected children");
            var intersection =
                expandedEntry
                    .Children
                    .Select(x => x.Path)
                    .Intersect(expectedDirectoryPaths);

            Assert.AreEqual(expectedDirectoryPaths.Count, intersection.Count());
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void ExpandActiveDirectoryEntryForGroup()
        {
            Domain domain = Domain.GetComputerDomain();
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain.Name);
            GroupPrincipal grp = new GroupPrincipal(ctx);
            PrincipalSearcher searcher = new PrincipalSearcher(grp);
            var results = searcher.FindAll();

            GroupPrincipal groupForTesting = null;

            foreach (GroupPrincipal result in results)
            {
                var membersOfGroup = result.GetMembers();
                if (membersOfGroup.Count() > 0)
                {
                    groupForTesting = result;
                    break;
                }
            }

            Assert.IsNotNull(groupForTesting);
            Console.WriteLine($"Testing expansion of {groupForTesting.DistinguishedName}");

            ActiveDirectoryEntryDTO entryToExpand = new ActiveDirectoryEntryDTO(ActiveDirectoryEntryType.Group)
            {
                Name = groupForTesting.Name,
                Path = $"LDAP://{searcher.Context.ConnectedServer}/{groupForTesting.DistinguishedName}"
            };

            var expandedEntry = AdministrationService.ExpandActiveDirectoryEntry(entryToExpand);

            Console.WriteLine($"Expanded entry has {expandedEntry.Children.Length} children");

            List<string> expectedGroupSids = groupForTesting.GetMembers().Select(x => x.Sid.ToString()).ToList();

            Console.WriteLine($"Expected count of children is {expectedGroupSids.Count}");
            Assert.AreEqual(expectedGroupSids.Count, expandedEntry.Children.Length);

            Console.WriteLine("Checking that expanded entry children match expected children");
            var intersection =
                expandedEntry
                    .Children
                    .Select(x => x.SecurityIdentifier)
                    .Intersect(expectedGroupSids);

            Assert.AreEqual(expectedGroupSids.Count, intersection.Count());
        }

        [TestCategory(TestCategories.AdministrationServiceTests), TestMethod]
        public void SearchActiveDirectory()
        {
            Domain domain = Domain.GetComputerDomain();
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain.Name);
            GroupPrincipal grp = new GroupPrincipal(ctx);
            PrincipalSearcher searcher = new PrincipalSearcher(grp);
            var results = searcher.FindAll();

            GroupPrincipal groupForTesting = null;
            UserPrincipal userForTesting = null;

            foreach (GroupPrincipal result in results)
            {
                var membersOfGroup = result.GetMembers();
                if (membersOfGroup.Count() > 0)
                {
                    foreach (Principal principleMember in membersOfGroup)
                    {
                        // Want a group with at least one user we can search for
                        if (principleMember is UserPrincipal up)
                        {
                            userForTesting = up;
                            break;
                        }
                    }
                    if (userForTesting != null && userForTesting.GivenName != null && userForTesting.SamAccountName != null && userForTesting.SamAccountName != null && userForTesting.Surname != null)
                    {
                        groupForTesting = result;
                        break;
                    }
                }
            }

            Assert.IsNotNull(groupForTesting);
            Assert.IsNotNull(userForTesting);

            Console.WriteLine($"Testing search of group name {groupForTesting.Name}");

            var searchQuery = new ActiveDirectorySearchQueryDTO
            {
                Domain = $"LDAP://{domain.Name}",
                QueryType = ActiveDirectorySearchQueryType.Name,
                Query = groupForTesting.Name
            };

            var groupSearchResult =
                AdministrationService
                    .SearchActiveDirectory(searchQuery)
                    .Where(x => x.SecurityIdentifier == groupForTesting.Sid.ToString())
                    .FirstOrDefault();

            Assert.IsNotNull(groupSearchResult);

            Console.WriteLine($"Testing search of UserName {userForTesting.SamAccountName}");

            searchQuery.QueryType = ActiveDirectorySearchQueryType.UserName;
            searchQuery.Query = userForTesting.SamAccountName;

            var userNameSearchResult =
                AdministrationService
                    .SearchActiveDirectory(searchQuery)
                    .Where(x => x.SecurityIdentifier == userForTesting.Sid.ToString())
                    .FirstOrDefault();

            Assert.IsNotNull(userNameSearchResult);

            Console.WriteLine($"Testing search of First Name {userForTesting.GivenName}");

            searchQuery.QueryType = ActiveDirectorySearchQueryType.FirstName;
            searchQuery.Query = userForTesting.GivenName;

            var firstNameSearchResult =
                AdministrationService
                    .SearchActiveDirectory(searchQuery)
                    .Where(x => x.SecurityIdentifier == userForTesting.Sid.ToString())
                    .FirstOrDefault();

            Assert.IsNotNull(firstNameSearchResult);

            Console.WriteLine($"Testing search of Last Name {userForTesting.Surname}");

            searchQuery.QueryType = ActiveDirectorySearchQueryType.LastName;
            searchQuery.Query = userForTesting.Surname;

            var lastNameSearchResult =
                AdministrationService
                    .SearchActiveDirectory(searchQuery)
                    .Where(x => x.SecurityIdentifier == userForTesting.Sid.ToString())
                    .FirstOrDefault();

            Assert.IsNotNull(lastNameSearchResult);
        }
    }
}
