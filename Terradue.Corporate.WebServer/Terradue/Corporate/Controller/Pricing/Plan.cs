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

    public class Plan { 

        public Role Role { get; set; }

        public Domain Domain { get; set; }
    }

    public class PlanFactory{

        public const string NONE = "No Plan";
        public const string TRIAL = "Free Trial";
        public const string EXPLORER = "Explorer";
        public const string SCALER = "Scaler";
        public const string PREMIUM = "Premium";

        public const string ROLEPREFIX = "plan_";
        public const string DOMAINPREFIX = "terradue_";

        private IfyContext context { get; set; }

        public PlanFactory(IfyContext context){
            this.context = context;
        }

        /// <summary>
        /// Verify if the role can be used for plans.
        /// </summary>
        /// <returns><c>true</c>, if role can be used for plan, <c>false</c> otherwise.</returns>
        /// <param name="role">role</param>
        public static bool IsRoleForPlan (Role role) {
            return role.Identifier.StartsWith (ROLEPREFIX);
        }

        /// <summary>
        /// Verify if the domain can be used for plans.
        /// </summary>
        /// <returns><c>true</c>, if domain can be used for plan, <c>false</c> otherwise.</returns>
        /// <param name="domain">domain</param>
        public static bool IsDomainForPlan (Domain domain)
        {
            return domain.Identifier.StartsWith (DOMAINPREFIX);
        }

        /// <summary>
        /// Gets the plan for user.
        /// </summary>
        /// <returns>The plan for user.</returns>
        /// <param name="user">User.</param>
        /// <param name="domain">Domain.</param>
        public Plan GetPlanForUser(User user, Domain domain){
            Role[] roles = Role.GetUserRolesForDomain (context, user.Id, domain.Id);

            //user can only have one role for a domain
            if (roles.Length > 0) return new Plan { Role = roles [0], Domain = domain };
            else return null;
        }

        /// <summary>
        /// Gets the plans for user.
        /// </summary>
        /// <returns>The plans for user.</returns>
        /// <param name="user">User.</param>
        public List<Plan> GetPlansForUser (User user){
            List<Plan> plans = new List<Plan> ();
            List<Domain> domains = GetAllDomains ();

            foreach (var domain in domains) {
                var plan = GetPlanForUser (user, domain);
                if (plan != null) plans.Add (plan);
            }
            return plans;
        }

        /// <summary>
        /// Gets all roles that can be used for plans.
        /// </summary>
        /// <returns>all roles.</returns>
        public List<Role> GetAllRoles(){
            List<Role> plans = new List<Role>();

            EntityList<Role> allRoles = new EntityList<Role> (context);
            allRoles.Load ();

            foreach (var role in allRoles)
                if (IsRoleForPlan(role)) plans.Add (role);

            return plans;
        }

        /// <summary>
        /// Gets all domains that can be used for plans.
        /// </summary>
        /// <returns>all domains.</returns>
        public List<Domain> GetAllDomains ()
        {
            List<Domain> plans = new List<Domain> ();

            EntityList<Domain> allDomains = new EntityList<Domain> (context);
            allDomains.Load ();

            foreach (var domain in allDomains)
                if (IsDomainForPlan (domain)) plans.Add (domain);

            return plans;
        }

    }
}

