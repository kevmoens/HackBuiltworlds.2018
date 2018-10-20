using Newtonsoft.Json;
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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class SmartHomeService : ISmartHomeService
    {
        public string GetData(string value)
        {
            return string.Format("You entered: {0}", value);
        }
        public void Ping() { }
        public void Reset()
        {
            Sessions.Clients = new System.Collections.Concurrent.ConcurrentDictionary<string, Guid>();
        }

        public OpenSessionResult OpenSession(string value)
        {
            OpenSessionResult resp = new OpenSessionResult();
            try
            {
                resp.IsPrimary = true;  //  Change For Multiple Users Sessions.Clients.Keys.Count == 0;
                Sessions.Clients.TryAdd(value, Guid.NewGuid()); // Guid.Parse(value));
            } catch (Exception ex){
                resp.Error = ex.Message;
            }
            return resp;
        }

        public List<BulbAddedDto> GetNotes()
        {
            return Sessions.Notes;
        }

        public void AddNote(System.IO.Stream StreamData)
        {
            var reader = new System.IO.StreamReader(StreamData);
            string Input = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            BulbAddedDto bulb = (BulbAddedDto)JsonConvert.DeserializeObject(Input, typeof(BulbAddedDto));
            if (Sessions.Notes == null) Sessions.Notes = new List<BulbAddedDto>();
            Sessions.Notes.Add(bulb);
        }
    }
}
