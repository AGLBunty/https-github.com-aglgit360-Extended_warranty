using System;
using System.Collections.Generic;
using System.Linq;
using DMS.DataService.ServiceContract;
using System.ServiceModel.Activation;
using DMS.DataService.DataContract;
using DMS.DataService.Common.Enum;
using DMS.DataService.LogHelper;
using DMS.DataService.DataLayer;
using System.Data;
using System.ServiceModel.Web;
using System.Data.OracleClient;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Globalization;

namespace DMS.DataService.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DmsService : IExtended_Warranty
    {
		#region USP names, Objects and Variables
		string constr = ConfigurationManager.ConnectionStrings["conoracle"].ConnectionString;
		OracleConnection con;
		OracleCommand cmd;
		DataSet ds;
		OracleDataAdapter da;
		DataTable dt;
                
        string SP_GET_WARRANTY_TYPE = ConfigurationManager.AppSettings["SP_GET_WARRANTY_TYPE"].ToString();
        string SP_GET_VIN_DETAIL = ConfigurationManager.AppSettings["SP_GET_VIN_DETAIL"].ToString();
        string SP_GENERATE_ENQUIRY = ConfigurationManager.AppSettings["SP_GENERATE_ENQUIRY"].ToString();
        string SP_GET_ENQUIRY_STATUS = ConfigurationManager.AppSettings["SP_GET_ENQUIRY_STATUS"].ToString();

        #endregion


        #region GET_WARRANTY_TYPE
        public BaseListReturnType<GET_WARRANTY_TYPE> GET_WARRANTY_TYPE(string pn_pmc)
        {
            BaseListReturnType<GET_WARRANTY_TYPE> response = new BaseListReturnType<GET_WARRANTY_TYPE>();
            List<GET_WARRANTY_TYPE> mainlist = new List<GET_WARRANTY_TYPE>();
            GET_WARRANTY_TYPE list;
            try
            {
                ServiceHeaderInfo headerInfo = ServiceHelper.Authenticate(WebOperationContext.Current.IncomingRequest);
                #region TOKEN
                if (!headerInfo.IsAuthenticated)
                {
                    response.code = (int)ServiceMassageCode.UNAUTHORIZED_REQUEST;
                    response.message = Convert.ToString(ServiceMassageCode.ERROR);
                    response.result = null;
                    return response;
                }
                #endregion
                OracleDataReader rdrUnassigned;
                CreateLogFiles Err = new CreateLogFiles();
                con = new OracleConnection(constr);
                cmd = new OracleCommand();
                cmd.Connection = con;
                cmd.CommandText = SP_GET_WARRANTY_TYPE;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("pn_pmc", OracleType.Number).Value = Convert.ToInt32(pn_pmc);
                cmd.Parameters.Add("P_LIST_CURSOR", OracleType.Cursor).Direction = ParameterDirection.Output;// output Ref Cursor
                cmd.Parameters.Add("po_err_cd", OracleType.Number).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("po_err_msg", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                cmd.ExecuteNonQuery();
                rdrUnassigned = (OracleDataReader)cmd.Parameters["P_LIST_CURSOR"].Value;
                string outputStr = string.Empty;
                if (!string.IsNullOrEmpty(cmd.Parameters["po_err_msg"].Value.ToString()))
                {
                    response.code = Convert.ToInt32(cmd.Parameters["po_err_cd"].Value.ToString());
                    response.message = cmd.Parameters["po_err_msg"].Value.ToString();
                    response.result = null;
                    con.Close();
                    cmd.Dispose();
                    return response;
                }
                if (rdrUnassigned.HasRows)
                {
                    while (rdrUnassigned.Read())
                    {
                        try
                        {
                            list = new GET_WARRANTY_TYPE();
                            list.LIST_CODE = rdrUnassigned["LIST_CODE"].ToString();
                            list.WARRANTY_TYPE = rdrUnassigned["WARRANTY_TYPE"].ToString();
                     
                            mainlist.Add(list);
                        }
                        catch (Exception ex)
                        {
                            con.Close();
                            cmd.Dispose();
                            //Err.LogException(ex, "ResourceNonAvailabeList");
                            //Logging.Error(ex, "DmsService:ManpowerList");
                            response.code = (int)ServiceMassageCode.ERROR;
                            response.message = ex.Message; //Convert.ToString(ServiceMassageCode.ERROR);
                            response.result = null;
                            return response;
                        }
                    }
                }
                con.Close();
                response.code = (int)ServiceMassageCode.SUCCESS;
                response.message = Convert.ToString(ServiceMassageCode.SUCCESS);
                response.result = mainlist;
            }
            catch (Exception ex)
            {
                // CreateLogFiles Err = new CreateLogFiles();
                // Err.ErrorLog((@"ErrorLog/Logfile"), ex.Message);

                //Logging.Error(ex, "DMS:PushEvaluaton");
                response.code = 100; //(int)ServiceMassageCode.ERROR;
                response.message = ex.Message; //Convert.ToString(ServiceMassageCode.ERROR);
                response.result = null;
                con.Close();
                cmd.Dispose();
            }
            finally
            {
                con.Close();
                cmd.Dispose();
                OracleConnection.ClearPool(con);
            }
            return response;
        }


        #endregion


        #region GET_VIN_DETAIL
        public BaseReturnType<GET_VIN_DETAIL> GET_VIN_DETAIL(string pn_pmc, string pn_vin)
        {
            BaseReturnType<GET_VIN_DETAIL> response = new BaseReturnType<GET_VIN_DETAIL>();
            //DateTime tempDOB;
            try
            {
                GET_VIN_DETAIL result = new GET_VIN_DETAIL();
                ServiceHeaderInfo headerInfo = ServiceHelper.Authenticate(WebOperationContext.Current.IncomingRequest);
                // DateTime DateOfEval;
                if (!headerInfo.IsAuthenticated)
                {
                    response.code = (int)ServiceMassageCode.UNAUTHORIZED_REQUEST;
                    response.message = Convert.ToString(ServiceMassageCode.ERROR);
                    response.result = null;
                    return response;
                }
                con = new OracleConnection(constr);
                cmd = new OracleCommand();
                cmd.Connection = con;
                cmd.CommandText = SP_GET_VIN_DETAIL;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("pn_pmc", OracleType.VarChar).Value = Convert.ToInt32(pn_pmc); // Input
                cmd.Parameters.Add("pn_vin", OracleType.VarChar).Value = pn_vin; // Input

                cmd.Parameters.Add("po_reg_num", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;//out put
                cmd.Parameters.Add("po_cutomer_name", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;//out put
                cmd.Parameters.Add("po_model", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;//out put
                cmd.Parameters.Add("po_enq_num", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;//out put
                cmd.Parameters.Add("po_err_cd", OracleType.Number).Direction = ParameterDirection.Output;//out put
                cmd.Parameters.Add("po_err_msg", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;//out put 
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                cmd.ExecuteNonQuery();
                if (string.IsNullOrEmpty(cmd.Parameters["po_err_msg"].Value.ToString()))
                {
                    response.code = Convert.ToInt32(cmd.Parameters["po_err_cd"].Value.ToString());
                    response.message = cmd.Parameters["po_err_msg"].Value.ToString();
                    response.result = null;
                    con.Close();
                    cmd.Dispose();
                    return response;
                }
                try
                {
                    result.po_reg_num = cmd.Parameters["po_reg_num"].Value.ToString();
                    result.po_cutomer_name = cmd.Parameters["po_cutomer_name"].Value.ToString();
                    result.po_model = cmd.Parameters["po_model"].Value.ToString();
                    result.po_enq_num = cmd.Parameters["po_enq_num"].Value.ToString();



                }
                catch (Exception ex)
                {
                    //CreateLogFiles Err = new CreateLogFiles();
                    response.code = 100; //(int)ServiceMassageCode.SUCCESS;
                                         //Err.LogException(ex, "WORKSHOP_ENQ_CREATE");
                    response.message = ex.Message;
                    response.result = null;
                }
                con.Close();
                response.code = (int)ServiceMassageCode.SUCCESS;
                response.message = Convert.ToString(ServiceMassageCode.SUCCESS);
                response.result = result;
            }
            catch (Exception ex)
            {
               
                response.code = 100; //(int)ServiceMassageCode.ERROR;
                response.message = ex.Message; //Convert.ToString(ServiceMassageCode.ERROR);
                response.result = null;
                con.Close();
                cmd.Dispose();
            }
            finally
            {
                con.Close();
                cmd.Dispose();
                OracleConnection.ClearPool(con);
            }
            return response;
        }


        #endregion


        #region GENERATE_ENQUIRY
        public BaseReturnType<GENERATE_ENQUIRY> GENERATE_ENQUIRY(string pn_pmc, string pn_vin, string pn_reg_num, string pn_odometer_reading,string pn_EW_TYPE,string pn_contact_number)
        {
            BaseReturnType<GENERATE_ENQUIRY> response = new BaseReturnType<GENERATE_ENQUIRY>();
            //DateTime tempDOB;
            try
            {
                GENERATE_ENQUIRY result = new GENERATE_ENQUIRY();
                ServiceHeaderInfo headerInfo = ServiceHelper.Authenticate(WebOperationContext.Current.IncomingRequest);
                // DateTime DateOfEval;
                if (!headerInfo.IsAuthenticated)
                {
                    response.code = (int)ServiceMassageCode.UNAUTHORIZED_REQUEST;
                    response.message = Convert.ToString(ServiceMassageCode.ERROR);
                    response.result = null;
                    return response;
                }
                con = new OracleConnection(constr);
                cmd = new OracleCommand();
                cmd.Connection = con;
                cmd.CommandText = SP_GENERATE_ENQUIRY;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("pn_pmc", OracleType.VarChar).Value = Convert.ToInt32(pn_pmc); // Input
                cmd.Parameters.Add("pn_vin", OracleType.VarChar).Value = pn_vin; // Input
                cmd.Parameters.Add("pn_reg_num", OracleType.VarChar).Value = pn_reg_num; // Input
                cmd.Parameters.Add("pn_odometer_reading", OracleType.VarChar).Value = pn_odometer_reading; // Input
                cmd.Parameters.Add("pn_EW_TYPE", OracleType.VarChar).Value = pn_EW_TYPE; // Input
                cmd.Parameters.Add("pn_contact_number", OracleType.VarChar).Value = pn_contact_number; // Input

                cmd.Parameters.Add("po_enq_number", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;//out put
                cmd.Parameters.Add("po_err_cd", OracleType.Number).Direction = ParameterDirection.Output;//out put
                cmd.Parameters.Add("po_err_msg", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;//out put 
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                cmd.ExecuteNonQuery();
                if (string.IsNullOrEmpty(cmd.Parameters["po_err_msg"].Value.ToString()))
                {
                    response.code = Convert.ToInt32(cmd.Parameters["po_err_cd"].Value.ToString());
                    response.message = cmd.Parameters["po_err_msg"].Value.ToString();
                    response.result = null;
                    con.Close();
                    cmd.Dispose();
                    return response;
                }
                try
                {
                    result.po_enq_number = cmd.Parameters["po_enq_number"].Value.ToString();


                }
                catch (Exception ex)
                {
                    //CreateLogFiles Err = new CreateLogFiles();
                    response.code = 100; //(int)ServiceMassageCode.SUCCESS;
                                         //Err.LogException(ex, "WORKSHOP_ENQ_CREATE");
                    response.message = ex.Message;
                    response.result = null;
                }
                con.Close();
                response.code = (int)ServiceMassageCode.SUCCESS;
                response.message = Convert.ToString(ServiceMassageCode.SUCCESS);
                response.result = result;
            }
            catch (Exception ex)
            {

                response.code = 100; //(int)ServiceMassageCode.ERROR;
                response.message = ex.Message; //Convert.ToString(ServiceMassageCode.ERROR);
                response.result = null;
                con.Close();
                cmd.Dispose();
            }
            finally
            {
                con.Close();
                cmd.Dispose();
                OracleConnection.ClearPool(con);
            }
            return response;
        }


        #endregion


        #region GET_ENQUIRY_STATUS
        public BaseListReturnType<GET_ENQUIRY_STATUS> GET_ENQUIRY_STATUS(string pn_enq_number)
        {
            BaseListReturnType<GET_ENQUIRY_STATUS> response = new BaseListReturnType<GET_ENQUIRY_STATUS>();
            List<GET_ENQUIRY_STATUS> mainlist = new List<GET_ENQUIRY_STATUS>();
            GET_ENQUIRY_STATUS list;
            try
            {
                ServiceHeaderInfo headerInfo = ServiceHelper.Authenticate(WebOperationContext.Current.IncomingRequest);
                #region TOKEN
                if (!headerInfo.IsAuthenticated)
                {
                    response.code = (int)ServiceMassageCode.UNAUTHORIZED_REQUEST;
                    response.message = Convert.ToString(ServiceMassageCode.ERROR);
                    response.result = null;
                    return response;
                }
                #endregion
                OracleDataReader rdrUnassigned;
                CreateLogFiles Err = new CreateLogFiles();
                con = new OracleConnection(constr);
                cmd = new OracleCommand();
                cmd.Connection = con;
                cmd.CommandText = SP_GET_WARRANTY_TYPE;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("pn_enq_number", OracleType.VarChar).Value = pn_enq_number;
                cmd.Parameters.Add("P_LIST_CURSOR", OracleType.Cursor).Direction = ParameterDirection.Output;// output Ref Cursor
                cmd.Parameters.Add("po_err_cd", OracleType.Number).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("po_err_msg", OracleType.VarChar, 4000).Direction = ParameterDirection.Output;
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                cmd.ExecuteNonQuery();
                rdrUnassigned = (OracleDataReader)cmd.Parameters["P_LIST_CURSOR"].Value;
                string outputStr = string.Empty;
                if (!string.IsNullOrEmpty(cmd.Parameters["po_err_msg"].Value.ToString()))
                {
                    response.code = Convert.ToInt32(cmd.Parameters["po_err_cd"].Value.ToString());
                    response.message = cmd.Parameters["po_err_msg"].Value.ToString();
                    response.result = null;
                    con.Close();
                    cmd.Dispose();
                    return response;
                }
                if (rdrUnassigned.HasRows)
                {
                    while (rdrUnassigned.Read())
                    {
                        try
                        {
                            list = new GET_ENQUIRY_STATUS();
                            list.SATGE_NAME = rdrUnassigned["SATGE_NAME"].ToString();
                            list.ENQUIRY_STATUS = rdrUnassigned["ENQUIRY_STATUS"].ToString();
                            list.created_date = rdrUnassigned["created_date"].ToString();

                            mainlist.Add(list);
                        }
                        catch (Exception ex)
                        {
                            con.Close();
                            cmd.Dispose();
                            //Err.LogException(ex, "ResourceNonAvailabeList");
                            //Logging.Error(ex, "DmsService:ManpowerList");
                            response.code = (int)ServiceMassageCode.ERROR;
                            response.message = ex.Message; //Convert.ToString(ServiceMassageCode.ERROR);
                            response.result = null;
                            return response;
                        }
                    }
                }
                con.Close();
                response.code = (int)ServiceMassageCode.SUCCESS;
                response.message = Convert.ToString(ServiceMassageCode.SUCCESS);
                response.result = mainlist;
            }
            catch (Exception ex)
            {
                // CreateLogFiles Err = new CreateLogFiles();
                // Err.ErrorLog((@"ErrorLog/Logfile"), ex.Message);

                //Logging.Error(ex, "DMS:PushEvaluaton");
                response.code = 100; //(int)ServiceMassageCode.ERROR;
                response.message = ex.Message; //Convert.ToString(ServiceMassageCode.ERROR);
                response.result = null;
                con.Close();
                cmd.Dispose();
            }
            finally
            {
                con.Close();
                cmd.Dispose();
                OracleConnection.ClearPool(con);
            }
            return response;
        }


        #endregion

    }
}
