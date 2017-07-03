using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using YammerUserExtract.Contracts;

namespace YammerUserExtract
{
    class Program
    {
        private static string BearerToken = "PROVIDE YOUR OWN TOKEN"; // https://developer.yammer.com/docs/test-token
        static void Main(string[] args)
        {
            var users = GetAllUsers().Result;

            using (var writer = File.CreateText("users.csv"))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(users);
            }

            var connections = users
                .Select(yammerUser =>
                {
                    var followers = GetFollowers(yammerUser).Result;

                    return new { User = yammerUser, Followers = followers };

                }).SelectMany(a => a.Followers, (x, follower) =>
                new
                {
                    userId = x.User.id,
                    userFullName = x.User.full_name,
                    followerId = follower.id,
                    followerFullName = users.Single(a => a.id == follower.id).full_name
                });

            using (var writer = File.CreateText("connections.csv"))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(connections);
            }
        }

        private static async Task<YammerUser[]> GetAllUsers()
        {
            var usersEndpoint = "https://www.yammer.com/api/v1/users.json";
            var allYammerUsers = new List<YammerUser>();
            var currentPage = 1;
            using (var client = new HttpClient())
            {
                PrepareHttpClient(client, usersEndpoint);

                while (true)
                {
                    Console.WriteLine($"Loading yammer users page {currentPage}");
                    var response = await client.GetAsync($"?page={currentPage}");
                    var payload = await response.Content.ReadAsStringAsync();
                    var users = Newtonsoft.Json.JsonConvert.DeserializeObject<YammerUser[]>(payload);
                    allYammerUsers.AddRange(users);
                    currentPage++;
                    await Task.Delay(1000); // Yammer throttling
                    if (users.Length < 50)
                        break;
                }
            }
            return allYammerUsers.ToArray();
        }

        private static async Task<Follower[]> GetFollowers(YammerUser yammerUser)
        {
            var followersEndpoint = $"https://api.yammer.com/api/v1/users/following/{yammerUser.id}.json";
            var allFollowers = new List<Follower>();
            var currentPage = 1;
            using (var client = new HttpClient())
            {
                PrepareHttpClient(client, followersEndpoint);

                while (true)
                {
                    Console.WriteLine($"Loading page {currentPage} of yammer followers of {yammerUser.full_name}");
                    var response = await client.GetAsync($"?page={currentPage}");
                    var payload = await response.Content.ReadAsStringAsync();
                    var followers = Newtonsoft.Json.JsonConvert.DeserializeObject<Followers>(payload);
                    if (followers.users == null || followers.users.Count == 0)
                        break;
                    allFollowers.AddRange(followers.users);
                    currentPage++;

                    await Task.Delay(1000); // Yammer throttling
                    if (!followers.more_available)
                        break;
                }
            }

            return allFollowers.ToArray();
        }

        private static void PrepareHttpClient(HttpClient client, string usersEndpoint)
        {
            client.BaseAddress = new Uri(usersEndpoint);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
        }
    }
}
