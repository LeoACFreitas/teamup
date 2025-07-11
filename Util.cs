using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Teamup
{
    public static class Util
    {
        public static string HttpRequest(string url)
        {
            return new StreamReader(new HttpClient().GetAsync(new Uri(url)).Result.Content.ReadAsStream()).ReadToEnd();
        }

        public static string GetSubFromJwt(this HttpContext h)
        {
            var userClaims = h.User.Claims;
            return userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        public static User GetCurrentUser(this HttpContext httpContext, MyDbContext db)
        {
            var sub = Convert.ToDecimal(httpContext.GetSubFromJwt());
            return db.User.FirstOrDefault(u => u.Sub.Equals(sub));
        }

        public static void SendEmail(this HttpContext h, string message)
        {
            try
            {
                string targetEmail = "leoacdefreitas@gmail.com";
                var key = Environment.GetEnvironmentVariable("emailapikey");

                var client = new RestClient("https://api.mailgun.net/v3/sandbox3d078b52ff244435a5eb24a688564477.mailgun.org/messages");
                var request = new RestRequest();
                var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"api:{key}"));
                request.AddHeader("Authorization", $"Basic {authToken}");
                request.AddParameter("from", "Teamup Gaming <mailgun@sandbox3d078b52ff244435a5eb24a688564477.mailgun.org>");
                request.AddParameter("to", targetEmail);
                request.AddParameter("subject", "Teamup Alert");
                request.AddParameter("text", message);
                request.Method = RestSharp.Method.Post;

                client.Execute(request);
            }
            catch (Exception e)
            {
                var logger = h.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError($"Something went wrong: {e.Message}");
            }
        }

        public static readonly List<List<string>> Countries = [["Afghanistan", "AF"], ["Albania", "AL"], ["Algeria", "DZ"], ["Andorra", "AD"], ["Angola", "AO"], ["Antigua and Barbuda", "AG"], ["Argentina", "AR"], ["Armenia", "AM"], ["Australia", "AU"], ["Austria", "AT"], ["Azerbaijan", "AZ"], ["Bahamas", "BS"], ["Bahrain", "BH"], ["Bangladesh", "BD"], ["Barbados", "BB"], ["Belarus", "BY"], ["Belgium", "BE"], ["Belize", "BZ"], ["Benin", "BJ"], ["Bhutan", "BT"], ["Bolivia", "BO"], ["Bosnia and Herzegovina", "BA"], ["Botswana", "BW"], ["Brazil", "BR"], ["Brunei", "BN"], ["Bulgaria", "BG"], ["Burkina Faso", "BF"], ["Burundi", "BI"], ["Côte d'Ivoire", "CI"], ["Cabo Verde", "CV"], ["Cambodia", "KH"], ["Cameroon", "CM"], ["Canada", "CA"], ["Central African Republic", "CF"], ["Chad", "TD"], ["Chile", "CL"], ["China", "CN"], ["Colombia", "CO"], ["Comoros", "KM"], ["Congo", "CG"], ["Costa Rica", "CR"], ["Croatia", "HR"], ["Cuba", "CU"], ["Cyprus", "CY"], ["Czechia", "CZ"], ["Democratic Republic of the Congo", "CD"], ["Denmark", "DK"], ["Djibouti", "DJ"], ["Dominica", "DM"], ["Dominican Republic", "DO"], ["Ecuador", "EC"], ["Egypt", "EG"], ["El Salvador", "SV"], ["Equatorial Guinea", "GQ"], ["Eritrea", "ER"], ["Estonia", "EE"], ["Eswatini", "SZ"], ["Ethiopia", "ET"], ["Fiji", "FJ"], ["Finland", "FI"], ["France", "FR"], ["Gabon", "GA"], ["Gambia", "GM"], ["Georgia", "GE"], ["Germany", "DE"], ["Ghana", "GH"], ["Greece", "GR"], ["Grenada", "GD"], ["Guatemala", "GT"], ["Guinea", "GN"], ["Guinea-Bissau", "GW"], ["Guyana", "GY"], ["Haiti", "HT"], ["Holy See", "VA"], ["Honduras", "HN"], ["Hungary", "HU"], ["Iceland", "IS"], ["India", "IN"], ["Indonesia", "ID"], ["Iran", "IR"], ["Iraq", "IQ"], ["Ireland", "IE"], ["Israel", "IL"], ["Italy", "IT"], ["Jamaica", "JM"], ["Japan", "JP"], ["Jordan", "JO"], ["Kazakhstan", "KZ"], ["Kenya", "KE"], ["Kiribati", "KI"], ["Kuwait", "KW"], ["Kyrgyzstan", "KG"], ["Laos", "LA"], ["Latvia", "LV"], ["Lebanon", "LB"], ["Lesotho", "LS"], ["Liberia", "LR"], ["Libya", "LY"], ["Liechtenstein", "LI"], ["Lithuania", "LT"], ["Luxembourg", "LU"], ["Madagascar", "MG"], ["Malawi", "MW"], ["Malaysia", "MY"], ["Maldives", "MV"], ["Mali", "ML"], ["Malta", "MT"], ["Marshall Islands", "MH"], ["Mauritania", "MR"], ["Mauritius", "MU"], ["Mexico", "MX"], ["Micronesia", "FM"], ["Moldova", "MD"], ["Monaco", "MC"], ["Mongolia", "MN"], ["Montenegro", "ME"], ["Morocco", "MA"], ["Mozambique", "MZ"], ["Myanmar", "MM"], ["Namibia", "NA"], ["Nauru", "NR"], ["Nepal", "NP"], ["Netherlands", "NL"], ["New Zealand", "NZ"], ["Nicaragua", "NI"], ["Niger", "NE"], ["Nigeria", "NG"], ["North Korea", "KP"], ["North Macedonia", "MK"], ["Norway", "NO"], ["Oman", "OM"], ["Pakistan", "PK"], ["Palau", "PW"], ["Palestine", "PS"], ["Panama", "PA"], ["Papua New Guinea", "PG"], ["Paraguay", "PY"], ["Peru", "PE"], ["Philippines", "PH"], ["Poland", "PL"], ["Portugal", "PT"], ["Qatar", "QA"], ["Romania", "RO"], ["Russia", "RU"], ["Rwanda", "RW"], ["Saint Kitts and Nevis", "KN"], ["Saint Lucia", "LC"], ["Saint Vincent and the Grenadines", "VC"], ["Samoa", "WS"], ["San Marino", "SM"], ["Sao Tome and Principe", "ST"], ["Saudi Arabia", "SA"], ["Senegal", "SN"], ["Serbia", "RS"], ["Seychelles", "SC"], ["Sierra Leone", "SL"], ["Singapore", "SG"], ["Slovakia", "SK"], ["Slovenia", "SI"], ["Solomon Islands", "SB"], ["Somalia", "SO"], ["South Africa", "ZA"], ["South Korea", "KR"], ["South Sudan", "SS"], ["Spain", "ES"], ["Sri Lanka", "LK"], ["Sudan", "SD"], ["Suriname", "SR"], ["Sweden", "SE"], ["Switzerland", "CH"], ["Syria", "SY"], ["Tajikistan", "TJ"], ["Tanzania", "TZ"], ["Thailand", "TH"], ["Timor-Leste", "TL"], ["Togo", "TG"], ["Tonga", "TO"], ["Trinidad and Tobago", "TT"], ["Tunisia", "TN"], ["Turkey", "TR"], ["Turkmenistan", "TM"], ["Tuvalu", "TV"], ["Uganda", "UG"], ["Ukraine", "UA"], ["United Arab Emirates", "AE"], ["United Kingdom", "GB"], ["United States of America", "US"], ["Uruguay", "UY"], ["Uzbekistan", "UZ"], ["Vanuatu", "VU"], ["Venezuela", "VE"], ["Vietnam", "VN"], ["Yemen", "YE"], ["Zambia", "ZM"], ["Zimbabwe", "ZW"]];
    }
}
