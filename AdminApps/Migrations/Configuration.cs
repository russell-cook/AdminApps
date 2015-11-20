namespace AdminApps.Migrations
{
    using System.Data.Entity.Migrations;
    using AdminApps.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Omu.ValueInjecter;
    using System.Collections.Generic;
    using System.Web;
    using AdminApps.DAL;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using AdminApps.Models.ProjectManagement;


    internal sealed class Configuration : DbMigrationsConfiguration<AdminApps.DAL.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }


        protected override void Seed(ApplicationDbContext context)
        {
            // 2014-11-19: Initialize NEBS testing values, starting at the top of the dependency hierarchy. These values are meant for development purposes; 
            // they will be replaced by populated values from NEBS DB using functions in NebsConcurrencyController.
            context.BudgetPeriods.AddOrUpdate(i => i.ID,
                    new BudgetPeriod { ID = 7, Description = "2015-2017 Biennium (FY16-17)", ActualYear = "2014", WorkProgramYear = "2015", BeginYear = "2016", EndYear = "2017" },
                    new BudgetPeriod { ID = 7, Description = "2017-2019 Biennium (FY18-19)", ActualYear = "2016", WorkProgramYear = "2017", BeginYear = "2018", EndYear = "2019" }
                );

            context.BudgetSessions.AddOrUpdate(i => i.ID,
                    new BudgetSession { ID = 1, Title = "77th Regular Session" },
                    new BudgetSession { ID = 2, Title = "78th Regular Session" }
                );

            context.Depts.AddOrUpdate(i => i.ID,
                    new Dept { ID = 212516, BudgetPeriodID = 7, Code = "08", Description = "DEPARTMENT OF ADMINISTRATION" }
                );

            context.Divs.AddOrUpdate(i => i.ID,
                    new Div { ID = 212568, BudgetPeriodID = 7, DeptID = 212516, Code = "080", Description = "ADMIN - DIRECTOR'S OFFICE", SortOrder = 0 }
                );

            // Initialize AppGlobalSettings values. There should never be more than a single record in this table.
            // In the production app this can be configured via the GlobalSettingsController.
            context.AppGlobalSettings.AddOrUpdate(i => i.ID,
                    new AppGlobalSetting { ID = 1, BudgetPeriodID = 7, BudgetSessionID = 2 }
                );

            // Initialize list of AppModules
            context.AppModules.AddOrUpdate(i => i.ID,
                    new AppModule { ID = 1, Title = "Manage User Account", DefaultController = "Manage", DefaultAction = "Index" },
                    new AppModule { ID = 2, Title = "Global App Settings", DefaultController = "Home", DefaultAction = "Index" },
                    new AppModule { ID = 3, Title = "Role Administration", DefaultController = "RolesAdmin", DefaultAction = "Index" },
                    new AppModule { ID = 4, Title = "User Administration", DefaultController = "UsersAdmin", DefaultAction = "Index" },
                    new AppModule { ID = 5, Title = "Bill Tracking", DefaultController = "Bills", DefaultAction = "Home" },
                    new AppModule { ID = 6, Title = "User Projects", DefaultController = "UserProjects", DefaultAction = "Index" }
                );


            // Initialize reference values for Bill Tracking module

            context.BillPrefixes.AddOrUpdate(i => i.ID,
                new BillPrefix {ID = 1, Prefix = "AB",  Description = "Assembly Bill"},
                new BillPrefix {ID = 2, Prefix = "ACR", Description = "Assembly Concurrent Resolution"},
                new BillPrefix {ID = 3, Prefix = "AJR", Description = "Assembly Joint Resolution"},
                new BillPrefix {ID = 4, Prefix = "AR",  Description = "Assembly Resolution"},
                new BillPrefix {ID = 5, Prefix = "SB",  Description = "Senate Bill"},
                new BillPrefix {ID = 6, Prefix = "SCR", Description = "Senate Concurrent Resolution"},
                new BillPrefix {ID = 7, Prefix = "SJR", Description = "Senate Joint Resolution"},
                new BillPrefix {ID = 8, Prefix = "SR",  Description = "Senate Resolution"}
            );

            context.GovActions.AddOrUpdate(i => i.ID,
                new GovAction {ID = 1, Description = "Vetoed"},
                new GovAction {ID = 2, Description = "Approved"}
            );

            context.LegCommittees.AddOrUpdate(i => i.ID,
                new LegCommittee { ID = 1,   House = House.Assembly, CommitteeName = "Commerce and Labor"},
                new LegCommittee { ID = 2,   House = House.Assembly, CommitteeName = "Education"},
                new LegCommittee { ID = 3,   House = House.Assembly, CommitteeName = "Government Affairs"},
                new LegCommittee { ID = 4,   House = House.Assembly, CommitteeName = "Health and Human Services"},
                new LegCommittee { ID = 5,   House = House.Assembly, CommitteeName = "Judiciary"},
                new LegCommittee { ID = 6,   House = House.Assembly, CommitteeName = "Legislative Operations and Elections"},
                new LegCommittee { ID = 7,   House = House.Assembly, CommitteeName = "Natural Resources, Agriculture, and Mining"},
                new LegCommittee { ID = 8,   House = House.Assembly, CommitteeName = "Taxation"},
                new LegCommittee { ID = 9,   House = House.Assembly, CommitteeName = "Transportation"},
                new LegCommittee { ID = 10,   House = House.Assembly, CommitteeName = "Ways and Means"},
                new LegCommittee { ID = 11, House = House.Senate, CommitteeName = "Finance" },
                new LegCommittee { ID = 12, House = House.Senate, CommitteeName = "Commerce, Labor and Energy" },
                new LegCommittee { ID = 13, House = House.Senate, CommitteeName = "Education" },
                new LegCommittee { ID = 14, House = House.Senate, CommitteeName = "Government Affairs" },
                new LegCommittee { ID = 15, House = House.Senate, CommitteeName = "Health and Human Services" },
                new LegCommittee { ID = 16, House = House.Senate, CommitteeName = "Judiciary" },
                new LegCommittee { ID = 17, House = House.Senate, CommitteeName = "Legislative Operations and Elections" },
                new LegCommittee { ID = 18, House = House.Senate, CommitteeName = "Natural Resources" },
                new LegCommittee { ID = 19, House = House.Senate, CommitteeName = "Revenue and Economic Development" },
                new LegCommittee { ID = 19,  House = House.Senate,   CommitteeName = "Transportation"}
            );

            context.LegStatuses.AddOrUpdate(i => i.ID,
                new LegStatus { ID = 1, Description = "Pending" },
                new LegStatus { ID = 2, Description = "Withdrawn" },
                new LegStatus { ID = 3, Description = "Failed" },
                new LegStatus { ID = 4, Description = "Passed" },
                new LegStatus { ID = 5, Description = "Enrolled" }
            );

            context.BillReviewRecommendations.AddOrUpdate(i => i.ID,
                new BillReviewRecommendation { ID = 1, Description = "Support" },
                new BillReviewRecommendation { ID = 2, Description = "Have Concerns" },
                new BillReviewRecommendation { ID = 3, Description = "Neutral" }
            );


            // Initialize Identity data. This routine has been adapted from ApplicationDbInitializer; it was relocated here so that it would run as part of the update-database migration command.

            // These variables use the extensible ApplicationUser class and the custom/extensible ApplicationRole class per the tutorial at: http://typecastexception.com/post/2014/06/22/ASPNET-Identity-20-Customizing-Users-and-Roles.aspx
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(userStore);
            var roleStore = new RoleStore<ApplicationRole>(context);
            var roleManager = new ApplicationRoleManager(roleStore);

            // set username and password for default admin user
            const string name = "admin@adminapps.nv.gov";
            const string password = "Admin@1234";

            // Initialize ApplicationRoles in db--these roles are used to control access to AdminApp's various modules via the [Authorize] attribute in the module's controller, as well as for showing/hiding access to these modules in the UI via conditional logic in the "Shared => _Layout.cshtml" View: "@if (Request.IsAuthenticated && User.IsInRole("RoleName"))"
            List<ApplicationRole> initRoles = new List<ApplicationRole> {
                // first role in list is default Administration Role for default user, used by function below when selecting initRoles[0].Name
                new ApplicationRole(){Name = "GlobalAdmin", AppModuleID = 2, AppModuleApprovalLevel = 1, AppModuleApprovalTitle = "GlobalAdmin", Description = "Highest level of administration; allows configuration of Global App Settings" },
                new ApplicationRole(){Name = "RolesAdmin",  AppModuleID = 3, AppModuleApprovalLevel = 1, AppModuleApprovalTitle = "RolesAdmin", Description = "Allows for User/Role administration" },
                new ApplicationRole(){Name = "UsersAdmin",  AppModuleID = 4, AppModuleApprovalLevel = 1, AppModuleApprovalTitle = "UsersAdmin", Description = "Allows for User administration, but not Role administration"},
                new ApplicationRole(){Name = "BillsClerc",  AppModuleID = 5, AppModuleApprovalLevel = 0, AppModuleApprovalTitle = "Clerical",   Description = "Bill Tracking module: Clerical access, including CRUD actions for Bill records"},
                new ApplicationRole(){Name = "BillsDept",   AppModuleID = 5, AppModuleApprovalLevel = 1, AppModuleApprovalTitle = "Department", Description = "Bill Tracking module: Department-level access, including all Reviews from Divisions within that Department"},
                new ApplicationRole(){Name = "BillsDiv",    AppModuleID = 5, AppModuleApprovalLevel = 2, AppModuleApprovalTitle = "Division",   Description = "Bill Tracking module: Division-level access"},
                new ApplicationRole(){Name = "BillsAgency", AppModuleID = 5, AppModuleApprovalLevel = 3, AppModuleApprovalTitle = "Agency",     Description = "Bill Tracking module: Agency-level access"},
                new ApplicationRole(){Name = "UserProjects", AppModuleID = 6, AppModuleApprovalLevel = 0, AppModuleApprovalTitle = "",     Description = "User Projects module: basic access"}
            };

            //Create ApplicationRole for each roleNames if it does not exist
            foreach (ApplicationRole initRole in initRoles)
            {
                var role = roleManager.FindByName(initRole.Name);
                if (role == null)
                {
                    var roleresult = roleManager.Create(initRole);
                }
            }

            // Create default admin@example.com user if it does not exist
            var user = userManager.FindByName(name);
            if (user == null)
            {
                user = new ApplicationUser { UserName = name, Email = name, FirstName = "Russell", LastName = "Cook", DeptID = 212516, DivID = 212568, AppModuleID = 1, IsActive = true, AutoPwdReplaced = true };
                var result = userManager.Create(user, password);
                result = userManager.SetLockoutEnabled(user.Id, false);
            }

            // Add user admin to default Administration Role if not already added.
            var rolesForUser = userManager.GetRoles(user.Id);
            if (!rolesForUser.Contains(initRoles[0].Name))
            {
                // adds default admin user to first role in list above
                var result = userManager.AddToRole(user.Id, initRoles[0].Name);
            }


            // Seed data for Project Management Module
            context.ProjectStatuses.AddOrUpdate(i => i.ID,
                new ProjectStatus { ID = 1, Description = "Pending" },
                new ProjectStatus { ID = 2, Description = "In-Progress" },
                new ProjectStatus { ID = 3, Description = "Completed" },
                new ProjectStatus { ID = 4, Description = "Deferred" },
                new ProjectStatus { ID = 5, Description = "Deferred (In-Progress)" },
                new ProjectStatus { ID = 6, Description = "Ongoing (Recurring)" },
                new ProjectStatus { ID = 7, Description = "Discontinued (Recurring)" }
            );

        }
    }
}
