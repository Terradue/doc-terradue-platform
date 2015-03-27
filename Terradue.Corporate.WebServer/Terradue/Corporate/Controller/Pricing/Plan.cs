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


namespace Terradue.Corporate.Controller {

    public enum PlanType{
        NONE,
        TRIAL,
        DEVELOPER,
        INTEGRATOR,
        PRODUCER
    }

    /// <summary>
    /// Plan.
    /// </summary>
    public class Plan {

        private const string PLAN_NONE = "No plan";
        private const string PLAN_TRIAL = "Free Trial";
        private const string PLAN_DEVELOPER = "Developer";
        private const string PLAN_INTEGRATOR = "Integrator";
        private const string PLAN_PRODUCER = "Producer";

        public PlanType PlanType { get; set; }
        IfyContext context { get; set; }
        int UserId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.Controller.Plan"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="usrid">Usrid.</param>
        public Plan(IfyContext context, int usrid){
            this.context = context;
            this.UserId = usrid;
            this.PlanType = GetPlan();
        }

        /// <summary>
        /// Transform from PlanType to string.
        /// </summary>
        /// <returns>The plan as string.</returns>
        /// <param name="type">Type.</param>
        public static string PlanToString(PlanType type){
            switch ((int)type) {
                case (int)PlanType.TRIAL:
                    return PLAN_TRIAL;
                case (int)PlanType.DEVELOPER:
                    return PLAN_DEVELOPER;
                case (int)PlanType.INTEGRATOR:
                    return PLAN_INTEGRATOR;
                case (int)PlanType.PRODUCER:
                    return PLAN_PRODUCER;
                default:
                    return PLAN_NONE;
            }
        }

        /// <summary>
        /// transform from string to PlanType.
        /// </summary>
        /// <returns>The string as plantype.</returns>
        /// <param name="type">Type.</param>
        public static PlanType StringToPlan(string type){
            switch (type) {
                case PLAN_TRIAL:
                    return PlanType.TRIAL;
                case PLAN_DEVELOPER:
                    return PlanType.DEVELOPER;
                case PLAN_INTEGRATOR:
                    return PlanType.INTEGRATOR;
                case PLAN_PRODUCER:
                    return PlanType.PRODUCER;
                default:
                    return PlanType.NONE;
            }
        }

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <returns>The plan.</returns>
        public PlanType GetPlan(){
            string sql = String.Format("SELECT role.name FROM role INNER JOIN usr_role ON role.id=usr_role.id_role WHERE usr_role.id_usr={0};",
                                       this.UserId);
            try{
                string plan = context.GetQueryStringValue(sql); 
                return StringToPlan(plan);
            }catch(Exception){
                return PlanType.NONE;
            }
        }

        /// <summary>
        /// Upgrade the specified type.
        /// </summary>
        /// <param name="type">Type.</param>
        public void Upgrade(PlanType type){
            //delete old plan
            string sql = String.Format ("DELETE FROM usr_role WHERE id_usr={0} AND id_role IN (SELECT role.id FROM role WHERE role.name={1});",
                                        this.UserId,
                                        StringUtils.EscapeSql(PlanToString(this.PlanType)));
            context.Execute (sql);

            this.PlanType = type;
            if (type != PlanType.NONE) {
                //insert new plan
                sql = String.Format("INSERT INTO usr_role (id_usr,id_role) SELECT usr.id, role.id FROM usr INNER JOIN role WHERE usr.id={0} AND role.name={1};", 
                                this.UserId, 
                                StringUtils.EscapeSql(PlanToString(this.PlanType)));
                context.Execute(sql);
            }
        }

    }
}

