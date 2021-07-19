using System.ServiceModel.Web;

namespace DMS.DataService.Services
{
    public class ServiceHelper
    {
        public static ServiceHeaderInfo Authenticate(IncomingWebRequestContext requestContext)
        {
            ServiceHeaderInfo headerInfo = new ServiceHeaderInfo();          
            System.Net.WebHeaderCollection headerCollection = requestContext.Headers;        
            headerInfo.Token = headerCollection["Token"];

            if (headerInfo.Token == "dR$545#h^jjmanJ")
            {
                headerInfo.IsAuthenticated = true;
            }
            else
            {
                headerInfo.IsAuthenticated = false;
            }

            return headerInfo;
        }
    }

    public class ServiceHeaderInfo
    {

        public bool IsAuthenticated { get; set; }
        public string Token { get; set; }
    }
}