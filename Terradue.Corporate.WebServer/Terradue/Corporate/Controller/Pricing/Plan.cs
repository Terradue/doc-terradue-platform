/*
\startuml{plans.png}
!define DIAG_NAME Plans/Roles Analysis Sequence Diagram

participant "Admin" as A
participant "User" as U
participant "Group" as G
participant "Domain" as D
participant "Role" as R

autonumber

== Group creation ==
A -> G: create
activate G
G -> D: create
deactivate G

== Roles creation ==
A -> G: select roles \n(from a list of "no domain" roles?)
G -> R: add roles 
activate R
R -> D: Creation of roles for this domain
R -> G: Group have access to a list of roles
deactivate R

== Add user to a group ==
A -> U: add to group
U -> G: added to group
G -> R: associate user to roles \n(usr_group table)
R -> U: usr has access to roles

== User access roles ==
alt "User is in Group"
    U -> G: belongs to
    activate G
    G -> D: get roles
    deactivate G
    D -> R: get roles
    activate R
    R -> U: give access to roles
    deactivate R
end

== Get a private plan ==
== Get a group plan ==




footer
DIAG_NAME
(c) Terradue Srl
endfooter
\enduml

@}
*/
using System;
using Terradue.Portal;
using Terradue.Util;
using System.Collections.Generic;
using System.Data;


namespace Terradue.Corporate.Controller {
    
    /// <summary>
    /// Plan.
    /// </summary>
    public class Plan {

        public const string NONE = "No Plan";
        public const string TRIAL = "Free Trial";
        public const string EXPLORER = "Explorer";
        public const string SCALER = "Scaler";
        public const string PREMIUM = "Premium";

        public int Id { get; set; }
        public string Name { get; set; }

        public Plan(){
            Id = 0;
            Name = Plan.NONE;
        }

        public static Plan FromId(IfyContext context, int id){
            Plan plan = new Plan();
            if (id != 0) {
                plan.Id = id;
                plan.Name = context.GetQueryStringValue(String.Format("SELECT role.name FROM role WHERE role.id={0};",id));
            }
            return plan;
        }

        public static Plan FromName(IfyContext context, string name){
            Plan plan = new Plan();
            if (!name.Equals(Plan.NONE)) {
                var pid = context.GetQueryIntegerValue(String.Format("SELECT role.id FROM role WHERE role.name={0};",StringUtils.EscapeSql(name)));
                if (pid == 0) throw new Exception("Invalid plan name");
                plan.Name = name;
                plan.Id = pid;
            }
            return plan;
        }
    }

    public class PlanFactory{

        private IfyContext context { get; set; }

        public PlanFactory(IfyContext context){
            this.context = context;
        }

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <returns>The plan.</returns>
        public Plan GetPlanForUser(int userId){
            Plan plan = new Plan();

            string sql = String.Format("SELECT role.id, role.name FROM role INNER JOIN usr_role ON role.id=usr_role.id_role WHERE usr_role.id_usr={0};",
                                       userId);
            IDbConnection dbConnection = context.GetDbConnection();
            IDataReader reader = context.GetQueryResult(sql, dbConnection);
            try{
                if(reader.Read()){
                    plan = new Plan{
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };
                }
            }catch(Exception e){
                var t = e;
            }
            context.CloseQueryResult (reader, dbConnection);
            return plan;
        }

        /// <summary>
        /// Gets all plans.
        /// </summary>
        /// <returns>The all plans.</returns>
        public List<Plan> GetAllPlans(){
            List<Plan> plans = new List<Plan>();

            //add default plan
            plans.Add(new Plan());

            string sql = String.Format("SELECT id, name FROM role order by id;");
            IDbConnection dbConnection = context.GetDbConnection();
            IDataReader reader = context.GetQueryResult(sql, dbConnection);

            while (reader.Read()) {
                if (reader.GetValue(0) != DBNull.Value)
                    plans.Add(new Plan{
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
            context.CloseQueryResult (reader, dbConnection);
            return plans;
        }

        /// <summary>
        /// Upgrades the user plan.
        /// </summary>
        /// <param name="usrId">Usr identifier.</param>
        /// <param name="plan">Plan.</param>
        public void UpgradeUserPlan(int usrId,Plan plan){
            //delete old plan
            string sql = String.Format ("DELETE FROM usr_role WHERE id_usr={0};",
                                        usrId);
                                                    
            context.Execute (sql);

            if (plan.Id != 0) {
                //insert new plan
                sql = String.Format("INSERT INTO usr_role (id_usr,id_role) VALUES ({0},{1});", 
                                    usrId, 
                                    plan.Id);
                context.Execute(sql);
            }
        }
    }
}

