using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Script.Serialization;
using MasterCatalogService;
using MasterCatalogService.Criteria;
using MasterCatalogService.DTOs;
using MasterCatObjects.BaseDefines;
using MasterCatObjects.CatalogObjects;
using MasterCatObjects.CriteriaExpression;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weatherford.POP.CatalogServiceTests
{
    [TestClass]
    public class CatalogRRLObjects
    {
        #region Objects
        public static CatalogObjectType[] objTypes = new CatalogObjectType[] {
                CatalogObjectType.RRL_APISRP_BARREL_TYPE,
                CatalogObjectType.RRL_APISRP_EXT_BARREL_ACC,
                CatalogObjectType.RRL_APISRP_EXT_BARREL_TYPE,
                CatalogObjectType.RRL_APISRP_EXT_PLUNGER_ACC,
                CatalogObjectType.RRL_APISRP_EXT_PLUNGER_PIN,
                CatalogObjectType.RRL_APISRP_EXT_PLUNGER_TYPE,
                CatalogObjectType.RRL_APISRP_EXT_PUMP_TYPE,
                CatalogObjectType.RRL_APISRP_EXT_SAND,
                CatalogObjectType.RRL_APISRP_EXT_SEAT_LOCATION,
                CatalogObjectType.RRL_APISRP_EXT_SEAT_TYPE,
                CatalogObjectType.RRL_APISRP_EXT_STANDING_VALVE,
                CatalogObjectType.RRL_APISRP_EXT_STANDING_VALVE_CAGE,
                CatalogObjectType.RRL_APISRP_EXT_TRAVELLING_VALVE,
                CatalogObjectType.RRL_APISRP_EXT_TRAVELLING_VALVE_CAGE,
                CatalogObjectType.RRL_APISRP_EXT_TRAVELLING_VALVE_PLUNGER,
                CatalogObjectType.RRL_APISRP_EXT_VROD,
                CatalogObjectType.RRL_APISRP_EXT_WIPER,
                CatalogObjectType.RRL_APISRP_MEASURED_TYPE,
                CatalogObjectType.RRL_APISRP_PUMP_BORE,
                CatalogObjectType.RRL_APISRP_PUMP_TYPE,
                CatalogObjectType.RRL_APISRP_SEAT_LOCATION,
                CatalogObjectType.RRL_APISRP_SEAT_TYPE,
                CatalogObjectType.RRL_APISRP_TUBING_SIZE,
                CatalogObjectType.RRL_CATALOG_ITEM,
                CatalogObjectType.RRL_COMPONENT_CONDITION,
                CatalogObjectType.RRL_COMPONENT_GROUPING,
                CatalogObjectType.RRL_COMPONENT_PROPERTY,
                CatalogObjectType.RRL_COMPONENT_USAGE,
                CatalogObjectType.RRL_CRANK,
                CatalogObjectType.RRL_CRANK_WEIGHT,
                CatalogObjectType.RRL_ELECTRIC_MOTOR,
                CatalogObjectType.RRL_GRADE,
                CatalogObjectType.RRL_GRADE_MANUFACTURER,
                CatalogObjectType.RRL_MANUFACTURER,
                CatalogObjectType.RRL_MATERIAL,
                CatalogObjectType.RRL_MFG_UNIT_TYPE,
                CatalogObjectType.RRL_MOTOR_SIZE,
                CatalogObjectType.RRL_MOTOR_SLIP_TORQUE,
                CatalogObjectType.RRL_MOTOR_TYPE,
                CatalogObjectType.RRL_PART_TYPE,
                CatalogObjectType.RRL_PERFORATION,
                CatalogObjectType.RRL_POLISHED_ROD,
                CatalogObjectType.RRL_PRIME_MOVER_MODE,
                CatalogObjectType.RRL_PRIME_MOVER_TYPE,
                CatalogObjectType.RRL_PUMPING_UNIT,
                CatalogObjectType.RRL_PUMPING_UNIT_CRANK,
                CatalogObjectType.RRL_ROD,
                CatalogObjectType.RRL_ROD_MANUFACTURER,
                CatalogObjectType.RRL_ROD_PUMP,
                CatalogObjectType.RRL_TUBING_HANGER,
                CatalogObjectType.RRL_TUBING_PUMP,
                CatalogObjectType.RRL_TUBULAR,
                CatalogObjectType.RRL_TUBULAR_CASING,
                CatalogObjectType.RRL_TUBULAR_TUBING,
                CatalogObjectType.RRL_TUBULAR_TUBING_SUB,
                CatalogObjectType.RRL_UNIT_TYPE_MFG,
                CatalogObjectType.RRL_WEIGHT,
                CatalogObjectType.RRL_WELLBORE_TYPE,
                CatalogObjectType.RRL_TUBULAR_CONNECTION_SPECS
            };
        #endregion

        [TestMethod]
        public void FaultHandlingTest()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost/CatalogService/restService/");
                Uri singleUri = new Uri(client.BaseAddress, "GetSingle");
                Uri manyUri = new Uri(client.BaseAddress, "GetMany");
                Uri validUri = new Uri(client.BaseAddress, "ValidateDBConnection");

                var result = new List<RRLManufacturerDTO>();
                HttpResponseMessage response;

                ConditionExpressionDTO condition = new ConditionExpressionDTO()
                {
                    AttributeName = "mfgManufacturerName",
                    Operation = OperatorType.Equal,
                    Value = new ConditionDataDTO() { StringValue = string.Empty },
                    EnumAttributeName = MasterCatObjects.CatalogObjects.CatalogObjectAttr.RRLManufacturermfgManufacturerName,
                };

                SortExpressionDTO sort = new SortExpressionDTO()
                {
                    AttributeName = "mfgManufacturerName",
                    SortType = SortType.Ascending
                };
                QueryCriteriaDTO criteria = new QueryCriteriaDTO()
                {
                    ObjectType = CatalogObjectType.RRL_MFG_UNIT_TYPE,
                };

                criteria.SQLConditions = new ConditionExpressionDTO[] { condition };
                criteria.Sorts = new SortExpressionDTO[] { sort };

                var para = new JavaScriptSerializer().Serialize(criteria);
                var content = new StringContent(para, System.Text.Encoding.UTF8, "application/json");

                response = client.PostAsync(validUri, null).Result;
                response = client.PostAsync(singleUri, content).Result;
                content = new StringContent(para, System.Text.Encoding.UTF8, "application/json");
                response = client.PostAsync(manyUri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.ToString());
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    string str = response.RequestMessage.ToString();
                    Console.WriteLine(str);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private string GetResponseMessage(HttpResponseMessage response)
        {
            if (response == null)
            {
                return null;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.Accepted:
                    return "Equivalent to HTTP status 202. Accepted indicates that the request has been accepted for further processing.";
                case HttpStatusCode.Ambiguous:
                    return "Equivalent to HTTP status 300. Ambiguous indicates that the requested information has multiple representations. The default action is to treat this status as a redirect and follow the contents of the Location header associated with this response.";
                case HttpStatusCode.BadGateway:
                    return "Equivalent to HTTP status 502. BadGateway indicates that an intermediate proxy server received a bad response from another proxy or the origin server.";
                case HttpStatusCode.BadRequest:
                    return "Equivalent to HTTP status 400. BadRequest indicates that the request could not be understood by the server.BadRequest is sent when no other error is applicable, or if the exact error is unknown or does not have its own error code.";
                case HttpStatusCode.Conflict:
                    return "Equivalent to HTTP status 409. Conflict indicates that the request could not be carried out because of a conflict on the server.";
                case HttpStatusCode.Continue:
                    return "Equivalent to HTTP status 100. Continue indicates that the client can continue with its request.";
                case HttpStatusCode.Created:
                    return "Equivalent to HTTP status 201. Created indicates that the request resulted in a new resource created before the response was sent.";
                case HttpStatusCode.ExpectationFailed:
                    return "Equivalent to HTTP status 417. ExpectationFailed indicates that an expectation given in an Expect header could not be met by the server.";
                case HttpStatusCode.Forbidden:
                    return "Equivalent to HTTP status 403. Forbidden indicates that the server refuses to fulfill the request.";
                case HttpStatusCode.Found:
                    return "Equivalent to HTTP status 302. Found indicates that the requested information is located at the URI specified in the Location header.The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method.";
                case HttpStatusCode.GatewayTimeout:
                    return "Equivalent to HTTP status 504. GatewayTimeout indicates that an intermediate proxy server timed out while waiting for a response from another proxy or the origin server.";
                case HttpStatusCode.Gone:
                    return "Equivalent to HTTP status 410. Gone indicates that the requested resource is no longer available.";
                case HttpStatusCode.HttpVersionNotSupported:
                    return "Equivalent to HTTP status 505. HttpVersionNotSupported indicates that the requested HTTP version is not supported by the server.";
                case HttpStatusCode.InternalServerError:
                    return "Equivalent to HTTP status 500. InternalServerError indicates that a generic error has occurred on the server.";
                case HttpStatusCode.LengthRequired:
                    return "Equivalent to HTTP status 411. LengthRequired indicates that the required Content - length header is missing.";
                case HttpStatusCode.MethodNotAllowed:
                    return "Equivalent to HTTP status 405. MethodNotAllowed indicates that the request method(POST or GET) is not allowed on the requested resource.";
                case HttpStatusCode.Moved:
                    return "Equivalent to HTTP status 301. Moved indicates that the requested information has been moved to the URI specified in the Location header.The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method.";
                case HttpStatusCode.NoContent:
                    return "Equivalent to HTTP status 204. NoContent indicates that the request has been successfully processed and that the response is intentionally blank.";
                case HttpStatusCode.NonAuthoritativeInformation:
                    return "Equivalent to HTTP status 203. NonAuthoritativeInformation indicates that the returned metainformation is from a cached copy instead of the origin server and therefore may be incorrect.";
                case HttpStatusCode.NotAcceptable:
                    return "Equivalent to HTTP status 406. NotAcceptable indicates that the client has indicated with Accept headers that it will not accept any of the available representations of the resource.";
                case HttpStatusCode.NotFound:
                    return "Equivalent to HTTP status 404. NotFound indicates that the requested resource does not exist on the server.";
                case HttpStatusCode.NotImplemented:
                    return "Equivalent to HTTP status 501. NotImplemented indicates that the server does not support the requested function.";
                case HttpStatusCode.NotModified:
                    return "Equivalent to HTTP status 304. NotModified indicates that the client's cached copy is up to date. The contents of the resource are not transferred.";
                case HttpStatusCode.OK:
                    return "Equivalent to HTTP status 200. OK indicates that the request succeeded and that the requested information is in the response. This is the most common status code to receive.";
                case HttpStatusCode.PartialContent:
                    return "Equivalent to HTTP status 206. PartialContent indicates that the response is a partial response as requested by a GET request that includes a byte range.";
                case HttpStatusCode.PaymentRequired:
                    return "Equivalent to HTTP status 402. PaymentRequired is reserved for future use.";
                case HttpStatusCode.PreconditionFailed:
                    return "Equivalent to HTTP status 412. PreconditionFailed indicates that a condition set for this request failed, and the request cannot be carried out. Conditions are set with conditional request headers like If - Match, If - None - Match, or If - Unmodified - Since.";
                case HttpStatusCode.ProxyAuthenticationRequired:
                    return "Equivalent to HTTP status 407. ProxyAuthenticationRequired indicates that the requested proxy requires authentication.The Proxy-authenticate header contains the details of how to perform the authentication.";
                case HttpStatusCode.RedirectKeepVerb:
                    return "Equivalent to HTTP status 307. RedirectKeepVerb indicates that the request information is located at the URI specified in the Location header.The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will also use the POST method.";
                case HttpStatusCode.RedirectMethod:
                    return "Equivalent to HTTP status 303. RedirectMethodautomatically redirects the client to the URI specified in the Location header as the result of a POST. The request to the resource specified by the Location header will be made with a GET.";
                case HttpStatusCode.RequestedRangeNotSatisfiable:
                    return "Equivalent to HTTP status 416. RequestedRangeNotSatisfiable indicates that the range of data requested from the resource cannot be returned, either because the beginning of the range is before the beginning of the resource, or the end of the range is after the end of the resource.";
                case HttpStatusCode.RequestEntityTooLarge:
                    return "Equivalent to HTTP status 413. RequestEntityTooLarge indicates that the request is too large for the server to process.";
                case HttpStatusCode.RequestTimeout:
                    return "Equivalent to HTTP status 408. RequestTimeout indicates that the client did not send a request within the time the server was expecting the request.";
                case HttpStatusCode.RequestUriTooLong:
                    return "Equivalent to HTTP status 414. RequestUriTooLong indicates that the URI is too long.";
                case HttpStatusCode.ResetContent:
                    return "Equivalent to HTTP status 205. ResetContent indicates that the client should reset (not reload) the current resource.";
                case HttpStatusCode.ServiceUnavailable:
                    return "Equivalent to HTTP status 503. ServiceUnavailable indicates that the server is temporarily unavailable, usually due to high load or maintenance.";
                case HttpStatusCode.SwitchingProtocols:
                    return "Equivalent to HTTP status 101. SwitchingProtocols indicates that the protocol version or protocol is being changed.";
                case HttpStatusCode.Unauthorized:
                    return "Equivalent to HTTP status 401. Unauthorized indicates that the requested resource requires authentication.The WWW-Authenticate header contains the details of how to perform the authentication.";
                case HttpStatusCode.UnsupportedMediaType:
                    return "Equivalent to HTTP status 415. UnsupportedMediaType indicates that the request is an unsupported type.";
                case HttpStatusCode.Unused:
                    return "Equivalent to HTTP status 306. Unused is a proposed extension to the HTTP / 1.1 specification that is not fully specified.";
                case HttpStatusCode.UpgradeRequired:
                    return "Equivalent to HTTP status 426. UpgradeRequired indicates that the client should switch to a different protocol such as TLS / 1.0.";
                case HttpStatusCode.UseProxy:
                    return "Equivalent to HTTP status 305. UseProxy indicates that the request should use the proxy server at the URI specified in the Location header.";
                default:
                    return "";
            }
        }

        [TestMethod]
        public void InClauseTest()
        {
            var channelFactory = new ChannelFactory<IMasterCatalogService>("CatalogService");
            IMasterCatalogService client = channelFactory.CreateChannel();

            QueryCriteriaDTO criterio = new QueryCriteriaDTO();
            criterio.ObjectType = CatalogObjectType.RRL_TUBULAR_TUBING;
            //int[] vals = new int[] { 11775, 11844, 26717 };
            string[] strVals = new string[] { "C-110  1.050\" OD/1.14#  0.824\" ID  0.730\" Drift",
                "C-110  1.050\" OD/1.20#  0.820\" ID  0.726\" Drift",
                "C-110  1.050\" OD/1.20#  0.824\" ID  0.730\" Drift",
                "C-110  1.050\" OD/1.50#  0.742\" ID  0.648\" Drift" };
            ConditionExpressionDTO condition1 = new ConditionExpressionDTO();
            condition1.EnumAttributeName = CatalogObjectAttr.RRLTubularTubingTubulartubPrimaryKey;
            condition1.Operation = OperatorType.Like;
            condition1.Value = new ConditionDataDTO();
            condition1.Value.StringValue = "1%";
            ConditionExpressionDTO condition2 = new ConditionExpressionDTO();
            condition2.EnumAttributeName = CatalogObjectAttr.RRLTubularTubingTubulartubDescription;
            condition2.Operation = OperatorType.In;
            condition2.Value = new ConditionDataDTO();
            condition2.Value.StringValues = strVals;
            criterio.SQLConditions = new ConditionExpressionDTO[] { condition1, condition2 };
            CatalogDTO[] dtos = client.GetMany(criterio);
            if (dtos == null)
            {
                Assert.Fail(criterio.ObjectType.ToString() + " : null");
            }
            else
            {
                Trace.WriteLine(criterio.ObjectType.ToString() + ": (" + dtos.Length.ToString() + ") " + dtos.ToString());
            }
        }

        [TestMethod]
        public void GetSingle()
        {
            var channelFactory = new ChannelFactory<IMasterCatalogService>("CatalogService");
            IMasterCatalogService client = channelFactory.CreateChannel();

            foreach (CatalogObjectType type in objTypes)
            {
                QueryCriteriaDTO criterio = BuildCriteria(type);

                var result = client.GetSingle(criterio);

                if (result == null)
                {
                    Assert.Fail(type.ToString() + " : null");
                }
                else
                {
                    Trace.WriteLine(type.ToString() + " : " + result.ToString());
                }
            }
        }

        [TestMethod]
        public void GetMany()
        {
            var channelFactory = new ChannelFactory<IMasterCatalogService>("CatalogService");
            IMasterCatalogService client = channelFactory.CreateChannel();
            foreach (CatalogObjectType type in objTypes)
            {
                QueryCriteriaDTO criterio = BuildCriteria(type);

                CatalogDTO[] dtos = client.GetMany(criterio);
                if (dtos == null)
                {
                    Assert.Fail(type.ToString() + ": (null) ");
                }
                else
                {
                    Trace.WriteLine(type.ToString() + ": (" + dtos.Length.ToString() + ") " + dtos.ToString());
                }
            }
        }

        private static QueryCriteriaDTO BuildCriteria(CatalogObjectType type)
        {
            QueryCriteriaDTO criterio = new QueryCriteriaDTO();
            ConditionExpressionDTO condition = null;
            criterio.ObjectType = type;

            switch (type)
            {
                case CatalogObjectType.RRL_APISRP_BARREL_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPBarrelTypeabtPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_BARREL_ACC:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtBarrelAccabaPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_BARREL_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtBarrelTypexhiPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_PLUNGER_ACC:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtPlungerAccapaPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_PLUNGER_PIN:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtPlungerPinappPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_PLUNGER_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtPlungerTypeapgPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_PUMP_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtPumpTypexhgPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_SAND:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtSandaesPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_SEAT_LOCATION:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtSeatLocationxhjPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_SEAT_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtSeatTypexhkPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_STANDING_VALVE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtStandingValveasvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_STANDING_VALVE_CAGE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtStandingValveCageasgPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_TRAVELLING_VALVE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtTravellingValveatvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_TRAVELLING_VALVE_CAGE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtTravellingValveCageatcPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_TRAVELLING_VALVE_PLUNGER:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtTravellingValvePlungeraspPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_VROD:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtVRodavrPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_EXT_WIPER:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPExtWiperawpPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_MEASURED_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPMeasuredTypeamtPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_PUMP_BORE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPPumpBoreapbPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_PUMP_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPPumpTypeaptPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_SEAT_LOCATION:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPSeatLocationaslPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_SEAT_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPSeatTypeasaPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_APISRP_TUBING_SIZE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLAPISRPTubingSizeatsPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_COMPONENT_CONDITION:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLComponentConditionrpcPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_COMPONENT_GROUPING:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLComponentGroupingstrPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_COMPONENT_PROPERTY:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLComponentPropertyCatalogItems_PrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_COMPONENT_USAGE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLComponentUsageusePrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_CRANK:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLCrankCrankIdentifier;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "10010%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_CRANK_WEIGHT:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLCrankWeightCrankPK_rrlCrank;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "11%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_ELECTRIC_MOTOR:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLElectricMotorinvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_GRADE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLGradergrPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "12%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_GRADE_MANUFACTURER:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLGradeManufacturerrgrPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_MANUFACTURER:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLManufacturermfgPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "17%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_MATERIAL:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLMaterialmtlPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "16%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_MFG_UNIT_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLMFGUnitTypemfgPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_MOTOR_SIZE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLMotorSizepmpPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_MOTOR_SLIP_TORQUE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLMotorSlipTorquepmvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_MOTOR_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLMotorTypeprmPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_PART_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLPartTypeptyPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_PERFORATION:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLPerforationinvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_POLISHED_ROD:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLPolishedRodinvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_PRIME_MOVER_MODE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLPrimeMoverModepmvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_PRIME_MOVER_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLPrimeMoverTypeprmPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_PUMPING_UNIT:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLPumpingUnitPumpPK_rrlSurfaceBase;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "10%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_PUMPING_UNIT_CRANK:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLPumpingUnitCrankPK_rrlSurfaceBase;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "10%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_ROD:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLRodRodrdmPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_ROD_MANUFACTURER:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLRodManufacturermfgPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_ROD_PUMP:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLRodPumpinvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_TUBING_HANGER:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLTubingHangerinvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_TUBING_PUMP:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLTubingPumpinvPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_TUBULAR:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLTubularTubingTubulartubPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "10%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_TUBULAR_CASING:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLTubularTubingTubulartubPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "10%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_TUBULAR_TUBING:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLTubularTubingTubulartubPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "11%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_TUBULAR_TUBING_SUB:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLTubularTubingTubulartubPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "17%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_UNIT_TYPE_MFG:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLUnitTypeMFGsugSurfaceUnitGeography;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "L%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_WEIGHT:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLWeightWeightPK_rrlWeight;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "10%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                case CatalogObjectType.RRL_WELLBORE_TYPE:
                    condition = new ConditionExpressionDTO();
                    condition.EnumAttributeName = CatalogObjectAttr.RRLWellboreTypesatPrimaryKey;
                    condition.Operation = OperatorType.Like;
                    condition.Value = new ConditionDataDTO();
                    condition.Value.StringValue = "1%";
                    criterio.SQLConditions = new ConditionExpressionDTO[1];
                    criterio.SQLConditions[0] = condition;
                    break;
                default:
                    break;
            }

            return criterio;
        }
    }
}
