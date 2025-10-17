using System.Collections.Generic;

public class Consts
{
    public static string gitVersion()
    {
        return GGitVersion.GitVersion;
    }
    public const string ADMIN_USER_COOKIE_KEY = "adminUserId";
    public const string ADMIN_TOKEN_COOKIE_KEY = "adminSes";
    public const string ADMIN_SESSION_COOKIE_KEY = "adminToken";



    public const string ITEM_USER_ID = "userId";
    public const string ITEM_NUSER_ID = "nUserId";
    public const string ITEM_Session_ID = "sessionId";

    public static List<string> servers = new()
    {
        "http://localhost:3010",
        "https://localhost:3011",
        "https://admin-server.oncodraw.com",
        "https://admin-server2.oncodraw.com",
        "https://alfa-admin.oncodraw.com"
    };
    public static List<string> adminClients = new()
    {
                                                "https://localhost:3000",
                                                "http://localhost:3000",

                                                "http://localhost:3020",
                                                "https://localhost:3021",


                                                "http://oncodraw.com:3020",
                                                "https://oncodraw.com:3021",

                                                 "http://admin.oncodraw.com",
                                                "https://admin.oncodraw.com",

    };
}