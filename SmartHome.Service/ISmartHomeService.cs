using Shared;
using Shared.SmartHomeService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace SmartHome.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ISmartHomeService
    {

        [OperationContract]
        [WebInvoke(Method ="GET", ResponseFormat =WebMessageFormat.Json, UriTemplate ="/GetData/{value}")]
        string GetData(string value);


        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Ping")]
        void Ping();


        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/OpenSession/{value}")]
        OpenSessionResult OpenSession(string value);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/Reset")]
        void Reset();

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/GetNotes")]
        List<BulbAddedDto> GetNotes();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle= WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/AddNote")]
        void AddNote(System.IO.Stream StreamData);
    }

}
