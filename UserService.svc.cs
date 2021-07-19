using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using M3M.DataService.ServiceContract;
using System.ServiceModel.Web;
using M3M.DataService.DataContract;
using System.ServiceModel.Activation;

namespace M3M.DataService.Services
{
   [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class UserService : IUserService
    {

       public BaseReturnType<UserValidateRequest> VaidateUser(UserValidateRequest user)
        {
            BaseReturnType<UserValidateRequest> lst = new BaseReturnType<UserValidateRequest>();
            UserValidateRequest User = new UserValidateRequest();
            User.UserId = "Manjay";
            lst.result = User;
            lst.code = 100;
            lst.message = "Success";
            return lst;
        }


       
    }
}
