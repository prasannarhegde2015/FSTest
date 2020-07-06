using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class AlarmServiceTests : APIClientTestBase
    {
        List<AlarmTypeDTO> _alarmTypesToRemove;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _alarmTypesToRemove = new List<AlarmTypeDTO>();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
            foreach (AlarmTypeDTO alarmTypeToRemove in _alarmTypesToRemove)
            {
                try
                {
                    AlarmService.RemoveAlarmType(alarmTypeToRemove.Id.ToString());
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Failed to remove alarm type {alarmTypeToRemove.AlarmType} ({alarmTypeToRemove.Id}): {ex.ToString()}");
                }
            }
        }
    }
}