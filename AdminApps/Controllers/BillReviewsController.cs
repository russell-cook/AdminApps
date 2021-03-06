﻿using AdminApps.DAL;
using AdminApps.Models;
using AdminApps.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Omu.ValueInjecter;
using PagedList;
using System;
using System.Collections.Generic;
//using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AdminApps.Controllers
{
    [Authorize(Roles = "BillsClerc, BillsDept, BillsDiv, BillsAgency")]
    public class BillReviewsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BillReviews
        public async Task<ActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page, int pageSize = 10)
        {
            // configure column sort parameters
            ViewBag.CurrentSort = sortOrder;
            ViewBag.BillSortParm = String.IsNullOrEmpty(sortOrder) ? "billNum_desc" : "";
            //ViewBag.CreatedBySortParm = sortOrder == "CreatedBy" ? "createdBy_desc" : "CreatedBy";
            //ViewBag.DivisionSortParm = sortOrder == "Division" ? "division_desc" : "Division";
            ViewBag.RecommendationSortParm = sortOrder == "Recommendation" ? "recommendation_desc" : "Recommendation";
            ViewBag.CreatedAtSortParm = sortOrder == "CreatedAt" ? "createdAt_desc" : "CreatedAt";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            // initialize query
            ApplicationUser user = await ReturnCurrentUserAsync();
            var billReviewsQry = db.BillReviews
                                .Where(r => r.ApplicationUserID == user.Id)
                                .Include(r => r.Bill)
                                .Include(r => r.CreatedByUser)
                                .Include(r => r.CreatedByUserInDiv)
                                .Include(r => r.Recommendation);

            // apply pre-query sort order
            switch (sortOrder)
            {
                case "billNum_desc":
                    billReviewsQry = billReviewsQry.OrderByDescending(r => r.Bill.BillPrefixID).ThenByDescending(r => r.Bill.Suffix);
                    break;
                case "CreatedAt":
                    billReviewsQry = billReviewsQry.OrderBy(r => r.CreatedAt);
                    break;
                case "createdAt_desc":
                    billReviewsQry = billReviewsQry.OrderByDescending(r => r.CreatedAt);
                    break;
                default:
                    billReviewsQry = billReviewsQry.OrderBy(r => r.Bill.BillPrefixID).ThenBy(r => r.Bill.Suffix);
                    break;
            }

            // execute query. The following filter/sort operations are performed on calculated properties 
            // in the result set and cannot be included in the LINQ query above
            var billReviewsQryResults = await billReviewsQry.ToListAsync();

            // remove any Bill Reviews which aren't the most recent version
            var billReviewsGrouping = billReviewsQryResults.GroupBy(r => r.BillID).ToList();
            var billReviewsList = new List<BillReview>();
            foreach (IGrouping<int, BillReview> group in billReviewsGrouping)
            {
                var mostRecentReview = group.OrderByDescending(g => g.BillVersion.VersionNum).FirstOrDefault();
                billReviewsList.Add(mostRecentReview);
            }

            // apply search string (if exists)
            if (!String.IsNullOrEmpty(searchString))
            {
                string capSearchString = searchString.ToUpper();
                foreach (BillReview r in billReviewsList.ToList())
                {
                    if (!r.Bill.CompositeBillNumber.Contains(capSearchString))
                    {
                        billReviewsList.Remove(r);
                    }
                }
            }

            // apply post-query sort order
            switch (sortOrder)
            {
                case "Division":
                    billReviewsList = billReviewsList.OrderBy(r => r.CreatedByUserInDiv.Description).ToList();
                    break;
                case "division_desc":
                    billReviewsList = billReviewsList.OrderByDescending(r => r.CreatedByUserInDiv.Description).ToList();
                    break;
                default:
                    break;
            }


            int pageNumber = (page ?? 1);
            PagedList<BillReview> viewModel = new PagedList<BillReview>(billReviewsList, pageNumber, pageSize);

            return View(viewModel);
        }

        // GET: BillReviews/Dept
        [Authorize(Roles = "BillsDept, BillsClerc")]
        public async Task<ActionResult> Dept(string sortOrder, string currentFilter, string searchString, int? page, int pageSize = 10)
        {
            // configure column sort parameters
            ViewBag.CurrentSort = sortOrder;
            ViewBag.BillSortParm = String.IsNullOrEmpty(sortOrder) ? "billNum_desc" : "";
            ViewBag.CreatedBySortParm = sortOrder == "CreatedBy" ? "createdBy_desc" : "CreatedBy";
            ViewBag.DivisionSortParm = sortOrder == "Division" ? "division_desc" : "Division";
            ViewBag.RecommendationSortParm = sortOrder == "Recommendation" ? "recommendation_desc" : "Recommendation";
            ViewBag.CreatedAtSortParm = sortOrder == "CreatedAt" ? "createdAt_desc" : "CreatedAt";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;


            // initialize query
            ApplicationUser user = await ReturnCurrentUserAsync();
            var billReviewsQry = db.BillReviews
                                .Where(r => r.DeptID == user.DeptID && r.Approvals.Where(a => a.ApprovalLevel == 2).Any())
                                .Include(r => r.Bill)
                                .Include(r => r.Recommendation);

            // apply pre-query sort order
            switch (sortOrder)
            {
                case "billNum_desc":
                    billReviewsQry = billReviewsQry.OrderByDescending(r => r.Bill.BillPrefixID).ThenByDescending(r => r.Bill.Suffix);
                    break;
                case "CreatedAt":
                    billReviewsQry = billReviewsQry.OrderBy(r => r.CreatedAt);
                    break;
                case "createdAt_desc":
                    billReviewsQry = billReviewsQry.OrderByDescending(r => r.CreatedAt);
                    break;
                default:
                    billReviewsQry = billReviewsQry.OrderBy(r => r.Bill.BillPrefixID).ThenBy(r => r.Bill.Suffix);
                    break;
            }

            // execute query. The following filter/sort operations are performed on calculated properties 
            // in the result set and cannot be included in the LINQ query above
            var billReviewsQryResults = await billReviewsQry.ToListAsync();

            // remove any Bill Reviews which aren't the most recent version
            var billReviewsGrouping = billReviewsQryResults.GroupBy(r => r.BillID).ToList();
            var billReviewsList = new List<BillReview>();
            foreach (IGrouping<int, BillReview> group in billReviewsGrouping)
            {
                var mostRecentReview = group.OrderByDescending(g => g.BillVersion.VersionNum).FirstOrDefault();
                billReviewsList.Add(mostRecentReview);
            }

            // apply search string (if exists)
            if (!String.IsNullOrEmpty(searchString))
            {
                string capSearchString = searchString.ToUpper();
                foreach (BillReview r in billReviewsList.ToList())
                {
                    if (!r.Bill.CompositeBillNumber.Contains(capSearchString))
                    {
                        billReviewsList.Remove(r);
                    }
                }
            }

            // apply post-query sort order
            switch (sortOrder)
            {
                case "CreatedBy":
                    billReviewsList = billReviewsList.OrderBy(r => r.CreatedByUser.LastName).ToList();
                    break;
                case "createdBy_desc":
                    billReviewsList = billReviewsList.OrderByDescending(r => r.CreatedByUser.LastName).ToList();
                    break;
                case "Division":
                    billReviewsList = billReviewsList.OrderBy(r => r.CreatedByUserInDiv.Description).ToList();
                    break;
                case "division_desc":
                    billReviewsList = billReviewsList.OrderByDescending(r => r.CreatedByUserInDiv.Description).ToList();
                    break;
                case "Recommendation":
                    billReviewsList = billReviewsList.OrderBy(r => r.Recommendation.Description).ToList();
                    break;
                case "recommendation_desc":
                    billReviewsList = billReviewsList.OrderByDescending(r => r.Recommendation.Description).ToList();
                    break;
                default:
                    break;
            }

            int pageNumber = (page ?? 1);
            PagedList<BillReview> viewModel = new PagedList<BillReview>(billReviewsList, pageNumber, pageSize);

            return View(viewModel);
        }

        // GET: BillReviews/Unread
        [Authorize(Roles = "BillsDept, BillsDiv")]
        public async Task<ActionResult> Unread(string sortOrder, string currentFilter, string searchString, int? page, int pageSize = 10)
        {
            // configure column sort parameters
            ViewBag.CurrentSort = sortOrder;
            ViewBag.BillSortParm = String.IsNullOrEmpty(sortOrder) ? "billNum_desc" : "";
            ViewBag.CreatedBySortParm = sortOrder == "CreatedBy" ? "createdBy_desc" : "CreatedBy";
            ViewBag.DivisionSortParm = sortOrder == "Division" ? "division_desc" : "Division";
            ViewBag.RecommendationSortParm = sortOrder == "Recommendation" ? "recommendation_desc" : "Recommendation";
            ViewBag.CreatedAtSortParm = sortOrder == "CreatedAt" ? "createdAt_desc" : "CreatedAt";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            // load user properties
            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            // initialize query
            var billReviewsQry =  Enumerable.Empty<BillReview>().AsQueryable();
            switch (userModuleApprovalLevel)
            {
                case 0:
                case 1:
                    billReviewsQry = db.BillReviews
                        .Where(r =>
                            r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Any() &&
                            r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault().IsRead == false &&
                            r.CreatedByUserInDept.ID == user.DeptID)
                        .Include(r => r.Bill)
                        .Include(r => r.Recommendation);
                    ViewBag.SubmittedByRole = "Division";
                    ViewBag.SubmittedForAgency = string.Format("Department ({0}):", user.Dept.CompositeDeptName);
                    break;
                case 2:
                    billReviewsQry = db.BillReviews
                        .Where(r =>
                            r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Any() &&
                            r.Notifications.Where(a => a.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault().IsRead == false &&
                            r.CreatedByUserInDiv.ID == user.DivID)
                        .Include(r => r.Bill)
                        .Include(r => r.Recommendation);
                    ViewBag.SubmittedByRole = "Agency";
                    ViewBag.SubmittedForAgency = string.Format("Division ({0}):", user.Div.CompositeDivName);
                    break;
                default:
                    break;

            }

            // apply pre-query sort order
            switch (sortOrder)
            {
                case "billNum_desc":
                    billReviewsQry = billReviewsQry.OrderByDescending(r => r.Bill.BillPrefixID).ThenByDescending(r => r.Bill.Suffix);
                    break;
                case "CreatedAt":
                    billReviewsQry = billReviewsQry.OrderBy(r => r.CreatedAt);
                    break;
                case "createdAt_desc":
                    billReviewsQry = billReviewsQry.OrderByDescending(r => r.CreatedAt);
                    break;
                default:
                    billReviewsQry = billReviewsQry.OrderBy(r => r.Bill.BillPrefixID).ThenBy(r => r.Bill.Suffix);
                    break;
            }

            // execute query. The following filter/sort operations are performed on calculated properties 
            // in the result set and cannot be included in the LINQ query above
            var billReviewsQryResults = await billReviewsQry.ToListAsync();

            // remove any Bill Reviews which aren't the most recent version
            var billReviewsGrouping = billReviewsQryResults.GroupBy(r => r.BillID).ToList();
            var billReviewsList = new List<BillReview>();
            foreach (IGrouping<int, BillReview> group in billReviewsGrouping)
            {
                var mostRecentUnreadReview = group.OrderByDescending(g => g.BillVersion.VersionNum).FirstOrDefault();
                var mostRecentReviewID = db.BillReviews.Where(r => r.BillID == mostRecentUnreadReview.BillID && r.ApplicationUserID == mostRecentUnreadReview.ApplicationUserID).OrderByDescending(r => r.BillVersion.VersionNum).FirstOrDefault().ID;
                if (mostRecentReviewID == mostRecentUnreadReview.ID)
                {
                    billReviewsList.Add(mostRecentUnreadReview);
                }
            }

            // apply search string (if exists)
            if (!String.IsNullOrEmpty(searchString))
            {
                string capSearchString = searchString.ToUpper();
                foreach (BillReview r in billReviewsList.ToList())
                {
                    if (!r.Bill.CompositeBillNumber.Contains(capSearchString))
                    {
                        billReviewsList.Remove(r);
                    }
                }
            }

            // apply post-query sort order
            switch (sortOrder)
            {
                case "CreatedBy":
                    billReviewsList = billReviewsQry.OrderBy(r => r.CreatedByUser.LastName).ToList();
                    break;
                case "createdBy_desc":
                    billReviewsList = billReviewsQry.OrderByDescending(r => r.CreatedByUser.LastName).ToList();
                    break;
                case "Division":
                    billReviewsList = billReviewsQry.OrderBy(r => r.CreatedByUserInDiv.Description).ToList();
                    break;
                case "division_desc":
                    billReviewsList = billReviewsQry.OrderByDescending(r => r.CreatedByUserInDiv.Description).ToList();
                    break;
                case "Recommendation":
                    billReviewsList = billReviewsQry.OrderBy(r => r.Recommendation.Description).ToList();
                    break;
                case "recommendation_desc":
                    billReviewsList = billReviewsQry.OrderByDescending(r => r.Recommendation.Description).ToList();
                    break;
                default:
                    break;
            }

            int pageNumber = (page ?? 1);
            PagedList<BillReview> viewModel = new PagedList<BillReview>(billReviewsList, pageNumber, pageSize);

            return View(viewModel);
        }

        // GET: BillReviews/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BillReview review = await db.BillReviews
                .Where(r => r.ID == id).
                Include("Bill.BillVersions").
                Include(v => v.BillVersion).
                Include(n => n.Notifications).
                FirstOrDefaultAsync();

            if (review == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            BillReviewViewModel viewModel = new BillReviewViewModel();
            viewModel.InjectFrom(review);

            // calculate BillReview status and set viewModel properties accordingly
            CalculateBillReviewStatus(review, viewModel, user,userModuleApprovalLevel);

            return View(viewModel);
        }

        private static void CalculateBillReviewStatus(BillReview review, BillReviewViewModel viewModel, ApplicationUser user, int userModuleApprovalLevel)
        {
            var currentVersion = review.Bill.BillVersions.OrderByDescending(v => v.VersionNum).FirstOrDefault();
            viewModel.CurrentBillVersion = currentVersion.VersionDescription;

            // for Dept and Div users, if review was sent up from lower approval level, calculate whether BillReviewNotification has been read.
            // otherwise, set DisplayAsRead to true (this hides the 'mark as read' button)
            if (userModuleApprovalLevel < review.CreatedAtApprovalLevel && userModuleApprovalLevel != 0)
            {
                viewModel.DisplayAsRead = review.Notifications.Where(n => n.ApprovalLevel == userModuleApprovalLevel).Single().IsRead;
            }
            else
            {
                viewModel.DisplayAsRead = true;
            }

            // if user created this BillReview, calculate whether or not user can edit. 
            if (user.Id == review.ApplicationUserID)
            {
                // if user is not Dept level user, determine whether the Review's notificaiton has been read
                if (userModuleApprovalLevel != 1)
                {
                    var notification = review.Notifications.Where(n => n.ApprovalLevel == userModuleApprovalLevel - 1).FirstOrDefault();
                    if (notification != null)
                    {
                        if (!notification.IsRead)
                        {
                            viewModel.UserCanEdit = true;
                        }
                        else
                        {
                            switch (userModuleApprovalLevel)
                            {
                                case 2:
                                    viewModel.EditMessage = string.Format("This review has already been received at the Department level and can no longer be edited.");
                                    break;
                                case 3:
                                    viewModel.EditMessage = string.Format("This review has already been received at the Division level and can no longer be edited.");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        viewModel.UserCanEdit = true;
                    }
                }
                // otherwise, Dept level users have no restrictions on editing their own Reviews
                else
                {
                    viewModel.UserCanEdit = true;
                }
            }

            // determine whether user can approve
            var approvedReview = new BillReview();
            switch (userModuleApprovalLevel)
            {
                // BillsClerc users cannot approve BillReviews
                case 0:
                    approvedReview = review.Bill.Reviews
                        .Where(r =>
                            r.Approvals.Where(a => a.ApprovalLevel == 1).Any() &&
                            r.CreatedByUserInDept.ID == user.DeptID)
                        .FirstOrDefault();
                    if (approvedReview != null)
                    {
                        if (approvedReview.ID == review.ID)
                        {
                            viewModel.ApprovedReviewMessage = string.Format("This is the Approved Review for {0}.", approvedReview.CreatedByUserInDept.CompositeDeptName);
                        }
                    }
                    viewModel.UserCanApprove = false;
                    break;
                // BillsDept users can approve a BillReview if the current BillReview is not already the approved review for its Bill
                case 1:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == 1).Any() && r.CreatedByUserInDept.ID == user.DeptID)
                        .FirstOrDefault();
                    if (approvedReview != null)
                    {
                        if (approvedReview.ID == review.ID)
                        {
                            viewModel.ApprovedReviewMessage = string.Format("This is the Approved Review for {0}.", approvedReview.CreatedByUserInDept.CompositeDeptName);
                            viewModel.UserCanApprove = false;
                        }
                        else
                        {
                            viewModel.UserCanApprove = true;
                        }
                    }
                    else
                    {
                        viewModel.UserCanApprove = true;
                    }
                    break;
                // BillsDiv users can approve a BillReview if the current BillReview is not already the approved review,
                // but only if the existing (alternate) Approved Review has not been MarkedAsRead at the Dept level
                case 2:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == 2).Any() && r.CreatedByUserInDiv.ID == user.DivID)
                        .FirstOrDefault();
                    if (approvedReview != null)
                    {
                        if (approvedReview.ID == review.ID)
                        {
                            viewModel.ApprovedReviewMessage = string.Format("This is the Approved Review for {0}.", approvedReview.CreatedByUserInDiv.CompositeDivName);
                            viewModel.UserCanApprove = false;
                        }
                        else if (approvedReview.Notifications.Where(n => n.ApprovalLevel == 1).FirstOrDefault().IsRead)
                        {
                            viewModel.UserCanApprove = false;
                        }
                        else
                        {
                            viewModel.UserCanApprove = true;
                        }
                    }
                    else
                    {
                        viewModel.UserCanApprove = true;
                    }
                    break;
                // BillsAgency users cannot approve BillReviews
                case 3:
                    viewModel.UserCanApprove = false;
                    break;
                default:
                    break;
            }


            // calculate version status
            if (review.BillVersion.VersionNum == currentVersion.VersionNum)
            {
                viewModel.UpToDate = true;
                viewModel.StatusMessage = "Up to date";
            }
            else
            {
                viewModel.UpToDate = false;
                viewModel.StatusMessage = "Out of date";
                if (review.CreatedByUser.Id == user.Id)
                {
                    viewModel.UserCanUpdate = true;
                }
            }
        }

        // GET: BillReviews/Create/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Bill parentBill = await db.Bills
                                        .Where(b => b.ID == id)
                                        .Include(b => b.BillVersions)
                                        .FirstOrDefaultAsync();
            ViewBag.ParentID = parentBill.CompositeBillName;
            ViewBag.ParentNelisLink = parentBill.calculatedHyperlink;

            BillReviewViewModel viewModel = new BillReviewViewModel();
            viewModel.Create = true;
            viewModel.UpToDate = true;
            viewModel.BillID = id.Value;
            viewModel.BillVersion = parentBill.BillVersions.OrderByDescending(v => v.VersionNum).FirstOrDefault();
            viewModel.BillVersionID = viewModel.BillVersion.ID;
            ViewBag.BillReviewRecommendationID = new SelectList(await db.BillReviewRecommendations.ToListAsync(), "ID", "Description");
            return View(viewModel);
        }

        // POST: BillReviews/Create/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "ID,BillID,BillVersionID,BillReviewRecommendationID,Comments,RequiresTestimony,InformationToBeProvided,ActivelyTracking,PolicyImpact,FiscalImpactYr1,FiscalImpactYr2,FiscalImpactFuture,FiscalNoteSubmitted,Notes")] BillReview review)
        public async Task<ActionResult> Create(BillReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await ReturnCurrentUserAsync();
                var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

                BillReview review = new BillReview();
                review.InjectFrom(model);

                review.ApplicationUserID = user.Id;
                review.DeptID = user.DeptID;
                review.DivID = user.DivID;
                review.CreatedAtApprovalLevel = userModuleApprovalLevel;
                review.CreatedAt = DateTime.Now;
                db.BillReviews.Add(review);
                await db.SaveChangesAsync();

                // for Agency users, create Notification for Div level, and check to see if Review was created in response to a Request
                if (userModuleApprovalLevel == 3)
                {
                    BillReviewNotification notification = new BillReviewNotification();
                    notification.BillReviewID = review.ID;
                    notification.ApprovalLevel = userModuleApprovalLevel - 1;
                    notification.IsRead = false;
                    db.BillReviewNotifications.Add(notification);

                    // if review was created in response to a Request, mark the Request as fulfilled
                    var request = await db.BillReviewRequests.Where(r => r.BillID == review.BillID && r.RequestedToUserID == user.Id).FirstOrDefaultAsync();
                    if (request != null)
                    {
                        request.Fulfilled = true;
                    }

                    await db.SaveChangesAsync();
                }

                Success("Bill Review submitted successfully.");
                return RedirectToAction("Details", "Bills", new { id = review.BillID } );
            }

            //ViewBag.BillID = new SelectList(db.Bills, "ID", "NebsBdrNumber", review.BillID);
            ViewBag.BillReviewRecommendationID = new SelectList(db.BillReviewRecommendations, "ID", "Description", model.BillReviewRecommendationID);
            return View(model);
        }

        // GET: BillReviews/Edit/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillReview review = await db.BillReviews
                                        .Where(r => r.ID == id)
                                        .Include(r => r.BillVersion)
                                        .Include("Bill.BillVersions")
                                        .FirstOrDefaultAsync();
            if (review == null)
            {
                return HttpNotFound();
            }
            BillReviewViewModel viewModel = new BillReviewViewModel();
            viewModel.InjectFrom(review);

            // ensure BillReview is up-to-date with current BillVersion
            var currentVersionID = review.Bill.BillVersions.OrderByDescending(v => v.VersionNum).FirstOrDefault().ID;
            if (review.BillVersionID == currentVersionID)
            {
                viewModel.UpToDate = true;
                ViewBag.BillReviewRecommendationID = new SelectList(db.BillReviewRecommendations, "ID", "Description", review.BillReviewRecommendationID);
                return View(viewModel);
            }

            Danger("Only up-to-date Bill Reviews can be edited. To modify your Bill Review please click the 'Update!' button below.");
            return RedirectToAction("Details", new { id = review.ID });
        }

        // POST: BillReviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "ID,BillID,BillVersionID,BillReviewRecommendationID,Comments,RequiresTestimony,InformationToBeProvided,ActivelyTracking,PolicyImpact,FiscalImpactYr1,FiscalImpactYr2,FiscalImpactFuture,FiscalNoteSubmitted,Notes,CreatedAtApprovalLevel,CreatedAt,ApplicationUserID,DeptID,DivID,RowVersion")] BillReview review)
        public async Task<ActionResult> Edit(BillReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(review).State = EntityState.Modified;
                BillReview review = await db.BillReviews
                                            .Where(r => r.ID == model.ID)
                                            .FirstOrDefaultAsync();
                review.InjectFrom(model);
                await db.SaveChangesAsync();
                return RedirectToAction("Details", new { id = model.ID });
            }
            ViewBag.BillReviewRecommendationID = new SelectList(db.BillReviewRecommendations, "ID", "Description", model.BillReviewRecommendationID);
            return View(model);
        }

        // GET: BillReviews/Delete/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillReview review = await db.BillReviews.FindAsync(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // POST: BillReviews/Delete/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            BillReview review = await db.BillReviews.FindAsync(id);
            db.BillReviews.Remove(review);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: BillReviews/Update/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        public async Task<ActionResult> Update(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // load BillReview; Includes are used by CalculateBillReviewStatus
            BillReview review = await db.BillReviews
                .Where(r => r.ID == id)
                .Include("Bill.BillVersions")
                .Include(r => r.BillVersion)
                .Include(r => r.Notifications)
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            // verify that current user created the BillReview
            if (review.ApplicationUserID != user.Id)
            {
                Danger("Error: A Bill Review can only be updated by the user who created the review.", true);
                return RedirectToAction("Index");
            }

            BillReviewViewModel viewModel = new BillReviewViewModel();

            // calculate BillReview status and set viewModel properties accordingly
            CalculateBillReviewStatus(review, viewModel, user, userModuleApprovalLevel);

            viewModel.InjectFrom(review);
            viewModel.Bill.InjectFrom(review.Bill);

            return View(viewModel);
        }

        // POST: BillReviews/Update/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int id)
        {
            BillReview review = await db.BillReviews
                                        .Where(r => r.ID == id)
                                        .Include("Bill.BillVersions")
                                        .Include("Approvals")
                                        .FirstOrDefaultAsync();
            if (review == null)
            {
                return HttpNotFound();
            }

            var currentVersionID = review.Bill.BillVersions.OrderByDescending(v => v.VersionNum).FirstOrDefault().ID;

            // create VerifiedDupOfPrevReview record
            BillReview newReview = new BillReview()
            {
                ActivelyTracking = review.ActivelyTracking,
                ApplicationUserID = review.ApplicationUserID,
                BillID = review.BillID,
                BillReviewRecommendationID = review.BillReviewRecommendationID,
                BillVersionID = currentVersionID,
                Comments = review.Comments,
                CreatedAt = DateTime.Now,
                CreatedAtApprovalLevel = review.CreatedAtApprovalLevel,
                DeptID = review.DeptID,
                DivID = review.DivID,
                FiscalImpactFuture = review.FiscalImpactFuture,
                FiscalImpactYr1 = review.FiscalImpactYr1,
                FiscalImpactYr2 = review.FiscalImpactYr2,
                FiscalNoteSubmitted = review.FiscalNoteSubmitted,
                InformationToBeProvided = review.InformationToBeProvided,
                IsVerifiedDupOfPrevReview = true,
                PolicyImpact = review.PolicyImpact,
                PrevReviewID = review.ID,
                RequiresTestimony = review.RequiresTestimony
            };
            db.BillReviews.Add(newReview);

            // if review was Approved, create new Approval record(s) for VerifiedDup
            var prevApprovals = review.Approvals.ToList();
            if (prevApprovals.Any())
            {
                foreach (BillReviewApproval a in prevApprovals)
                {
                    var newApproval = new BillReviewApproval()
                    {
                        ApplicationUserID = a.ApplicationUserID,
                        BillReviewID = newReview.ID,
                        ApprovalLevel = a.ApprovalLevel,
                        ApprovedAt = a.ApprovedAt
                    };
                    db.BillReviewApprovals.Add(newApproval);
                }
            }

            // check for attached Approval Notifications; if one or more exists--and it's been marked as Read--create new record(s) for newApproval
            var prevNotifications = review.Notifications.ToList();
            if (prevNotifications.Any())
            {
                foreach (BillReviewNotification n in prevNotifications)
                {
                    var newNotification = new BillReviewNotification()
                    {
                        ApplicationUserID = n.ApplicationUserID,
                        BillReviewID = newReview.ID,
                        ApprovalLevel = n.ApprovalLevel,
                        IsRead = n.IsRead,
                        ReadAt = n.ReadAt
                    };
                    db.BillReviewNotifications.Add(newNotification);
                }
            }

            await db.SaveChangesAsync();

            Success("Your previous Bill Review has been verified as accurate. The current version is displayed below.");
            return RedirectToAction("Details", new { id = newReview.ID });
        }

        // GET: BillReviews/Revise/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        public async Task<ActionResult> Revise(int? id, bool isOverride = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // load BillReview; Includes are used by CalculateBillReviewStatus
            BillReview review = await db.BillReviews
                .Where(r => r.ID == id)
                .Include("Bill.BillVersions")
                .Include(r => r.BillVersion)
                .Include(r => r.Notifications)
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return HttpNotFound();
            }

            BillReviewViewModel viewModel = new BillReviewViewModel();
            viewModel.InjectFrom(review);

            if (isOverride)
            {
                viewModel.IsOverrideRevision = true;
            }

            ViewBag.BillReviewRecommendationID = new SelectList(db.BillReviewRecommendations, "ID", "Description", review.BillReviewRecommendationID);
            return View(viewModel);
        }

        // POST: BillReviews/Revise/5
        [Authorize(Roles = "BillsDept, BillsDiv, BillsAgency")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Revise(BillReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                // determine current BillVersion
                var bill = await db.Bills.Where(b => b.ID == model.BillID).FirstOrDefaultAsync();
                var currentVersion = bill.BillVersions.OrderByDescending(v => v.VersionNum).FirstOrDefault();

                // create RevisionOfPrevReview record
                BillReview newReview = new BillReview()
                {
                    ActivelyTracking = model.ActivelyTracking,
                    ApplicationUserID = model.ApplicationUserID,
                    BillID = model.BillID,
                    BillReviewRecommendationID = model.BillReviewRecommendationID,
                    BillVersionID = currentVersion.ID,
                    Comments = model.Comments,
                    CreatedAt = DateTime.Now,
                    CreatedAtApprovalLevel = model.CreatedAtApprovalLevel,
                    DeptID = model.DeptID,
                    DivID = model.DivID,
                    FiscalImpactFuture = model.FiscalImpactFuture,
                    FiscalImpactYr1 = model.FiscalImpactYr1,
                    FiscalImpactYr2 = model.FiscalImpactYr2,
                    FiscalNoteSubmitted = model.FiscalNoteSubmitted,
                    InformationToBeProvided = model.InformationToBeProvided,
                    IsRevisionOfPrevReview = true,
                    Notes = model.Notes,
                    PolicyImpact = model.PolicyImpact,
                    PrevReviewID = model.ID,
                    RequiresTestimony = model.RequiresTestimony
                };
                db.BillReviews.Add(newReview);

                // load user properties
                ApplicationUser user = await ReturnCurrentUserAsync();
                var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

                switch (userModuleApprovalLevel)
                {
                    // when a Dept user revises an Approved Review, create new Approval record
                    case 1:
                        var prevDeptApproval = db.BillReviews
                                                .Where(r => r.ID == model.ID)
                                                .FirstOrDefault()
                                                    .Approvals
                                                    .Where(approval => approval.ApprovalLevel == userModuleApprovalLevel)
                                                .FirstOrDefault();

                        if (prevDeptApproval != null)
                        {
                            // create new Dept-level Approval record
                            CreateRevisedBillReviewApprovalRecord(newReview, prevDeptApproval);

                            // create new BillReviewNotification
                            CreateRevisedBillReviewNotification(newReview, userModuleApprovalLevel);

                            // create email notification(s) for Dept users
                            await SendRevisedBillReviewEmailNotificationAsync(model, currentVersion, newReview, userModuleApprovalLevel, user);
                        }

                        break;
                    // when a Div user revises an Approved Review, create new Approval record, 
                    // and create notification record(s) and email notification(s) for Dept users
                    case 2:
                        var prevDivApproval = db.BillReviews
                                                .Where(r => r.ID == model.ID)
                                                .FirstOrDefault()
                                                    .Approvals
                                                    .Where(approval => approval.ApprovalLevel == userModuleApprovalLevel)
                                                .FirstOrDefault();

                        if (prevDivApproval != null)
                        {
                            // create new Div-level Approval record
                            CreateRevisedBillReviewApprovalRecord(newReview, prevDivApproval);

                            // create new BillReviewNotification
                            CreateRevisedBillReviewNotification(newReview, userModuleApprovalLevel);

                            // create email notification(s) for Dept users
                            await SendRevisedBillReviewEmailNotificationAsync(model, currentVersion, newReview, userModuleApprovalLevel, user);
                        }

                        break;
                    // when an Agency user revises a review, send notification to Div users
                    case 3:
                        CreateRevisedBillReviewNotification(newReview, userModuleApprovalLevel);
                        await SendRevisedBillReviewEmailNotificationAsync(model, currentVersion, newReview, userModuleApprovalLevel, user);
                        break;
                    default:
                        break;
                }

                await db.SaveChangesAsync();
                Success("Your Bill Review has been updated successfully.");
                return RedirectToAction("Details", new { id = newReview.ID });
            }
            ViewBag.BillReviewRecommendationID = new SelectList(db.BillReviewRecommendations, "ID", "Description", model.BillReviewRecommendationID);
            return View(model);
        }

        private void CreateRevisedBillReviewApprovalRecord(BillReview review, BillReviewApproval prevApproval)
        {
            var newApproval = new BillReviewApproval()
            {
                ApplicationUserID = prevApproval.ApplicationUserID,
                BillReviewID = review.ID,
                ApprovalLevel = prevApproval.ApprovalLevel,
                ApprovedAt = prevApproval.ApprovedAt
            };
            db.BillReviewApprovals.Add(newApproval);
        }

        private async Task SendRevisedBillReviewEmailNotificationAsync(BillReviewViewModel model, BillVersion currentVersion, BillReview review, int userModuleApprovalLevel, ApplicationUser user)
        {
            string templateName = "";
            string targetRole = "";
            switch (userModuleApprovalLevel)
            {
                case 1:
                    templateName = "BillReviewDeptRevisionNotification.html";
                    targetRole = "BillsDept";
                    break;
                case 2:
                    templateName = "BillReviewDivRevisionNotification.html";
                    targetRole = "BillsDept";
                    break;
                case 3:
                    templateName = "BillReviewAgencyRevisionNotification.html";
                    targetRole = "BillsDiv";
                    break;
                default:
                    break;
            }
            string body = "";
            using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + templateName))
            {
                body = await sr.ReadToEndAsync();
            }
            string messageSubject = string.Format("AdminApps Bill Tracking: Notice of Revised Bill Review for {0}: {1}", review.Bill.CompositeBillNumber, currentVersion.VersionDescription);
            string domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            string viewReviewUrl = string.Format("{0}/BillReviews/Details/{1}", domain, model.ID);

            var targetUsers = GetUsersInRoleAndDept(targetRole, review.DeptID);
            // for Dept level users, omit current user from email notification
            if (userModuleApprovalLevel == 1)
            {
                targetUsers = targetUsers.Where(x => x.Id != user.Id).ToList();
            }
            foreach (ApplicationUser u in targetUsers)
            {
                string messageBody = string.Format(body, u.FirstName, review.Bill.calculatedHyperlink, review.Bill.CompositeBillNumber, user.FullName, user.Div.CompositeDivName, viewReviewUrl, currentVersion.VersionDescription);
                await UserManager.SendEmailAsync(u.Id, messageSubject, messageBody);
            }
        }

        private void CreateRevisedBillReviewNotification(BillReview review, int userModuleApprovalLevel)
        {
            BillReviewNotification notification = new BillReviewNotification();
            notification.BillReviewID = review.ID;
            notification.ApprovalLevel = userModuleApprovalLevel - 1;
            notification.IsRead = false;
            db.BillReviewNotifications.Add(notification);
        }


        // GET: BillReviews/Approve/5
        [Authorize(Roles = "BillsDept, BillsDiv")]
        public async Task<ActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillReview review = await db.BillReviews
                                        .Where(r => r.ID == id)
                                        .Include("Bill.BillVersions")
                                        .FirstOrDefaultAsync();
            if (review == null)
            {
                return HttpNotFound();
            }

            BillReviewApprovalViewModel viewModel = new BillReviewApprovalViewModel();
            viewModel.ReviewToApprove.InjectFrom(review);

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            // calculate BillReview status; if BillReview is out of date then determine whether user can Update
            CalculateBillReviewStatus(review, viewModel.ReviewToApprove, user, userModuleApprovalLevel);

            // check for previously approved Review
            var approvedReview = new BillReview();
            switch (userModuleApprovalLevel)
            {
                case 1:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Count() > 0 && r.CreatedByUserInDept.ID == user.DeptID)
                        .FirstOrDefault();
                    ViewBag.ApprovedReviewAgency = user.Dept.CompositeDeptName;
                    break;
                case 2:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Count() > 0 && r.CreatedByUserInDiv.ID == user.DivID)
                        .FirstOrDefault();
                    ViewBag.ApprovedReviewAgency = user.Div.CompositeDivName;
                    break;
                default:
                    break;
            }

            if (approvedReview != null)
            {
                viewModel.ApprovedReview.InjectFrom(approvedReview);
                BillReviewApproval approval = approvedReview.Approvals
                    .Where(r => r.ApprovalLevel == userModuleApprovalLevel)
                    .FirstOrDefault();
                viewModel.ApprovedReview.ApprovedByUser = approval.ApprovedBy;
                viewModel.ApprovedReview.ApprovedAt = approval.ApprovedAt;
            }

            return View(viewModel);
        }

        // POST: BillReviews/Approve/5
        [Authorize(Roles = "BillsDept, BillsDiv")]
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveConfirmed(int id)
        {
            BillReview review = await db.BillReviews
                                        .Where(r => r.ID == id)
                                        .Include(r => r.Bill)
                                        .Include(r => r.CreatedByUser)
                                        .Include(r => r.CreatedByUserInDept)
                                        .Include(r => r.CreatedByUserInDiv)
                                        .FirstOrDefaultAsync();
            if (review == null)
            {
                return HttpNotFound();
            }

            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            // if the review had not been previously marked as read, mark it read
            var previousNotification = review.Notifications.Where(n => n.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault();
            if (previousNotification != null)
            {
                previousNotification.IsRead = true;
            }

            // check if the Bill already had an approved Review.
            var approvedReview = new BillReview();
            switch (userModuleApprovalLevel)
            {
                case 1:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Count() > 0 && r.CreatedByUserInDept.ID == user.DeptID)
                        .FirstOrDefault();
                    //ViewBag.ApprovedReviewAgency = user.Dept.CompositeDeptName;
                    break;
                case 2:
                    approvedReview = review.Bill.Reviews
                        .Where(r => r.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).Count() > 0 && r.CreatedByUserInDiv.ID == user.DivID)
                        .FirstOrDefault();
                    //ViewBag.ApprovedReviewAgency = user.Div.CompositeDivName;
                    break;
                default:
                    break;
            }

            // if an approved Review existed, remove the previous approval
            if (approvedReview != null)
            {
                BillReviewApproval approvalToRemove = approvedReview.Approvals.Where(a => a.ApprovalLevel == userModuleApprovalLevel).FirstOrDefault();
                db.BillReviewApprovals.Remove(approvalToRemove);
            }

            // create new approval for Review
            BillReviewApproval approval = new BillReviewApproval();
            approval.ApplicationUserID = user.Id;
            approval.BillReviewID = review.ID;
            approval.ApprovalLevel = userModuleApprovalLevel;
            approval.ApprovedAt = DateTime.Now;
            db.BillReviewApprovals.Add(approval);

            // check if this Review was created in response to a Request. If so, mark all Request from that user's Division as fulfilled 
            var requests = await db.BillReviewRequests.Where(r => r.BillID == review.BillID && r.RequestedToUser.DivID == user.DivID).ToListAsync();
            if (requests.Any())
            {
                foreach (BillReviewRequest r in requests)
                {
                    r.Fulfilled = true;
                    r.FulfilledAt = DateTime.Now;
                    r.BillReviewID = review.ID;
                }
            }

            // when a Div user approves a Review at the Div level, create a notification for the Dept level
            string body = ""; //initialize string for email body
            if (userModuleApprovalLevel == 2)
            {
                BillReviewNotification notification = new BillReviewNotification();
                notification.BillReviewID = review.ID;
                notification.ApprovalLevel = userModuleApprovalLevel - 1;
                notification.IsRead = false;
                db.BillReviewNotifications.Add(notification);

                // create email notification
                using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "BillReviewDivApprovalNotification.html"))
                {
                    body = await sr.ReadToEndAsync();
                }
                string messageSubject = string.Format("AdminApps Bill Tracking: A Review of {0} has been submitted on behalf of {1}", review.Bill.CompositeBillNumber, review.CreatedByUserInDiv.CompositeDivName);
                string domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                string viewReviewUrl = string.Format("{0}/BillReviews/Details/{1}", domain, review.ID);

                var targetUsers = GetUsersInRoleAndDept("BillsDept", review.DeptID);
                foreach (ApplicationUser u in targetUsers)
                {
                    string messageBody = string.Format(body, u.FirstName, review.Bill.calculatedHyperlink, review.Bill.CompositeBillNumber, review.CreatedByUser.FullName, review.CreatedByUserInDiv.CompositeDivName, viewReviewUrl);
                    await UserManager.SendEmailAsync(u.Id, messageSubject, messageBody);
                }
            }

            // save changes
            await db.SaveChangesAsync();

            Success(string.Format("Bill Review successfully submitted to {0}", review.CreatedByUserInDept.CompositeDeptName));
            return RedirectToAction("Details", "Bills", new { id = approval.BillReview.BillID });

        }

        // GET: BillReviews/MarkAsRead/5
        [Authorize(Roles = "BillsDept, BillsDiv")]
        public async Task<ActionResult> MarkAsRead(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillReview review = await db.BillReviews
                                        .Where(r => r.ID == id)
                                        .Include("Bill.BillVersions")
                                        .FirstOrDefaultAsync();
            if (review == null)
            {
                return HttpNotFound();
            }
            BillReviewViewModel viewModel = new BillReviewViewModel();
            viewModel.InjectFrom(review);


            // calculate BillReview status
            var currentVersion = review.Bill.BillVersions.OrderByDescending(v => v.VersionNum).FirstOrDefault();
            viewModel.CurrentBillVersion = currentVersion.VersionDescription;
            if (review.BillVersion.VersionNum == currentVersion.VersionNum)
            {
                viewModel.UpToDate = true;
                viewModel.StatusMessage = "Up to date";
            }
            else
            {
                viewModel.UpToDate = false;
                viewModel.StatusMessage = "Out of date";
            }

            return View(viewModel);
        }

        // POST: BillReviews/MarkAsRead/5
        [Authorize(Roles = "BillsDept, BillsDiv")]
        [HttpPost, ActionName("MarkAsRead")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsReadConfirmed(int id)
        {
            ApplicationUser user = await ReturnCurrentUserAsync();
            var userModuleApprovalLevel = await ReturnUserModuleApprovalLevelAsync(user, 5);

            BillReviewNotification notification = await db.BillReviewNotifications.Where(n => n.BillReviewID == id && n.ApprovalLevel == userModuleApprovalLevel).SingleAsync();
            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            notification.ApplicationUserID = user.Id;

            db.Entry(notification).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Bills", new { id = notification.BillReview.BillID });

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // This function was intended to get Ann Wilkinson caught up on email notifications, which had not been implemented yet as of 3/18/15
        // public async Task<ActionResult> Catchup()
        // {
        //     var approvals = await db.BillReviewApprovals
        //                             .Where(a => a.ApprovalLevel == 2)
        //                             .Include("BillReview.Bill")
        //                             .Include("BillReview.CreatedByUserInDiv")
        //                             .ToListAsync();
        //     string body = "";
        //     using (var sr = new StreamReader(Server.MapPath("\\Templates\\") + "BillReviewDivApprovalNotification.html"))
        //     {
        //         body = await sr.ReadToEndAsync();
        //     }
        //     var targetUsers = GetUsersInRole("BillsDept");
        //     foreach (ApplicationUser u in targetUsers)
        //     {
        //         foreach (BillReviewApproval a in approvals)
        //         {
        //             string messageSubject = string.Format("AdminApps Bill Tracking: A Review of {0} has been submitted on behalf of {1}", a.BillReview.Bill.CompositeBillNumber, a.BillReview.CreatedByUserInDiv.CompositeDivName);
        //             string viewReviewUrl = string.Format("http://adminapps.nv.gov/BillReviews/Details/{0}", a.BillReview.ID);
        //             string messageBody = string.Format(body, u.FirstName, a.BillReview.Bill.NelisHyperlink, a.BillReview.Bill.CompositeBillNumber, a.BillReview.CreatedByName, a.BillReview.CreatedByUserInDiv.CompositeDivName, viewReviewUrl);
        //             await UserManager.SendEmailAsync(u.Id, messageSubject, messageBody);
        //         }
        //     }
        //     Success(string.Format("{0} email notifications sent", approvals.Count()));
        //     return RedirectToAction("Index");
        //}


    }
}
