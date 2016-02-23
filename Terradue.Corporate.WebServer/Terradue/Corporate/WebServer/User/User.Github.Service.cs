﻿using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.OpenNebula;
using Terradue.Corporate.Controller;
using Terradue.Security.Certification;
using Terradue.Github;
using Terradue.Github.WebService;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserGithubService : ServiceStack.ServiceInterface.Service {

        public object Put(GetNewGithubToken request) {
            if (request.Code == null)
                throw new Exception("Code is empty");

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebGithubProfile result;

            try {
                context.Open();
                GithubProfile user = GithubProfile.FromId(context, context.UserId);

                user.GetNewAuthorizationToken(request.Code);
                result = new WebGithubProfile(user);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return result;
        }

        public object Post(AddGithubSSHKeyToCurrentUser request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebGithubProfile result;

            try {
                context.Open();
                GithubProfile user = GithubProfile.FromId(context, context.UserId);
                try{
                    user.LoadPublicKeyFromSafe();
                }catch(Exception e){
                    user.PublicSSHKey = null;
                }
                GithubClient client = new GithubClient(context);
                if(!user.IsAuthorizationTokenValid()) throw new UnauthorizedAccessException("Invalid token");
                if(user.PublicSSHKey == null) throw new UnauthorizedAccessException("No available public ssh key");
                client.AddSshKey("Terradue certificate", user.PublicSSHKey, user.Token);
                result = new WebGithubProfile(user);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return result;
        }

        public object Get(GetGithubUser request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebGithubProfile result;

            try {
                context.Open();
                GithubProfile user = GithubProfile.FromId(context, context.UserId);
                try{
                    user.LoadPublicKeyFromSafe();
                }catch(Exception e){
                    user.PublicSSHKey = null;
                }
                result = new WebGithubProfile(user);
                result.HasSSHKey = true;
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return result;
        }

        public object Put(UpdateGithubUser request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebGithubProfile result;

            try {
                context.Open();
                GithubProfile user = GithubProfile.FromId(context, request.Id);
                user = request.ToEntity(context, user);
                user.Store();
                user.Load(); //to get information from Github
                result = new WebGithubProfile(user);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return result;
        }

    }
}

