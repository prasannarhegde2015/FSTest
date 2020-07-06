using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;
using Weatherford.POP.Quantities;

namespace Weatherford.POP.Server.IntegrationTests
{
    internal class TestSettingValues
    {
        public string settingName { get; set; }
        public Dictionary<SettingValueType, Tuple<string, double?, double[]>> settingValue { get; set; }
        public TestSettingValues(string name, Dictionary<SettingValueType, Tuple<string, double?, double[]>> values)
        {
            settingName = name;
            settingValue = values;
        }
    }


    [TestClass]
    public class SettingServiceTests : APIClientTestBase
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

        private static string GetTestSettingName(SettingType type, SettingValueType valueType)
        {
            return $"TEST{type}{valueType}";
        }

        private static Dictionary<string, SettingValueType> GetSettingValueTypeNames(SettingType settingType)
        {
            return Enum.GetValues(typeof(SettingValueType)).OfType<SettingValueType>()
                .Where(t => t != SettingValueType.DatabaseLookup)
                .ToDictionary(t => GetTestSettingName(settingType, t), t => t);
        }

        #region Test Methods

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void AddUpdateRemoveSystemSettings()
        {
            SettingType settingType = SettingType.System;
            SettingDTO[] systemSettings = SettingService.GetSettingsByType(settingType.ToString());

            // Okay... we need one of each value type.
            Dictionary<string, SettingValueType> systemSettingNameAndType = GetSettingValueTypeNames(settingType);

            List<TestSettingValues> settingValues = getSettingValues();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> addValues = settingValues.Where(x => x.settingName == "addValues").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> updateValues = settingValues.Where(x => x.settingName == "updateValues").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues1 = settingValues.Where(x => x.settingName == "badValues1").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues2 = settingValues.Where(x => x.settingName == "badValues2").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues3 = settingValues.Where(x => x.settingName == "badValues3").Select(x => x.settingValue).FirstOrDefault();

            var systemSettingsByType = new Dictionary<SettingValueType, SettingDTO>();
            foreach (var kvp in systemSettingNameAndType)
            {
                SettingDTO oneSetting = systemSettings.FirstOrDefault(s => s.Name == kvp.Key);
                Assert.IsNotNull(oneSetting, $"Failed to get setting with name {kvp.Key} and expected type {kvp.Value}.");
                Assert.AreEqual(kvp.Value, oneSetting.SettingValueType, $"Setting with name {kvp.Key} has unexpected type.");
                systemSettingsByType.Add(oneSetting.SettingValueType, oneSetting);
            }
            var settingValuesToRemove = new List<SystemSettingDTO>();
            foreach (SettingDTO setting in systemSettingsByType.Values)
            {
                SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(setting.Name);
                Assert.IsNotNull(settingValue, $"Failed to get setting value for setting {setting.Name}.");
                if (settingValue.Id != 0)
                {
                    settingValuesToRemove.Add(SettingService.GetSystemSettingByName(setting.Name));
                    // Set PK to 0 so that it will add if we removed it.
                    settingValue.Id = 0;
                    _systemSettingsToRestore.Add(settingValue);
                }
                else
                {
                    _userSettingNamesToRemove.Add(setting.Name);
                }
            }
            // First remove existing values.
            foreach (SystemSettingDTO value in settingValuesToRemove)
            {
                SettingService.RemoveSystemSetting(value.Id.ToString());
            }

            // Test getting default values (should get value and id should be 0).
            foreach (SettingValueType valType in systemSettingsByType.Keys)
            {
                SettingDTO setting = systemSettingsByType[valType];
                SystemSettingDTO value = SettingService.GetSystemSettingByName(setting.Name);
                Assert.IsNotNull(value, $"Failed to get default value for setting {setting.Name}.");
                Assert.AreEqual(0, value.Id, $"Default value should have an id of zero for setting {setting.Name}.");
                switch (setting.SettingValueType)
                {
                    case SettingValueType.TrueOrFalse:
                        Assert.IsTrue(value.NumericValue == 0.0 || value.NumericValue == 1.0, $"Unexpected default numeric value {value.NumericValue} for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsTrue(value.InternalNumericValue == 0.0 || value.InternalNumericValue == 1.0, $"Unexpected default internal numeric value {value.InternalNumericValue} for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    case SettingValueType.DecimalNumber:
                    case SettingValueType.Number:
                        Assert.IsNotNull(value.NumericValue, $"Numeric value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.InternalNumericValue, $"Internal numeric value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    case SettingValueType.DecimalNumberArray:
                        Assert.IsNull(value.NumericValue, $"Numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericValue, $"Internal numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.NumericArrayValue, $"Numeric array value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.InternalNumericArrayValue, $"Internal numeric array value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    default:
                        Assert.IsNull(value.NumericValue, $"Numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericValue, $"Internal numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.StringValue, $"String value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;
                }
            }

            // Add value so that it is no longer the default value.
            foreach (SettingValueType valType in systemSettingsByType.Keys)
            {
                SettingDTO setting = systemSettingsByType[valType];
                var value = new SystemSettingDTO() { Setting = setting, SettingId = setting.Id, StringValue = addValues[valType].Item1, NumericValue = addValues[valType].Item2, NumericArrayValue = addValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                long pk = SettingService.SaveSystemSetting(value);
                Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {setting.Name} after add.");
                SystemSettingDTO actualValue = SettingService.GetSystemSettingByName(setting.Name);
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after add.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after add.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Interal numeric value does not match for setting {setting.Name} after add.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after add.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after add.");
            }

            // Update value.
            foreach (SettingValueType valType in systemSettingsByType.Keys)
            {
                SettingDTO setting = systemSettingsByType[valType];
                SystemSettingDTO existingValue = SettingService.GetSystemSettingByName(setting.Name);
                SystemSettingDTO expectedValue = SettingService.GetSystemSettingByName(setting.Name);
                expectedValue.StringValue = updateValues[valType].Item1;
                expectedValue.NumericValue = updateValues[valType].Item2;
                expectedValue.NumericArrayValue = updateValues[valType].Item3;
                expectedValue.InternalNumericValue = expectedValue.NumericValue;
                expectedValue.InternalNumericArrayValue = expectedValue.NumericArrayValue;
                long pk = SettingService.SaveSystemSetting(expectedValue);
                Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {setting.Name} after update.");
                SystemSettingDTO actualValue = SettingService.GetSystemSettingByName(setting.Name);
                Assert.IsNotNull(actualValue, $"Failed to get updated value for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after update.");
                CollectionAssert.AreEqual(expectedValue.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after update.");
                CollectionAssert.AreEqual(expectedValue.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.Id, actualValue.Id, $"System setting primary key does not match value returned from add call for setting {setting.Name} after update.");
            }

            // Remove everything to test multiple calls and to test remove.
            foreach (string name in systemSettingNameAndType.Keys)
            {
                SystemSettingDTO value = SettingService.GetSystemSettingByName(name);
                Assert.IsNotNull(value, $"Failed to get system setting value for {name} to remove before testing save multiple.");
                SettingService.RemoveSystemSetting(value.Id.ToString());
            }

            // Test getting default values again for giggles.
            foreach (SettingValueType valType in systemSettingsByType.Keys)
            {
                SettingDTO setting = systemSettingsByType[valType];
                SystemSettingDTO value = SettingService.GetSystemSettingByName(setting.Name);
                Assert.IsNotNull(value, $"Failed to get default value for setting {setting.Name}.");
                Assert.AreEqual(0, value.Id, $"Default value should have an id of zero for setting {setting.Name}.");
            }

            // Add multiple.
            var values = new List<SystemSettingDTO>();
            foreach (SettingValueType valType in systemSettingsByType.Keys)
            {
                SettingDTO setting = systemSettingsByType[valType];
                var value = new SystemSettingDTO() { Setting = setting, SettingId = setting.Id, StringValue = addValues[valType].Item1, NumericValue = addValues[valType].Item2, NumericArrayValue = addValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                values.Add(value);
            }
            SettingService.SaveSystemSettings(values.ToArray());
            foreach (SettingValueType valType in systemSettingsByType.Keys)
            {
                SystemSettingDTO value = values.Single(s => s.Setting.SettingValueType == valType);
                SettingDTO setting = value.Setting;
                SystemSettingDTO actualValue = SettingService.GetSystemSettingByName(setting.Name);
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after add multiple.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after add multiple.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after add multiple.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after add multiple.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after add multiple.");
            }

            // Update multiple.
            values.Clear();
            foreach (SettingValueType valType in systemSettingsByType.Keys)
            {
                SettingDTO setting = systemSettingsByType[valType];
                var value = new SystemSettingDTO() { Setting = setting, SettingId = setting.Id, StringValue = updateValues[valType].Item1, NumericValue = updateValues[valType].Item2, NumericArrayValue = updateValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                values.Add(value);
            }
            SettingService.SaveSystemSettings(values.ToArray());
            foreach (SettingValueType valType in systemSettingsByType.Keys)
            {
                SystemSettingDTO value = values.Single(s => s.Setting.SettingValueType == valType);
                SettingDTO setting = value.Setting;
                SystemSettingDTO actualValue = SettingService.GetSystemSettingByName(setting.Name);
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after update multiple.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after update multiple.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after update multiple.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after update multiple.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after update multiple.");
            }

            // Test bad value set 1.
            foreach (SettingValueType valType in badValues1.Keys)
            {
                SettingDTO setting = systemSettingsByType[valType];
                SystemSettingDTO badValue = SettingService.GetSystemSettingByName(setting.Name);
                badValue.StringValue = badValues1[valType].Item1;
                badValue.NumericValue = badValues1[valType].Item2;
                badValue.NumericArrayValue = badValues1[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (1) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveSystemSetting(badValue));
            }

            // Test bad value set 2.
            foreach (SettingValueType valType in badValues2.Keys)
            {
                SettingDTO setting = systemSettingsByType[valType];
                SystemSettingDTO badValue = SettingService.GetSystemSettingByName(setting.Name);
                badValue.StringValue = badValues2[valType].Item1;
                badValue.NumericValue = badValues2[valType].Item2;
                badValue.NumericArrayValue = badValues2[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (2) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveSystemSetting(badValue));
            }

            // Test bad value set 3.
            foreach (SettingValueType valType in badValues3.Keys)
            {
                SettingDTO setting = systemSettingsByType[valType];
                SystemSettingDTO badValue = SettingService.GetSystemSettingByName(setting.Name);
                badValue.StringValue = badValues3[valType].Item1;
                badValue.NumericValue = badValues3[valType].Item2;
                badValue.NumericArrayValue = badValues3[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (3) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveSystemSetting(badValue));
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void AddUpdateRemoveUserSettings()
        {
            SettingType settingType = SettingType.User;
            SettingDTO[] userSettings = SettingService.GetSettingsByType(settingType.ToString());

            // Okay... we need one of each value type.
            Dictionary<string, SettingValueType> userSettingNameAndType = GetSettingValueTypeNames(settingType);

            List<TestSettingValues> settingValues = getSettingValues();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> addValues = settingValues.Where(x => x.settingName == "addValues").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> updateValues = settingValues.Where(x => x.settingName == "updateValues").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues1 = settingValues.Where(x => x.settingName == "badValues1").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues2 = settingValues.Where(x => x.settingName == "badValues2").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues3 = settingValues.Where(x => x.settingName == "badValues3").Select(x => x.settingValue).FirstOrDefault();

            var userSettingsByType = new Dictionary<SettingValueType, SettingDTO>();
            foreach (var kvp in userSettingNameAndType)
            {
                SettingDTO oneSetting = userSettings.FirstOrDefault(s => s.Name == kvp.Key);
                Assert.IsNotNull(oneSetting, $"Failed to get setting with name {kvp.Key} and expected type {kvp.Value}.");
                Assert.AreEqual(kvp.Value, oneSetting.SettingValueType, $"Setting with name {kvp.Key} has unexpected type.");
                userSettingsByType.Add(oneSetting.SettingValueType, oneSetting);
            }
            var settingValuesToRemove = new List<UserSettingDTO>();
            foreach (SettingDTO setting in userSettingsByType.Values)
            {
                UserSettingDTO settingValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                Assert.IsNotNull(settingValue, $"Failed to get setting value for setting {setting.Name}.");
                if (settingValue.Id != 0)
                {
                    settingValuesToRemove.Add(SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString()));
                    // Set PK to 0 so that it will add if we removed it.
                    settingValue.Id = 0;
                    _userSettingsToRestore.Add(settingValue);
                }
                else
                {
                    _userSettingNamesToRemove.Add(setting.Name);
                }
            }
            // First remove existing values.
            foreach (UserSettingDTO value in settingValuesToRemove)
            {
                SettingService.RemoveUserSetting(value.Id.ToString());
            }

            // Test getting default values (should get value and id should be 0).
            foreach (SettingValueType valType in userSettingsByType.Keys)
            {
                SettingDTO setting = userSettingsByType[valType];
                UserSettingDTO value = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                Assert.IsNotNull(value, $"Failed to get default value for setting {setting.Name}.");
                Assert.AreEqual(0, value.Id, $"Default value should have an id of zero for setting {setting.Name}.");
                switch (setting.SettingValueType)
                {
                    case SettingValueType.TrueOrFalse:
                        Assert.IsTrue(value.NumericValue == 0.0 || value.NumericValue == 1.0, $"Unexpected default numeric value {value.NumericValue} for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsTrue(value.InternalNumericValue == 0.0 || value.InternalNumericValue == 1.0, $"Unexpected default internal numeric value {value.NumericValue} for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    case SettingValueType.DecimalNumber:
                    case SettingValueType.Number:
                        Assert.IsNotNull(value.NumericValue, $"Numeric value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.InternalNumericValue, $"Internal numeric value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    case SettingValueType.DecimalNumberArray:
                        Assert.IsNull(value.NumericValue, $"Numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericValue, $"Internal numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.NumericArrayValue, $"Numeric array value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.InternalNumericArrayValue, $"Internal numeric array value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    default:
                        Assert.IsNull(value.NumericValue, $"Numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericValue, $"Internal numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.StringValue, $"String value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;
                }
            }

            // Add value so that it is no longer the default value.
            foreach (SettingValueType valType in userSettingsByType.Keys)
            {
                SettingDTO setting = userSettingsByType[valType];
                var value = new UserSettingDTO() { Setting = setting, SettingId = setting.Id, OwnerId = AuthenticatedUser.Id, StringValue = addValues[valType].Item1, NumericValue = addValues[valType].Item2, NumericArrayValue = addValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                long pk = SettingService.SaveUserSetting(value);
                Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {setting.Name} after add.");
                UserSettingDTO actualValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after add.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after add.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after add.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after add.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after add.");
            }

            // Update value.
            foreach (SettingValueType valType in userSettingsByType.Keys)
            {
                SettingDTO setting = userSettingsByType[valType];
                UserSettingDTO existingValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                UserSettingDTO expectedValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                expectedValue.StringValue = updateValues[valType].Item1;
                expectedValue.NumericValue = updateValues[valType].Item2;
                expectedValue.NumericArrayValue = updateValues[valType].Item3;
                expectedValue.InternalNumericValue = expectedValue.NumericValue;
                expectedValue.InternalNumericArrayValue = expectedValue.NumericArrayValue;
                long pk = SettingService.SaveUserSetting(expectedValue);
                Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {setting.Name} after update.");
                UserSettingDTO actualValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                Assert.IsNotNull(actualValue, $"Failed to get updated value for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after update.");
                CollectionAssert.AreEqual(expectedValue.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after update.");
                CollectionAssert.AreEqual(expectedValue.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.Id, actualValue.Id, $"User setting primary key does not match value returned from add call for setting {setting.Name} after update.");
            }

            // Remove everything to test multiple calls and to test remove.
            foreach (string name in userSettingNameAndType.Keys)
            {
                SettingDTO setting = userSettings.FirstOrDefault(s => s.Name == name);
                UserSettingDTO value = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                Assert.IsNotNull(value, $"Failed to get user setting value for {name} to remove before testing save multiple.");
                SettingService.RemoveUserSetting(value.Id.ToString());
            }

            // Test getting default values again for giggles.
            foreach (SettingValueType valType in userSettingsByType.Keys)
            {
                SettingDTO setting = userSettingsByType[valType];
                UserSettingDTO value = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                Assert.IsNotNull(value, $"Failed to get default value for setting {setting.Name}.");
                Assert.AreEqual(0, value.Id, $"Default value should have an id of zero for setting {setting.Name}.");
            }

            // Add multiple.
            var values = new List<UserSettingDTO>();
            foreach (SettingValueType valType in userSettingsByType.Keys)
            {
                SettingDTO setting = userSettingsByType[valType];
                var value = new UserSettingDTO() { Setting = setting, SettingId = setting.Id, OwnerId = AuthenticatedUser.Id, StringValue = addValues[valType].Item1, NumericValue = addValues[valType].Item2, NumericArrayValue = addValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                values.Add(value);
            }
            SettingService.SaveUserSettings(values.ToArray());
            foreach (SettingValueType valType in userSettingsByType.Keys)
            {
                UserSettingDTO value = values.Single(s => s.Setting.SettingValueType == valType);
                SettingDTO setting = value.Setting;
                UserSettingDTO actualValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after add multiple.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after add multiple.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after add multiple.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after add multiple.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after add multiple.");
            }

            // Update multiple.
            values.Clear();
            foreach (SettingValueType valType in userSettingsByType.Keys)
            {
                SettingDTO setting = userSettingsByType[valType];
                var value = new UserSettingDTO() { Setting = setting, SettingId = setting.Id, OwnerId = AuthenticatedUser.Id, StringValue = updateValues[valType].Item1, NumericValue = updateValues[valType].Item2, NumericArrayValue = updateValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                values.Add(value);
            }
            SettingService.SaveUserSettings(values.ToArray());
            foreach (SettingValueType valType in userSettingsByType.Keys)
            {
                UserSettingDTO value = values.Single(s => s.Setting.SettingValueType == valType);
                SettingDTO setting = value.Setting;
                UserSettingDTO actualValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after update multiple.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after update multiple.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after update multiple.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after update multiple.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after update multiple.");
            }

            // Test bad value set 1.
            foreach (SettingValueType valType in badValues1.Keys)
            {
                SettingDTO setting = userSettingsByType[valType];
                UserSettingDTO badValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                badValue.StringValue = badValues1[valType].Item1;
                badValue.NumericValue = badValues1[valType].Item2;
                badValue.NumericArrayValue = badValues1[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (1) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveUserSetting(badValue));
            }

            // Test bad value set 2.
            foreach (SettingValueType valType in badValues2.Keys)
            {
                SettingDTO setting = userSettingsByType[valType];
                UserSettingDTO badValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                badValue.StringValue = badValues2[valType].Item1;
                badValue.NumericValue = badValues2[valType].Item2;
                badValue.NumericArrayValue = badValues2[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (2) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveUserSetting(badValue));
            }

            // Test bad value set 3.
            foreach (SettingValueType valType in badValues3.Keys)
            {
                SettingDTO setting = userSettingsByType[valType];
                UserSettingDTO badValue = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                badValue.StringValue = badValues3[valType].Item1;
                badValue.NumericValue = badValues3[valType].Item2;
                badValue.NumericArrayValue = badValues3[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (3) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveUserSetting(badValue));
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void AddUpdateRemoveAssetSettings()
        {
            SettingType settingType = SettingType.Asset;
            SettingDTO[] assetSettings = SettingService.GetSettingsByType(settingType.ToString());

            string assetName = "SettingAssetTest1";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "Setting Asset Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset1 = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
            Assert.AreEqual(asset1.Name, assetName);
            _assetsToRemove.Add(asset1);

            // Okay... we need one of each value type.
            Dictionary<string, SettingValueType> assetSettingNameAndType = GetSettingValueTypeNames(settingType);

            List<TestSettingValues> settingValues = getSettingValues();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> addValues = settingValues.Where(x => x.settingName == "addValues").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> updateValues = settingValues.Where(x => x.settingName == "updateValues").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues1 = settingValues.Where(x => x.settingName == "badValues1").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues2 = settingValues.Where(x => x.settingName == "badValues2").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues3 = settingValues.Where(x => x.settingName == "badValues3").Select(x => x.settingValue).FirstOrDefault();

            var assetSettingsByType = new Dictionary<SettingValueType, SettingDTO>();
            foreach (var kvp in assetSettingNameAndType)
            {
                SettingDTO oneSetting = assetSettings.FirstOrDefault(s => s.Name == kvp.Key);
                Assert.IsNotNull(oneSetting, $"Failed to get setting with name {kvp.Key} and expected type {kvp.Value}.");
                Assert.AreEqual(kvp.Value, oneSetting.SettingValueType, $"Setting with name {kvp.Key} has unexpected type.");
                assetSettingsByType.Add(oneSetting.SettingValueType, oneSetting);
            }
            var settingValuesToRemove = new List<AssetSettingDTO>();
            foreach (SettingDTO setting in assetSettingsByType.Values)
            {
                AssetSettingDTO settingValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                Assert.IsNotNull(settingValue, $"Failed to get setting value for setting {setting.Name}.");
                if (settingValue.Id != 0)
                {
                    settingValuesToRemove.Add(GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id));
                    // Set PK to 0 so that it will add if we removed it.
                    settingValue.Id = 0;
                    _assetSettingsToRestore.Add(settingValue);
                }
                else
                {
                    _assetSettingNamesToRemove.Add(Tuple.Create(asset1.Id, setting.Name));
                }
            }
            // First remove existing values.
            foreach (AssetSettingDTO value in settingValuesToRemove)
            {
                SettingService.RemoveAssetSetting(value.Id.ToString());
            }

            // Test getting default values (should get value and id should be 0).
            foreach (SettingValueType valType in assetSettingsByType.Keys)
            {
                SettingDTO setting = assetSettingsByType[valType];
                AssetSettingDTO value = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                Assert.IsNotNull(value, $"Failed to get default value for setting {setting.Name}.");
                Assert.AreEqual(0, value.Id, $"Default value should have an id of zero for setting {setting.Name}.");
                switch (setting.SettingValueType)
                {
                    case SettingValueType.TrueOrFalse:
                        Assert.IsTrue(value.NumericValue == 0.0 || value.NumericValue == 1.0, $"Unexpected default numeric value {value.NumericValue} for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsTrue(value.InternalNumericValue == 0.0 || value.InternalNumericValue == 1.0, $"Unexpected default internal numeric value {value.NumericValue} for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    case SettingValueType.DecimalNumber:
                    case SettingValueType.Number:
                        Assert.IsNotNull(value.NumericValue, $"Numeric value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.InternalNumericValue, $"Internal numeric value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    case SettingValueType.DecimalNumberArray:
                        Assert.IsNull(value.NumericValue, $"Numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericValue, $"Internal numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.NumericArrayValue, $"Numeric array value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.InternalNumericArrayValue, $"Internal numeric array value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    default:
                        Assert.IsNull(value.NumericValue, $"Numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericValue, $"Internal numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.StringValue, $"String value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;
                }
            }

            // Add value so that it is no longer the default value.
            foreach (SettingValueType valType in assetSettingsByType.Keys)
            {
                SettingDTO setting = assetSettingsByType[valType];
                var value = new AssetSettingDTO() { Setting = setting, SettingId = setting.Id, AssetId = asset1.Id, StringValue = addValues[valType].Item1, NumericValue = addValues[valType].Item2, NumericArrayValue = addValues[valType].Item3 };
                long pk = SettingService.SaveAssetSetting(value);
                Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {setting.Name} after add.");
                AssetSettingDTO actualValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after add.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after add.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after add.");
            }

            // Update value.
            foreach (SettingValueType valType in assetSettingsByType.Keys)
            {
                SettingDTO setting = assetSettingsByType[valType];
                AssetSettingDTO existingValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                AssetSettingDTO expectedValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                expectedValue.StringValue = updateValues[valType].Item1;
                expectedValue.NumericValue = updateValues[valType].Item2;
                expectedValue.NumericArrayValue = updateValues[valType].Item3;
                expectedValue.InternalNumericValue = expectedValue.NumericValue;
                expectedValue.InternalNumericArrayValue = expectedValue.NumericArrayValue;
                long pk = SettingService.SaveAssetSetting(expectedValue);
                Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {setting.Name} after update.");
                AssetSettingDTO actualValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                Assert.IsNotNull(actualValue, $"Failed to get updated value for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after update.");
                CollectionAssert.AreEqual(expectedValue.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after update.");
                CollectionAssert.AreEqual(expectedValue.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.Id, actualValue.Id, $"Well setting primary key does not match value returned from add call for setting {setting.Name} after update.");
            }

            // Remove everything to test multiple calls and to test remove.
            foreach (string name in assetSettingNameAndType.Keys)
            {
                SettingDTO setting = assetSettings.FirstOrDefault(s => s.Name == name);
                AssetSettingDTO value = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                Assert.IsNotNull(value, $"Failed to get asset setting value for {name} to remove before testing save multiple.");
                SettingService.RemoveAssetSetting(value.Id.ToString());
            }

            // Test getting default values again for giggles.
            foreach (SettingValueType valType in assetSettingsByType.Keys)
            {
                SettingDTO setting = assetSettingsByType[valType];
                AssetSettingDTO value = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                Assert.IsNotNull(value, $"Failed to get default value for setting {setting.Name}.");
                Assert.AreEqual(0, value.Id, $"Default value should have an id of zero for setting {setting.Name}.");
            }

            // Add multiple.
            var values = new List<AssetSettingDTO>();
            foreach (SettingValueType valType in assetSettingsByType.Keys)
            {
                SettingDTO setting = assetSettingsByType[valType];
                var value = new AssetSettingDTO() { Setting = setting, SettingId = setting.Id, AssetId = asset1.Id, StringValue = addValues[valType].Item1, NumericValue = addValues[valType].Item2, NumericArrayValue = addValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                values.Add(value);
            }
            SettingService.SaveAssetSettings(values.ToArray());
            foreach (SettingValueType valType in assetSettingsByType.Keys)
            {
                AssetSettingDTO value = values.Single(s => s.Setting.SettingValueType == valType);
                SettingDTO setting = value.Setting;
                AssetSettingDTO actualValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after add multiple.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after add multiple.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after add multiple.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after add multiple.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after add multiple.");
            }

            // Update multiple.
            values.Clear();
            foreach (SettingValueType valType in assetSettingsByType.Keys)
            {
                SettingDTO setting = assetSettingsByType[valType];
                var value = new AssetSettingDTO() { Setting = setting, SettingId = setting.Id, AssetId = asset1.Id, StringValue = updateValues[valType].Item1, NumericValue = updateValues[valType].Item2, NumericArrayValue = updateValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                values.Add(value);
            }
            SettingService.SaveAssetSettings(values.ToArray());
            foreach (SettingValueType valType in assetSettingsByType.Keys)
            {
                AssetSettingDTO value = values.Single(s => s.Setting.SettingValueType == valType);
                SettingDTO setting = value.Setting;
                AssetSettingDTO actualValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after update multiple.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after update multiple.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after update multiple.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after update multiple.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after update multiple.");
            }

            // Test bad value set 1.
            foreach (SettingValueType valType in badValues1.Keys)
            {
                SettingDTO setting = assetSettingsByType[valType];
                AssetSettingDTO badValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                badValue.StringValue = badValues1[valType].Item1;
                badValue.NumericValue = badValues1[valType].Item2;
                badValue.NumericArrayValue = badValues1[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (1) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveAssetSetting(badValue));
            }

            // Test bad value set 2.
            foreach (SettingValueType valType in badValues2.Keys)
            {
                SettingDTO setting = assetSettingsByType[valType];
                AssetSettingDTO badValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                badValue.StringValue = badValues2[valType].Item1;
                badValue.NumericValue = badValues2[valType].Item2;
                badValue.NumericArrayValue = badValues2[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (2) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveAssetSetting(badValue));
            }

            // Test bad value set 3.
            foreach (SettingValueType valType in badValues3.Keys)
            {
                SettingDTO setting = assetSettingsByType[valType];
                AssetSettingDTO badValue = GetAssetSettingByAssetIdAndSettingId(asset1.Id, setting.Id);
                badValue.StringValue = badValues3[valType].Item1;
                badValue.NumericValue = badValues3[valType].Item2;
                badValue.NumericArrayValue = badValues3[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (3) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveAssetSetting(badValue));
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void AddUpdateRemoveWellSettings()
        {
            SettingType settingType = SettingType.Well;
            SettingDTO[] wellSettings = SettingService.GetSettingsByType(settingType.ToString());

            // Add a well to use for this test.
            var well = SetDefaultFluidType(new WellDTO { Name = DefaultWellName, CommissionDate = DateTime.Today, WellType = WellTypeId.GInj });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
            well = WellService.GetWellByName(well.Name);
            Assert.IsNotNull(well, "Failed to add well for test.");
            _wellsToRemove.Add(well);

            // Okay... we need one of each value type.
            Dictionary<string, SettingValueType> wellSettingNameAndType = GetSettingValueTypeNames(settingType);

            List<TestSettingValues> settingValues = getSettingValues();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> addValues = settingValues.Where(x => x.settingName == "addValues").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> updateValues = settingValues.Where(x => x.settingName == "updateValues").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues1 = settingValues.Where(x => x.settingName == "badValues1").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues2 = settingValues.Where(x => x.settingName == "badValues2").Select(x => x.settingValue).FirstOrDefault();
            Dictionary<SettingValueType, Tuple<string, double?, double[]>> badValues3 = settingValues.Where(x => x.settingName == "badValues3").Select(x => x.settingValue).FirstOrDefault();

            var wellSettingsByType = new Dictionary<SettingValueType, SettingDTO>();
            foreach (var kvp in wellSettingNameAndType)
            {
                SettingDTO oneSetting = wellSettings.FirstOrDefault(s => s.Name == kvp.Key);
                Assert.IsNotNull(oneSetting, $"Failed to get setting with name {kvp.Key} and expected type {kvp.Value}.");
                Assert.AreEqual(kvp.Value, oneSetting.SettingValueType, $"Setting with name {kvp.Key} has unexpected type.");
                wellSettingsByType.Add(oneSetting.SettingValueType, oneSetting);
            }
            var settingValuesToRemove = new List<WellSettingDTO>();
            foreach (SettingDTO setting in wellSettingsByType.Values)
            {
                WellSettingDTO settingValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                Assert.IsNotNull(settingValue, $"Failed to get setting value for setting {setting.Name}.");
                if (settingValue.Id != 0)
                {
                    settingValuesToRemove.Add(GetWellSettingByWellIdAndSettingId(well.Id, setting.Id));
                    // Set PK to 0 so that it will add if we removed it.
                    settingValue.Id = 0;
                    _wellSettingsToRestore.Add(settingValue);
                }
                else
                {
                    _wellSettingNamesToRemove.Add(Tuple.Create(well.Id, setting.Name));
                }
            }
            // First remove existing values.
            foreach (WellSettingDTO value in settingValuesToRemove)
            {
                SettingService.RemoveWellSetting(value.Id.ToString());
            }

            // Test getting default values (should get value and id should be 0).
            foreach (SettingValueType valType in wellSettingsByType.Keys)
            {
                SettingDTO setting = wellSettingsByType[valType];
                WellSettingDTO value = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                Assert.IsNotNull(value, $"Failed to get default value for setting {setting.Name}.");
                Assert.AreEqual(0, value.Id, $"Default value should have an id of zero for setting {setting.Name}.");
                switch (setting.SettingValueType)
                {
                    case SettingValueType.TrueOrFalse:
                        Assert.IsTrue(value.NumericValue == 0.0 || value.NumericValue == 1.0, $"Unexpected default numeric value {value.NumericValue} for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsTrue(value.InternalNumericValue == 0.0 || value.InternalNumericValue == 1.0, $"Unexpected default internal numeric value {value.NumericValue} for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    case SettingValueType.DecimalNumber:
                    case SettingValueType.Number:
                        Assert.IsNotNull(value.NumericValue, $"Numeric value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.InternalNumericValue, $"Internal numeric value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    case SettingValueType.DecimalNumberArray:
                        Assert.IsNull(value.NumericValue, $"Numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericValue, $"Internal numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.NumericArrayValue, $"Numeric array value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.InternalNumericArrayValue, $"Internal numeric array value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.StringValue, $"String value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;

                    default:
                        Assert.IsNull(value.NumericValue, $"Numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericValue, $"Internal numeric value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.NumericArrayValue, $"Numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNull(value.InternalNumericArrayValue, $"Internal numeric array value should be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        Assert.IsNotNull(value.StringValue, $"String value should not be null for type {setting.SettingValueType} on setting {setting.Name}.");
                        break;
                }
            }

            // Add value so that it is no longer the default value.
            foreach (SettingValueType valType in wellSettingsByType.Keys)
            {
                SettingDTO setting = wellSettingsByType[valType];
                var value = new WellSettingDTO() { Setting = setting, SettingId = setting.Id, WellId = well.Id, StringValue = addValues[valType].Item1, NumericValue = addValues[valType].Item2, NumericArrayValue = addValues[valType].Item3 };
                long pk = SettingService.SaveWellSetting(value);
                Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {setting.Name} after add.");
                WellSettingDTO actualValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after add.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after add.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after add.");
            }

            // Update value.
            foreach (SettingValueType valType in wellSettingsByType.Keys)
            {
                SettingDTO setting = wellSettingsByType[valType];
                WellSettingDTO existingValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                WellSettingDTO expectedValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                expectedValue.StringValue = updateValues[valType].Item1;
                expectedValue.NumericValue = updateValues[valType].Item2;
                expectedValue.NumericArrayValue = updateValues[valType].Item3;
                expectedValue.InternalNumericValue = expectedValue.NumericValue;
                expectedValue.InternalNumericArrayValue = expectedValue.NumericArrayValue;
                long pk = SettingService.SaveWellSetting(expectedValue);
                Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {setting.Name} after update.");
                WellSettingDTO actualValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                Assert.IsNotNull(actualValue, $"Failed to get updated value for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after update.");
                CollectionAssert.AreEqual(expectedValue.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after update.");
                CollectionAssert.AreEqual(expectedValue.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after update.");
                Assert.AreEqual(expectedValue.Id, actualValue.Id, $"Well setting primary key does not match value returned from add call for setting {setting.Name} after update.");
            }

            // Remove everything to test multiple calls and to test remove.
            foreach (string name in wellSettingNameAndType.Keys)
            {
                SettingDTO setting = wellSettings.FirstOrDefault(s => s.Name == name);
                WellSettingDTO value = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                Assert.IsNotNull(value, $"Failed to get well setting value for {name} to remove before testing save multiple.");
                SettingService.RemoveWellSetting(value.Id.ToString());
            }

            // Test getting default values again for giggles.
            foreach (SettingValueType valType in wellSettingsByType.Keys)
            {
                SettingDTO setting = wellSettingsByType[valType];
                WellSettingDTO value = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                Assert.IsNotNull(value, $"Failed to get default value for setting {setting.Name}.");
                Assert.AreEqual(0, value.Id, $"Default value should have an id of zero for setting {setting.Name}.");
            }

            // Add multiple.
            var values = new List<WellSettingDTO>();
            foreach (SettingValueType valType in wellSettingsByType.Keys)
            {
                SettingDTO setting = wellSettingsByType[valType];
                var value = new WellSettingDTO() { Setting = setting, SettingId = setting.Id, WellId = well.Id, StringValue = addValues[valType].Item1, NumericValue = addValues[valType].Item2, NumericArrayValue = addValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                values.Add(value);
            }
            SettingService.SaveWellSettings(values.ToArray());
            foreach (SettingValueType valType in wellSettingsByType.Keys)
            {
                WellSettingDTO value = values.Single(s => s.Setting.SettingValueType == valType);
                SettingDTO setting = value.Setting;
                WellSettingDTO actualValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after add multiple.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after add multiple.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after add multiple.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after add multiple.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after add multiple.");
            }

            // Update multiple.
            values.Clear();
            foreach (SettingValueType valType in wellSettingsByType.Keys)
            {
                SettingDTO setting = wellSettingsByType[valType];
                var value = new WellSettingDTO() { Setting = setting, SettingId = setting.Id, WellId = well.Id, StringValue = updateValues[valType].Item1, NumericValue = updateValues[valType].Item2, NumericArrayValue = updateValues[valType].Item3 };
                value.InternalNumericValue = value.NumericValue;
                value.InternalNumericArrayValue = value.NumericArrayValue;
                values.Add(value);
            }
            SettingService.SaveWellSettings(values.ToArray());
            foreach (SettingValueType valType in wellSettingsByType.Keys)
            {
                WellSettingDTO value = values.Single(s => s.Setting.SettingValueType == valType);
                SettingDTO setting = value.Setting;
                WellSettingDTO actualValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                Assert.AreEqual(value.StringValue, actualValue.StringValue, $"String value does not match for setting {setting.Name} after update multiple.");
                Assert.AreEqual(value.NumericValue, actualValue.NumericValue, $"Numeric value does not match for setting {setting.Name} after update multiple.");
                Assert.AreEqual(value.InternalNumericValue, actualValue.InternalNumericValue, $"Internal numeric value does not match for setting {setting.Name} after update multiple.");
                CollectionAssert.AreEqual(value.NumericArrayValue, actualValue.NumericArrayValue, $"Numeric array value does not match for setting {setting.Name} after update multiple.");
                CollectionAssert.AreEqual(value.InternalNumericArrayValue, actualValue.InternalNumericArrayValue, $"Internal numeric array value does not match for setting {setting.Name} after update multiple.");
            }

            // Test bad value set 1.
            foreach (SettingValueType valType in badValues1.Keys)
            {
                SettingDTO setting = wellSettingsByType[valType];
                WellSettingDTO badValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                badValue.StringValue = badValues1[valType].Item1;
                badValue.NumericValue = badValues1[valType].Item2;
                badValue.NumericArrayValue = badValues1[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (1) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveWellSetting(badValue));
            }

            // Test bad value set 2.
            foreach (SettingValueType valType in badValues2.Keys)
            {
                SettingDTO setting = wellSettingsByType[valType];
                WellSettingDTO badValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                badValue.StringValue = badValues2[valType].Item1;
                badValue.NumericValue = badValues2[valType].Item2;
                badValue.NumericArrayValue = badValues2[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (2) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveWellSetting(badValue));
            }

            // Test bad value set 3.
            foreach (SettingValueType valType in badValues3.Keys)
            {
                SettingDTO setting = wellSettingsByType[valType];
                WellSettingDTO badValue = GetWellSettingByWellIdAndSettingId(well.Id, setting.Id);
                badValue.StringValue = badValues3[valType].Item1;
                badValue.NumericValue = badValues3[valType].Item2;
                badValue.NumericArrayValue = badValues3[valType].Item3;
                TestThrowInternalServerError($"Saving setting {setting.Name} of type {valType} with bad values (3) {badValue.StringValue} | {badValue.NumericValue} | {badValue.NumericArrayValue} should fail.", () => SettingService.SaveWellSetting(badValue));
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void CascadeCheck()
        {
            string cascadeCheck = "CASCADE CHECK";

            //Create System Setting
            SettingDTO cascadeSetting = SettingService.GetSettingByName(SettingServiceStringConstants.CASCADE_TEST_SETTING);
            Assert.IsNotNull(cascadeSetting);
            SystemSettingDTO cascadeSystemSetting = new SystemSettingDTO();
            cascadeSystemSetting.Setting = cascadeSetting;
            cascadeSystemSetting.SettingId = cascadeSetting.Id;
            cascadeSystemSetting.StringValue = cascadeCheck;
            long cascadeSystemSettingId = SettingService.SaveSystemSetting(cascadeSystemSetting);
            cascadeSystemSetting.Id = cascadeSystemSettingId;
            Assert.AreNotEqual(0, cascadeSystemSettingId);
            _systemSettingsToRemove.Add(cascadeSystemSetting);

            //Create Assets
            string assetName1 = "CascadeAssetTest1";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName1, Description = "Cascade Asset Test Description" });
            var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset1 = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName1));
            Assert.AreEqual(asset1.Name, assetName1);
            _assetsToRemove.Add(asset1);

            string assetName2 = "CascadeAssetTest2";
            SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName2, Description = "Cascade Asset Test Description" });
            allAssets = SurfaceNetworkService.GetAllAssets().ToList();
            AssetDTO asset2 = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName2));
            Assert.AreEqual(asset2.Name, assetName2);
            _assetsToRemove.Add(asset2);

            //Create Wells
            string wellName1 = "CascadeWell1";
            WellDTO well1 = SetDefaultFluidType(new WellDTO { Name = wellName1, CommissionDate = DateTime.Today, WellType = WellTypeId.GInj, AssetId = asset1.Id });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well1 });
            well1 = WellService.GetWellByName(well1.Name);
            Assert.IsNotNull(well1, "Failed to add well for test.");
            _wellsToRemove.Add(well1);

            string wellName2 = "CascadeWell2";
            WellDTO well2 = SetDefaultFluidType(new WellDTO { Name = wellName2, CommissionDate = DateTime.Today, WellType = WellTypeId.GInj, AssetId = asset2.Id });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well2 });
            well2 = WellService.GetWellByName(well2.Name);
            Assert.IsNotNull(well2, "Failed to add well for test.");
            _wellsToRemove.Add(well2);


            //Since value is in system, it should be returned for all three:
            //Check Well Setting
            bool foundWellSetting = false;
            WellSettingDTO[] wellSettings = SettingService.GetWellSettingsByWellIdAndCategory(well1.Id.ToString(), SettingCategory.None.ToString());
            foreach (WellSettingDTO wellSetting in wellSettings)
            {
                if (wellSetting.SettingId == cascadeSetting.Id)
                {
                    Assert.AreEqual(cascadeCheck, wellSetting.StringValue);
                    foundWellSetting = true;
                }
            }
            Assert.IsTrue(foundWellSetting, "Could not find cascaded system value in well.");

            //Check Asset Setting
            bool foundAssetSetting = false;
            AssetSettingDTO[] assetSettings = SettingService.GetAssetSettingsByAssetId(asset1.Id.ToString());
            foreach (AssetSettingDTO assetSetting in assetSettings)
            {
                if (assetSetting.SettingId == cascadeSetting.Id)
                {
                    Assert.AreEqual(cascadeCheck, assetSetting.StringValue);
                    foundAssetSetting = true;
                }
            }
            Assert.IsTrue(foundAssetSetting, "Could not find cascaded system value in asset.");

            //Check System Setting
            bool foundSystemSetting = false;
            SystemSettingDTO[] systemSettings = SettingService.GetSystemSettingsByCategory(SettingCategory.None.ToString());
            foreach (SystemSettingDTO systemSetting in systemSettings)
            {
                if (systemSetting.SettingId == cascadeSetting.Id)
                {
                    Assert.AreEqual(cascadeCheck, systemSetting.StringValue);
                    foundSystemSetting = true;
                }
            }
            Assert.IsTrue(foundSystemSetting, "Could not find cascaded system value in system.");

            //Removing the System setting to not pollute lower tests.
            SettingService.RemoveSystemSetting(cascadeSystemSettingId.ToString());

            //Since value is in asset, it should be returned for both Well and Asset but not System.              

            //Create System Setting
            AssetSettingDTO cascadeAssetSetting = new AssetSettingDTO();
            cascadeAssetSetting.AssetId = asset1.Id;
            cascadeAssetSetting.Setting = cascadeSetting;
            cascadeAssetSetting.SettingId = cascadeSetting.Id;
            cascadeAssetSetting.StringValue = cascadeCheck;
            long cascadeAssetSettingId = SettingService.SaveAssetSetting(cascadeAssetSetting);
            cascadeAssetSetting.Id = cascadeAssetSettingId;
            Assert.AreNotEqual(0, cascadeAssetSettingId);
            _assetSettingsToRemove.Add(cascadeAssetSetting);

            //Check Well Setting
            foundWellSetting = false;
            wellSettings = SettingService.GetWellSettingsByWellIdAndCategory(well1.Id.ToString(), SettingCategory.None.ToString());
            foreach (WellSettingDTO wellSetting in wellSettings)
            {
                if (wellSetting.SettingId == cascadeSetting.Id)
                {
                    Assert.AreEqual(cascadeCheck, wellSetting.StringValue);
                    foundWellSetting = true;
                }
            }
            Assert.IsTrue(foundWellSetting, "Could not find cascaded asset value in well.");

            //Check Asset Setting
            foundAssetSetting = false;
            assetSettings = SettingService.GetAssetSettingsByAssetId(asset1.Id.ToString());
            foreach (AssetSettingDTO assetSetting in assetSettings)
            {
                if (assetSetting.SettingId == cascadeSetting.Id)
                {
                    Assert.AreEqual(cascadeCheck, assetSetting.StringValue);
                    foundAssetSetting = true;
                }
            }
            Assert.IsTrue(foundAssetSetting, "Could not find cascaded asset value in asset.");

            //Check that Wells look in their own asset.  Should be null.
            foundWellSetting = false;
            wellSettings = SettingService.GetWellSettingsByWellIdAndCategory(well2.Id.ToString(), SettingCategory.None.ToString());
            foreach (WellSettingDTO wellSetting in wellSettings)
            {
                if (wellSetting.SettingId == cascadeSetting.Id)
                {
                    Assert.AreEqual(null, wellSetting.StringValue);
                    foundWellSetting = true;
                }
            }
            Assert.IsTrue(foundWellSetting, "Could not find cascaded asset value in well.");


            //Check System Setting. Should be null.
            foundSystemSetting = false;
            systemSettings = SettingService.GetSystemSettingsByCategory(SettingCategory.None.ToString());
            foreach (SystemSettingDTO systemSetting in systemSettings)
            {
                if (systemSetting.SettingId == cascadeSetting.Id)
                {
                    Assert.AreEqual(null, systemSetting.StringValue);
                    foundSystemSetting = true;
                }
            }
            Assert.IsTrue(foundSystemSetting, "Could not find cascaded system value in system.");
        }

        protected WellSettingDTO GetWellSettingByWellIdAndSettingId(long wellId, long settingId)
        {
            return SettingService.GetWellSettingsByWellId(wellId.ToString()).FirstOrDefault(ws => ws.SettingId == settingId);
        }

        protected AssetSettingDTO GetAssetSettingByAssetIdAndSettingId(long assetId, long settingId)
        {
            return SettingService.GetAssetSettingsByAssetId(assetId.ToString()).FirstOrDefault(ws => ws.SettingId == settingId);
        }

        public void AddWell()
        {
            string facilityId;
            if (s_isRunningInATS)
                facilityId = "RPOC_00001";
            else
                facilityId = "RPOC_0001";
            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    IntervalAPI = "IntervalAPI",
                    SubAssemblyAPI = "SubAssemblyAPI",
                    AssemblyAPI = "AssemblyAPI",
                    CommissionDate = DateTime.Today,
                    WellType = WellTypeId.RRL,
                })
            });
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void SettingUoM()
        {
            try
            {
                const string settingName = SettingServiceStringConstants.UOM_TEST_SETTING;
                const double factor = 3.2808398950131233595800524934383;
                SettingDTO uomTestSetting = SettingService.GetSettingByName(settingName);
                Assert.IsNotNull(uomTestSetting, $"Failed to get setting named {settingName}, did you forget to add TestMode to true in the server config?");

                const string expectedUnitKey = UnitKeys.Feet;
                const string expectedInternalUnitKey = UnitKeys.Meters;

                string settingType, settingTypeLower;
                // Test system setting.
                {
                    settingType = "System";
                    settingTypeLower = settingType.ToLower();
                    // Get the system setting if set and remove
                    SystemSettingDTO systemSetting = SettingService.GetSystemSettingByName(settingName);
                    Assert.IsNotNull(systemSetting, $"Failed to get {settingTypeLower} setting.");
                    if (systemSetting.Id != 0)
                    {
                        SettingService.RemoveSystemSetting(systemSetting.Id.ToString());
                        systemSetting = SettingService.GetSystemSettingByName(settingName);
                    }
                    Assert.AreEqual(0, systemSetting.Id, $"{settingType} setting should be default and thus have an id of 0.");
                    Assert.AreEqual(expectedUnitKey, systemSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect.");
                    Assert.IsNotNull(systemSetting.NumericValue, $"{settingType} setting numeric value should not be null.");
                    Assert.AreEqual(1.5 * factor, systemSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected converted default value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, systemSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect.");
                    Assert.IsNotNull(systemSetting.InternalNumericValue, $"{settingType} setting internal numeric value should not be null.");
                    Assert.AreEqual(1.5, systemSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected internal default value not found.");

                    systemSetting.NumericValue = 2.0;
                    systemSetting.Setting.UnitKey = UnitKeys.Meters;
                    SettingService.SaveSystemSetting(systemSetting);
                    int updateCount = 1;
                    systemSetting = SettingService.GetSystemSettingByName(settingName);
                    Assert.AreNotEqual(0, systemSetting.Id, "Saved system setting should not have id of 0.");
                    Assert.IsNotNull(systemSetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    _systemSettingNamesToRemove.Add(settingName);
                    Assert.AreEqual(expectedUnitKey, systemSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(systemSetting.NumericValue, $"{settingType} setting numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0 * factor, systemSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, systemSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(systemSetting.InternalNumericValue, $"{settingType} setting internal numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0, systemSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) internal value not found.");

                    systemSetting.NumericValue = 2.0;
                    SettingService.SaveSystemSetting(systemSetting);
                    updateCount += 1;
                    systemSetting = SettingService.GetSystemSettingByName(settingName);
                    Assert.IsNotNull(systemSetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    Assert.AreEqual(expectedUnitKey, systemSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(systemSetting.NumericValue, $"{settingType} setting numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0, systemSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, systemSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(systemSetting.InternalNumericValue, $"{settingType} setting internal numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0 / factor, systemSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) internal value not found.");

                    SettingService.RemoveSystemSetting(systemSetting.Id.ToString());
                    systemSetting = SettingService.GetSystemSettingByName(settingName);
                    Assert.AreEqual(0, systemSetting.Id, $"Failed to remove {settingTypeLower} setting.");
                }

                // Test user setting.
                {
                    settingType = "User";
                    settingTypeLower = settingType.ToLower();
                    // Get the user setting if set and remove.
                    UserSettingDTO userSetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    Assert.IsNotNull(userSetting, $"Failed to get {settingTypeLower} setting.");
                    if (userSetting.Id != 0)
                    {
                        SettingService.RemoveUserSetting(userSetting.Id.ToString());
                        userSetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    }
                    Assert.AreEqual(0, userSetting.Id, $"{settingType} setting should be default and thus have an id of 0.");
                    Assert.AreEqual(expectedUnitKey, userSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect.");
                    Assert.IsNotNull(userSetting.NumericValue, $"{settingType} setting numeric value should not be null.");
                    Assert.AreEqual(1.5 * factor, userSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected converted default value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, userSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect.");
                    Assert.IsNotNull(userSetting.InternalNumericValue, $"{settingType} setting internal numeric value should not be null.");
                    Assert.AreEqual(1.5, userSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected internal default value not found.");

                    userSetting.NumericValue = 2.0;
                    userSetting.Setting.UnitKey = UnitKeys.Meters;
                    SettingService.SaveUserSetting(userSetting);
                    int updateCount = 1;
                    userSetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    Assert.IsNotNull(userSetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    _userSettingNamesToRemove.Add(settingName);
                    Assert.AreEqual(expectedUnitKey, userSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(userSetting.NumericValue, $"{settingType} setting numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0 * factor, userSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, userSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(userSetting.InternalNumericValue, $"{settingType} setting internal numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0, userSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) internal value not found.");

                    userSetting.NumericValue = 2.0;
                    SettingService.SaveUserSetting(userSetting);
                    updateCount += 1;
                    userSetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    Assert.IsNotNull(userSetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    Assert.AreEqual(expectedUnitKey, userSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(userSetting.NumericValue, $"{settingType} setting numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0, userSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, userSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(userSetting.InternalNumericValue, $"{settingType} setting internal numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0 / factor, userSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) internal value not found.");

                    SettingService.RemoveUserSetting(userSetting.Id.ToString());
                    userSetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    Assert.AreEqual(0, userSetting.Id, $"Failed to remove {settingTypeLower} setting.");
                }

                // Add a well to use for this test.
                var well = SetDefaultFluidType(new WellDTO { Name = DefaultWellName, CommissionDate = DateTime.Today, WellType = WellTypeId.GInj });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                well = WellService.GetWellByName(well.Name);
                Assert.IsNotNull(well, "Failed to add well for test.");
                _wellsToRemove.Add(well);

                // Test well setting.
                {
                    settingType = "Well";
                    settingTypeLower = settingType.ToLower();

                    // Get the well setting if set and remove.
                    WellSettingDTO wellSetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    Assert.IsNotNull(wellSetting, $"Failed to get {settingTypeLower} setting.");
                    if (wellSetting.Id != 0)
                    {
                        SettingService.RemoveWellSetting(wellSetting.Id.ToString());
                        wellSetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    }
                    Assert.AreEqual(0, wellSetting.Id, $"{settingType} setting should be default and thus have an id of 0.");
                    Assert.AreEqual(expectedUnitKey, wellSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect.");
                    Assert.IsNotNull(wellSetting.NumericValue, $"{settingType} setting numeric value should not be null.");
                    Assert.AreEqual(1.5 * factor, wellSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected converted default value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, wellSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect.");
                    Assert.IsNotNull(wellSetting.InternalNumericValue, $"{settingType} setting internal numeric value should not be null.");
                    Assert.AreEqual(1.5, wellSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected internal default value not found.");

                    wellSetting.NumericValue = 2.0;
                    wellSetting.Setting.UnitKey = UnitKeys.Meters;
                    SettingService.SaveWellSetting(wellSetting);
                    int updateCount = 1;
                    wellSetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    Assert.IsNotNull(wellSetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    _wellSettingNamesToRemove.Add(Tuple.Create(well.Id, settingName));
                    Assert.AreEqual(expectedUnitKey, wellSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(wellSetting.NumericValue, $"{settingType} setting numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0 * factor, wellSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, wellSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(wellSetting.InternalNumericValue, $"{settingType} setting numeric internal value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0, wellSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) internal value not found.");

                    wellSetting.NumericValue = 2.0;
                    SettingService.SaveWellSetting(wellSetting);
                    updateCount += 1;
                    wellSetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    Assert.IsNotNull(wellSetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    Assert.AreEqual(expectedUnitKey, wellSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(wellSetting.NumericValue, $"{settingType} setting numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0, wellSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, wellSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(wellSetting.InternalNumericValue, $"{settingType} setting numeric internal value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0 / factor, wellSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) internal value not found.");

                    SettingService.RemoveWellSetting(wellSetting.Id.ToString());
                    wellSetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    Assert.AreEqual(0, wellSetting.Id, $"Failed to remove {settingTypeLower} setting.");
                }

                // Add an asset to use for this test.
                string assetName1 = "UOMAssetTest1";
                SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName1, Description = "UOM Asset Test Description" });
                var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
                AssetDTO asset1 = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName1));
                Assert.AreEqual(asset1.Name, assetName1);
                _assetsToRemove.Add(asset1);

                // Test asset setting.
                {
                    settingType = "Asset";
                    settingTypeLower = settingType.ToLower();

                    // Get the asset setting if set and remove.
                    AssetSettingDTO assetSetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    Assert.IsNotNull(assetSetting, $"Failed to get {settingTypeLower} setting.");
                    if (assetSetting.Id != 0)
                    {
                        SettingService.RemoveAssetSetting(assetSetting.Id.ToString());
                        assetSetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    }
                    Assert.AreEqual(0, assetSetting.Id, $"{settingType} setting should be default and thus have an id of 0.");
                    Assert.AreEqual(expectedUnitKey, assetSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect.");
                    Assert.IsNotNull(assetSetting.NumericValue, $"{settingType} setting numeric value should not be null.");
                    Assert.AreEqual(1.5 * factor, assetSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected converted default value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, assetSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect.");
                    Assert.IsNotNull(assetSetting.InternalNumericValue, $"{settingType} setting internal numeric value should not be null.");
                    Assert.AreEqual(1.5, assetSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected internal default value not found.");

                    assetSetting.NumericValue = 2.0;
                    assetSetting.Setting.UnitKey = UnitKeys.Meters;
                    SettingService.SaveAssetSetting(assetSetting);
                    int updateCount = 1;
                    assetSetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    Assert.IsNotNull(assetSetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    _assetSettingNamesToRemove.Add(Tuple.Create(asset1.Id, settingName));
                    Assert.AreEqual(expectedUnitKey, assetSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(assetSetting.NumericValue, $"{settingType} setting numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0 * factor, assetSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, assetSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(assetSetting.InternalNumericValue, $"{settingType} setting numeric internal value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0, assetSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) internal value not found.");

                    assetSetting.NumericValue = 2.0;
                    SettingService.SaveAssetSetting(assetSetting);
                    updateCount += 1;
                    assetSetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    Assert.IsNotNull(assetSetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    Assert.AreEqual(expectedUnitKey, assetSetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(assetSetting.NumericValue, $"{settingType} setting numeric value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0, assetSetting.NumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, assetSetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(assetSetting.InternalNumericValue, $"{settingType} setting numeric internal value should not be null on updated (#{updateCount}).");
                    Assert.AreEqual(2.0 / factor, assetSetting.InternalNumericValue ?? 0.0, 0.01, $"{settingType} setting expected updated (#{updateCount}) internal value not found.");

                    SettingService.RemoveAssetSetting(assetSetting.Id.ToString());
                    assetSetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    Assert.AreEqual(0, assetSetting.Id, $"Failed to remove {settingTypeLower} setting.");
                }

                // Test validation range.
                TestValidationRangeUoM(settingName, uomTestSetting, well, factor, 1.0, 5000.0);
            }

            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void OtherGetMethods()
        {
            Trace.WriteLine("OtherGetMethods(), enumerate the Setting Categories and check the counts.");

            SettingDTO[] allSettings = SettingService.GetAllSettings();
            SystemSettingDTO[] allSystemSettings = SettingService.GetSystemSettings();
            UserSettingDTO[] allUserSettings = SettingService.GetUserSettingsByUserId(AuthenticatedUser.Id.ToString());

            Trace.WriteLine("************* All Settings obtained from API 1 :  [GetAllSettings] :***********");
            Trace.WriteLine($"Count of All Settings from API 1 :  [GetAllSettings] : {allSettings.Length}");
            int i = 1;
            foreach (SettingDTO setname in allSettings)
            {
                Trace.WriteLine($" {i} ==> Setting name: { setname.Name}");
                i++;
            }

            foreach (SettingCategory category in Enum.GetValues(typeof(SettingCategory)))
            {
                Trace.WriteLine($"Category={category}");

                List<SettingDTO> categorySettingsFromAll;
                List<SystemSettingDTO> categorySystemSettingsFromAll;
                List<UserSettingDTO> categoryUserSettingsFromAll;
                if (category != SettingCategory.None)
                {
                    categorySettingsFromAll = allSettings.Where(s => s.SettingCategory.HasFlag(category)).ToList();
                    categorySystemSettingsFromAll = allSystemSettings.Where(s => s.Setting.SettingCategory.HasFlag(category)).ToList();
                    categoryUserSettingsFromAll = allUserSettings.Where(s => s.Setting.SettingCategory.HasFlag(category)).ToList();
                }
                else
                {
                    Trace.WriteLine("Category=None, special handling");

                    categorySettingsFromAll = allSettings.Where(s => s.SettingCategory == category).ToList();
                    categorySystemSettingsFromAll = allSystemSettings.Where(s => s.Setting.SettingCategory == category).ToList();
                    categoryUserSettingsFromAll = allUserSettings.Where(s => s.Setting.SettingCategory == category).ToList();
                }


                SettingDTO[] categorySettings = SettingService.GetSettingsByCategory(category.ToString());
                Trace.WriteLine("*************  All Settings obtained from API 2 :  [GetAllSettingsBySettingCategory] :***********");
                Trace.WriteLine($"Count of All Settings from API 2 :  [GetAllSettingsBySettingCategory] : {categorySettings.Length}  and Category name : {category.ToString()} ");
                i = 1;
                foreach (SettingDTO setname in categorySettings)
                {
                    Trace.WriteLine($" {i} ==> Setting name: { setname.Name}");
                    i++;
                }

                Assert.AreEqual(categorySettingsFromAll.Count, categorySettings.Length, $"Unexpected setting count from {nameof(ISettingService.GetSettingsByCategory)} for category {category}.");

                SystemSettingDTO[] categorySystemSettings = SettingService.GetSystemSettingsByCategory(category.ToString());
                Assert.AreEqual(categorySystemSettingsFromAll.Count, categorySystemSettings.Length, $"Unexpected setting count from {nameof(ISettingService.GetSystemSettingsByCategory)} for category {category}.");

                UserSettingDTO[] categoryUserSettings = SettingService.GetUserSettingsByUserIdAndCategory(AuthenticatedUser.Id.ToString(), category.ToString());
                Assert.AreEqual(categoryUserSettingsFromAll.Count, categoryUserSettings.Length, $"Unexpected setting count from {nameof(ISettingService.GetUserSettingsByUserIdAndCategory)} for category {category}.");
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void SettingUoMArray()
        {
            try
            {
                const string settingName = SettingServiceStringConstants.UOM_TEST_SETTING_2;
                const double factor = 3.2808398950131233595800524934383;
                SettingDTO uomTestSetting = SettingService.GetSettingByName(settingName);
                Assert.IsNotNull(uomTestSetting, $"Failed to get setting named {settingName}, did you forget to add TestMode to true in the server config?");

                const string expectedUnitKey = UnitKeys.Feet;
                const string expectedInternalUnitKey = UnitKeys.Meters;

                string settingType, settingTypeLower;
                // Test system setting.
                {
                    settingType = "System";
                    settingTypeLower = settingType.ToLower();
                    // Get the system setting if set.
                    SystemSettingDTO systemArraySetting = SettingService.GetSystemSettingByName(settingName);
                    Assert.IsNotNull(systemArraySetting, $"Failed to get {settingTypeLower} array setting.");
                    if (systemArraySetting.Id != 0)
                    {
                        SettingService.RemoveSystemSetting(systemArraySetting.Id.ToString());
                        systemArraySetting = SettingService.GetSystemSettingByName(settingName);
                    }
                    Assert.AreEqual(0, systemArraySetting.Id, $"{settingType} array setting should be default and thus have an id of 0.");
                    Assert.AreEqual(expectedUnitKey, systemArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect for array.");
                    Assert.IsNotNull(systemArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null for array.");
                    AreEqual(new double[] { 5.6 * factor, 9.7 * factor }, systemArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted default array value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, systemArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect for array.");
                    Assert.IsNotNull(systemArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null for array.");
                    AreEqual(new double[] { 5.6, 9.7 }, systemArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal default array value not found.");

                    systemArraySetting.NumericArrayValue = new double[] { 98.37, 66.27, 82.389, 127.903 };
                    systemArraySetting.Setting.UnitKey = UnitKeys.Meters;
                    SettingService.SaveSystemSetting(systemArraySetting);
                    int updateCount = 1;
                    systemArraySetting = SettingService.GetSystemSettingByName(settingName);
                    Assert.IsNotNull(systemArraySetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    _systemSettingNamesToRemove.Add(settingName);
                    Assert.AreEqual(expectedUnitKey, systemArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(systemArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null on updated (#{updateCount}).");
                    AreEqual(new double[] { factor * 98.37, factor * 66.27, factor * 82.389, factor * 127.903 }, systemArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, systemArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(systemArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null on updated (#{updateCount}).");
                    AreEqual(new double[] { 98.37, 66.27, 82.389, 127.903 }, systemArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal updated (#{updateCount}) value not found.");

                    systemArraySetting.NumericArrayValue = new double[] { 98.37, 66.27, 82.389, 127.903 };
                    SettingService.SaveSystemSetting(systemArraySetting);
                    updateCount += 1;
                    systemArraySetting = SettingService.GetSystemSettingByName(settingName);
                    Assert.IsNotNull(systemArraySetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    Assert.AreEqual(expectedUnitKey, systemArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(systemArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null on updated (#{updateCount}) value.");
                    AreEqual(new double[] { 98.37, 66.27, 82.389, 127.903 }, systemArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, systemArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(systemArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null on updated (#{updateCount}) value.");
                    AreEqual(new double[] { 98.37 / factor, 66.27 / factor, 82.389 / factor, 127.903 / factor }, systemArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal updated (#{updateCount}) value not found.");

                    SettingService.RemoveSystemSetting(systemArraySetting.Id.ToString());
                    systemArraySetting = SettingService.GetSystemSettingByName(settingName);
                    Assert.AreEqual(0, systemArraySetting.Id, $"Failed to remove {settingTypeLower} setting.");
                }

                // Test user setting.
                {
                    settingType = "User";
                    settingTypeLower = settingType.ToLower();
                    // Get the user setting if set.
                    UserSettingDTO userArraySetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    Assert.IsNotNull(userArraySetting, $"Failed to get {settingTypeLower} array setting.");
                    if (userArraySetting.Id != 0)
                    {
                        SettingService.RemoveUserSetting(userArraySetting.Id.ToString());
                        userArraySetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    }
                    Assert.AreEqual(0, userArraySetting.Id, $"{settingType} array setting should be default and thus have an id of 0.");
                    Assert.AreEqual(expectedUnitKey, userArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect for array.");
                    Assert.IsNotNull(userArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null for array.");
                    AreEqual(new double[] { 5.6 * factor, 9.7 * factor }, userArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted default array value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, userArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect for array.");
                    Assert.IsNotNull(userArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null for array.");
                    AreEqual(new double[] { 5.6, 9.7 }, userArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal default array value not found.");

                    userArraySetting.NumericArrayValue = new double[] { 98.37, 66.27, 82.389, 127.903 };
                    userArraySetting.Setting.UnitKey = UnitKeys.Meters;
                    SettingService.SaveUserSetting(userArraySetting);
                    int updateCount = 1;
                    userArraySetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    Assert.IsNotNull(userArraySetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    _userSettingNamesToRemove.Add(settingName);
                    Assert.AreEqual(expectedUnitKey, userArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(userArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null on updated (#{updateCount}).");
                    AreEqual(new double[] { factor * 98.37, factor * 66.27, factor * 82.389, factor * 127.903 }, userArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, userArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(userArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null on updated (#{updateCount}).");
                    AreEqual(new double[] { 98.37, 66.27, 82.389, 127.903 }, userArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal updated (#{updateCount}) value not found.");

                    userArraySetting.NumericArrayValue = new double[] { 98.37, 66.27, 82.389, 127.903 };
                    SettingService.SaveUserSetting(userArraySetting);
                    updateCount += 1;
                    userArraySetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    Assert.IsNotNull(userArraySetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    Assert.AreEqual(expectedUnitKey, userArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(userArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null on updated (#{updateCount}) value.");
                    AreEqual(new double[] { 98.37, 66.27, 82.389, 127.903 }, userArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, userArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(userArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null on updated (#{updateCount}) value.");
                    AreEqual(new double[] { 98.37 / factor, 66.27 / factor, 82.389 / factor, 127.903 / factor }, userArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal updated (#{updateCount}) value not found.");

                    SettingService.RemoveUserSetting(userArraySetting.Id.ToString());
                    userArraySetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                    Assert.AreEqual(0, userArraySetting.Id, $"Failed to remove {settingTypeLower} setting.");
                }

                // Add a well to use for this test.
                var well = SetDefaultFluidType(new WellDTO { Name = DefaultWellName, CommissionDate = DateTime.Today, WellType = WellTypeId.GInj });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                well = WellService.GetWellByName(well.Name);
                Assert.IsNotNull(well, "Failed to add well for test.");
                _wellsToRemove.Add(well);

                // Test well setting.
                {
                    settingType = "Well";
                    settingTypeLower = settingType.ToLower();

                    // Get the well setting if set.
                    WellSettingDTO wellArraySetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    Assert.IsNotNull(wellArraySetting, $"Failed to get {settingTypeLower} array setting.");
                    if (wellArraySetting.Id != 0)
                    {
                        SettingService.RemoveWellSetting(wellArraySetting.Id.ToString());
                        wellArraySetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    }
                    Assert.AreEqual(0, wellArraySetting.Id, $"{settingType} array setting should be default and thus have an id of 0.");
                    Assert.AreEqual(expectedUnitKey, wellArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect for array.");
                    Assert.IsNotNull(wellArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null for array.");
                    AreEqual(new double[] { 5.6 * factor, 9.7 * factor }, wellArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted default array value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, wellArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect for array.");
                    Assert.IsNotNull(wellArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null for array.");
                    AreEqual(new double[] { 5.6, 9.7 }, wellArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal default array value not found.");

                    wellArraySetting.NumericArrayValue = new double[] { 98.37, 66.27, 82.389, 127.903 };
                    wellArraySetting.Setting.UnitKey = UnitKeys.Meters;
                    SettingService.SaveWellSetting(wellArraySetting);
                    int updateCount = 1;
                    wellArraySetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    Assert.IsNotNull(wellArraySetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    _wellSettingNamesToRemove.Add(Tuple.Create(well.Id, settingName));
                    Assert.AreEqual(expectedUnitKey, wellArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(wellArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null on updated (#{updateCount}).");
                    AreEqual(new double[] { factor * 98.37, factor * 66.27, factor * 82.389, factor * 127.903 }, wellArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, wellArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(wellArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null on updated (#{updateCount}).");
                    AreEqual(new double[] { 98.37, 66.27, 82.389, 127.903 }, wellArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal updated (#{updateCount}) value not found.");

                    wellArraySetting.NumericArrayValue = new double[] { 98.37, 66.27, 82.389, 127.903 };
                    SettingService.SaveWellSetting(wellArraySetting);
                    updateCount += 1;
                    wellArraySetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    Assert.IsNotNull(wellArraySetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    Assert.AreEqual(expectedUnitKey, wellArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(wellArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null on updated (#{updateCount}) value.");
                    AreEqual(new double[] { 98.37, 66.27, 82.389, 127.903 }, wellArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, wellArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(wellArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null on updated (#{updateCount}) value.");
                    AreEqual(new double[] { 98.37 / factor, 66.27 / factor, 82.389 / factor, 127.903 / factor }, wellArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal updated (#{updateCount}) value not found.");

                    SettingService.RemoveWellSetting(wellArraySetting.Id.ToString());
                    wellArraySetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                    Assert.AreEqual(0, wellArraySetting.Id, $"Failed to remove {settingTypeLower} setting.");
                }

                // Add an asset to use for this test.
                string assetName1 = "UOMAssetTest1";
                SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName1, Description = "UOM Asset Test Description" });
                var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
                AssetDTO asset1 = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName1));
                Assert.AreEqual(asset1.Name, assetName1);
                _assetsToRemove.Add(asset1);

                // Test asset setting.
                {
                    settingType = "Asset";
                    settingTypeLower = settingType.ToLower();

                    // Get the asset setting if set.
                    AssetSettingDTO assetArraySetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    Assert.IsNotNull(assetArraySetting, $"Failed to get {settingTypeLower} array setting.");
                    if (assetArraySetting.Id != 0)
                    {
                        SettingService.RemoveAssetSetting(assetArraySetting.Id.ToString());
                        assetArraySetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    }
                    Assert.AreEqual(0, assetArraySetting.Id, $"{settingType} array setting should be default and thus have an id of 0.");
                    Assert.AreEqual(expectedUnitKey, assetArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect for array.");
                    Assert.IsNotNull(assetArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null for array.");
                    AreEqual(new double[] { 5.6 * factor, 9.7 * factor }, assetArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted default array value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, assetArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect for array.");
                    Assert.IsNotNull(assetArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null for array.");
                    AreEqual(new double[] { 5.6, 9.7 }, assetArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal default array value not found.");

                    assetArraySetting.NumericArrayValue = new double[] { 98.37, 66.27, 82.389, 127.903 };
                    assetArraySetting.Setting.UnitKey = UnitKeys.Meters;
                    SettingService.SaveAssetSetting(assetArraySetting);
                    int updateCount = 1;
                    assetArraySetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    Assert.IsNotNull(assetArraySetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    _assetSettingNamesToRemove.Add(Tuple.Create(asset1.Id, settingName));
                    Assert.AreEqual(expectedUnitKey, assetArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(assetArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null on updated (#{updateCount}).");
                    AreEqual(new double[] { factor * 98.37, factor * 66.27, factor * 82.389, factor * 127.903 }, assetArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, assetArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(assetArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null on updated (#{updateCount}).");
                    AreEqual(new double[] { 98.37, 66.27, 82.389, 127.903 }, assetArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal updated (#{updateCount}) value not found.");

                    assetArraySetting.NumericArrayValue = new double[] { 98.37, 66.27, 82.389, 127.903 };
                    SettingService.SaveAssetSetting(assetArraySetting);
                    updateCount += 1;
                    assetArraySetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    Assert.IsNotNull(assetArraySetting, $"Failed to get {settingTypeLower} setting after update (#{updateCount}).");
                    Assert.AreEqual(expectedUnitKey, assetArraySetting.Setting.UnitKey, $"{settingType} setting unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(assetArraySetting.NumericArrayValue, $"{settingType} setting numeric array value should not be null on updated (#{updateCount}) value.");
                    AreEqual(new double[] { 98.37, 66.27, 82.389, 127.903 }, assetArraySetting.NumericArrayValue, 0.01, $"{settingType} setting expected converted updated (#{updateCount}) value not found.");
                    Assert.AreEqual(expectedInternalUnitKey, assetArraySetting.Setting.InternalUnitKey, $"{settingType} setting internal unit key incorrect on updated (#{updateCount}) value.");
                    Assert.IsNotNull(assetArraySetting.InternalNumericArrayValue, $"{settingType} setting internal numeric array value should not be null on updated (#{updateCount}) value.");
                    AreEqual(new double[] { 98.37 / factor, 66.27 / factor, 82.389 / factor, 127.903 / factor }, assetArraySetting.InternalNumericArrayValue, 0.01, $"{settingType} setting expected internal updated (#{updateCount}) value not found.");

                    SettingService.RemoveAssetSetting(assetArraySetting.Id.ToString());
                    assetArraySetting = GetAssetSettingByAssetIdAndSettingId(asset1.Id, uomTestSetting.Id);
                    Assert.AreEqual(0, assetArraySetting.Id, $"Failed to remove {settingTypeLower} setting.");
                }

                // Test validation range.
                TestValidationRangeUoM(settingName, uomTestSetting, well, factor, 10.0, 10000.0);
            }
            finally
            {

                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void SettingPrecision()
        {
            try
            {
                // Add a well to use for this test.
                var well = SetDefaultFluidType(new WellDTO { Name = DefaultWellName, CommissionDate = DateTime.Today, WellType = WellTypeId.GInj });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                well = WellService.GetWellByName(well.Name);
                Assert.IsNotNull(well, "Failed to add well for test.");
                _wellsToRemove.Add(well);

                string settingName = SettingServiceStringConstants.UOM_TEST_SETTING;
                sbyte expectedPrecision = 2;
                string unitSystem = SettingServiceStringConstants.UNIT_SYSTEM_NAME_US;
                SettingDTO uomTestSetting = SettingService.GetSettingByName(settingName);
                Assert.IsNotNull(uomTestSetting, $"Failed to get setting named {settingName}, did you forget to add TestMode to true in the server config?");
                Assert.AreEqual(expectedPrecision, uomTestSetting.Precision ?? 0, $"Setting has unexpected precision for {unitSystem} units.");
                SystemSettingDTO systemSetting = SettingService.GetSystemSettingByName(settingName);
                Assert.AreEqual(expectedPrecision, systemSetting.Setting.Precision ?? 0, $"System setting has unexpected precision for {unitSystem} units.");
                UserSettingDTO userSetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                Assert.AreEqual(expectedPrecision, userSetting.Setting.Precision ?? 0, $"User setting has unexpected precision for {unitSystem} units.");
                WellSettingDTO wellSetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                Assert.AreEqual(expectedPrecision, userSetting.Setting.Precision ?? 0, $"Well setting has unexpected precision for {unitSystem} units.");

                expectedPrecision = 3;
                unitSystem = SettingServiceStringConstants.UNIT_SYSTEM_NAME_METRIC;
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                uomTestSetting = SettingService.GetSettingByName(settingName);
                Assert.AreEqual(expectedPrecision, uomTestSetting.Precision ?? 0, $"Setting has unexpected precision for {unitSystem} units.");
                systemSetting = SettingService.GetSystemSettingByName(settingName);
                Assert.AreEqual(expectedPrecision, systemSetting.Setting.Precision ?? 0, $"System setting has unexpected precision for {unitSystem} units.");
                userSetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
                Assert.AreEqual(expectedPrecision, userSetting.Setting.Precision ?? 0, $"User setting has unexpected precision for {unitSystem} units.");
                wellSetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
                Assert.AreEqual(expectedPrecision, userSetting.Setting.Precision ?? 0, $"Well setting has unexpected precision for {unitSystem} units.");
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        private void TestValidationRangeUoM(string settingName, SettingDTO uomTestSetting, WellDTO well, double factor, double min, double max)
        {
            // Test converted values.
            double expectedMin = min * factor;
            double expectedMax = max * factor;

            Assert.AreEqual(expectedMin, uomTestSetting.MinValue.Value, 0.001, "Converted minimum validation value for setting is incorrect.");
            Assert.AreEqual(expectedMax, uomTestSetting.MaxValue.Value, 0.001, "Converted maximum validation value for setting is incorrect.");

            SystemSettingDTO systemSetting = SettingService.GetSystemSettingByName(settingName);
            Assert.AreEqual(expectedMin, systemSetting.Setting.MinValue.Value, 0.001, "Converted minimum validation value for system setting is incorrect.");
            Assert.AreEqual(expectedMax, systemSetting.Setting.MaxValue.Value, 0.001, "Converted maximum validation value for system setting is incorrect.");

            UserSettingDTO userSetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
            Assert.AreEqual(expectedMin, userSetting.Setting.MinValue.Value, 0.001, "Converted minimum validation value for user setting is incorrect.");
            Assert.AreEqual(expectedMax, userSetting.Setting.MaxValue.Value, 0.001, "Converted maximum validation value for user setting is incorrect.");

            WellSettingDTO wellSetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
            Assert.AreEqual(expectedMin, wellSetting.Setting.MinValue.Value, 0.001, "Converted minimum validation value for well setting is incorrect.");
            Assert.AreEqual(expectedMax, wellSetting.Setting.MaxValue.Value, 0.001, "Converted maximum validation value for well setting is incorrect.");

            // Now test unconverted values.
            expectedMin = min;
            expectedMax = max;
            ChangeUnitSystem("Metric");
            ChangeUnitSystemUserSetting("Metric");
            uomTestSetting = SettingService.GetSettingByName(settingName);
            Assert.AreEqual(expectedMin, uomTestSetting.MinValue.Value, 0.001, "Unconverted minimum validation value for setting is incorrect.");
            Assert.AreEqual(expectedMax, uomTestSetting.MaxValue.Value, 0.001, "Unconverted maximum validation value for setting is incorrect.");

            systemSetting = SettingService.GetSystemSettingByName(settingName);
            Assert.AreEqual(expectedMin, systemSetting.Setting.MinValue.Value, 0.001, "Unconverted minimum validation value for system setting is incorrect.");
            Assert.AreEqual(expectedMax, systemSetting.Setting.MaxValue.Value, 0.001, "Unconverted maximum validation value for system setting is incorrect.");

            userSetting = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), uomTestSetting.Id.ToString());
            Assert.AreEqual(expectedMin, userSetting.Setting.MinValue.Value, 0.001, "Unconverted minimum validation value for user setting is incorrect.");
            Assert.AreEqual(expectedMax, userSetting.Setting.MaxValue.Value, 0.001, "Unconverted maximum validation value for user setting is incorrect.");

            wellSetting = GetWellSettingByWellIdAndSettingId(well.Id, uomTestSetting.Id);
            Assert.AreEqual(expectedMin, wellSetting.Setting.MinValue.Value, 0.001, "Unconverted minimum validation value for well setting is incorrect.");
            Assert.AreEqual(expectedMax, wellSetting.Setting.MaxValue.Value, 0.001, "Unconverted maximum validation value for well setting is incorrect.");
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void TestSetGetWellAnalysisOptionUserSettings()
        {
            AddWell();
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            //Validating Show/Hide feature while checking all the checkboxes and save it.
            SettingService.SetUserWellAnalysisOptionsSettings(new WellAnalysisOptionsUserSettingsDTO { Options = "1:1:1:1:1:1:1:1:1:1:1" });
            WellAnalysisOptionsUserSettingsDTO GetType = SettingService.GetUserWellAnalysisOptionsSettings();
            string strErrorMsg = GetType.errorMessage;
            string strOptions = GetType.Options;
            Trace.WriteLine("Error message = " + strErrorMsg);
            Trace.WriteLine("Options string = " + strOptions);
            Assert.AreEqual("Success", strErrorMsg, "Failed");
            Assert.AreEqual("1:1:1:1:1:1:1:1:1:1:1", strOptions, "Options String is mismatch");

            //Validating Show/Hide feature while unchecking all the checkboxes and save it.
            SettingService.SetUserWellAnalysisOptionsSettings(new WellAnalysisOptionsUserSettingsDTO { Options = "0:0:0:0:0:0:0:0:0:0:0" });
            WellAnalysisOptionsUserSettingsDTO GetType_1 = SettingService.GetUserWellAnalysisOptionsSettings();
            string strErrorMsg_1 = GetType_1.errorMessage;
            string strOptions_1 = GetType_1.Options;
            Trace.WriteLine("Error message = " + strErrorMsg_1);
            Trace.WriteLine("Options string = " + strOptions_1);
            Assert.AreEqual("Success", strErrorMsg_1, "Failed");
            Assert.AreEqual("0:0:0:0:0:0:0:0:0:0:0", strOptions_1, "Options String is mismatch");
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void GetSaveAndRemoveGridLayoutTest()
        {
            // Clear out any residual layouts for test layout "1234" from prior test failures
            GridLayoutDTO[] gridLayouts = SettingService.GetGridLayouts("1234");
            foreach (GridLayoutDTO gridLayout in gridLayouts)
            {
                SettingService.RemoveLayouts(new[] { gridLayout.Id.ToString() });
            }

            // GetGridLayouts: First call, confirm layouts cleared for test layout "1234"
            gridLayouts = SettingService.GetGridLayouts("1234");
            Assert.AreEqual(gridLayouts.Count(), 0, "Unexpected non-empty result from GetGridLayouts");

            // First Test Layout: Create initial layout object
            GridLayoutDTO gridLayoutDtoA = new GridLayoutDTO();
            gridLayoutDtoA.GridId = "1234";
            gridLayoutDtoA.Name = "TestLayoutA";

            // First Test Layout: Setup first Filter Info
            GridFilterInfoDTO gridFilterInfoDtoA1 = new GridFilterInfoDTO();
            gridFilterInfoDtoA1.ColumnName = "TestColumnA1";
            gridFilterInfoDtoA1.DataType = "string";
            gridFilterInfoDtoA1.Value = "TestValue";
            gridFilterInfoDtoA1.Operator = "=";

            // First Test Layout: Setup second Filter Info
            GridFilterInfoDTO gridFilterInfoDtoA2 = new GridFilterInfoDTO();
            gridFilterInfoDtoA2.ColumnName = "TestColumnA2";
            gridFilterInfoDtoA2.DataType = "string";
            gridFilterInfoDtoA2.Value = "TestValue";
            gridFilterInfoDtoA2.Operator = "=";

            // First Test Layout: Add to Filters array
            gridLayoutDtoA.Filters = new GridFilterInfoDTO[2];
            gridLayoutDtoA.Filters[0] = gridFilterInfoDtoA1;
            gridLayoutDtoA.Filters[1] = gridFilterInfoDtoA2;

            // First Test Layout: Setup first Column Info
            GridColumnInfoDTO gridColumnInfoDtoA1 = new GridColumnInfoDTO();
            gridColumnInfoDtoA1.Id = 1;
            gridColumnInfoDtoA1.Width = 250;

            // First Test Layout: Setup second Column Info
            GridColumnInfoDTO gridColumnInfoDtoA2 = new GridColumnInfoDTO();
            gridColumnInfoDtoA2.Id = 2;
            gridColumnInfoDtoA2.Width = 500;

            // First Test Layout: Add to Columns array
            gridLayoutDtoA.Columns = new GridColumnInfoDTO[2];
            gridLayoutDtoA.Columns[0] = gridColumnInfoDtoA1;
            gridLayoutDtoA.Columns[1] = gridColumnInfoDtoA2;

            // First Test Layout: Save to DB
            SettingService.SaveGridLayout(gridLayoutDtoA);

            // GetGridLayouts: Second call, confirm first layout saved as expected
            gridLayouts = SettingService.GetGridLayouts("1234");
            Assert.AreEqual(gridLayouts.Count(), 1, "Expected 1 grid layout from GetGridLayouts");
            Assert.AreEqual(gridLayouts[0].GridId, "1234", "Unexpected value for GridId");
            Assert.AreEqual(gridLayouts[0].Name, "TestLayoutA", "Unexpected value for Name");
            Assert.AreEqual(gridLayouts[0].Filters[0].ColumnName, "TestColumnA1", "Unexpected value for Filters[0].ColumnName");
            Assert.AreEqual(gridLayouts[0].Filters[0].DataType, "string", "Unexpected value for Filters[0].DataType");
            Assert.AreEqual(gridLayouts[0].Filters[0].Value, "TestValue", "Unexpected value for Filters[0].Value");
            Assert.AreEqual(gridLayouts[0].Filters[0].Operator, "=", "Unexpected value for Filters[0].Operator");
            Assert.AreEqual(gridLayouts[0].Filters[1].ColumnName, "TestColumnA2", "Unexpected value for Filters[1].ColumnName");
            Assert.AreEqual(gridLayouts[0].Filters[1].DataType, "string", "Unexpected value for Filters[1].DataType");
            Assert.AreEqual(gridLayouts[0].Filters[1].Value, "TestValue", "Unexpected value for Filters[1].Value");
            Assert.AreEqual(gridLayouts[0].Filters[1].Operator, "=", "Unexpected value for Filters[1].Operator");
            Assert.AreEqual(gridLayouts[0].Columns[0].Id, 1, "Unexpected value for Columns[1].Id");
            Assert.AreEqual(gridLayouts[0].Columns[0].Width, 250, "Unexpected value for Columns[1].Id");
            Assert.AreEqual(gridLayouts[0].Columns[1].Id, 2, "Unexpected value for Columns[1].Id");
            Assert.AreEqual(gridLayouts[0].Columns[1].Width, 500, "Unexpected value for Columns[1].Id");

            // First Test Layout: Capture assigned ID and UserId from initial save operation so that next save operation
            // can find the proper layout and update the changed information, accordingly
            gridLayoutDtoA.Id = gridLayouts[0].Id;
            gridLayoutDtoA.UserId = gridLayouts[0].UserId;

            // First Test Layout: Update second Filter info
            gridFilterInfoDtoA2.DataType = "bool";
            gridFilterInfoDtoA2.Value = "false";
            gridFilterInfoDtoA2.Operator = "<>";
            gridLayoutDtoA.Filters[1] = gridFilterInfoDtoA2;

            // First Test Layout: Save the updated layout information
            SettingService.SaveGridLayout(gridLayoutDtoA);

            // GetGridLayouts: Third call, confirm first layout updated
            gridLayouts = SettingService.GetGridLayouts("1234");
            Assert.AreEqual(gridLayouts.Count(), 1, "Expected 1 grid layout from GetGridLayouts");
            Assert.AreEqual(gridLayouts[0].Filters[1].DataType, "bool", "Unexpected value for Filters[1].DataType");
            Assert.AreEqual(gridLayouts[0].Filters[1].Value, "false", "Unexpected value for Filters[1].Value");
            Assert.AreEqual(gridLayouts[0].Filters[1].Operator, "<>", "Unexpected value for Filters[1].Operator");

            // Second Test Layout: Create layout object with same Grid ID
            GridLayoutDTO gridLayoutDtoB = new GridLayoutDTO();
            gridLayoutDtoB.GridId = "1234";
            gridLayoutDtoB.Name = "TestLayoutB";

            // Second Test Layout: Setup Filter Info
            GridFilterInfoDTO gridFilterInfoDtoB = new GridFilterInfoDTO();
            gridFilterInfoDtoB.ColumnName = "TestColumnB";
            gridFilterInfoDtoB.DataType = "integer";
            gridFilterInfoDtoB.Value = "21";
            gridFilterInfoDtoB.Operator = ">=";
            gridLayoutDtoB.Filters = new GridFilterInfoDTO[1];
            gridLayoutDtoB.Filters[0] = gridFilterInfoDtoB;

            // Second Test Layout: Setup Column Info
            GridColumnInfoDTO gridColumnInfoDtoB = new GridColumnInfoDTO();
            gridColumnInfoDtoB.Id = 3;
            gridColumnInfoDtoB.Width = 300;
            gridLayoutDtoB.Columns = new GridColumnInfoDTO[1];
            gridLayoutDtoB.Columns[0] = gridColumnInfoDtoB;

            // Second Test Layout: Save layout information
            SettingService.SaveGridLayout(gridLayoutDtoB);

            // GetGridLayouts: Fourth call, confirm first and second layouts saved
            gridLayouts = SettingService.GetGridLayouts("1234");
            Assert.AreEqual(gridLayouts.Count(), 2, "Expected 2 grid layouts from GetGridLayouts");
            Assert.AreEqual(gridLayouts[1].GridId, "1234", "Unexpected value for GridId");
            Assert.AreEqual(gridLayouts[1].Name, "TestLayoutB", "Unexpected value for Name");
            Assert.AreEqual(gridLayouts[1].Filters[0].ColumnName, "TestColumnB", "Unexpected value for Filters[0].ColumnName");
            Assert.AreEqual(gridLayouts[1].Filters[0].DataType, "integer", "Unexpected value for Filters[0].DataType");
            Assert.AreEqual(gridLayouts[1].Filters[0].Value, "21", "Unexpected value for Filters[0].Value");
            Assert.AreEqual(gridLayouts[1].Filters[0].Operator, ">=", "Unexpected value for Filters[0].Operator");
            Assert.AreEqual(gridLayouts[1].Columns[0].Id, 3, "Unexpected value for Columns[1].Id");
            Assert.AreEqual(gridLayouts[1].Columns[0].Width, 300, "Unexpected value for Columns[1].Id");

            // Call RemoveLayouts for all layouts associated with layout 1234
            foreach (GridLayoutDTO gridLayout in gridLayouts)
            {
                SettingService.RemoveLayouts(new[] { gridLayout.Id.ToString() });
            }

            // Final call to GetGridLayouts, confirm no layouts are saved anymore
            gridLayouts = SettingService.GetGridLayouts("1234");
            Assert.AreEqual(gridLayouts.Count(), 0, "Unexpected non-empty result from GetGridLayouts");
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void GetSaveAndRemoveTrendTemplateTest()
        {
            // Purge all the trend templates to make sure we don't have any trend templates in the db.
            string trendTemplateId = "WellTrend";
            var trendTemplates = SettingService.GetTrendTemplates(trendTemplateId);

            List<string> trendTemplateIdList = new List<string>();
            foreach (var trendTemplate in trendTemplates)
            {
                trendTemplateIdList.Add(trendTemplate.Id.ToString());
                SettingService.RemoveTrendTemplates(trendTemplateIdList.ToArray());
            }

            trendTemplates = SettingService.GetTrendTemplates(trendTemplateId);
            Assert.AreEqual(0, trendTemplates.Count(), "There should not be any trend templates in the db with WellTrend as their trend template id.");

            trendTemplateIdList.Clear();

            // Start creating a trend template
            TrendTemplateDTO layoutDTO = new TrendTemplateDTO();
            layoutDTO.Name = "First Test";
            layoutDTO.WellType = WellTypeId.RRL;
            layoutDTO.TrendTemplateId = trendTemplateId;

            // Add one trend info in the trend template
            List<TrendInfoDTO> listInfo = new List<TrendInfoDTO>();
            TrendInfoDTO trendDTO1 = new TrendInfoDTO();
            trendDTO1.ChartNum = 1;
            trendDTO1.Color = "0x1234";
            trendDTO1.Quantity = "13";
            trendDTO1.TrendType = TrendType.RRLANALYSIS;
            trendDTO1.YAxis = TrendYAxis.LEFT;
            trendDTO1.DashType = "Dashed";
            trendDTO1.PlotType = "Line";

            listInfo.Add(trendDTO1);
            layoutDTO.Trends = listInfo.ToArray();

            // Save it into db
            var trendId = SettingService.SaveTrendTemplate(layoutDTO);

            trendTemplates = SettingService.GetTrendTemplates(trendTemplateId);
            Assert.AreEqual(1, trendTemplates.Count(), "Incorrect trend template count.");

            // Also make sure that we have same number of trends in the template
            TrendInfoDTO[] trendInfoDTO = trendTemplates[0].Trends;
            Assert.AreEqual(1, trendInfoDTO.Count(), "Unexpected TrendInfoDTO instance count.");
            Assert.AreEqual(trendDTO1.ChartNum, trendInfoDTO[0].ChartNum, "Mismatch in chart number.");
            Assert.AreEqual(trendDTO1.Color, trendInfoDTO[0].Color, "Mismatch in trend color.");
            Assert.AreEqual(trendDTO1.Quantity, trendInfoDTO[0].Quantity, "Mismatch in quantity.");
            Assert.AreEqual(trendDTO1.TrendType, trendInfoDTO[0].TrendType, "Mismatch in trend type.");
            Assert.AreEqual(trendDTO1.YAxis, trendInfoDTO[0].YAxis, "Mismatch in YAxis.");
            Assert.AreEqual(trendDTO1.PlotType, trendInfoDTO[0].PlotType, "Mismatch in PlotType.");
            Assert.AreEqual(trendDTO1.DashType, trendInfoDTO[0].DashType, "Mismatch in DashType.");

            // Add another trend info on the same template and update
            TrendInfoDTO trendDTO2 = new TrendInfoDTO();
            trendDTO2.ChartNum = 2;
            trendDTO2.Color = "0x2345";
            trendDTO2.Quantity = "20";
            trendDTO2.TrendType = TrendType.SURVEILLANCE;
            trendDTO2.YAxis = TrendYAxis.RIGHT;
            trendDTO2.DashType = "Dotted";
            trendDTO2.PlotType = "Scatter";

            listInfo.Add(trendDTO2);
            layoutDTO.Trends = listInfo.ToArray();
            layoutDTO.Id = trendId;
            layoutDTO.UserId = trendTemplates[0].UserId;
            trendId = SettingService.SaveTrendTemplate(layoutDTO);

            trendTemplates = SettingService.GetTrendTemplates(trendTemplateId);
            Assert.AreEqual(1, trendTemplates.Count(), "Unexpected trend template count.");

            // there should be two trend info now
            trendInfoDTO = trendTemplates[0].Trends;
            Assert.AreEqual(2, trendInfoDTO.Count(), "Unexpected TrendInfoDTO instance count.");
            Assert.AreEqual(trendDTO1.ChartNum, trendInfoDTO[0].ChartNum, "Mismatch in chart number.");
            Assert.AreEqual(trendDTO1.Color, trendInfoDTO[0].Color, "Mismatch in trend color.");
            Assert.AreEqual(trendDTO1.Quantity, trendInfoDTO[0].Quantity, "Mismatch in quantity.");
            Assert.AreEqual(trendDTO1.TrendType, trendInfoDTO[0].TrendType, "Mismatch in trend type.");
            Assert.AreEqual(trendDTO1.YAxis, trendInfoDTO[0].YAxis, "Mismatch in YAxis.");

            Assert.AreEqual(trendDTO2.ChartNum, trendInfoDTO[1].ChartNum, "Mismatch in chart number.");
            Assert.AreEqual(trendDTO2.Color, trendInfoDTO[1].Color, "Mismatch in trend color.");
            Assert.AreEqual(trendDTO2.Quantity, trendInfoDTO[1].Quantity, "Mismatch in quantity.");
            Assert.AreEqual(trendDTO2.TrendType, trendInfoDTO[1].TrendType, "Mismatch in trend type.");
            Assert.AreEqual(trendDTO2.YAxis, trendInfoDTO[1].YAxis, "Mismatch in YAxis.");
            Assert.AreEqual(trendDTO2.DashType, trendInfoDTO[1].DashType, "Mismatch in DashType.");
            Assert.AreEqual(trendDTO2.PlotType, trendInfoDTO[1].PlotType, "Mismatch in PlotType.");

            // Remove the trend templates
            trendTemplateIdList.Clear();
            foreach (var trendTemplate in trendTemplates)
            {
                trendTemplateIdList.Add(trendTemplate.Id.ToString());
                SettingService.RemoveTrendTemplates(trendTemplateIdList.ToArray());
            }

            trendTemplates = SettingService.GetTrendTemplates(trendTemplateId);
            Assert.AreEqual(0, trendTemplates.Count(), "Incorrect trend template count.");
        }

        private WellSettingDTO[] WellSettingsbyWellId(string wellId)
        {
            WellSettingDTO[] wellSettings = SettingService.GetWellSettingsByWellId(wellId);
            return wellSettings;
        }

        private WellSettingDTO[] WellSettingsbyWellIdAndCategory(string wellId, string settingCategoryId)
        {
            WellSettingDTO[] wellSettings = SettingService.GetWellSettingsByWellIdAndCategory(wellId, settingCategoryId);
            return wellSettings;
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void GetWellSettingsbyWellIdTest()
        {
            AddWell();
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellSettingDTO[] wellSettings;
            //Valid WellId
            try
            {
                wellSettings = WellSettingsbyWellId(well.Id.ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to get wellSettings by Valid Well Id" + ex.Message);
            }
            //InValid WellId
            try
            {
                wellSettings = WellSettingsbyWellId("abc");
                Assert.Fail("Failed to throw POP User Exception");
            }
            catch (WebException ex)
            {
                CheckPOPUserException(ex, "Invalid well id 'abc'");
            }
            //Locale
            try
            {
                ChangeLocale("es");
                wellSettings = WellSettingsbyWellId("abc");
                Assert.Fail("Failed to throw POP User Exception");
            }
            catch (WebException ex)
            {
                CheckPOPUserException(ex, "ID de pozo no válido 'abc'");
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void GetAlternateWellNameSettingTest()
        {
            SettingDTO settingDto = null;
            try
            {
                settingDto = SettingService.GetSettingByName(SettingServiceStringConstants.ALTERNATE_WELL_ID);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to retrieve setting " + SettingServiceStringConstants.ALTERNATE_WELL_ID + ": " + ex.Message);
            }

            SystemSettingDTO savedSettingDTO = new SystemSettingDTO
            {
                ChangeDate = DateTime.Now.ToUniversalTime(),
                ChangeUser = AuthorizationStringConstants.SystemUserName,
                Setting = new SettingDTO
                {
                    Id = settingDto.Id,
                    ChangeDate = DateTime.Now.ToUniversalTime(),
                    ChangeUser = AuthorizationStringConstants.SystemUserName,
                    Description = SettingServiceStringConstants.ALTERNATE_WELL_ID,
                    DropdownOptions = new string[]
                    {
                        "WellName",
                        "API10",
                        "API12",
                        "API14"
                    },
                    EnumTypeName = "NavigatorWellName",
                    Key = SettingServiceStringConstants.ALTERNATE_WELL_ID,
                    Name = SettingServiceStringConstants.ALTERNATE_WELL_ID,
                    SettingCategory = SettingCategory.None,
                    SettingType = SettingType.System | SettingType.Well,
                    SettingValueType = SettingValueType.Dropdown,
                    UnitCategory = UnitCategory.None,

                },
            };

            savedSettingDTO.StringValue = "WellName";
            SystemSettingDTO retrievedSetting = null;

            string failureToProcessAlternateWellID = "Failure processing system setting " + SettingServiceStringConstants.ALTERNATE_WELL_ID + " when set to ";
            try
            {
                SettingService.SaveSystemSetting(savedSettingDTO);
                retrievedSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ALTERNATE_WELL_ID);
                Assert.AreEqual(savedSettingDTO.StringValue, retrievedSetting.StringValue, $"Error: Saved alternate display name: { savedSettingDTO.StringValue }   Retrieved alternate display name: {retrievedSetting.StringValue} ");
            }
            catch (Exception ex)
            {
                Assert.Fail(failureToProcessAlternateWellID + savedSettingDTO.StringValue + ": " + ex.Message);
            }

            savedSettingDTO.StringValue = "API10";

            try
            {
                SettingService.SaveSystemSetting(savedSettingDTO);
                retrievedSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ALTERNATE_WELL_ID);
                Assert.AreEqual(savedSettingDTO.StringValue, retrievedSetting.StringValue, $"Error: Saved alternate display name: { savedSettingDTO.StringValue }   Retrieved alternate display name: {retrievedSetting.StringValue} ");
            }
            catch (Exception ex)
            {
                Assert.Fail(failureToProcessAlternateWellID + savedSettingDTO.StringValue + ": " + ex.Message);
            }

            savedSettingDTO.StringValue = "API12";

            try
            {
                SettingService.SaveSystemSetting(savedSettingDTO);
                retrievedSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ALTERNATE_WELL_ID);
                Assert.AreEqual(savedSettingDTO.StringValue, retrievedSetting.StringValue, $"Error: Saved alternate display name: { savedSettingDTO.StringValue }   Retrieved alternate display name: {retrievedSetting.StringValue} ");
            }
            catch (Exception ex)
            {
                Assert.Fail(failureToProcessAlternateWellID + savedSettingDTO.StringValue + ": " + ex.Message);
            }

            savedSettingDTO.StringValue = "API14";

            try
            {
                SettingService.SaveSystemSetting(savedSettingDTO);
                retrievedSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ALTERNATE_WELL_ID);
                Assert.AreEqual(savedSettingDTO.StringValue, retrievedSetting.StringValue, $"Error: Saved alternate display name: { savedSettingDTO.StringValue }   Retrieved alternate display name: {retrievedSetting.StringValue} ");
            }
            catch (Exception ex)
            {
                Assert.Fail(failureToProcessAlternateWellID + savedSettingDTO.StringValue + ": " + ex.Message);
            }

            SettingService.RemoveSystemSetting(retrievedSetting.Id.ToString());
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void GetWellSettingsbyWellIdAndCategoryTest()
        {
            AddWell();
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellSettingDTO[] wellSettings;
            int settingCategoryId = (int)SettingCategory.OperatingLimit;
            //Valid WellId & Setting Category Id
            try
            {
                wellSettings = WellSettingsbyWellIdAndCategory(well.Id.ToString(), settingCategoryId.ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to get wellSettings by Valid Well Id" + ex.Message);
            }
            //InValid WellId
            try
            {
                wellSettings = WellSettingsbyWellIdAndCategory("abc", settingCategoryId.ToString());
                Assert.Fail("Failed to throw POP User Exception");
            }
            catch (WebException ex)
            {
                CheckPOPUserException(ex, "Invalid well id 'abc'");
            }
            //Invalid Setting Category Id
            try
            {
                wellSettings = WellSettingsbyWellIdAndCategory(well.Id.ToString(), "abc");
                Assert.Fail("Failed to throw POP User Exception");
            }
            catch (WebException ex)
            {
                CheckPOPUserException(ex, "Invalid setting category 'abc'");
            }
            //Invalid WellId & Setting Category Id
            try
            {
                wellSettings = WellSettingsbyWellIdAndCategory("abc", "abc");
                Assert.Fail("Failed to throw POP User Exception");
            }
            catch (WebException ex)
            {
                CheckPOPUserException(ex, "Invalid well id 'abc'");
            }
            //Locale
            ChangeLocale("es");
            try
            {
                wellSettings = WellSettingsbyWellIdAndCategory(well.Id.ToString(), "abc");
                Assert.Fail("Failed to throw POP User Exception");
            }
            catch (WebException ex)
            {
                CheckPOPUserException(ex, "Categoría de configuración inválida 'abc'");
            }
            try
            {
                wellSettings = WellSettingsbyWellIdAndCategory("abc", settingCategoryId.ToString());
                Assert.Fail("Failed to throw POP User Exception");
            }
            catch (WebException ex)
            {
                CheckPOPUserException(ex, "ID de pozo no válido 'abc'");
            }
            try
            {
                wellSettings = WellSettingsbyWellIdAndCategory("abc", "abc");
                Assert.Fail("Failed to throw POP User Exception");
            }
            catch (WebException ex)
            {
                CheckPOPUserException(ex, "ID de pozo no válido 'abc'");
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void POPDBByTableAndColumnTest()
        {

            POPDBRepDTO popDB = SettingService.GetPOPDBByTableAndColumn("Well", "welUserDef01");
            //Verify the response is not null
            Assert.IsNotNull(popDB, "GetPOPDBByTableAndColumn response is Null");

            //Saving the Original response in beforeUpdate 
            POPDBRepDTO beforeUpdate = new POPDBRepDTO();
            beforeUpdate.Attribute = popDB.Attribute;
            beforeUpdate.ColumnName = popDB.ColumnName;
            beforeUpdate.EntityName = popDB.EntityName;
            beforeUpdate.Id = popDB.Id;
            beforeUpdate.Title = popDB.Title;
            beforeUpdate.UserConfig = popDB.UserConfig;

            //Changing the tile, UserConfig & Attribute value
            popDB.Title = "User Defined 100";
            popDB.UserConfig = true;
            popDB.Attribute = true;
            try
            {
                //saving the updated popDB object
                SettingService.UpdatePOPDBRep(popDB);

                //Calling GetPOPDBByTableAndColumn(“Well”,”welUserDef01”) again to pull back what should be the updated information from the database
                POPDBRepDTO afterUpdate = SettingService.GetPOPDBByTableAndColumn("Well", "welUserDef01");

                //Verifying the title,UserConfig & Attribute value sent in Update request and Get Response is matching
                Assert.IsNotNull(afterUpdate, "GetPOPDBByTableAndColumn response is Null");
                Assert.AreEqual(popDB.Title, afterUpdate.Title, "Title retreieved from response is incorrect");
                Assert.AreEqual(popDB.UserConfig, afterUpdate.UserConfig, "UserConfig retreieved from response is incorrect");
                Assert.AreEqual(popDB.Attribute, afterUpdate.Attribute, "Attribute retreieved from response is incorrect");
            }
            finally
            {
                //Reverting changes
                SettingService.UpdatePOPDBRep(beforeUpdate);
            }
        }

        private static void CompareGenericGridViews(GenericGridViewDTO expected, GenericGridViewDTO actual, string message)
        {
            CompareObjectsUsingReflection(expected, actual, message, new HashSet<string>() { nameof(GenericGridViewDTO.Id), nameof(GenericGridViewDTO.ChangeDate), nameof(GenericGridViewDTO.ChangeUser), nameof(GenericGridViewDTO.Data) });
            AssertNullEquivalency(expected.Data, actual.Data, nameof(GenericGridViewDTO.Data), message);

            AssertNullEquivalency(expected.Data?.ColorRules, actual.Data?.ColorRules, nameof(GenericGridViewDataDTO.ColorRules), message);
            Assert.AreEqual(expected.Data?.ColorRules?.Length, actual.Data?.ColorRules?.Length, message + $"{nameof(GenericGridViewDataDTO.ColorRules)} lengths do not match.");
            for (int ii = 0; ii < (expected.Data?.ColorRules?.Length ?? 0); ii++)
            {
                CompareObjectsUsingReflection(expected.Data.ColorRules[ii], actual.Data.ColorRules[ii], $"{nameof(GenericGridViewDataDTO.ColorRules)} element {ii} does not match.");
            }

            AssertNullEquivalency(expected.Data?.ColumnCoreStates, actual.Data?.ColumnCoreStates, nameof(GenericGridViewDataDTO.ColumnCoreStates), message);
            Assert.AreEqual(expected.Data?.ColumnCoreStates?.Length, actual.Data?.ColumnCoreStates?.Length, message + $"{nameof(GenericGridViewDataDTO.ColumnCoreStates)} lengths do not match.");
            for (int ii = 0; ii < (expected.Data?.ColumnCoreStates?.Length ?? 0); ii++)
            {
                CompareObjectsUsingReflection(expected.Data.ColumnCoreStates[ii], actual.Data.ColumnCoreStates[ii], $"{nameof(GenericGridViewDataDTO.ColumnCoreStates)} element {ii} does not match.");
            }

            AssertNullEquivalency(expected.Data?.ColumnSortStates, actual.Data?.ColumnSortStates, nameof(GenericGridViewDataDTO.ColumnSortStates), message);
            Assert.AreEqual(expected.Data?.ColumnSortStates?.Length, actual.Data?.ColumnSortStates?.Length, message + $"{nameof(GenericGridViewDataDTO.ColumnSortStates)} lengths do not match.");
            for (int ii = 0; ii < (expected.Data?.ColumnSortStates?.Length ?? 0); ii++)
            {
                CompareObjectsUsingReflection(expected.Data.ColumnSortStates[ii], actual.Data.ColumnSortStates[ii], $"{nameof(GenericGridViewDataDTO.ColumnSortStates)} element {ii} does not match.");
            }

            AssertNullEquivalency(expected.Data?.FilterElements, actual.Data?.FilterElements, nameof(GenericGridViewDataDTO.FilterElements), message);
            Assert.AreEqual(expected.Data?.FilterElements?.Length, actual.Data?.FilterElements?.Length, message + $"{nameof(GenericGridViewDataDTO.FilterElements)} lengths do not match.");
            for (int ii = 0; ii < (expected.Data?.FilterElements?.Length ?? 0); ii++)
            {
                CompareObjectsUsingReflection(expected.Data.FilterElements[ii], actual.Data.FilterElements[ii], $"{nameof(GenericGridViewDataDTO.FilterElements)} element {ii} does not match.", new HashSet<string>() { nameof(GenericGridColumnFilterStateElementDTO.FilterConditions) });
                AssertNullEquivalency(expected.Data.FilterElements[ii].FilterConditions, actual.Data.FilterElements[ii].FilterConditions, nameof(GenericGridColumnFilterStateElementDTO.FilterConditions), message);
                for (int jj = 0; jj < (expected.Data.FilterElements[ii]?.FilterConditions?.Length ?? 0); jj++)
                {
                    CompareObjectsUsingReflection(expected.Data.FilterElements[ii].FilterConditions[jj], actual.Data.FilterElements[ii].FilterConditions[jj], $"{nameof(GenericGridColumnFilterStateElementDTO.FilterConditions)} element {ii}, {jj} does not match.");
                }
            }

            AssertNullEquivalency(expected?.Data?.GridProperties, actual?.Data?.GridProperties, nameof(GenericGridViewDataDTO.GridProperties), message);
            CompareObjectsUsingReflection(expected?.Data?.GridProperties, actual?.Data?.GridProperties, $"{nameof(GenericGridViewDataDTO.GridProperties)} does not match.", new HashSet<string>());
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void GenericGridViews()
        {
            const GenericGridId gridId = GenericGridId.RRLWellGroupStatus;
            var view = new GenericGridViewDTO();
            view.UserId = AuthenticatedUser.Id;
            view.GridId = gridId;
            view.Name = "My View";
            view.Data.ColorRules = new[]
            {
                new GenericGridColumnColorRuleDTO() { BackgroundColor = "white", TextColor = "black", ColumnId = "My Column", Comparison = GenericGridColumnComparison.Equals, Target = GenericGridColumnColorTarget.Cell, Value = "OMGVALUE" },
                new GenericGridColumnColorRuleDTO() { BackgroundColor = "white", TextColor = "black", ColumnId = "My Column", Comparison = GenericGridColumnComparison.Equals, Target = GenericGridColumnColorTarget.Cell, Value = new DateTime(2000, 1,1,10,30,23, DateTimeKind.Utc) },
            };
            view.Data.ColumnCoreStates = new[] { new GenericGridColumnCoreStateDTO() { AggregateFunction = null, ColumnId = "My Column", IsHidden = null, PinnedState = GenericGridColumnPinnedState.None, PivotIndex = null, RowGroupIndex = null, Width = 100 } };
            view.Data.ColumnSortStates = new[] { new GenericGridColumnSortStateDTO() { ColumnId = "My Column", SortDirection = GenericGridColumnSortDirection.None } };
            view.Data.FilterElements = new[] { new GenericGridColumnFilterStateElementDTO() { ColumnId = "My Column", FilterConditions = new GenericGridColumnFilterStateConditionBaseDTO[] { new GenericGridColumnFilterStateConditionTextDTO() { Comparison = GenericGridColumnComparison.GreaterThanOrEqual, Filter = "Wheee" } } } };
            view.Data.GridProperties = new GenericGridViewPropertiesDTO();
            view.Data.GridProperties.AreRowNumbersVisible = true;
            view.Data.GridProperties.IsPivotEnabled = false;
            view.Data.GridProperties.IsUOMVisible = true;
            GenericGridViewDTO gridViewFromSave = SettingService.SaveGenericGridView(view);
            Assert.AreNotEqual(0, gridViewFromSave.Id, "Saved view id should not be zero.");

            GenericGridViewDTO[] gridViews = SettingService.GetGenericGridViews(gridId.ToString());
            Assert.IsNotNull(gridViews, "GetGenericGridViews should not return a null array.");
            var viewsToRemove = new List<GenericGridViewDTO>();
            if (gridViews.Length > 0)
            {
                viewsToRemove.Add(gridViews[0]);
            }
            try
            {
                CompareGenericGridViews(view, gridViewFromSave, "Grid view returned from save should match source object.");
                Assert.AreEqual(1, gridViews.Length, "There should only be one grid view present.");
                CompareGenericGridViews(view, gridViews[0], "Retrieved grid view should match source object.");

                view = gridViewFromSave;
                view.Name = "My view 2";
                gridViewFromSave = SettingService.SaveGenericGridView(view);
                gridViews = SettingService.GetGenericGridViews(gridId.ToString());
                Assert.IsNotNull(gridViews, "GetGenericGridViews should not return a null array (2).");
                CompareGenericGridViews(view, gridViewFromSave, "Grid view returned from save should match source object (2).");
                Assert.AreEqual(1, gridViews.Length, "There should only be one grid view present (2).");
                CompareGenericGridViews(view, gridViews[0], "Retrieved grid view should match source object (2).");

                SettingService.RemoveGenericGridViews(gridViews.Select(t => t.Id).ToArray());
                viewsToRemove.Clear();
                gridViews = SettingService.GetGenericGridViews(gridId.ToString());
                Assert.IsNotNull(gridViews, "GetGenericGridViews should not return a null array.");
                Assert.AreEqual(0, gridViews.Length, "There should be no grid views present.");
            }
            finally
            {
                if (viewsToRemove.Any())
                {
                    try
                    {
                        SettingService.RemoveGenericGridViews(viewsToRemove.Select(t => t.Id).ToArray());
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to remove grid views: {ex.Message}{Environment.NewLine}{ex.ToString()}");
                    }
                }
            }
        }

        [TestCategory(TestCategories.SettingServiceTests), TestMethod]
        public void AddUpdateRemoveRelativeFacilitySettings()
        {
            string relativeFacilityMappingStr = "Gas Meter";

            #region Relative Facility mapping for Well Quantity
            Dictionary<WellQuantity, string> wellQuantities = new Dictionary<WellQuantity, string>();
            wellQuantities[WellQuantity.GasRateMeasured] = relativeFacilityMappingStr;
            AddUpdateRelativeFacilitySystemSettingForWellQuantities("GLMapping", WellTypeId.GLift, wellQuantities);

            wellQuantities[WellQuantity.GasRateMeasured] = "";
            AddUpdateRelativeFacilitySystemSettingForWellQuantities("GLMapping", WellTypeId.GLift, wellQuantities);
            #endregion

            #region Relative Facility mapping for Additional UDCs
            //Add additional UDCs
            SystemSettingDTO systemSettingAdditionalUDC = new SystemSettingDTO() { Setting = SettingService.GetSettingByName(SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS) };
            ConfigurableUDCInfo infos = new ConfigurableUDCInfo();
            infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC("Tubing Head Pressure", "PRTUBXIN", null, UnitCategory.None, CygNetPointType.Analog, false));
            infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC("Casing Head Pressure", "PRCASXIN", null, UnitCategory.None, CygNetPointType.Analog, false));
            systemSettingAdditionalUDC.StringValue = infos.GetSettingValue();
            SettingService.SaveSystemSetting(systemSettingAdditionalUDC);

            AddUpdateRelativeFacilitySystemSettingForAdditionalUDCs("GLMapping", WellTypeId.GLift);
            #endregion

            #region Relative Facility mapping for Well Commands
            Dictionary<WellCommand, string> wellCommands = new Dictionary<WellCommand, string>();
            wellCommands[WellCommand.StartRPC] = relativeFacilityMappingStr;
            AddUpdateRelativeFacilitySystemSettingForWellCommands("RRLMapping", WellTypeId.RRL, wellCommands);

            wellCommands[WellCommand.StartRPC] = "";
            AddUpdateRelativeFacilitySystemSettingForWellCommands("RRLMapping", WellTypeId.RRL, wellCommands);
            #endregion

            RemoveRelativeFacilitySystemSetting();
        }

        #endregion Test Methods

        private List<TestSettingValues> getSettingValues()
        {
            List<TestSettingValues> settingValues = new List<TestSettingValues>();

            settingValues.Add(new TestSettingValues("addValues", new Dictionary<SettingValueType, Tuple<string, double?, double[]>>()
            {
                { SettingValueType.DecimalNumber , Tuple.Create<string, double?, double[]>(null, 3.7, null) },
                { SettingValueType.Dropdown , Tuple.Create<string, double?, double[]>(WellTypeId.ESP.ToString(), null, null) },
                { SettingValueType.Number , Tuple.Create<string, double?, double[]>(null, 60, null) },
                { SettingValueType.Text , Tuple.Create<string, double?, double[]>("ThisIsATest", null, null) },
                { SettingValueType.TrueOrFalse , Tuple.Create<string, double?, double[]>(null, 1, null) },
                { SettingValueType.DecimalNumberArray , Tuple.Create<string, double?, double[]>(null, null, new double[] { 5.5, 38.92 }) },
            }));

            settingValues.Add(new TestSettingValues("updateValues", new Dictionary<SettingValueType, Tuple<string, double?, double[]>>()
            {
                { SettingValueType.DecimalNumber , Tuple.Create<string, double?, double[]>(null, 5.5, null) },
                { SettingValueType.Dropdown , Tuple.Create<string, double?, double[]>(WellTypeId.GInj.ToString(), null, null) },
                { SettingValueType.Number , Tuple.Create<string, double?, double[]>(null, 30, null) },
                { SettingValueType.Text , Tuple.Create<string, double?, double[]>("ThisIsATestPart2", null, null) },
                { SettingValueType.TrueOrFalse , Tuple.Create<string, double?, double[]>(null, 0, null) },
                { SettingValueType.DecimalNumberArray , Tuple.Create<string, double?, double[]>(null, null, new double[] { 23.556, 182.8822, 283.2813, 892.2283 }) },
            }));

            settingValues.Add(new TestSettingValues("badValues1", new Dictionary<SettingValueType, Tuple<string, double?, double[]>>()
            {
                { SettingValueType.DecimalNumber , Tuple.Create<string, double?, double[]>("5.5", 5.5, null) },
                { SettingValueType.Dropdown , Tuple.Create<string, double?, double[]>(WellTypeId.ESP.ToString(), 77.51, null) },
                { SettingValueType.Number , Tuple.Create<string, double?, double[]>("30", 30, null) },
                { SettingValueType.Text , Tuple.Create<string, double?, double[]>("ThisIsATestPart2", 47.5, null) },
                { SettingValueType.TrueOrFalse , Tuple.Create<string, double?, double[]>("True", 0, null) },
                { SettingValueType.DecimalNumberArray , Tuple.Create<string, double?, double[]>("George", null, new double[] { 23.556, 182.8822, 283.2813, 892.2283 }) },
            }));

            settingValues.Add(new TestSettingValues("badValues2", new Dictionary<SettingValueType, Tuple<string, double?, double[]>>()
            {
                { SettingValueType.DecimalNumber , Tuple.Create<string, double?, double[]>(null, 5.5, new double[] { 6.6, 82.13 }) },
                { SettingValueType.Dropdown , Tuple.Create<string, double?, double[]>(WellTypeId.ESP.ToString(), null, new double[] { 6.6, 82.34, 833.178 }) },
                { SettingValueType.Number , Tuple.Create<string, double?, double[]>(null, 30, new double[] { 5.83, 28.39, 38.48, 483.193 }) },
                { SettingValueType.Text , Tuple.Create<string, double?, double[]>("ThisIsATestPart2", null, new double[] { 4.4, 49.821, 0.8734, 18.298 }) },
                { SettingValueType.TrueOrFalse , Tuple.Create<string, double?, double[]>(null, 0, new double[] { 389.489, 98302, 8934032.3 } ) },
                { SettingValueType.DecimalNumberArray , Tuple.Create<string, double?, double[]>(null, 58.88418, new double[] { 23.556, 182.8822, 283.2813, 892.2283 }) },
            }));

            settingValues.Add(new TestSettingValues("badValues3", new Dictionary<SettingValueType, Tuple<string, double?, double[]>>()
            {
                { SettingValueType.Dropdown , Tuple.Create<string, double?, double[]>("Whee", null, null) },
                { SettingValueType.TrueOrFalse , Tuple.Create<string, double?, double[]>(null, 1.5, null) },
            }));

            return settingValues;
        }
    }
}