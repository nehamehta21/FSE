using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyTwitterClone.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace MyTwitterClone.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult UpdateProfile(Entity model)
        {
            if (Session["UserName"] != null)
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ToString());
                con.Open();

                string query = "select password,fullName,email from person where user_id=@userName";

                var comm = new SqlCommand(query, con);
                try
                {
                    comm.Parameters.AddWithValue("@userName", Session["UserName"].ToString());
                    SqlDataReader reader = comm.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        model.Password = Convert.ToString(reader["password"]);
                        model.FullName = Convert.ToString(reader["fullName"]);
                        model.Email = Convert.ToString(reader["email"]);                        
                    }

                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "An Error Occurred!";
                    // MessageBox.Show("Not Saved");
                }
                finally
                {
                    con.Close();
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult UpdateProfile(Entity model, string name)
        {
            if (Session["UserName"] != null)
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ToString());
                con.Open();

                string query = "update person set password=@password,fullName=@fullName,email=@email where user_id=@userName";
               
                var comm = new SqlCommand(query, con);
                try
                {                    
                    comm.Parameters.AddWithValue("@password", model.Password);
                    comm.Parameters.AddWithValue("@fullName", model.FullName);
                    comm.Parameters.AddWithValue("@email", model.Email);
                    comm.Parameters.AddWithValue("@userName", Session["UserName"].ToString());
                    comm.ExecuteNonQuery();
                    ViewBag.UpdateMessage = "Profile Successfully Updated!";

                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "An Error Occurred!";
                    // MessageBox.Show("Not Saved");
                }
                finally
                {
                    con.Close();
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            } 
            
        }

        [HttpPost]
        public ActionResult Index(UserDetails model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ToString());
            con.Open();
            string query = "SELECT COUNT(*) FROM Person where user_id=@userid and password=@password";

            var comm = new SqlCommand(query, con);
            comm.Parameters.AddWithValue("@userid", model.UserName);
            comm.Parameters.AddWithValue("@password", model.Password);
            try
            {
                var count = (Int32)comm.ExecuteScalar();
                if (count > 0)
                {
                    Session["UserName"] = model.UserName;
                    return RedirectToAction("Home");
                }
                else
                {
                    ViewBag.LoginMessage = "Login Failed!";
                }

            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An Error Occurred!";
                // MessageBox.Show("Not Saved");
            }
            finally
            {
                con.Close();
            }
            return View();
        }
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(UserDetails model)
        {
            //bool isSuccessful = false;
            bool isExistingUser = IsExistingUser(model);
            if (!isExistingUser)
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ToString());
                con.Open();

                string query = "insert into person(user_id,password,fullName,email," +
                    "joined,active)values(@userName,@password,@fullname,@email,@joined,@active)";

                var comm = new SqlCommand(query, con);
                try
                {
                    comm.Parameters.AddWithValue("@userName", model.UserName);
                    comm.Parameters.AddWithValue("@password", model.Password);
                    comm.Parameters.AddWithValue("@fullname", model.FullName);
                    comm.Parameters.AddWithValue("@email", model.Email);
                    comm.Parameters.AddWithValue("@joined", DateTime.Now);
                    comm.Parameters.AddWithValue("@active", "true");
                    comm.ExecuteNonQuery();
                    ViewBag.SignUpMessage = "Registration Done!";

                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "An Error Occurred!";
                    // MessageBox.Show("Not Saved");
                }
                finally
                {
                    con.Close();
                }
            }

            return View();
        }

        private bool IsExistingUser(UserDetails model)
        {
            bool isExistingUser = false;
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ToString());
            con.Open();
            string query = "SELECT COUNT(*) FROM Person where user_id=@userid";
            var comm = new SqlCommand(query, con);
            comm.Parameters.AddWithValue("@userid", model.UserName);
            try
            {
                var count = (Int32)comm.ExecuteScalar();
                if (count > 0)
                {
                    isExistingUser = true;
                    ViewBag.SignUpMessage = "You are already registered!";
                }
                else
                {
                    isExistingUser = false;
                }

            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An Error Occurred!";
                // MessageBox.Show("Not Saved");
            }
            finally
            {
                con.Close();
            }
            return isExistingUser;
        }

        [HttpGet]
        public ActionResult Home(Entity model)
        {
            if (Session["UserName"] != null)
            {
                GetTweets(model);

                return View(model);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        private void GetTweets(Entity model)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ToString());
            con.Open();

            string query1 = "select tweet.user_id,Tweet.message, tweet.created from Tweet " +
            "left join following on tweet.user_id = Following.following_id" +
            " where Following.user_id = @userName1 union select tweet.user_id," +
            "Tweet.message, tweet.created from Tweet where Tweet.user_id = @userName1" +
            " order by tweet.created desc";

            var comm1 = new SqlCommand(query1, con);
            try
            {
                comm1.Parameters.AddWithValue("@userName1", Session["UserName"].ToString());

                SqlDataReader reader = comm1.ExecuteReader();
                model.TweetList = null;
                while (reader.Read())
                {
                    model.TweetList = model.TweetList ?? new List<Tweet>();
                    Tweet tweet = new Tweet
                    {
                        UserName = Convert.ToString(reader["user_id"]),
                        Created = Convert.ToDateTime(reader["created"]),
                        Message = Convert.ToString(reader["message"])
                    };
                    model.TweetList.Add(tweet);
                }

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An Error Occurred!";
                // MessageBox.Show("Not Saved");
            }
            finally
            {
                con.Close();
            }
        }

        [HttpPost]
        public ActionResult Search(Entity model, string sname)
        {
            if (!string.IsNullOrWhiteSpace(sname))
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ToString());
                con.Open();

                string query1 = "select USER_ID from person where user_id like @userName1";

                var comm1 = new SqlCommand(query1, con);
                try
                {
                    comm1.Parameters.AddWithValue("@userName1", sname + "%");

                    SqlDataReader reader = comm1.ExecuteReader();
                    model.SearchUserList = null;
                    while (reader.Read())
                    {
                        model.SearchUserList = model.SearchUserList ?? new List<RegisteredUser>();
                        var objOfRegUser = new RegisteredUser()
                        {
                            UserName = Convert.ToString(reader["USER_ID"])
                        };
                        model.SearchUserList.Add(objOfRegUser);
                    }

                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "An Error Occurred!";
                    // MessageBox.Show("Not Saved");
                }
                finally
                {
                    con.Close();
                }
            }

            GetTweets(model);
            return View("Home", model);
        }
        [HttpGet]
        public ActionResult Follow(string UserName)
        {
            if (Session["UserName"] != null)
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ToString());
                con.Open();

                string query1 = "insert into Following values (@userId,@followinId)";

                var comm1 = new SqlCommand(query1, con);
                try
                {
                    comm1.Parameters.AddWithValue("@userId", Session["UserName"].ToString());
                    comm1.Parameters.AddWithValue("@followinId", UserName);

                    comm1.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "An Error Occurred!";
                    // MessageBox.Show("Not Saved");
                }
                finally
                {
                    con.Close();
                }
                //GetTweets(model);
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Home(Entity model, string userName)
        {
            //bool isSuccessful = false;

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ToString());
            con.Open();

            string query = "insert into Tweet(user_id,message,created)" +
                "values(@userName,@message,@created)";

            var comm = new SqlCommand(query, con);
            string query1 = "select tweet.user_id,Tweet.message, tweet.created from Tweet " +
                "left join following on tweet.user_id = Following.following_id" +
                " where Following.user_id = @userName1 union select tweet.user_id," +
                "Tweet.message, tweet.created from Tweet where Tweet.user_id = @userName1" +
                " order by tweet.created desc";

            var comm1 = new SqlCommand(query1, con);
            try
            {
                comm.Parameters.AddWithValue("@userName", Session["UserName"].ToString());
                comm.Parameters.AddWithValue("@message", model.TweetMessage);
                comm.Parameters.AddWithValue("@created", DateTime.Now);
                comm.ExecuteNonQuery();
                //ViewBag.SignUpMessage = "Registration Done!";


                comm1.Parameters.AddWithValue("@userName1", Session["UserName"].ToString());

                SqlDataReader reader = comm1.ExecuteReader();
                model.TweetList = null;
                while (reader.Read())
                {
                    model.TweetList = model.TweetList ?? new List<Tweet>();
                    Tweet tweet = new Tweet
                    {
                        UserName = Convert.ToString(reader["user_id"]),
                        Created = Convert.ToDateTime(reader["created"]),
                        Message = Convert.ToString(reader["message"])
                    };
                    model.TweetList.Add(tweet);
                }

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An Error Occurred!";
                // MessageBox.Show("Not Saved");
            }
            finally
            {
                con.Close();
            }

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}