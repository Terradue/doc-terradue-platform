using System;
using System.Runtime.Serialization;

namespace Terradue.Corporate.WebServer {

    [DataContract]
    public class ConnectResponse{

        [DataMember]
        public Result result { get; set; }

        [DataMember]
        public Error error { get; set; }

        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string jsonrpc { get; set; }
    }

    [DataContract]
    public class Result{

        [DataMember]
        public string CID { get; set; }
    }

    [DataContract]
    public class Error{

        [DataMember]
        public string code { get; set; }

        [DataMember]
        public string message { get; set; }
    }
}

