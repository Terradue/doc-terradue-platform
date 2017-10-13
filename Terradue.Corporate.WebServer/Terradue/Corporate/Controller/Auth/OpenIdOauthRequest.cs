using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Terradue.Corporate.Controller {

    [DataContract]
    public class OauthTokenResponse {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public long expires_in { get; set; }
        [DataMember]
        public string id_token { get; set; }
        [DataMember]
        public string refresh_token { get; set; }
        [DataMember]
        public List<string> scope { get; set; }
    }

    [DataContract]
    public class OauthUserInfoResponse {
        //standard OpenId Connect claims
        [DataMember]
        public string sub { get; set; }
		[DataMember]
		public string user_name { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string given_name { get; set; }
        [DataMember]
        public string family_name { get; set; }
        [DataMember]
        public string middle_name { get; set; }
        [DataMember]
        public string nickname { get; set; }
        [DataMember]
        public string preferred_username { get; set; }
        [DataMember]
        public string profile { get; set; }
        [DataMember]
        public string picture { get; set; }
        [DataMember]
        public string website { get; set; }
        [DataMember]
        public string email { get; set; }
        [DataMember]
        public bool email_verifier { get; set; }
        [DataMember]
        public string gender { get; set; }
        [DataMember]
        public string birthdate { get; set; }
        [DataMember]
        public string zoneinfo { get; set; }
        [DataMember]
        public string locale { get; set; }
        [DataMember]
        public string phone_number { get; set; }
        [DataMember]
        public bool phone_number_verified { get; set; }
        [DataMember]
        public OauthUserAddress address { get; set; }
        [DataMember]
        public long updated_at { get; set; }

        //Non standard claims
        [DataMember]
        public List<string> groups { get; set; }
        [DataMember]
        public string sshPublicKey { get; set; }

        //everest
        [DataMember]
        public string VRC { get; set; }

    }

    [DataContract]
    public class OauthUserAddress {
        [DataMember]
        public string formatted { get; set; }
        [DataMember]
        public string street_address { get; set; }
        [DataMember]
        public string locality { get; set; }
        [DataMember]
        public string region { get; set; }
        [DataMember]
        public string postal_code { get; set; }
        [DataMember]
        public string country { get; set; }
    }
}
