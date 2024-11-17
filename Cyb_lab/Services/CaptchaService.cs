using Azure;
using Cyb_lab.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace Cyb_lab.Services;

public class CaptchaService
{
	private IConfiguration _configuration;
    public CaptchaService(IConfiguration iConfig)
    {
        _configuration = iConfig;
    }
    public static async Task<bool> VerifiyReCaptchaV2(string response, string secret)
	{
		using var client = new HttpClient();

		string url = "https://www.google.com/recaptcha/api/siteverify";

		MultipartFormDataContent content = new();
		content.Add(new StringContent(response), "response");
		content.Add(new StringContent(secret), "secret");

		var result = await client.PostAsync(url, content);

		if (!result.IsSuccessStatusCode)
		{
			return false;
		}

		var strResponse = await result.Content.ReadAsStringAsync();
		Console.WriteLine($"strResponse: {strResponse}");

		var jsonResponse = JsonNode.Parse(strResponse);

		if (jsonResponse is null)
		{
			return false;
		}

		var success = (bool?)jsonResponse["success"];

		if (!success.HasValue)
		{
			return false;
		}

		return (bool)success;
	}

	public static async Task<bool> VerifyReCaptchaV3(string token, string secret)
	{
		try
		{
			var url = $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}";

			using (var client = new HttpClient())
			{
				var httpResult = await client.GetAsync(url);
				if(httpResult.StatusCode != System.Net.HttpStatusCode.OK)
				{
					return false;
				}

				var responseString = await httpResult.Content.ReadAsStringAsync();

				var result = JsonConvert.DeserializeObject<GoogleCaptchaV3Response>(responseString);

				return result.success && result.score >= 0.5;
			}
		}
		catch (Exception e)
		{
			return false;
		}
	}
}
