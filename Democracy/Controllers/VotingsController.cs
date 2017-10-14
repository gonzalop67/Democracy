﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Democracy.Models;

namespace Democracy.Controllers
{
    public class VotingsController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        [Authorize(Roles = "User")]
        public ActionResult Vote(int votingId)
        {
            var voting = db.Votings.Find(votingId);
            
            var view = new VotingVoteView
            {
                DateTimeEnd = voting.DateTimeEnd,
                DateTimeStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnabledBlankVotes = voting.IsEnabledBlankVotes,
                IsForAllUsers = voting.IsForAllUsers,
                MyCandidates = voting.Candidates.ToList(),
                Remarks = voting.Remarks,
                VotingId = voting.VotingId
            };

            return View(view);
        }

        [Authorize(Roles = "User")]
        public ActionResult MyVotings()
        {
            var user = db.Users.Where(u => u.UserName == this.User.Identity.Name).FirstOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "There an error with the current user, call the suport.");
                return View();
            }

            // Get event votings for the correct time
            var state = this.GetState("Open");

            var votings = db.Votings.Where(v => v.StateId == state.StateId &&
                                                v.DateTimeStart <= DateTime.Now &&
                                                v.DateTimeEnd >= DateTime.Now)
                                    .Include(v => v.Candidates)
                                    .Include(v => v.VotingGroups)
                                    .Include(v => v.State)
                                    .ToList();

            // Discard events in the wich the user already vote
            for (int i = 0; i < votings.Count; i++)
            {
                int userId = user.UserId;
                int votingId = votings[i].VotingId;

                var votingDetail = db.VotingDetails.Where(vd => vd.VotingId == votingId && vd.UserId == userId).FirstOrDefault();

                if (votingDetail != null)
                {
                    votings.RemoveAt(i);
                }
            }

            // Discard events by groups in wich the user are not included
            for (int i = 0; i < votings.Count; i++)
            {
                if (!votings[i].IsForAllUsers)
                {
                    bool userBelongsToGroup = false;

                    foreach (var votingGroup in votings[i].VotingGroups)
                    {
                        var userGroup = votingGroup.Group.GroupMembers
                            .Where(gm => gm.UserId == user.UserId)
                            .FirstOrDefault();

                        if (userGroup != null)
                        {
                            userBelongsToGroup = true;
                            break;
                        }

                    }

                    if (!userBelongsToGroup)
                    {
                        votings.RemoveAt(i);
                    }
                }
            }


            return View(votings);
        }

        private State GetState(string stateName)
        {
            var state = db.States.Where(s => s.Description == stateName).FirstOrDefault();

            if (state == null)
            {
                state = new State
                {
                    Description = stateName,
                };

                db.States.Add(state);
                db.SaveChanges();
            }

            return state;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteGroup(int id)
        {
            var votingGroup = db.VotingGroups.Find(id);
            if (votingGroup != null)
            {
                db.VotingGroups.Remove(votingGroup);
                db.SaveChanges();
            }

            return RedirectToAction(string.Format("Details/{0}", votingGroup.VotingId));
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCandidate(int id)
        {
            var candidate = db.Candidates.Find(id);
            if (candidate != null)
            {
                db.Candidates.Remove(candidate);
                db.SaveChanges();
            }

            return RedirectToAction(string.Format("Details/{0}", candidate.VotingId));
        }

        [HttpPost]
        public ActionResult AddCandidate(AddCandidateView view)
        {
            if (ModelState.IsValid)
            {
                var candidate = db.Candidates.Where(c => c.VotingId == view.VotingId && c.UserId == view.UserId).FirstOrDefault();

                if (candidate != null)
                {
                    ModelState.AddModelError(string.Empty, "The candidate already belongs to voting.");
                    ViewBag.UserId = new SelectList(db.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName), "UserId", "FullName");
                    return View(view);
                }

                candidate = new Candidate
                {
                    UserId = view.UserId,
                    VotingId = view.VotingId
                };

                db.Candidates.Add(candidate);
                db.SaveChanges();

                return RedirectToAction(string.Format("Details/{0}", view.VotingId));
            }

            ViewBag.UserId = new SelectList(db.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName), "UserId", "FullName");
            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddCandidate(int id)
        {
            var view = new AddCandidateView
            {
                VotingId = id,
            };

            ViewBag.UserId = new SelectList(db.Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName), "UserId", "FullName");
            return View(view);
        }

        [HttpPost]
        public ActionResult AddGroup(AddGroupView view)
        {
            if (ModelState.IsValid)
            {
                var votingGroup = db.VotingGroups.Where(vg => vg.VotingId == view.VotingId && vg.GroupId == view.GroupId).FirstOrDefault();
                
                if (votingGroup != null)
                {
                    ModelState.AddModelError(string.Empty, "The group already belongs to voting.");
                    ViewBag.GroupId = new SelectList(db.Groups.OrderBy(g => g.Description), "GroupId", "Description");
                    return View(view);
                }
                
                votingGroup = new VotingGroup
                {
                    GroupId = view.GroupId,
                    VotingId = view.VotingId
                };

                db.VotingGroups.Add(votingGroup);
                db.SaveChanges();

                return RedirectToAction(string.Format("Details/{0}", view.VotingId));
            }

            ViewBag.GroupId = new SelectList(db.Groups.OrderBy(g => g.Description), "GroupId", "Description");
            return View(view);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddGroup(int id)
        {

            ViewBag.GroupId = new SelectList(db.Groups.OrderBy(g => g.Description), "GroupId", "Description");

            var view = new AddGroupView
            {
                VotingId = id,
            };

            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var votings = db.Votings.Include(v => v.State);
            return View(votings.ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }

            var view = new DetailsVotingView
            {
                Candidates = voting.Candidates.ToList(),
                CandidateWinId = voting.CandidateWinId,
                DateTimeEnd = voting.DateTimeEnd,
                DateTimeStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnabledBlankVotes = voting.IsEnabledBlankVotes,
                IsForAllUsers = voting.IsForAllUsers,
                QuantityBlankVotes = voting.QuantityBlankVotes,
                QuantityVotes = voting.QuantityVotes,
                Remarks = voting.Remarks,
                StateId = voting.StateId,
                VotingGroups = voting.VotingGroups.ToList(),
                VotingId = voting.VotingId
            };

            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.StateId = new SelectList(db.States, "StateId", "Description");
            var view = new VotingView
            {
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now,
            };
            return View(view);
        }

        // POST: Votings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VotingView view)
        {
            if (ModelState.IsValid)
            {
                var voting = new Voting
                {
                    DateTimeEnd = view.DateEnd.AddHours(view.TimeEnd.Hour).AddMinutes(view.TimeEnd.Minute),
                    DateTimeStart = view.DateStart.AddHours(view.TimeStart.Hour).AddMinutes(view.TimeStart.Minute),
                    Description = view.Description,
                    IsEnabledBlankVotes = view.IsEnabledBlankVotes,
                    IsForAllUsers = view.IsForAllUsers,
                    Remarks = view.Remarks,
                    StateId = view.StateId,
                };

                db.Votings.Add(voting);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States, "StateId", "Description", view.StateId);
            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var voting = db.Votings.Find(id);

            if (voting == null)
            {
                return HttpNotFound();
            }

            var view = new VotingView
            {
                DateEnd = voting.DateTimeEnd,
                DateStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnabledBlankVotes = voting.IsEnabledBlankVotes,
                IsForAllUsers = voting.IsForAllUsers,
                Remarks = voting.Remarks,
                StateId = voting.StateId,
                TimeEnd = voting.DateTimeEnd,
                TimeStart = voting.DateTimeStart,
                VotingId = voting.VotingId
            };

            ViewBag.StateId = new SelectList(db.States, "StateId", "Description", voting.StateId);
            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VotingView view)
        {
            if (ModelState.IsValid)
            {
                var voting = new Voting
                {
                    DateTimeEnd = view.DateEnd.AddHours(view.TimeEnd.Hour).AddMinutes(view.TimeEnd.Minute),
                    DateTimeStart = view.DateStart.AddHours(view.TimeStart.Hour).AddMinutes(view.TimeStart.Minute),
                    Description = view.Description,
                    IsEnabledBlankVotes = view.IsEnabledBlankVotes,
                    IsForAllUsers = view.IsForAllUsers,
                    Remarks = view.Remarks,
                    StateId = view.StateId,
                    VotingId = view.VotingId
                };
                db.Entry(voting).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }

            //ViewBag.StateId = new SelectList(db.States, "StateId", "Description", view.StateId);
            return View(voting);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Voting voting = db.Votings.Find(id);
            db.Votings.Remove(voting);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
