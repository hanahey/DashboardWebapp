using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DashboardWebapp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace DashboardWebapp.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        DataContext db = new DataContext();
        static int currentPersonId;
        // GET: Items
        public ActionResult Index()
        {
            //get logged in user
            string currentUserId = System.Web.HttpContext.Current.GetOwinContext().
                 GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()).Id;
            currentPersonId = (from c in db.People where c.UserId == currentUserId select c).FirstOrDefault().Id;

            var items = (from i in db.Items
                         where i.PersonId == currentPersonId
                         orderby i.Id descending
                         select new ItemViewModel
                         {
                             Id = i.Id,
                             Name = i.Name,
                             Price = i.Price,
                             Store = i.Store,
                             Quantity = i.Quantity,
                             Measurement = i.Measurement,
                             PersonId = i.PersonId,
                             Person = i.Person
                         }).ToList();

            //get tags for all af this user's items
            foreach (ItemViewModel item in items)
            {
                var thisItemTags = (from i in db.Items
                                    join itemTag in db.ItemTags on i.Id equals itemTag.ItemId
                                    join tag in db.Tags on itemTag.TagId equals tag.Id
                                    where i.Id == item.Id
                                    select new
                                    {
                                        Id = tag.Id,
                                        Name = tag.Name,
                                        PersonId = tag.PersonId,
                                    }).ToList();

                item.Tags = thisItemTags.Select(x => new Tag
                {
                    Id = x.Id,
                    Name = x.Name,
                    PersonId = x.PersonId,
                }).ToList();
            }

            return View(items);
        }

        // GET: Items/AddItem
        public ActionResult AddItem()
        {
            ItemViewModel item = new ItemViewModel();
            item.TagCollection = from t in db.Tags where t.PersonId == currentPersonId select t;
            return PartialView(item);
        }

        // POST: Items/AddItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddItem(ItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                var item = new Item
                {
                    Name = model.Name,
                    Price = model.Price,
                    Store = model.Store,
                    Quantity = model.Quantity,
                    Measurement = model.Measurement,
                    PersonId = currentPersonId,
                };
                db.Items.Add(item);

                if (model.TagIds != null)
                {
                    foreach (int t in model.TagIds)
                    {
                        var itemTag = new Item_Tag
                        {
                            ItemId = item.Id,
                            TagId = t,
                        };
                        db.ItemTags.Add(itemTag);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                model.TagCollection = from t in db.Tags where t.PersonId == currentPersonId select t;
                return PartialView(model);
            }
        }

        // GET: Items/EditItem/5
        public ActionResult EditItem(int id)
        {
            ItemViewModel item = db.Items.Where(i => i.Id == id).Select(x => new ItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                Store = x.Store,
                Quantity = x.Quantity,
                Measurement = x.Measurement,
                PersonId = x.PersonId,
                Person = x.Person
            }).First();

            //Get all TagIds tagged to this item
            var tagIds = (from itemTag in db.ItemTags
                          join tag in db.Tags on itemTag.TagId equals tag.Id
                          where itemTag.ItemId == item.Id
                          select tag.Id).ToList();

            item.TagIds = tagIds;

            //Populate Tag dropdown list
            item.TagCollection = (from t in db.Tags where t.PersonId == currentPersonId select t).ToList();

            return PartialView(item);
        }

        // POST: Items/EditItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditItem(int id, ItemViewModel model)
        {
            var item = db.Items.Where(i => i.Id == id).FirstOrDefault();
            item.Name = model.Name;
            item.Price = model.Price;
            item.Store = model.Store;
            item.Quantity = model.Quantity;
            item.Measurement = model.Measurement;

            //check for existing Item_Tags for this item to ensure no duplicates are bing added
            List<int> thisItemExistingTagIds = (from i in db.Items
                                                join existingItemTag in db.ItemTags on i.Id equals existingItemTag.ItemId
                                                join tag in db.Tags on existingItemTag.TagId equals tag.Id
                                                where i.Id == item.Id
                                                select tag.Id).ToList();

            if (model.TagIds != null)
            {
                foreach (int t in model.TagIds)
                {
                    if(!thisItemExistingTagIds.Contains(t))
                    {
                        var itemTag = new Item_Tag
                        {
                            ItemId = item.Id,
                            TagId = t,
                        };
                        db.ItemTags.Add(itemTag);
                    }
                }

                //remove tags from item if they have been untagged
                foreach (int tagIdToRemove in thisItemExistingTagIds)
                {
                    if (!model.TagIds.Contains(tagIdToRemove))
                    {
                        var tagToRemove = (from t in db.ItemTags where t.TagId == tagIdToRemove && t.ItemId == model.Id select t).First();
                        db.ItemTags.Remove(tagToRemove);
                    }
                }
            }
            //remove all existing tags if none are selected
            else if (model.TagIds == null && thisItemExistingTagIds != null)
            {
                foreach (int tagIdToRemove in thisItemExistingTagIds)
                {
                    var tagToRemove = (from t in db.ItemTags where t.TagId == tagIdToRemove && t.ItemId == model.Id select t).First();
                    db.ItemTags.Remove(tagToRemove);
                }
            }

            if (ModelState.IsValid)
            {
                db.SaveChanges();
                TempData["Success"] = "Changes Saved!";
                return RedirectToAction("Index");
            }
            else
            {
                model.TagCollection = from t in db.Tags where t.PersonId == currentPersonId select t;
                return PartialView(model);
            }
        }

        // GET: Items/Delete/5
        public ActionResult DeleteItem(int id)
        {
            var item = db.Items.Where(i => i.Id == id).FirstOrDefault();
            return PartialView(item);
        }

        // POST: Items/Delete/5
        [HttpPost]
        public ActionResult DeleteItem(int id, Item item)
        {
            item = db.Items.Where(i => i.Id == id).FirstOrDefault();
            db.Items.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
