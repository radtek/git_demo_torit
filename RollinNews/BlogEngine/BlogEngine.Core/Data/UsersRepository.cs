﻿using BlogEngine.Core.Data.Contracts;
using BlogEngine.Core.Data.Models;
using RDN.Library.Classes.EmailServer;
using RDN.Library.Classes.Error;
using RDN.Portable.Config;
//using RN.Library.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Security;

namespace BlogEngine.Core.Data
{
    /// <summary>
    /// Users repository
    /// </summary>
    public class UsersRepository : IUsersRepository
    {
        /// <summary>
        /// Post list
        /// </summary>
        /// <param name="filter">Filter expression</param>
        /// <param name="order">Order expression</param>
        /// <param name="skip">Records to skip</param>
        /// <param name="take">Records to take</param>
        /// <returns>List of users</returns>
        public IEnumerable<BlogUser> Find(int take = 10, int skip = 0, string filter = "", string order = "")
        {


            var users = new List<BlogUser>();
            int count;

            var userCollection = Membership.Provider.GetAllUsers(0, 999999, out count);
            var members = userCollection.Cast<MembershipUser>().ToList();

            for (int i = 0; i < members.Count; i++)
            {
                users.Add(new BlogUser
                {
                    IsChecked = false,
                    UserName = members[i].UserName,
                    Email = members[i].Email,
                    //Profile = GetProfile(m.UserName),
                    Roles = GetRoles(members[i].UserName)
                });
            }

            var query = users.AsQueryable().Where(filter);

            // if take passed in as 0, return all
            if (take == 0) take = users.Count;

            return query.OrderBy(order).Skip(skip).Take(take);
        }

        /// <summary>
        /// Get single post
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>User object</returns>
        public BlogUser FindById(string id)
        {

            var member = Membership.GetUser(id);

            return new BlogUser
            {
                IsChecked = false,
                UserName = member.UserName,
                Email = member.Email,
                //Profile = GetProfile(m.UserName),
                Roles = GetRoles(member.UserName)
            };

        }

        /// <summary>
        /// Add new user
        /// </summary>
        /// <param name="user">Blog user</param>
        /// <returns>Saved user</returns>
        public BlogUser Add(BlogUser user)
        {
            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.CreateNewUsers))
                throw new System.UnauthorizedAccessException();

            if (user == null || string.IsNullOrEmpty(user.UserName)
                || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                throw new ApplicationException("Error adding new user; Missing required fields");
            }

            if (!Security.IsAuthorizedTo(Rights.CreateNewUsers))
                throw new ApplicationException("Not authorized");

            // create user
            var usr = Membership.CreateUser(user.UserName, user.Password, user.Email);
            if (usr == null)
                throw new ApplicationException("Error creating new user");

            UpdateUserProfile(user);

            UpdateUserRoles(user);

            user.Password = "";
            return user;
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="user">User to update</param>
        /// <returns>True on success</returns>
        public bool Update(BlogUser user)
        {
            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.EditOwnUser))
                throw new System.UnauthorizedAccessException();

            if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email))
                throw new ApplicationException("Error adding new user; Missing required fields");

            if (!Security.IsAuthorizedTo(Rights.EditOwnUser))
                throw new ApplicationException("Not authorized");

            // update user
            var usr = Membership.GetUser(user.UserName);

            if (usr == null)
                return false;

            usr.Email = user.Email;
            Membership.UpdateUser(usr);

            UpdateUserProfile(user);

            UpdateUserRoles(user);

            return true;
        }

        /// <summary>
        /// Save user profile
        /// </summary>
        /// <param name="user">Blog user</param>
        /// <returns>True on success</returns>
        public bool SaveProfile(BlogUser user)
        {
            return UpdateUserProfile(user);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>True on success</returns>
        public bool Remove(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            if (!Security.IsAuthorizedTo(BlogEngine.Core.Rights.DeleteUserSelf))
                throw new System.UnauthorizedAccessException();

            bool isSelf = id.Equals(Security.CurrentUser.Identity.Name, StringComparison.OrdinalIgnoreCase);

            if (isSelf && !Security.IsAuthorizedTo(Rights.DeleteUserSelf))
                throw new ApplicationException("Not authorized");

            else if (!isSelf && !Security.IsAuthorizedTo(Rights.DeleteUsersOtherThanSelf))
                throw new ApplicationException("Not authorized");

            // Last check - it should not be possible to remove the last use who has the right to Add and/or Edit other user accounts. If only one of such a 
            // user remains, that user must be the current user, and can not be deleted, as it would lock the user out of the BE environment, left to fix
            // it in XML or SQL files / commands. See issue 11990
            bool adminsExist = false;
            MembershipUserCollection users = Membership.GetAllUsers();
            foreach (MembershipUser user in users)
            {
                string[] roles = Roles.GetRolesForUser(user.UserName);

                // look for admins other than 'id' 
                if (!id.Equals(user.UserName, StringComparison.OrdinalIgnoreCase) && (Right.HasRight(Rights.EditOtherUsers, roles) || Right.HasRight(Rights.CreateNewUsers, roles)))
                {
                    adminsExist = true;
                    break;
                }
            }

            if (!adminsExist)
                throw new ApplicationException("Can not delete last admin");

            string[] userRoles = Roles.GetRolesForUser(id);

            try
            {
                if (userRoles.Length > 0)
                {
                    Roles.RemoveUsersFromRoles(new string[] { id }, userRoles);
                }

                Membership.DeleteUser(id);

                var pf = AuthorProfile.GetProfile(id);
                if (pf != null)
                {
                    BlogEngine.Core.Providers.BlogService.DeleteProfile(pf);
                }
            }
            catch (Exception ex)
            {
                Utils.Log("Error deleting user", ex.Message);
                return false;
            }
            return true;
        }

        #region Private methods

        static Profile GetProfile(string id)
        {
            if (!Utils.StringIsNullOrWhitespace(id))
            {
                var pf = AuthorProfile.GetProfile(id);
                if (pf == null)
                {
                    pf = new AuthorProfile(id);
                    pf.Birthday = DateTime.Parse("01/01/1900");
                    pf.DisplayName = id;
                    pf.EmailAddress = Utils.GetUserEmail(id);
                    pf.FirstName = id;
                    pf.Private = true;
                    pf.Save();
                }

                return new Profile
                {
                    AboutMe = string.IsNullOrEmpty(pf.AboutMe) ? "" : pf.AboutMe,
                    Birthday = pf.Birthday.ToShortDateString(),
                    CityTown = string.IsNullOrEmpty(pf.CityTown) ? "" : pf.CityTown,
                    Country = string.IsNullOrEmpty(pf.Country) ? "" : pf.Country,
                    DisplayName = pf.DisplayName,
                    EmailAddress = pf.EmailAddress,
                    PhoneFax = string.IsNullOrEmpty(pf.PhoneFax) ? "" : pf.PhoneFax,
                    FirstName = string.IsNullOrEmpty(pf.FirstName) ? "" : pf.FirstName,
                    Private = pf.Private,
                    LastName = string.IsNullOrEmpty(pf.LastName) ? "" : pf.LastName,
                    MiddleName = string.IsNullOrEmpty(pf.MiddleName) ? "" : pf.MiddleName,
                    PhoneMobile = string.IsNullOrEmpty(pf.PhoneMobile) ? "" : pf.PhoneMobile,
                    PhoneMain = string.IsNullOrEmpty(pf.PhoneMain) ? "" : pf.PhoneMain,
                    PhotoUrl = string.IsNullOrEmpty(pf.PhotoUrl) ? "" : pf.PhotoUrl.Replace("\"", ""),
                    RegionState = string.IsNullOrEmpty(pf.RegionState) ? "" : pf.RegionState
                };
            }
            return null;
        }

        static List<Data.Models.RoleItem> GetRoles(string id)
        {
            var roles = new List<Data.Models.RoleItem>();
            var userRoles = new List<Data.Models.RoleItem>();

            roles.AddRange(System.Web.Security.Roles.GetAllRoles().Select(r => new Data.Models.RoleItem { RoleName = r, IsSystemRole = Security.IsSystemRole(r) }));
            roles.Sort((r1, r2) => string.Compare(r1.RoleName, r2.RoleName));

            foreach (var r in roles)
            {
                if (System.Web.Security.Roles.IsUserInRole(id, r.RoleName))
                {
                    userRoles.Add(r);
                }
            }
            return userRoles;
        }

        static bool UpdateUserProfile(BlogUser user)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName))
                return false;

            var pf = AuthorProfile.GetProfile(user.UserName)
                ?? new AuthorProfile(user.UserName);
            try
            {
                pf.DisplayName = user.Profile.DisplayName;
                pf.FirstName = user.Profile.FirstName;
                pf.MiddleName = user.Profile.MiddleName;
                pf.LastName = user.Profile.LastName;
                pf.EmailAddress = user.Email; // user.Profile.EmailAddress;

                DateTime date;
                if (user.Profile.Birthday.Length == 0)
                    user.Profile.Birthday = "1/1/1001";

                if (DateTime.TryParse(user.Profile.Birthday, out date))
                    pf.Birthday = date;

                pf.PhotoUrl = user.Profile.PhotoUrl.Replace("\"", "");
                pf.Private = user.Profile.Private;

                pf.PhoneMobile = user.Profile.PhoneMobile;
                pf.PhoneMain = user.Profile.PhoneMain;
                pf.PhoneFax = user.Profile.PhoneFax;

                pf.CityTown = user.Profile.CityTown;
                pf.RegionState = user.Profile.RegionState;
                pf.Country = user.Profile.Country;
                pf.AboutMe = user.Profile.AboutMe;

                pf.Save();
                UpdateProfileImage(pf);
            }
            catch (Exception ex)
            {
                Utils.Log("Error editing profile", ex);
                return false;
            }
            return true;
        }

        static bool UpdateUserRoles(BlogUser user)
        {
            try
            {
                // remove all user roles and add only checked
                string[] currentRoles = Roles.GetRolesForUser(user.UserName);

                if (currentRoles.Length > 0)
                    Roles.RemoveUserFromRoles(user.UserName, currentRoles);
                bool contributorChange = false;
                bool authorChange = false;



                if (user.Roles.Count > 0)
                {
                    List<string> roles = user.Roles.Where(ur => ur.IsChecked).Select(r => r.RoleName).ToList();

                    if (!currentRoles.Contains("Contributor") && roles.Contains("Contributor"))
                        contributorChange = true;

                    if (!currentRoles.Contains("Author") && roles.Contains("Author"))
                        authorChange = true;

                    if (!currentRoles.Contains("Administrators") && roles.Contains("Administrators"))
                        roles.Remove("Administrators");

                    if (roles.Count > 0)
                        Roles.AddUsersToRoles(new string[] { user.UserName }, roles.ToArray());
                    else
                        Roles.AddUsersToRoles(new string[] { user.UserName }, new string[] { BlogConfig.AnonymousRole });
                }

                if (contributorChange)
                {
                    var id = RDN.Library.Classes.Account.User.GetMemberId(user.UserName);
                    var member = RDN.Library.Cache.SiteCache.GetPublicMember(id);
                    var emailData = new Dictionary<string, string>
                                        {
                                            { "derbyname",member.DerbyName},
                                            {"link",RollinNewsConfig.DEFAULT_LOGIN_URL}
                                          };

                    EmailServer.SendEmail(RollinNewsConfig.DEFAULT_EMAIL, RollinNewsConfig.DEFAULT_EMAIL_FROM_NAME, user.Email, EmailServer.DEFAULT_SUBJECT_ROLLIN_NEWS + " Added As Contributor", emailData, EmailServerLayoutsEnum.RNAddedAsEditor);
                }
                if (authorChange)
                {
                    var id = RDN.Library.Classes.Account.User.GetMemberId(user.UserName);
                    var member = RDN.Library.Cache.SiteCache.GetPublicMember(id);
                    var emailData = new Dictionary<string, string>
                                        {
                                            { "derbyname",member.DerbyName},
                                            {"link",RollinNewsConfig.DEFAULT_LOGIN_URL}
                                          };

                    EmailServer.SendEmail(RollinNewsConfig.DEFAULT_EMAIL, RollinNewsConfig.DEFAULT_EMAIL_FROM_NAME, user.Email, EmailServer.DEFAULT_SUBJECT_ROLLIN_NEWS + " Added As Author", emailData, EmailServerLayoutsEnum.RNAddedAsTrustedEditor);
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorDatabaseManager.AddException(ex, ex.GetType(), additionalInformation: Newtonsoft.Json.JsonConvert.SerializeObject(user));
                return false;
            }
        }

        /// <summary>
        /// Remove any existing profile images
        /// </summary>
        /// <param name="profile">User profile</param>
        static void UpdateProfileImage(AuthorProfile profile)
        {
            var dir = BlogEngine.Core.Providers.BlogService.GetDirectory("/avatars");

            if (string.IsNullOrEmpty(profile.PhotoUrl))
            {
                foreach (var f in dir.Files)
                {
                    var dot = f.Name.IndexOf(".");
                    var img = dot > 0 ? f.Name.Substring(0, dot) : f.Name;
                    if (profile.UserName == img)
                    {
                        f.Delete();
                    }
                }
            }
            else
            {
                foreach (var f in dir.Files)
                {
                    var dot = f.Name.IndexOf(".");
                    var img = dot > 0 ? f.Name.Substring(0, dot) : f.Name;
                    // delete old profile image saved with different name
                    // for example was admin.jpg and now admin.png
                    if (profile.UserName == img && f.Name != profile.PhotoUrl.Replace("\"", ""))
                    {
                        f.Delete();
                    }
                }
            }
        }

        #endregion
    }
}
