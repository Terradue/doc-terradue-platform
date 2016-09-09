using System;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.ServiceInterface;
using Terradue.Portal;
using Terradue.WebService.Model;
using Terradue.Corporate.Controller;
using Terradue.JFrog.Artifactory;

namespace Terradue.Corporate.WebServer
{
    
    [Route ("/user/repository", "GET", Summary = "get user repositories", Notes = "")]
    public class GetUserRepositories : IReturn<List<WebStorage>>
    {
        [ApiMember (Name = "id", Description = "id", ParameterType = "query", DataType = "int", IsRequired = false)]
        public int Id { get; set; }
    }

    [Route ("/user/repository", "POST", Summary = "create repository for user", Notes = "")]
    public class CreateUserRepository : IReturn<List<WebStorage>>
    {
        [ApiMember (Name = "repo", Description = "User repo", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string repo { get; set; }

        [ApiMember (Name = "id", Description = "id", ParameterType = "query", DataType = "int", IsRequired = false)]
        public int Id { get; set; }
    }

    [Route ("/user/repository/group", "GET", Summary = "get user repositories group", Notes = "")]
    public class GetUserRepositoriesGroup : IReturn<List<string>>
    {
        [ApiMember (Name = "id", Description = "id", ParameterType = "query", DataType = "int", IsRequired = false)]
        public int Id { get; set; }
    }

    [Route ("/user/repository/group", "POST", Summary = "create repository group for user", Notes = "")]
    public class CreateUserRepositoryGroup : IReturn<List<string>>
    {
        [ApiMember (Name = "id", Description = "id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// User.
    /// </summary>
    public class WebStorage
    {
        [ApiMember (Name = "FilesCount", Description = "storage files count", ParameterType = "query", DataType = "int", IsRequired = false)]
        public int FilesCount { get; set; }

        [ApiMember (Name = "FolderCount", Description = "storage folders count", ParameterType = "query", DataType = "int", IsRequired = false)]
        public int FoldersCount { get; set; }

        [ApiMember (Name = "Name", Description = "storage name", ParameterType = "query", DataType = "String", IsRequired = false)]
        public String Name { get; set; }

        [ApiMember (Name = "Type", Description = "storage type", ParameterType = "query", DataType = "String", IsRequired = false)]
        public String Type { get; set; }

        [ApiMember (Name = "UsedSpace", Description = "storage used space", ParameterType = "query", DataType = "String", IsRequired = false)]
        public String UsedSpace { get; set; }

        [ApiMember (Name = "Percentage", Description = "storage percentage", ParameterType = "query", DataType = "String", IsRequired = false)]
        public String Percentage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.WebServer.WebUserT2"/> class.
        /// </summary>
        public WebStorage () { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Terradue.Corporate.WebServer.WebStorage"/> class.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public WebStorage (RepositoriesSummary entity)
        {
            this.FilesCount = entity.filesCount;
            this.FoldersCount = entity.foldersCount;
            this.Name = entity.repoKey;
            this.Type = entity.repoType;
            this.UsedSpace = entity.usedSpace;
            this.Percentage = entity.percentage;
        }

    }
}

