using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class AuthenticationServiceTests : APIClientTestBase
    {

        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            s_suppressWebErrorMessages = false;
            base.Cleanup();
        }
        // This is frame work for adding integration testing for the Authentication Service.
        // Someone familiar with setting up CygNet Pass Through Authentication will need to fill this out with real tests.
        [TestCategory(TestCategories.AuthenticationServiceTests), TestMethod]
        public void RequestPublicKeyTest()
        {
            try
            {

                string token = AuthenticationService.GetPublicKey();
                // Show new user identity of impersonated user
                Trace.WriteLine("Request Public Key returned:" + token);
            }
            catch (WebException e)
            {
                CheckResponseCode(e, HttpStatusCode.Forbidden);
            }
        }
    }
}